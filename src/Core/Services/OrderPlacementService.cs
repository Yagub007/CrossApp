using Core.Domain;

namespace Core.Services;

/// <summary>
/// Додаткове завдання 2: інваріант, що охоплює ДВІ сутності (Customer + усі його Order).
///
/// Чому це СЕРВІС, а не метод сутності: одне замовлення (Order) бачить лише себе й не знає
/// про інші замовлення того самого клієнта. Правило «клієнт не може мати понад N відкритих
/// замовлень» потребує стану кількох об'єктів (клієнта + колекції його замовлень), тому його
/// природно виносять у сервіс (з тижня 5 такі сервіси працюватимуть разом зі сховищем).
/// Інваріанти про власний стан лишаються в сутностях; крос-сутнісні — тут.
/// </summary>
public sealed class OrderPlacementService
{
    public const int MaxOpenOrders = 3;

    /// <summary>
    /// Створює нове замовлення для клієнта, якщо не порушено крос-сутнісний інваріант.
    /// </summary>
    public Order PlaceOrder(Customer customer, IReadOnlyCollection<Order> existingOrders, string newOrderId)
    {
        int openForCustomer = existingOrders.Count(
            o => o.CustomerId == customer.Id && o.Status == OrderStatus.Draft);

        if (openForCustomer >= MaxOpenOrders)
            throw new InvalidOperationException(
                $"Клієнт {customer.Id} вже має {openForCustomer} відкритих замовлень " +
                $"(ліміт {MaxOpenOrders}) — нове створити не можна");

        return Order.Create(newOrderId, customer.Id);
    }
}
