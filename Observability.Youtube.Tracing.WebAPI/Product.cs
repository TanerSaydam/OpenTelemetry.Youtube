internal sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }

    internal record CreateDto(
        string Name,
        decimal Price);
}