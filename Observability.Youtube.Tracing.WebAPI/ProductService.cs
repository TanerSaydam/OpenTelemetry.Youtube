using System.Diagnostics;
using static Product;

internal sealed class ProductService(
        ProductRepository productRepository)
{
    public async Task CreateAsync(CreateDto request)
    {
        Activity.Current!.AddEvent(new ActivityEvent("Create Service"));

        Product product = new()
        {
            Name = request.Name,
            Price = request.Price
        };

        await productRepository.CreateAsync(product);
    }
}

