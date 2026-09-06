using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

// Коректний вивід кирилиці у консолі (зокрема на Windows).
Console.OutputEncoding = Encoding.UTF8;

// Дані про середовище виконання.
var info = new
{
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    ClrVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    BaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory
};

// Додаткове завдання 2: прапорець --json.
if (args.Contains("--json"))
{
    var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = false });
    Console.WriteLine(json);
    return;
}

Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
Console.WriteLine("Студент: Алієв Ягуб, група ФЕІ-32с");
Console.WriteLine(new string('-', 52));

Console.WriteLine($"ОС (OSDescription)   : {info.OSDescription}");
Console.WriteLine($"ОС (Environment)     : {info.OSVersion}");
Console.WriteLine($"Архітектура процесу  : {info.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR)    : {info.ClrVersion}");
Console.WriteLine($"Runtime              : {info.Runtime}");
Console.WriteLine($"Каталог застосунку   : {info.BaseDirectory}");
Console.WriteLine($"Поточний каталог     : {info.CurrentDirectory}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Замовлення (Customer, Product, Order, OrderLine)");
