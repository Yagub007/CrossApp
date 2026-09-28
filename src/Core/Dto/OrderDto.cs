namespace Core.Dto;

/// <summary>
/// Формат даних для замовлення (тиждень 3-стиль): переносить дані, НЕ захищає правила.
/// Зв'язок із сутністю Order — через методи Order.ToDto()/FromDto().
/// </summary>
public record OrderDto(
    string Id,
    string CustomerId,
    string Status,
    IReadOnlyList<OrderLineDto> Lines);

/// <summary>Рядок замовлення як дані.</summary>
public record OrderLineDto(
    string ProductId,
    string Name,
    decimal Price,
    int Quantity);
