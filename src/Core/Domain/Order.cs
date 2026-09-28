using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Замовлення — агрегат домену «Замовлення». Бізнес-правила живуть тут, а не в Cli/Api:
/// клієнтські проєкти лише викликають методи й обробляють винятки.
/// </summary>
public sealed class Order
{
    // Приватна колекція: назовні віддається лише для читання, щоб її не змінили в обхід правил.
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }   // записується лише всередині класу

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(l => l.LineTotal);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    // Фабрика: перевіряє вхідні аргументи ДО створення.
    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Клієнт обов'язковий", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim());
    }

    // Додати рядок можна лише до чернетки; перевірки рядка — усередині OrderLine.Create.
    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Замовлення {Id} у статусі {Status}: рядки додавати не можна");

        _lines.Add(OrderLine.Create(productId, name, price, quantity));
    }

    // Підтвердити можна лише непорожню чернетку.
    public void Confirm()
    {
        RequireTransition(OrderStatus.Confirmed);
        if (_lines.Count == 0)
            throw new InvalidOperationException(
                $"Замовлення {Id} порожнє: підтвердити не можна");
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        RequireTransition(OrderStatus.Cancelled);
        Status = OrderStatus.Cancelled;
    }

    // Додаткове завдання 3: допустимі переходи станів через switch expression.
    private void RequireTransition(OrderStatus to)
    {
        bool allowed = (Status, to) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            _ => false
        };
        if (!allowed)
            throw new InvalidOperationException(
                $"Неможливий перехід {Status} -> {to} для замовлення {Id}");
    }

    // Мапінг сутність ↔ DTO. FromDto відновлює стан через ті самі методи (ті самі інваріанти).
    public OrderDto ToDto() =>
        new(Id, CustomerId, Status.ToString(), _lines.Select(l => l.ToDto()).ToList());

    public static Order FromDto(OrderDto dto)
    {
        Order order = Create(dto.Id, dto.CustomerId);
        foreach (OrderLineDto line in dto.Lines)
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);

        switch (Enum.Parse<OrderStatus>(dto.Status))
        {
            case OrderStatus.Confirmed: order.Confirm(); break;
            case OrderStatus.Cancelled: order.Cancel(); break;
        }
        return order;
    }

    public override string ToString() =>
        $"Замовлення {Id} (клієнт {CustomerId}), рядків: {_lines.Count}, сума: {Total}, статус: {Status}";
}
