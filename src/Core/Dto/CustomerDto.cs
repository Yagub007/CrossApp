namespace Core.Dto;

/// <summary>
/// Клієнт домену «Замовлення». Використовується мішаним імпортером (додаткове завдання 2).
/// </summary>
public record CustomerDto(
    string Id,
    string Name,
    string? Email = null);  // пошта необов'язкова
