# Шпаргалка до захисту — Лабораторна 2

> Проєкт: **CrossApp** (Core + Cli), .NET 10, macOS `osx-arm64`, домен — **Замовлення**.
> У новому терміналі спочатку:
> ```bash
> export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
> ```

---

## Частина 1. Теорія простими словами (з прикладами)

### Class library (бібліотека) vs застосунок
**Аналогія:** бібліотека `Core` — це **коробка з інструментами**. У неї немає кнопки «Пуск», сама вона нічого не робить. `Cli` — це **пульт із кнопкою**, який бере інструменти з коробки і вмикає їх.

- `Core` збирається у файл `Core.dll` — його не можна запустити.
- `Cli` збирається в програму, яку **можна** запустити.

```bash
dotnet run --project src/Cli    # ✅ працює — це застосунок
dotnet run --project src/Core   # ❌ не працює — бібліотека без кнопки «Пуск». Це НОРМАЛЬНО
```
Бібліотеку тільки збирають: `dotnet build src/Core/Core.csproj`.

**Навіщо так?** Щоб той самий `Core` пізніше підключити ще й до `Api` та Blazor. Пишеш логіку раз — використовуєш скрізь.

---

### ProjectReference vs PackageReference
**Аналогія:** ProjectReference — інструмент, який ти зробив **сам** у сусідній кімнаті. PackageReference — інструмент, який ти **замовив у магазині** (NuGet, з інтернету).

- `ProjectReference` → твій проєкт: `Cli` посилається на `Core`.
- `PackageReference` → чужий пакет: напр. `Newtonsoft.Json` з NuGet.

У нас лише ProjectReference. У `Cli.csproj`:
```xml
<ProjectReference Include="..\Core\Core.csproj" />
```

**Напрямок — тільки в один бік: `Cli → Core`.**
**Аналогія:** пульт знає про коробку з інструментами, але інструменти не знають про пульт. Якщо зробити навпаки (`Core → Cli`), вони почнуть чекати один одного по колу → помилка `MSB4006 circular dependency`.

---

### TFM і RID (дві різні речі — часто плутають)
- **TFM** = *для якої версії .NET* зібрано код. Приклад: `net10.0`, `net8.0`. Пишеться в `.csproj`.
- **RID** = *для якої ОС і процесора*. Приклад: `osx-arm64` (macOS + Apple Silicon), `win-x64` (Windows + Intel/AMD 64).

RID читається просто: `osx-arm64` = `osx` (macOS) + `arm64` (процесор). `win-x64` = Windows + 64-бітний Intel/AMD.

---

### Multi-targeting
**Аналогія:** одну статтю друкуєш одразу двома мовами. Тут — одну бібліотеку збираєш під дві версії .NET.

У `Core.csproj` — множина (`Frameworks`, не `Framework`):
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```
Після збірки в `bin` з'являються **два підкаталоги**: `net8.0/` і `net10.0/` — по одній `.dll` на кожну версію.

**Різний код під різні версії** — директивами `#if`:
```csharp
#if NET10_0_OR_GREATER
    private const string BuildNote = "збірка під net10.0";
#else
    private const string BuildNote = "збірка під net8.0";
#endif
```
Це вирішується **під час компіляції**: для `net10.0` у код потрапляє верхній рядок, для `net8.0` — нижній.

---

### Публікація: self-contained vs framework-dependent
**Аналогія:**
- **Self-contained** = валіза, у яку ти поклав **і речі, і холодильник**. Приїжджаєш будь-куди — усе своє. Але важка (десятки МБ) і зроблена під конкретну країну (RID).
- **Framework-dependent** = валіза лише з **речами**. Легка (сотні КБ), але холодильник (.NET runtime) має вже стояти на місці.

```bash
# self-contained: код + залежності + КОПІЯ .NET runtime (працює без встановленого .NET)
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true    # ≈ 83 МБ

# framework-dependent: лише код (потрібен встановлений .NET на машині)
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained false   # ≈ 172 КБ
```
Self-contained великий **саме тому**, що всередині лежить увесь .NET (~80 МБ).

