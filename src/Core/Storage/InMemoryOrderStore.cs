using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Реалізація сховища в пам'яті: словник + початкові дані через конструктор.
/// Після виходу з програми дані зникають. Зручна для демо і для тестів (тиждень 8).
/// </summary>
public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    // seed ?? [] — порожня колекція, якщо початкових даних немає; ключ без урахування регістру.
    private readonly Dictionary<string, Order> _items =
        (seed ?? []).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List() => _items.Values.ToList();

    public Order? GetById(string id) => _items.GetValueOrDefault(id);

    public void Update(Order item) => _items[item.Id] = item;

    public bool Remove(string id) => _items.Remove(id);

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        // Унікальність id — умова щодо набору записів, тому її перевіряє сховище, а не сутність.
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");
        _items.Add(item.Id, item);
    }
}
