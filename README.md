# CrossApp

Наскрізний проєкт з крос-платформного програмування (усі 16 лабораторних — етапи одного репозиторію).

## Предметна область

**Замовлення.** Сутності: `Customer` (клієнт), `Product` (товар), `Order` (замовлення), `OrderLine` (рядок замовлення).

**Призначення:** оформлення замовлень і підрахунок сум.

**Чому саме вона:** домен близький до реальних e-commerce задач, має чіткі зв'язки (один клієнт — багато замовлень, одне замовлення — багато рядків, рядок посилається на товар) і природно розширюється підрахунком сум, знижок і звітів на наступних тижнях.

## Структура solution

```
CrossApp/
  CrossApp.sln
  README.md
  .gitignore
  src/
    Core/                     # class library (без точки входу) — спільна логіка
      Core.csproj             # multi-targeting: net8.0;net10.0
      EnvironmentInfo.cs      # namespace Core: EnvironmentReport (record) + EnvironmentInfo (static)
    Cli/                      # консольний застосунок (точка входу)
      Cli.csproj              # ProjectReference на Core
      Program.cs              # лише збирає звіт із Core і форматує вивід
```

Напрямок залежності односторонній: **Cli → Core**. Core ні на що не посилається, тому його згодом можна буде підключити і до майбутнього проєкту `Api` (тиждень 10) та `Blazor`-клієнта (тиждень 12).

### Домовленість про каталоги в Core (на весь семестр)

- `Core/Dto/` — record-типи формату даних (тиждень 3): `ProductDto` / `BookDto` / `OrderDto`
- `Core/Domain/` — сутності з поведінкою та інваріантами (тиждень 4)
- `Core/Storage/` — реалізації сховищ (тиждень 5)

Порожній каталог git не зберігає, тому кожен із них створюється разом із його першим типом (або з файлом-заглушкою `.gitkeep`).

## Збірка та запуск

```bash
dotnet build                          # збирає всю solution (Core у двох TFM + Cli)
dotnet build src/Core/Core.csproj     # зібрати лише бібліотеку
dotnet run --project src/Cli          # запустити CLI
dotnet run --project src/Cli -- --json  # той самий звіт одним JSON-рядком
```

`Core` — це бібліотека (classlib), у неї немає `Main`, тому `dotnet run --project src/Core` не працює — це нормально.

## Публікація

```bash
# self-contained: код + залежності + копія .NET runtime (працює без встановленого .NET)
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true

# framework-dependent: лише код + залежності (потрібен встановлений .NET runtime)
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained false
```

Запуск бінарника напряму з каталогу `publish` (без `dotnet run`):

```bash
./src/Cli/bin/Release/net10.0/osx-arm64/publish/Cli
```

Розмір каталогу publish: `du -sh <шлях publish>` (Linux/macOS) або
`(Get-ChildItem -Recurse <шлях> | Measure-Object Length -Sum).Sum/1MB` (Windows PowerShell).

### self-contained vs framework-dependent

**Self-contained** несе у собі копію .NET runtime, тому запускається на машині без встановленого .NET, але важить десятки МБ і прив'язаний до конкретного RID. **Framework-dependent** містить лише ваш код і залежності (сотні КБ), але вимагає, щоб на цільовій машині вже був встановлений сумісний .NET runtime.

### Таблиця: RID — режим — розмір — потрібен runtime

| RID        | Режим                  | Розмір publish | Файлів | Потрібен runtime |
|------------|------------------------|----------------|--------|------------------|
| osx-arm64  | self-contained         | ~83 МБ         | 193    | ні               |
| osx-arm64  | framework-dependent    | ~172 КБ        | 7      | так (.NET 10)    |
| win-x64    | self-contained         | ~77 МБ         | 194    | ні               |
| osx-arm64  | self-contained + SingleFile | ~76 МБ    | 3      | ні               |

> Числа зібрані на цій машині (macOS, Apple Silicon). Каталог `win-x64` не запускається на macOS — це крос-публікація для іншої ОС.

## Середовище

.NET SDK 10.0 (встановлений у `~/.dotnet`; у системі також є 8.0/7.0/6.0), macOS (osx-arm64). Крос-платформно: Windows x64 / Ubuntu x64.

> **Про TFM.** Проєкт таргетовано на `net10.0` (як у методичці). SDK встановлено локально в `~/.dotnet` (`dotnet-install.sh --channel 10.0`); щоб ним користуватися: `export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"`. Multi-targeting бібліотеки зроблено на `net8.0;net10.0`.

## Multi-targeting

`Core.csproj` містить `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>`, тому бібліотека компілюється окремо для кожного TFM — у `bin` з'являються підкаталоги `net8.0/` і `net10.0/`. У `EnvironmentInfo` є рядок, що відрізняється між TFM через `#if NET10_0_OR_GREATER` (поле `BuildNote`).

## Додаткові завдання

### `--json`

```bash
dotnet run --project src/Cli -- --json
```

Виводить той самий `EnvironmentReport` одним JSON-рядком (`System.Text.Json`).

### PublishSingleFile

```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true
```

Об'єднує компоненти в один виконуваний файл: у каталозі publish замість ~193 файлів лишається 3 (сам бінарник + `.pdb` + native-бібліотека, яку не можна вбудувати). Розмір приблизно той самий (~76 МБ), бо runtime нікуди не зникає.

### PublishTrimmed

`-p:PublishTrimmed=true` видаляє невикористаний код і зменшує розмір, але **небезпечний для коду з рефлексією**: тример статично аналізує виклики й може викинути типи/члени, до яких звертаються лише через `Reflection` у рантаймі, — тоді застосунок падає вже під час виконання, а не збірки.

### Запуск у Linux-контейнері (порівняння OSDescription)

```bash
docker run --rm -v "${PWD}":/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet run --project src/Cli
```

Той самий код, різні ОС: локально `Darwin ... (macOS)`, у контейнері `Debian GNU/Linux 12 (bookworm)`. Архітектура в обох `Arm64` — контейнер виконується на тому самому процесорі Apple Silicon.
