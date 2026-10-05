using System.Globalization;
using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// ===== Composition Root — ЄДИНЕ місце з конкретними класами сховищ =====
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
IOrderStore store = useFile
    ? new FileOrderStore(dataPath)
    : new InMemoryOrderStore(SampleData.Orders());
var service = new OrderService(store);          // ін'єкція сховища через конструктор

Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine($"Замовлень у сховищі на старті: {service.All().Count}");

// ===== Сценарій: створити, додати рядки, підтвердити, показати, знайти =====
Order created = service.CreateOrder("C-001");
service.AddLine(created.Id, "P-001", "Кава мелена 250г", 189.50m, 2);
service.AddLine(created.Id, "P-003", "Цукор 1кг", 42.90m, 1);
service.Confirm(created.Id);
Console.WriteLine($"Створено й підтверджено {created.Id}: сума {Money(created.Total)}, статус {created.Status}");

Console.WriteLine("Перші замовлення:");
foreach (Order o in service.All().Take(5))
    Console.WriteLine($"  {o.Id}  клієнт {o.CustomerId,-6} рядків {o.Lines.Count}  сума {Money(o.Total),9}  {o.Status}");

Order? found = service.Find(created.Id);
Console.WriteLine($"Find({created.Id}): {(found is null ? "не знайдено" : found.ToString())}");

// ===== Сценарії відмови =====
Console.WriteLine();
Console.WriteLine("=== Сценарії відмови ===");
TryDo("Confirm неіснуючого id", () => service.Confirm("NEMA-404"));
TryDo("дубль id у сховищі", () => store.Add(created));   // created уже є

// ===== Додаткове 2: пошук із предикатом =====
Console.WriteLine();
Console.WriteLine("=== Додаткове 2: пошук із предикатом ===");
IReadOnlyList<Order> confirmed = service.Search(o => o.Status == OrderStatus.Confirmed);
Console.WriteLine($"Підтверджених замовлень: {confirmed.Count}");

// ===== Додаткове 1: декоратор-кеш (той самий контракт) =====
Console.WriteLine();
Console.WriteLine("=== Додаткове 1: CachingOrderStore (декоратор) ===");
var caching = new CachingOrderStore(store);
caching.List(); caching.List(); caching.List();        // 1 реальний виклик, далі з кешу
Console.WriteLine($"Кеш-звернень (без походу в сховище): {caching.CacheHits}");

// ===== Додаткове 3: та сама композиція через фабрику =====
Console.WriteLine();
Console.WriteLine("=== Додаткове 3: StoreFactory ===");
IOrderStore viaFactory = StoreFactory.Create(args, dataPath);
Console.WriteLine($"Фабрика повернула: {viaFactory.GetType().Name}");

return 0;

static string Money(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

// Один обробник винятків: друкує тип і Message (без stack trace).
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}
