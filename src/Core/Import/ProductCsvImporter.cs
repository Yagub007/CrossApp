using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

/// <summary>
/// Розбір CSV-файлу товарів. Уся логіка розбору живе тут (Core), а не в Program.cs:
/// з тижня 5 її викликатиме сховище, з тижня 8 — тести.
/// Формат: id;name;price;category  (роздільник — «;», категорія необов'язкова).
/// </summary>
public static class ProductCsvImporter
{
    // Роздільник — крапка з комою: не конфліктує з комою в дробових цінах на деяких локалях.
    private const char Separator = ';';

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        // Явне UTF-8: кирилиця читається однаково на будь-якій ОС.
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;              // номери рядків для повідомлень про помилки
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;                    // порожні рядки та коментарі
            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;                    // рядок заголовків

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    // Розбір одного рядка через switch expression з різними патернами.
    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // патерн властивості + реляційний патерн: замало колонок
            { Length: < 4 } => new ParseFailed($"очікую 4 колонки, отримав {parts.Length}"),

            // патерн списку + константний патерн: порожня назва
            [_, "", _, _] => new ParseFailed("назва порожня"),

            // патерн списку + охоронна умова when: некоректна ціна
            [_, _, var price, _] when !TryPrice(price, out _)
                => new ParseFailed($"ціна '{price}' не є невід'ємним числом"),

            // патерн списку зі зв'язуванням: усе гаразд (порожня категорія -> null)
            [var id, var name, var price, var cat]
                => new ParseOk(new ProductDto(id, name, ParsePrice(price), cat is "" ? null : cat)),

            // гілка «усе інше»: забагато колонок
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    // Ціна парситься лише з InvariantCulture, інакше "12.50" на локалі з комою зламається.
    private static bool TryPrice(string s, out decimal value) =>
        decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value) && value >= 0;

    private static decimal ParsePrice(string s) =>
        decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);

    // Ієрархія record для результату розбору: або готовий запис, або причина помилки.
    private abstract record ParseOutcome;
    private sealed record ParseOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
