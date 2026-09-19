using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

/// <summary>
/// Додаткове завдання 2: різнорідні рядки за префіксом типу в одному файлі —
/// "P;..." — товар, "C;..." — клієнт. Один switch, два різні типи результату.
/// </summary>
public sealed record MixedCatalog(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<CustomerDto> Customers,
    IReadOnlyList<string> Errors);

public static class MixedCatalogImporter
{
    private const char Separator = ';';

    public static MixedCatalog Load(string path)
    {
        var products = new List<ProductDto>();
        var customers = new List<CustomerDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            string[] p = line.Split(Separator, StringSplitOptions.TrimEntries);

            switch (p)
            {
                case ["P", var id, var name, var price, var cat]
                        when name.Length > 0 && TryPrice(price, out _):
                    products.Add(new ProductDto(id, name, ParsePrice(price), cat is "" ? null : cat));
                    break;

                case ["P", ..]:
                    errors.Add($"рядок {number}: пошкоджений товар");
                    break;

                case ["C", var id, var name, var email] when name.Length > 0:
                    customers.Add(new CustomerDto(id, name, email is "" ? null : email));
                    break;

                case ["C", ..]:
                    errors.Add($"рядок {number}: пошкоджений клієнт");
                    break;

                case [var type, ..]:
                    errors.Add($"рядок {number}: невідомий тип '{type}'");
                    break;

                default:
                    errors.Add($"рядок {number}: порожній рядок");
                    break;
            }
        }

        return new MixedCatalog(products, customers, errors);
    }

    private static bool TryPrice(string s, out decimal value) =>
        decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value) && value >= 0;

    private static decimal ParsePrice(string s) =>
        decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);
}
