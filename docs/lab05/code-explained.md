# Пояснення коду — Лабораторна 5 (під Definition of Done)

По кожному пункту DoD — який код показати і що сказати своїми словами.

---

## DoD 1. Інтерфейс сховища
### Файл: `src/Core/Abstractions/IOrderStore.cs`
```csharp
public interface IOrderStore
{
    IReadOnlyList<Order> List();
    Order? GetById(string id);
    void Add(Order item);
    void Update(Order item);
    bool Remove(string id);
}
```
**Що сказати:**
- **Інтерфейс** — це контракт: лише імена операцій і типи, без тіл. Хто його реалізує, **зобов'язаний** надати всі методи, інакше код не збереться.
- `IReadOnlyList` — щоб ззовні не можна було додати/видалити в обхід `Add`/`Remove`.
- `Order?` — результату може не бути (запису з таким id немає).
- Методів рівно 5 — стільки, скільки треба сервісу. «Про запас» не додаємо: кожен метод довелося б писати в **кожній** реалізації.

---

## DoD 2. Дві реалізації
### `InMemoryOrderStore.cs` (ключове)
```csharp
public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    private readonly Dictionary<string, Order> _items =
        (seed ?? []).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<Order> List() => _items.Values.ToList();
    public Order? GetById(string id) => _items.GetValueOrDefault(id);
    ...
}
```
**Сказати:** «Зберігаю у словнику. `seed ?? []` — якщо початкових даних немає, порожня колекція. Дані живуть у пам'яті — після виходу зникають.»

### `FileOrderStore.cs` (ключове)
```csharp
private void EnsureLoaded()   // дозавантаження з файлу при першому зверненні
{
    if (_loaded) return;
    if (File.Exists(_path)) { /* десеріалізувати DTO -> Order.FromDto */ }
    _loaded = true;
}
private void Flush()          // запис на диск після кожної зміни
{
    Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    File.WriteAllText(_path, JsonSerializer.Serialize(
        _cache.Values.Select(o => o.ToDto()).ToList(), Options));
}
```
**Сказати:** «Той самий контракт, інша механіка: кеш у пам'яті, читаю файл один раз (`EnsureLoaded`), пишу після кожної зміни (`Flush`). На диск ідуть **DTO**, не сутності — через `ToDto()/FromDto()`.»

---

## DoD 3. Сервіс залежить лише від інтерфейсу
### Файл: `src/Core/Services/OrderService.cs`
```csharp
public sealed class OrderService(IOrderStore store)      // ТИП — інтерфейс, не клас
{
    private readonly IOrderStore _store =
        store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(string customerId)
    {
        Order order = Order.Create(Guid.NewGuid().ToString("N")[..8], customerId);
        _store.Add(order);
        return order;
    }
    public void Confirm(string orderId)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id={orderId}.");
        order.Confirm();
        _store.Update(order);
    }
    ...
}
```
**Сказати:**
- Конструктор приймає **`IOrderStore`** — тому сервіс не знає, пам'ять це чи файл. Це **Dependency Injection** (залежність передано ззовні).
- `?? throw` у першому рядку — захист від `null`-залежності.
- `GetById(...) ?? throw` — «запис не знайдено» перетворюємо на зрозумілу відмову, а не тихий `null`.
- Слів `new FileOrderStore` чи `File` тут немає — у цьому й суть.

---

## DoD 4. Composition root — конкретні класи лише тут
### Файл: `src/Cli/Program.cs`
```csharp
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
IOrderStore store = useFile
    ? new FileOrderStore(dataPath)
    : new InMemoryOrderStore(SampleData.Orders());
var service = new OrderService(store);        // ручний DI — звичайний new
```
**Сказати:** «Це composition root — **єдине** місце, де є конкретні `new ...Store`. Тернарним оператором обираю реалізацію за `--file`. Контейнер DI не потрібен — зв'язую руками.»

---

## DoD 5. Сценарій (додати / змінити / список / знайти)
```csharp
Order created = service.CreateOrder("C-001");
service.AddLine(created.Id, "P-001", "Кава мелена 250г", 189.50m, 2);
service.Confirm(created.Id);
foreach (Order o in service.All().Take(5)) { ... }   // список
Order? found = service.Find(created.Id);              // знайти за id
```
**Сказати:** «Створюю замовлення, додаю рядок, підтверджую, показую список, шукаю за id — усе через сервіс, який працює з абстракцією.»

---

## DoD 6. Помилка з людським повідомленням
```csharp
TryDo("Confirm неіснуючого id", () => service.Confirm("NEMA-404"));
TryDo("дубль id у сховищі", () => store.Add(created));
// catch -> Console.WriteLine($"{ex.GetType().Name} — {ex.Message}");
```
**Сказати:** «Виняток ловиться в Cli (`try/catch`), друкую тип і текст — `Немає замовлення з id=...` / `Замовлення з id=... уже існує.`, без stack trace.»

---

## DoD 7. Без DI-контейнера
**Сказати:** «`Microsoft.Extensions.DependencyInjection` не підключено — ін'єкція ручна, звичайним `new` у composition root. Контейнер з'явиться на тижні 10 з Api, тоді й порівняємо.»

---

## Додаткові завдання
### Дод. 1 — декоратор `CachingOrderStore.cs`
```csharp
public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
    private IReadOnlyList<Order>? _cache;
    public IReadOnlyList<Order> List()
    {
        if (_cache is not null) { CacheHits++; return _cache; }
        _cache = _inner.List();
        return _cache;
    }
    public void Add(Order item) { _inner.Add(item); _cache = null; }  // мутація скидає кеш
    ...
}
```
**Сказати:** «Декоратор: приймає інший `IOrderStore` і сам ним є — той самий контракт, нова поведінка. Кешує `List()`, а будь-яка зміна скидає кеш. Працює поверх будь-якої реалізації.»

### Дод. 2 — пошук із предикатом (`OrderService.cs`)
```csharp
public IReadOnlyList<Order> Search(Func<Order, bool> predicate) =>
    _store.List().Where(predicate).ToList();
```
**Сказати:** «`Func<Order,bool>` — це передана ззовні умова. `Search(o => o.Status == Confirmed)` поверне підтверджені. Заготовка під LINQ-звіти тижнів 6–7.»

### Дод. 3 — фабрика (`StoreFactory.cs`)
```csharp
public static IOrderStore Create(string[] args, string dataPath) =>
    args.Contains("--file") ? new FileOrderStore(dataPath)
                            : new InMemoryOrderStore(SampleData.Orders());
```
**Сказати:** «Фабрика інкапсулює вибір реалізації. Порівняно з "усе в Program.cs" — вибір в одному місці, і тести будують сховище так само, як застосунок.»

---

## Одне речення на весь захист
> «Сервіс працює з абстракцією `IOrderStore`, а не з конкретним сховищем. Реалізацій дві — пам'ять і файл; яку підставити, вирішує composition root у `Program.cs` за аргументом `--file`. Тому заміна на БД на тижні 11 — це один рядок у Program.cs, без змін у сервісі й домені.»
