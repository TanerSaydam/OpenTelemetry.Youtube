using Microsoft.EntityFrameworkCore;
using Observability.Youtube.WebAPI;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using static Product;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer("Data Source=TANER\\SQLEXPRESS;Initial Catalog=OpenTelemetryDb;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
});

builder.Services
    .AddOpenTelemetry().WithTracing(configure =>
    {
        configure
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebAPI"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
            options.EnrichWithIDbCommand = (activty, dbCommand) =>
            {
                //manuel zenginleþtirme
            };
        })
        .AddConsoleExporter()
        .AddOtlpExporter()
        ;
    })
    ;

builder.Services.AddTransient<ReqAndResActivityBodyMiddleware>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddTransient<ProductService>();
builder.Services.AddTransient<ProductRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Activity.Current!.SetStatus(Status.Error);
        Activity.Current!.AddEvent(new ActivityEvent("error"));
        Activity.Current!.AddTag("error.message", ex.Message);
        Activity.Current.AddTag("error.stack.trace", ex.StackTrace);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(ex.Message);
    }
});

app.UseMiddleware<ReqAndResActivityBodyMiddleware>();

app.MapGet("/exception-api", () =>
{
    Activity.Current!.AddTag("user.id", "1");

    throw new ArgumentException("my exception");
    return "Hello World! 1";
});
app.MapGet("/hello-world", () => "Hello World! 2");

app.MapPost("/create-product", async (CreateDto request, ProductService productService) =>
{
    await productService.CreateAsync(request);
    return Results.Ok(new { Message = "Create is successful" });
});

app.MapGet("get-todo", async (HttpClient httpClient) =>
{
    var message = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
    if (message.IsSuccessStatusCode)
    {
        var content = await message.Content.ReadFromJsonAsync<object>();
        return Results.Ok(content);
    }

    return Results.BadRequest("Something went wrong");
});

app.Run();
