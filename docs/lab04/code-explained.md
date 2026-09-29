# Пояснення коду — Лабораторна 4 (під Definition of Done)

Тут по кожному пункту DoD — який код показати і що сказати своїми словами.

---

## DoD 1–2. Сутність в Core/Domain + інкапсуляція
### Файл: `src/Core/Domain/Order.cs` (початок класу)
```csharp
public sealed class Order
{
    private readonly List<OrderLine> _lines = [];        // (1)
    public string Id { get; }                            // (2)
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }      // (3)
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();  // (4)
    public decimal Total => _lines.Sum(l => l.LineTotal);         // (5)
```
**Що сказати:**
- **(1)** `_lines` — **приватне поле**: список рядків замовлення. Ззовні його не видно, тому ніхто не додасть/видалить рядок в обхід правил. `readonly` — саме поле не можна перепризначити іншим списком. `[]` — порожній список.
- **(2)** `Id`, `CustomerId` — `{ get; }` без `set`: **лише читання**. Задаються один раз у конструкторі, після цього незмінні (це ідентичність замовлення).
- **(3)** `Status` — `{ get; private set; }`: читати можна звідусіль, а **записати лише всередині класу**. Тому `order.Status = ...` ззовні не компілюється.
- **(4)** `Lines` віддає колекцію **тільки для читання** (`AsReadOnly`) — без `Add`/`Clear`.
- **(5)** `Total` — сума по рядках, рахується «на льоту» (властивість-вираз через `=>`).

> Головна думка: **стан прихований, ззовні його не можна зіпсувати** — тільки через методи класу.

---

## DoD 3. Фабрика, без публічного конструктора
### Файл: `src/Core/Domain/Order.cs`
```csharp
private Order(string id, string customerId)          // приватний конструктор
{
    Id = id; CustomerId = customerId; Status = OrderStatus.Draft;
}

public static Order Create(string id, string customerId)   // фабрика
{
    if (string.IsNullOrWhiteSpace(id))
        throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));
    if (string.IsNullOrWhiteSpace(customerId))
        throw new ArgumentException("Клієнт обов'язковий", nameof(customerId));
    return new Order(id.Trim(), customerId.Trim());
}
```
**Що сказати:**
- Конструктор **приватний** — `new Order(...)` з іншого класу не спрацює. Він лише записує вже перевірені значення.
- Створити об'єкт можна **лише через `Create`** — це фабричний метод. Він **спершу перевіряє всі аргументи**, і аж потім викликає конструктор.
- `nameof(id)` — це ім'я параметра рядком (`"id"`); якщо перейменувати параметр, воно оновиться саме.
- Результат завжди один із двох: **коректний об'єкт або виняток**. Напівстворених об'єктів не буває.

---

## DoD 4–5. Інваріанти + правильні типи винятків
### Файл: `src/Core/Domain/OrderLine.cs` (фабрика)
```csharp
public static OrderLine Create(string productId, string name, decimal price, int quantity)
{
    if (string.IsNullOrWhiteSpace(productId))
        throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Назва товару обов'язкова", nameof(name));
    if (price < 0)
        throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною");
    if (quantity <= 0)
        throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість ... більшою за нуль");
    return new OrderLine(productId.Trim(), name.Trim(), price, quantity);
}
```
### Файл: `src/Core/Domain/Order.cs` (метод зміни стану)
```csharp
public void AddLine(string productId, string name, decimal price, int quantity)
{
    if (Status != OrderStatus.Draft)
        throw new InvalidOperationException($"Замовлення {Id} у статусі {Status}: рядки додавати не можна");
    _lines.Add(OrderLine.Create(productId, name, price, quantity));   // перевірки рядка — всередині
}

public void Confirm()
{
    RequireTransition(OrderStatus.Confirmed);
    if (_lines.Count == 0)
        throw new InvalidOperationException($"Замовлення {Id} порожнє: підтвердити не можна");
    Status = OrderStatus.Confirmed;      // зміна стану — в ОДНОМУ місці, після всіх перевірок
}
```
**Що сказати — головне про типи винятків:**
- **`ArgumentException` / `ArgumentOutOfRangeException`** — коли **вхідний аргумент поганий сам по собі**: порожня назва, від'ємна ціна, кількість ≤ 0.
- **`InvalidOperationException`** — коли аргумент **нормальний, але стан не дозволяє** операцію: додати рядок у вже підтверджене, підтвердити порожнє.
- Повідомлення **конкретні** (з Id і числами), щоб помилку було зрозуміло без коду.
- **Перевірка завжди ДО зміни** поля — якщо виняток злетів, стан лишається старим.

---