**Додаткові прапорці публікації (робив у лабі):**
- `-p:PublishSingleFile=true` — усе в один файл: замість 193 файлів лишається 3, розмір ≈ 76 МБ, запускається.
- `-p:PublishTrimmed=true` — вирізає невикористаний код: ≈ 83 МБ → **≈ 20 МБ** (35 файлів). **Але небезпечно для рефлексії:** збірка дала `warning IL2026`, і хоча зібралася, режим `--json` **падає в рантаймі** (`Reflection-based serialization has been disabled`), бо тример викинув типи, які серіалізатор шукає через рефлексію. Фікс — source generator (`JsonSerializerContext`) замість рефлексії.

---

### record vs class (чому `EnvironmentReport` — record)
**Аналогія:**
- `class` — це **людина**: має поведінку, робить дії, два тезки — різні люди.
- `record` — це **паспорт**: просто набір даних. Два паспорти з однаковими даними вважаються однаковими.

- `EnvironmentReport` — **record**, бо це просто дані (ОС, RID, каталог) — «паспорт середовища».
- `EnvironmentInfo` — **static class**, бо в ньому є **дія** — метод `Collect()`, який ці дані збирає.

Рівність за значенням у record «безкоштовно»:
```csharp
var a = new EnvironmentReport("macOS", ".NET 10", "Arm64", "osx-arm64", "osx-arm64", "/app/", "net10.0");
var b = new EnvironmentReport("macOS", ".NET 10", "Arm64", "osx-arm64", "osx-arm64", "/app/", "net10.0");
// a == b  →  true (у record порівнюються дані; у class було б false)
```

**Головна ідея:** `Core` **збирає дані** і повертає їх, а **друкує** їх `Cli`. Тому в `Program.cs` немає жодного `RuntimeInformation` — уся логіка в `Core`. Так само зможе користуватись Api чи Blazor, де консолі немає.

---

### Тернарний оператор і switch-вираз (з `DetectRid()`)
**Тернарний** `умова ? A : B` — коротке «якщо-інакше», що **повертає значення**:
```csharp
string os = IsOSPlatform(Windows) ? "win"
          : IsOSPlatform(Linux)   ? "linux"
          : IsOSPlatform(OSX)     ? "osx" : "unknown";
```
Читається: Windows → "win", інакше Linux → "linux", інакше macOS → "osx", інакше "unknown".

**switch-вираз** — вибір одного значення з багатьох; `_` = «усе інше»:
```csharp
string arch = ProcessArchitecture switch
{
    Architecture.X64   => "x64",
    Architecture.Arm64 => "arm64",
    _                  => "unknown"   // будь-що інше
};
```
Разом вони складають RID: `"osx" + "-" + "arm64"` = `osx-arm64`.

---

### Debug vs Release
- **Debug** — для розробки: без оптимізацій, з даними для відлагодження.
- **Release** — для користувача: оптимізований, менший, швидший.
- Публікують завжди в **Release** (`-c Release`).

---

## Частина 2. Відповіді на всі 12 питань захисту

**1. Навіщо class library, якщо код і так один?**
Щоб відокремити спільну логіку від точки входу. Одну `Core.dll` підключать і Cli, і майбутні Api (тиждень 10) та Blazor (тиждень 12) — пишеш раз, використовуєш скрізь.

**2. Чому Core не посилається на Cli? Що буде, якщо додати?**
Залежність має бути односторонньою (Cli → Core). Зворотне посилання дає **циклічну залежність** → збірка падає з `MSB4006`. До того ж незалежний Core можна підключати до інших проєктів.

**3. ProjectReference vs PackageReference?**
ProjectReference — посилання на інший **проєкт** твого рішення (твій код). PackageReference — на готовий **NuGet-пакет** з мережі (чужа бібліотека).

**4. Self-contained vs framework-dependent?**
Self-contained містить копію .NET runtime → працює без встановленого .NET, але великий і під один RID. Framework-dependent — лише код (малий), але потребує встановленого runtime.

**5. Чому self-contained такий великий і коли ця ціна виправдана?**
Бо всередині лежить увесь .NET runtime (~80 МБ). Виправдано, коли на машині немає й не можна поставити .NET (кіоск, ізольований сервер, роздача кінцевим користувачам).

