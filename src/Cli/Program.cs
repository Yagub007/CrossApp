using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;

Console.OutputEncoding = Encoding.UTF8;

// ===== Сценарій 1: успіх — стан змінюється лише через методи =====
Console.WriteLine("=== Сценарій 1: успіх ===");
Order order = Order.Create("O-001", "C-001");
order.AddLine("P-001", "Кава мелена 250г", 189.50m, 2);
order.AddLine("P-003", "Цукор 1кг", 42.90m, 3);
Console.WriteLine(order);
order.Confirm();
Console.WriteLine($"Після підтвердження: статус {order.Status}, сума {order.Total}");

// ===== Сценарій 2: порушення інваріантів — жодна відмова не змінює стан =====
Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
Order draft = Order.Create("O-002", "C-002");
TryDo("порожній клієнт", () => Order.Create("O-003", "   "));
TryDo("кількість 0", () => draft.AddLine("P-001", "Кава", 189.50m, 0));
TryDo("від'ємна ціна", () => draft.AddLine("P-002", "Чай", -5m, 1));
TryDo("підтвердити порожнє", () => draft.Confirm());
TryDo("додати рядок у підтверджене", () => order.AddLine("P-009", "Печиво", 54.80m, 1));
TryDo("повторне підтвердження", () => order.Confirm());
Console.WriteLine($"Стан order не змінився: статус {order.Status}, рядків {order.Lines.Count}, сума {order.Total}");

// ===== Додаткове 3: переходи станів (enum + switch expression) =====
Console.WriteLine();
Console.WriteLine("=== Додаткове 3: переходи статусів ===");
Order cancelled = Order.Create("O-010", "C-010");
cancelled.AddLine("P-001", "Кава мелена 250г", 189.50m, 1);
cancelled.Cancel();
Console.WriteLine($"O-010 статус: {cancelled.Status}");
TryDo("підтвердити скасоване", () => cancelled.Confirm());

// ===== Додаткове 1: побудова доменних рядків з імпорту тижня 3 =====
Console.WriteLine();
Console.WriteLine("=== Додаткове 1: рядки з імпорту (дані + помилки) ===");
string csv = Path.Combine("data", "sample.csv");
if (File.Exists(csv))
{
    ImportResult<ProductDto> import = ProductCsvImporter.Load(csv);
    (IReadOnlyList<OrderLine> lines, IReadOnlyList<string> errors) = OrderAssembler.BuildLines(import);
    Console.WriteLine($"Побудовано рядків: {lines.Count}, відхилено: {errors.Count}");
}

// ===== Додаткове 2: інваріант між двома сутностями (у сервісі) =====
Console.WriteLine();
Console.WriteLine("=== Додаткове 2: правило між двома сутностями (сервіс) ===");
Customer customer = Customer.Create("C-100", "ТОВ Ромашка");
var customerOrders = new List<Order>();
var placement = new OrderPlacementService();
for (int i = 1; i <= OrderPlacementService.MaxOpenOrders; i++)
    customerOrders.Add(placement.PlaceOrder(customer, customerOrders, $"O-2{i:00}"));
Console.WriteLine($"Відкритих замовлень у {customer.Id}: {customerOrders.Count} (ліміт {OrderPlacementService.MaxOpenOrders})");
TryDo("понад ліміт відкритих замовлень", () => placement.PlaceOrder(customer, customerOrders, "O-999"));

// ===== Мапінг ToDto / FromDto (для сховища тижня 5) =====
Console.WriteLine();
Console.WriteLine("=== Мапінг ToDto / FromDto ===");
OrderDto dto = order.ToDto();
Order restored = Order.FromDto(dto);
Console.WriteLine($"Відновлено з DTO: {restored}");

return 0;

// Один обробник винятків для всіх сценаріїв: виводить тип і Message (без stack trace).
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}
