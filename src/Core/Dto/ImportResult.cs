namespace Core.Dto;

/// <summary>
/// Результат імпорту: успішно прочитані дані РАЗОМ із переліком помилок.
/// Один пошкоджений рядок не перериває весь імпорт.
/// IReadOnlyList — той, хто отримав результат, не може дописати туди свої рядки.
/// </summary>
public sealed record ImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<string> Errors);
