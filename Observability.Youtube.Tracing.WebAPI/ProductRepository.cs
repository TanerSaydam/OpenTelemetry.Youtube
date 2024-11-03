using System.Diagnostics;

internal sealed class ProductRepository(
    ApplicationDbContext context)
{
    public async Task CreateAsync(Product product)
    {
        Activity.Current!.AddEvent(new ActivityEvent("Create Repository"));

        context.Add(product);
        await context.SaveChangesAsync();
    }
}

