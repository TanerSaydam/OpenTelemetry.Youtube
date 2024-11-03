using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Observability.Youtube.ConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        using TracerProvider tracerProvider = Sdk
            .CreateTracerProviderBuilder()
            .AddSource("ConsoleApp.Trace")
            .ConfigureResource(configure =>
            {
                configure.AddService("MyConsoleService");
                configure.AddAttributes(
                    new List<KeyValuePair<string, object>>()
                    {
                        new("test.key", "test value"),
                        new("machine.name", Environment.MachineName),
                        new("process.path", Environment.ProcessPath ?? "")
                    });
            })
            .AddConsoleExporter()
            .AddOtlpExporter()
            .Build();

        ServiceHelper serviceHelper = new();
        serviceHelper.Method1();


        Console.ReadLine();
    }
}


internal class ServiceHelper
{
    public void Method1()
    {
        using var acitivty = ActivitySourceProvider.ActivitySource.StartActivity()!;
        acitivty.ActivityTraceFlags = ActivityTraceFlags.Recorded;
        acitivty.AddTag("user.id", "1");
        acitivty.AddTag("user.id", "2");
        acitivty.AddTag("user.id", "3");
        //acitivty.SetTag("user.id", "3");


        Console.WriteLine("Process 1");
        Console.WriteLine("Process 2");
        acitivty.AddEvent(new ActivityEvent("my.event"));
        Console.WriteLine("Process 3");



        Method2();
    }

    public void Method2()
    {
        using var activity = ActivitySourceProvider.ActivitySource.StartActivity()!;

        Console.WriteLine("Process 4");
    }
}

internal static class ActivitySourceProvider
{
    public static ActivitySource ActivitySource = new("ConsoleApp.Trace", "1.0.0");
}