**6. Що таке multi-targeting і навіщо бібліотеці?**
Збірка під кілька TFM одразу. Бібліотеці — щоб її могли використовувати проєкти і на старіших (`net8.0`), і на нових (`net10.0`) версіях без окремих гілок коду.

**7. Як зробити різний код для net8.0 і net10.0?**
Директивами `#if NET10_0_OR_GREATER … #else … #endif`. Компілятор для кожного TFM бере свою гілку.

**8. Звідки .NET знає RID під час виконання і чим це відрізняється від RID публікації?**
Під час виконання RID дає `RuntimeInformation.RuntimeIdentifier` — визначає сам runtime. RID публікації — той, що передав у `-r` при `dotnet publish`. Зазвичай збігаються, але джерела різні (рантайм vs параметр збірки).

**9. Чому EnvironmentInfo повертає record, а не друкує сам?**
Розділення відповідальностей: Core **збирає дані**, Cli **форматує вивід**. Тоді ту саму логіку використають Api/Blazor. record — бо це дані-результат, а не сутність із поведінкою.

**10. Що буде, якщо запустити win-x64 publish на Linux?**
Не запуститься — це нативний бінарник під Windows («Exec format error»). Для Linux треба `-r linux-x64` (і за потреби `chmod +x`).

**11. Куди піде доменна модель наступного тижня і чому туди?**
У `Core`: `Core/Dto/` (тиждень 3), `Core/Domain/` (тиждень 4), `Core/Storage/` (тиждень 5). Бо домен — спільна логіка для Cli й майбутніх Api/Blazor. Наш домен — Замовлення (`Customer`, `Product`, `Order`, `OrderLine`).

**12. Debug vs Release і чому публікують у Release?**
Debug — без оптимізацій, для розробки. Release — оптимізований, менший, швидший. Публікують у Release, бо це фінальна версія для користувача.

---

## Частина 3. Що можуть попросити зробити / ввести

Спершу в новому терміналі:
```bash
export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
```

| Просять | Команда | Що сказати |
|---|---|---|
| Склад рішення | `dotnet sln list` | Два проєкти: Cli і Core |
| Показати ProjectReference | відкрити `src/Cli/Cli.csproj` | Рядок `<ProjectReference Include="..\Core\Core.csproj" />` |
| Що в Program.cs немає логіки | відкрити `src/Cli/Program.cs` | Лише `Collect()` + друк, жодного `RuntimeInformation` |
| Зібрати | `dotnet build` | Build succeeded, 0 Errors; Core у двох TFM |
| Запустити | `dotnet run --project src/Cli` | Пояснити рядки (ОС, Runtime, RID) |
| JSON-режим | `dotnet run --project src/Cli -- --json` | Той самий звіт одним рядком |
| Довести multi-targeting | `ls src/Core/bin/Debug` | Підкаталоги `net8.0` і `net10.0` |
| Циклічна залежність | (усно / скріншот 5) | Core → Cli дає `MSB4006`, зміну скасувати |
| Опублікувати й запустити з publish | `dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true` → `./src/Cli/bin/Release/net10.0/osx-arm64/publish/Cli` | Бінарник несе власний runtime |
| Розмір publish | `du -sh <шлях publish>` | self-contained ≈ 83 МБ vs framework-dependent ≈ 172 КБ |
| RID системи | `dotnet --info` | Рядок `RID: osx-arm64` |

**Каверзні прохання:**
- *«Запусти Core»* → `dotnet run --project src/Core` навмисно не працює (бібліотека без `Main`). Це правильна відповідь, не помилка.
- *«Додай Core → Cli»* → покаже `MSB4006`; поясни циклічну залежність і скасуй.
- *«Чому bin/obj не в git?»* → `git status` чистий; `.gitignore` виключає артефакти збірки — вони відтворюються з коду.
- *«Опублікуй під Linux/Windows»* → зміни `-r linux-x64` / `-r win-x64`; на macOS збереться, але не запуститься.

> Порада: жодна команда тут не потребує паролів, ключів чи даних картки — усе безпечне й публічне.
