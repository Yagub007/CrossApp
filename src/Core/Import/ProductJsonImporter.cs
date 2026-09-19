using System.Text.Json;
using Core.Dto;

namespace Core.Import;

/// <summary>
/// Додаткове завдання 1: другий імпортер на тих самих типах — JSON (System.Text.Json).
/// </summary>
public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            string json = File.ReadAllText(path);
            // ?? [] : якщо десеріалізація повернула null, працюємо з порожнім списком.
            var items = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];
            return new ImportResult<ProductDto>(items, []);
        }
        catch (JsonException ex)
        {
            // Пошкоджений JSON не має валити програму.
            return new ImportResult<ProductDto>([], [$"некоректний JSON: {ex.Message}"]);
        }
    }
}
