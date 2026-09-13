# Шпаргалка для захисту (3–5 хв)

Домен: **Замовлення** — `Customer`, `Product`, `Order`, `OrderLine`. Призначення: оформлення замовлень і підрахунок сум.
Середовище: macOS, RID `osx-arm64`, .NET SDK 8.0.

Перед початком:
```bash
cd ~/dev/CrossApp
```

---

## Крок 1. Дерево каталогів / склад solution
```bash
dotnet sln list
```
Сказати: solution `CrossApp` містить проєкт `src/Cli/Cli.csproj`; каталоги `Core/Api/Web/tests` додаються на пізніших тижнях.

## Крок 2. Збірка
```bash
dotnet build
```
Сказати: «Build succeeded, 0 Warning(s), 0 Error(s)» — проєкт збирається без попереджень.

## Крок 3. Запуск і коментар виводу
```bash
dotnet run --project src/Cli
```
Прокоментувати рядки:
- **OSDescription / Environment.OSVersion** — ОС і версія (тут Darwin/macOS).
- **Архітектура процесу** — Arm64 (Apple Silicon).
- **Версія .NET (CLR)** vs **Runtime** — числова версія рушія та опис фреймворку.
- **Каталог застосунку** (`bin/.../net8.0`) ≠ **Поточний каталог** (корінь проєкту) — різні шляхи.
- Останній рядок — обрана предметна область (Замовлення).

## Крок 4. README — розділ про предметну область
```bash
open README.md            # або показати на сторінці GitHub
```
Показати розділ «Предметна область»: сутності + призначення + чому саме вона.

## Крок 5. .gitignore і історія комітів
```bash
git status                # bin/ та obj/ НЕ у списку untracked
git log --oneline
```
Сказати: `.gitignore` виключає `bin/`/`obj/` (це артефакти збірки, їх не комітять); показати коміт `lab01`.

## Крок 6. (Бонус) Запуск self-contained бінарника без dotnet run
```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true
./src/Cli/bin/Release/net8.0/osx-arm64/publish/Cli
```
Сказати: бінарник несе власний runtime, тому працює без встановленого .NET.

---

## Додаткові завдання (якщо запитають)

**JSON-режим:**
```bash
dotnet run --project src/Cli -- --json
```

**Запуск у Linux-контейнері (порівняння OSDescription):**
```bash
docker run --rm -v "${PWD}":/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet run --project src/Cli
```
Локально: `Darwin ... ARM64` (macOS) → у контейнері: `Debian GNU/Linux 12 (bookworm)` (Linux). Той самий код, різні ОС — це і є крос-платформність.
