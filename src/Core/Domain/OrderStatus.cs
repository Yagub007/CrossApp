namespace Core.Domain;

/// <summary>
/// Явний стан замовлення (додаткове завдання 3). Допустимі переходи
/// перевіряються в Order через switch expression.
/// </summary>
public enum OrderStatus
{
    Draft,       // чернетка: можна додавати рядки
    Confirmed,   // підтверджене: змінювати не можна
    Cancelled    // скасоване: кінцевий стан
}
