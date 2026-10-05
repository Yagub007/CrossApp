using Core.Domain;

namespace Core;

/// <summary>
/// Зразкові дані для демонстрації та для звітів тижня 7 (15 замовлень із рядками).
/// Детермінований seed, щоб вивід був відтворюваним.
/// </summary>
public static class SampleData
{
    private static readonly (string Id, string Name, decimal Price)[] Catalog =
    [
        ("P-001", "Кава мелена 250г", 189.50m),
        ("P-003", "Цукор 1кг", 42.90m),
        ("P-006", "Молоко 2.5% 1л", 41.50m),
        ("P-008", "Шоколад чорний 90г", 72.25m),
        ("P-010", "Вода мінеральна 1.5л", 22.00m),
    ];

    public static IEnumerable<Order> Orders()
    {
        var rnd = new Random(42);
        var orders = new List<Order>();

        for (int i = 1; i <= 15; i++)
        {
            Order o = Order.Create($"O-{i:000}", $"C-{(i % 4) + 1:000}");
            int lineCount = 1 + rnd.Next(3);
            for (int j = 0; j < lineCount; j++)
            {
                (string id, string name, decimal price) = Catalog[rnd.Next(Catalog.Length)];
                o.AddLine(id, name, price, 1 + rnd.Next(5));
            }
            if (i % 2 == 0) o.Confirm();   // частину замовлень одразу підтверджуємо
            orders.Add(o);
        }

        return orders;
    }
}
