using Core.Abstractions;

namespace Core.Storage;

/// <summary>
/// Додаткове завдання 3: фабрика сховищ. Вибір реалізації за аргументами — в одному місці.
/// Порівняно з «усе в Program.cs»: композиція лишається в точці входу, але сам вибір
/// інкапсульовано, тому Program.cs не розростається розгалуженнями, а тести можуть
/// будувати сховище тим самим способом, що й застосунок.
/// </summary>
public static class StoreFactory
{
    public static IOrderStore Create(string[] args, string dataPath) =>
        args.Contains("--file")
            ? new FileOrderStore(dataPath)
            : new InMemoryOrderStore(SampleData.Orders());
}
