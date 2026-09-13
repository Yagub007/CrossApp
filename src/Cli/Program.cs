using System.Text;
using System.Text.Json;
using Core;

// Коректний вивід кирилиці у консолі (зокрема на Windows).
Console.OutputEncoding = Encoding.UTF8;

// Уся «інформація про середовище» живе в Core. Cli лише збирає звіт і друкує його.
EnvironmentReport report = EnvironmentInfo.Collect();

// Додаткове завдання: прапорець --json (той самий звіт одним JSON-рядком).
if (args.Contains("--json"))
{
    string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = false });
    Console.WriteLine(json);
    return;
}

Console.WriteLine("CrossApp – інформація про середовище");
Console.WriteLine("Студент: Алієв Ягуб, група ФЕІ-32с");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС             : {report.OsDescription}");
Console.WriteLine($"Runtime        : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура    : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено): {report.DetectedRid}");
Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
Console.WriteLine($"Каталог        : {report.BaseDirectory}");
Console.WriteLine($"Збірка (TFM)   : {report.BuildNote}");
