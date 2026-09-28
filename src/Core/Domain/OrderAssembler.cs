using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Додаткове завдання 1: зв'язок із тижнем 3. Бере ImportResult з імпортером
/// і будує доменні рядки замовлення ПЛЮС перелік рядків, що не пройшли доменні
/// інваріанти — та сама ідея «дані + помилки», але вже на рівні домену.
/// </summary>
public static class OrderAssembler
{
    public static (IReadOnlyList<OrderLine> Lines, IReadOnlyList<string> Errors) BuildLines(
        ImportResult<ProductDto> import)
    {
        var lines = new List<OrderLine>();
        var errors = new List<string>(import.Errors);   // помилки імпорту переносимо далі

        foreach (ProductDto dto in import.Items)
        {
            try
            {
                // Кожен імпортований товар -> рядок замовлення з кількістю 1.
                lines.Add(OrderLine.Create(dto.Id, dto.Name, dto.Price, 1));
            }
            catch (Exception ex)
            {
                // Доменний інваріант не пройдено — рядок у помилки, решта триває.
                errors.Add($"{dto.Id}: {ex.Message}");
            }
        }

        return (lines, errors);
    }
}
