using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

/// <summary>
/// Сервіс бізнес-операцій над замовленнями. Залежить ЛИШЕ від абстракції IOrderStore
/// (ін'єкція через конструктор), тому не знає, чи це пам'ять, чи файл, чи згодом БД.
/// Слів «File» і «new ...Store» тут немає.
/// </summary>
public sealed class OrderService(IOrderStore store)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(string customerId)
    {
        // Короткий id; унікальність усе одно перевірить сховище в Add.
        Order order = Order.Create(Guid.NewGuid().ToString("N")[..8], customerId);
        _store.Add(order);            // інваріанти вже перевірив Order.Create
        return order;
    }

    public void AddLine(string orderId, string productId, string name, decimal price, int quantity)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id={orderId}.");
        order.AddLine(productId, name, price, quantity);   // метод сутності з тижня 4
        _store.Update(order);
    }

    public void Confirm(string orderId)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id={orderId}.");
        order.Confirm();
        _store.Update(order);
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    // Додаткове завдання 2: пошук із предикатом — заготовка під LINQ-звіти тижнів 6–7.
    public IReadOnlyList<Order> Search(Func<Order, bool> predicate) =>
        _store.List().Where(predicate).ToList();
}
