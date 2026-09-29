namespace Core.Domain;

/// <summary>
/// Клієнт домену «Замовлення». Проста сутність із власним інваріантом (ключ/ім'я не порожні).
/// Використовується у правилі, що охоплює дві сутності (додаткове завдання 2).
/// </summary>
public sealed class Customer
{
    public string Id { get; }
    public string Name { get; }

    private Customer(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Customer Create(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор клієнта обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ім'я клієнта обов'язкове", nameof(name));

        return new Customer(id.Trim(), name.Trim());
    }

    public override string ToString() => $"Клієнт {Id} ({Name})";
}