## DoD 6. ToDto / FromDto
### Файл: `src/Core/Domain/Order.cs`
```csharp
public OrderDto ToDto() =>
    new(Id, CustomerId, Status.ToString(), _lines.Select(l => l.ToDto()).ToList());

public static Order FromDto(OrderDto dto)
{
    Order order = Create(dto.Id, dto.CustomerId);            // ті самі перевірки
    foreach (OrderLineDto line in dto.Lines)
        order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);  // ті самі перевірки
    switch (Enum.Parse<OrderStatus>(dto.Status))
    {
        case OrderStatus.Confirmed: order.Confirm(); break;
        case OrderStatus.Cancelled: order.Cancel(); break;
    }
    return order;
}
```
**Що сказати:**
- `ToDto` — перетворює сутність у **дані** (DTO), щоб зберегти.
- `FromDto` — відновлює сутність із даних, але **через ті самі методи** (`Create`, `AddLine`, `Confirm`). Тобто пошкоджений DTO **не створить** некоректну сутність — інваріанти ті самі.
- Цим на 5-му тижні користуватиметься сховище: збереже через `ToDto`, відновить через `FromDto`.

---

## DoD 7. Cli: успіх + відмова, без stack trace
### Файл: `src/Cli/Program.cs`
```csharp
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");  // тип + текст, БЕЗ stack trace
    }
}
```
**Що сказати:**
- `Action` — «метод без параметрів», а `() => order.Issue(...)` — це сама дія, яку передаємо і виконуємо всередині `TryDo` рядком `action()`.
- `try/catch` ловить виняток, щоб програма **не впала**. Виводимо `ex.GetType().Name` (який саме виняток) і `ex.Message` (текст правила) — **stack trace не показуємо**, він для розробника.
- Якщо виняток **не** злетів — значить інваріанту немає (гілка після `action()`).

---

## DoD 8. Колекції назовні — тільки для читання
```csharp
private readonly List<OrderLine> _lines = [];
public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
```
**Що сказати:** «Список зберігаю в приватному полі, а назовні віддаю `IReadOnlyList` через `AsReadOnly()` — зовні немає `Add`/`Clear`, і тип не можна привести назад до `List`. Рядки додаються лише методом `AddLine`, де є перевірки.»

---

## Додаткові завдання
### Дод. 3 — переходи статусів (`Order.cs`)
```csharp
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
        throw new InvalidOperationException($"Неможливий перехід {Status} -> {to} ...");
}
```
**Сказати:** «Дозволені переходи описані одним `switch` по парі `(звідки, куди)`. Усе, чого немає в списку (напр. `Cancelled -> Confirmed`), — заборонено.»

### Дод. 2 — правило між двома сутностями (`OrderPlacementService.cs`)
```csharp
public Order PlaceOrder(Customer customer, IReadOnlyCollection<Order> existingOrders, string newOrderId)
{
    int openForCustomer = existingOrders.Count(
        o => o.CustomerId == customer.Id && o.Status == OrderStatus.Draft);
    if (openForCustomer >= MaxOpenOrders)
        throw new InvalidOperationException($"Клієнт {customer.Id} вже має {openForCustomer} відкритих ...");
    return Order.Create(newOrderId, customer.Id);
}
```
**Сказати:** «Це правило охоплює **дві сутності** — клієнта і всі його замовлення. Одне `Order` не знає про інші, тому правило не можна покласти в сутність — воно живе у **сервісі**, який бачить і клієнта, і колекцію його замовлень. Після 3 відкритих замовлень 4-те кидає `InvalidOperationException`. З тижня 5 такі сервіси працюватимуть зі сховищем.»

### Дод. 1 — імпорт → сутності (`OrderAssembler.cs`)
```csharp
foreach (ProductDto dto in import.Items)
{
    try { lines.Add(OrderLine.Create(dto.Id, dto.Name, dto.Price, 1)); }
    catch (Exception ex) { errors.Add($"{dto.Id}: {ex.Message}"); }   // не пройшов інваріант
}
```
**Сказати:** «Беру результат імпорту тижня 3 і будую доменні рядки. Ті, що не проходять інваріанти, йдуть у список помилок — та сама ідея "дані + помилки", але вже на рівні домену.»

---

## Одне речення на весь захист
> «Дані — це DTO (без правил). Сутність (`Order`/`OrderLine`) ховає стан у приватних полях і дозволяє змінювати його лише методами, які перевіряють інваріанти. Створення — через фабрику `Create`, а не конструктор. Тип винятку залежить від причини: `Argument*` — поганий вхід, `InvalidOperation*` — стан не дозволяє. Cli лише викликає методи й ловить винятки.»
