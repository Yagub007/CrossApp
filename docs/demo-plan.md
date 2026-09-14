# План демонстрації на захисті (3–5 хв) — Лабораторна 2

> **Перед стартом** (обов'язково, бо .NET 10 стоїть локально):
> ```bash
> cd ~/dev/CrossApp
> export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
> ```
> Перевірка, що активна саме 10-та: `dotnet --version` → `10.0.401`.

---

## 1. Дерево solution + Cli.csproj з ProjectReference

**Команда:**
```bash
dotnet sln list
cat src/Cli/Cli.csproj
```

**Що сказати:** «У рішенні два проєкти — `Cli` (застосунок) і `Core` (бібліотека). У `Cli.csproj` є `ProjectReference` на `Core` — залежність одностороння: Cli → Core.»

**Очікуваний вивід:**
```
src/Cli/Cli.csproj
src/Core/Core.csproj
...
<ProjectReference Include="..\Core\Core.csproj" />
```

---

## 2. Program.cs — немає бізнес-логіки

**Команда:**
```bash
cat src/Cli/Program.cs
```

**Що сказати:** «Program.cs лише викликає `EnvironmentInfo.Collect()` з Core і форматує вивід. Жодного `RuntimeInformation` тут немає — уся логіка живе в Core.»

**Доказ одним рядком** (нічого не знайде):
```bash
grep RuntimeInformation src/Cli/Program.cs
```

---

## 3. Запуск застосунку

**Команда:**
```bash
dotnet run --project src/Cli
```

**Що сказати:** «Виводиться інформація про середовище. ОС — macOS, Runtime — .NET 10, архітектура Arm64. Два рядки RID: один я визначив вручну в коді, другий повідомляє сам .NET — вони збігаються (`osx-arm64`).»

**Очікуваний вивід:**
```
CrossApp – інформація про середовище
Студент: Алієв Ягуб, група ФЕІ-32с
----------------------------------------------------
ОС             : macOS 13.0.0
Runtime        : .NET 10.0.12
Архітектура    : Arm64
RID (визначено): osx-arm64
RID (від .NET) : osx-arm64
Каталог        : .../src/Cli/bin/Debug/net10.0/
Збірка (TFM)   : збірка під net10.0
```

---

## 4. Запуск бінарника напряму з каталогу publish

**Команда:**
```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true
./src/Cli/bin/Release/net10.0/osx-arm64/publish/Cli
```

**Що сказати:** «Це вже не `dotnet run`, а сам зібраний бінарник. Він несе власну копію .NET runtime, тому запускається без встановленого .NET. Вивід той самий, а рядок "Каталог" показує шлях усередині publish — доказ, що працює саме опублікований файл.»

---

## 5. Таблиця розмірів у README + різниця режимів

**Команда:**
```bash
open README.md        # або показати розділ «Таблиця: RID — режим — розмір»
```

**Що сказати (головне):**
- **Self-contained** ≈ 83 МБ — всередині лежить увесь .NET runtime, працює **без** встановленого .NET, але великий і під один RID.
- **Framework-dependent** ≈ 172 КБ — лише мій код, **потрібен** встановлений .NET на машині.
- Різниця у розмірі — це і є «вартість» незалежності від встановленого runtime.

**Таблиця (з README):**

| RID | Режим | Розмір | Runtime |
|---|---|---|---|
| osx-arm64 | self-contained | ~83 МБ | ні |
| osx-arm64 | framework-dependent | ~172 КБ | так |

---

## 6. Multi-targeting: `<TargetFrameworks>` + два підкаталоги в bin

**Команда:**
```bash
cat src/Core/Core.csproj          # показати <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
ls src/Core/bin/Debug             # два підкаталоги: net8.0  net10.0
```

**Що сказати:** «Бібліотека Core збирається під дві версії .NET одразу. Тому в `bin` два підкаталоги — по одній `.dll` на кожну версію. А рядок "Збірка (TFM)" у виводі формується через `#if NET10_0_OR_GREATER` — той самий код дає різний текст залежно від версії.»

> Якщо `bin` порожній — спершу `dotnet build`.

---

## 7. Відповіді на 2–3 питання (готові короткі)

**П: Чому Core не посилається на Cli?**
Бо залежність має бути односторонньою. Зворотне посилання дає циклічну залежність — збірка падає з помилкою `MSB4006`. До того ж незалежний Core можна підключити до майбутнього Api.

**П: Чим self-contained відрізняється від framework-dependent?**
Self-contained несе власний .NET runtime (працює без встановленого .NET, але важить десятки МБ). Framework-dependent — лише мій код (малий), але потребує встановленого сумісного runtime.

**П: Чому EnvironmentInfo повертає record, а не друкує сам?**
Розділення відповідальностей: Core збирає дані, Cli їх друкує. Тоді ту саму логіку зможе використати Api чи Blazor, де консолі немає. record — бо це просто дані-результат.

**П (запасне): Що станеться, якщо запустити `dotnet run --project src/Core`?**
Нічого — Core це бібліотека без `Main`. Запускаються лише Exe-проєкти (Cli). Це не помилка коду.

---

### Швидкий чек-лист перед захистом
- [ ] `export DOTNET_ROOT=...` виконано, `dotnet --version` = 10.0.401
- [ ] `dotnet build` проходить без помилок
- [ ] publish зроблено (щоб пункт 4 працював одразу)
- [ ] README відкритий на розділі з таблицею
