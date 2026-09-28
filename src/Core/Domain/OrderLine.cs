using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Рядок замовлення як сутність із власними інваріантами (на відміну від OrderLineDto — це дані).
/// Стан незмінний, створення лише через фабрику Create (усі перевірки там).
/// </summary>
public sealed class OrderLine
{
    public string ProductId { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    // Приватний конструктор: створити рядок в обхід перевірок неможливо.
    private OrderLine(string productId, string name, decimal price, int quantity)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    // Фабричний метод: усі інваріанти перевіряються ДО створення.
    public static OrderLine Create(string productId, string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару обов'язкова", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price,
                "Ціна не може бути від'ємною");
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Кількість у рядку має бути більшою за нуль");

        return new OrderLine(productId.Trim(), name.Trim(), price, quantity);
    }

    public decimal LineTotal => Price * Quantity;

    // Мапінг сутність ↔ DTO. FromDto проходить ТІ САМІ перевірки, що й Create.
    public OrderLineDto ToDto() => new(ProductId, Name, Price, Quantity);
    public static OrderLine FromDto(OrderLineDto dto) =>
        Create(dto.ProductId, dto.Name, dto.Price, dto.Quantity);

    public override string ToString() => $"{Quantity} x {Name} @ {Price} = {LineTotal}";
}
