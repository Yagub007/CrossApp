using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Додаткове завдання 1: декоратор. Приймає інший IOrderStore у конструкторі —
/// той самий контракт, нова поведінка: кешує результат List() доти, доки немає змін.
/// Будь-яка мутація (Add/Update/Remove) скидає кеш.
/// </summary>
public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
    private readonly IOrderStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private IReadOnlyList<Order>? _cache;

    public int CacheHits { get; private set; }   // для демонстрації, що кеш працює

    public IReadOnlyList<Order> List()
    {
        if (_cache is not null)
        {
            CacheHits++;
            return _cache;
        }
        _cache = _inner.List();
        return _cache;
    }

    public Order? GetById(string id) => _inner.GetById(id);

    public void Add(Order item)
    {
        _inner.Add(item);
        _cache = null;   // дані змінилися — кеш недійсний
    }

    public void Update(Order item)
    {
        _inner.Update(item);
        _cache = null;
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed) _cache = null;
        return removed;
    }
}
