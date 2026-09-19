using System.Globalization;
using System.Text;
using Core.Dto;
using Core.Import;

// Коректний вивід кирилиці у консолі (зокрема на Windows).
Console.OutputEncoding = Encoding.UTF8;

// Шлях до файлу з args[0], інакше data/sample.csv. Path.Combine — крос-платформно.
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    // Зрозуміле повідомлення замість необробленого винятку; показуємо, ДЕ шукали.
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

Console.WriteLine($"Джерело: {path}");
Console.WriteLine(new string('-', 52));

// Мішаний каталог (додаткове завдання 2) — окремий формат із двома типами.
if (Path.GetFileNameWithoutExtension(path).Contains("mixed", StringComparison.OrdinalIgnoreCase))
{
    MixedCatalog catalog = MixedCatalogImporter.Load(path);
    Console.WriteLine($"Товарів: {catalog.Products.Count}, клієнтів: {catalog.Customers.Count}");
    foreach (ProductDto p in catalog.Products.Take(5))
        Console.WriteLine($"  товар    {p.Id,-7} {p.Name,-26} {p.Price.ToString("F2", CultureInfo.InvariantCulture),10}");
    foreach (CustomerDto c in catalog.Customers.Take(5))
        Console.WriteLine($"  клієнт   {c.Id,-7} {c.Name,-26} {c.Email ?? "-"}");
    PrintErrors(catalog.Errors);
    return 0;
}

// Вибір імпортера за розширенням файлу — switch expression (додаткове завдання 1).
string ext = Path.GetExtension(path).ToLowerInvariant();
ImportResult<ProductDto> result = ext switch
{
    ".json" => ProductJsonImporter.Load(path),
    _ => ProductCsvImporter.Load(path)      // .csv і будь-що інше — як CSV
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
    Console.WriteLine($"  {p.Id,-7} {p.Name,-26} {p.Price.ToString("F2", CultureInfo.InvariantCulture),10}  {p.Category ?? "-"}");

PrintErrors(result.Errors);

// Статистика імпорту одним рядком (додаткове завдання 3) — заготовка під звіти тижня 7.
int total = result.Items.Count + result.Errors.Count;
double badPct = total == 0 ? 0 : 100.0 * result.Errors.Count / total;
Console.WriteLine(new string('-', 52));
Console.WriteLine($"Статистика: усього {total}, прийнято {result.Items.Count}, " +
                  $"пропущено {result.Errors.Count} ({badPct:F0}% помилок)");

return 0;

// Локальна функція: перелік пропущених рядків із номерами.
static void PrintErrors(IReadOnlyList<string> errors)
{
    if (errors.Count == 0)
        return;
    Console.WriteLine($"Пропущено рядків: {errors.Count}");
    foreach (string e in errors)
        Console.WriteLine($"  ! {e}");
}
