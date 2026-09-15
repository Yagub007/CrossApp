# Definition of Done — Лабораторна 2

> Проєкт: **CrossApp** (Core + Cli), .NET 10, macOS `osx-arm64`.
> Перед перевіркою в новому терміналі:
> ```bash
> cd ~/dev/CrossApp
> export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
> ```

| # | Пункт | Статус |
|---|-------|--------|
| 1 | Два проєкти в solution, посилання Cli → Core | ✅ |
| 2 | Уся «інформація про середовище» живе в Core, Cli лише форматує | ✅ |
| 3 | Program.cs не містить логіки, окрім виводу | ✅ |
| 4 | Є publish хоча б для однієї RID, запуск із каталогу publish | ✅ |
| 5 | README пояснює self-contained vs framework-dependent + таблиця | ✅ |
| 6 | Каталоги bin/obj/publish не в репозиторії | ✅ |
| 7 | Коміт lab02 | ✅ |

---

## 1. Два проєкти в solution, посилання Cli → Core ✅

**Де:** [CrossApp.sln](../CrossApp.sln), [src/Cli/Cli.csproj](../src/Cli/Cli.csproj)

**Перевірка:**
```bash
dotnet sln list
grep ProjectReference src/Cli/Cli.csproj
```
**Очікується:** два проєкти (`Cli`, `Core`) і рядок `<ProjectReference Include="..\Core\Core.csproj" />`.

---

## 2. Уся «інформація про середовище» живе в Core ✅

**Де:** [src/Core/EnvironmentInfo.cs](../src/Core/EnvironmentInfo.cs) — `record EnvironmentReport` + `static class EnvironmentInfo` з `Collect()` і `DetectRid()`.

**Перевірка:**
```bash
grep -c RuntimeInformation src/Core/EnvironmentInfo.cs   # > 0: логіка тут
```

---

## 3. Program.cs не містить логіки, окрім виводу ✅

**Де:** [src/Cli/Program.cs](../src/Cli/Program.cs) — лише `EnvironmentInfo.Collect()` і друк.

**Перевірка (нічого не має знайти):**
```bash
grep RuntimeInformation src/Cli/Program.cs
```

---

## 4. Publish хоча б для однієї RID + запуск із publish ✅

**RID:** `osx-arm64` (self-contained і framework-dependent).

**Перевірка:**
```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true
./src/Cli/bin/Release/net10.0/osx-arm64/publish/Cli
```
**Очікується:** та сама таблиця середовища; рядок «Каталог» вказує всередину `publish`.

---

## 5. README: self-contained vs framework-dependent + таблиця ✅

**Де:** [README.md](../README.md) — розділ «self-contained vs framework-dependent» і «Таблиця: RID — режим — розмір — потрібен runtime».

| RID | Режим | Розмір | Runtime |
|---|---|---|---|
| osx-arm64 | self-contained | ~83 МБ | ні |
| osx-arm64 | framework-dependent | ~172 КБ | так |

---

## 6. bin/obj/publish не в репозиторії ✅

**Де:** правила у [.gitignore](../.gitignore).

**Перевірка:**
```bash
git status --short                                   # bin/obj/publish відсутні
git check-ignore src/Cli/bin src/Core/obj            # підтверджує ігнорування
```

---

## 7. Коміт lab02 ✅

**Перевірка:**
```bash
git log --oneline | grep lab02
```
**Очікується:** коміт(и) з описом `lab02: ...`.

---

## Підсумок: як перевірити все одразу

```bash
export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
dotnet sln list                                   # 1
grep RuntimeInformation src/Cli/Program.cs || echo "3: Program.cs чистий"   # 3
dotnet build                                      # збірка проходить
git status --short                                # 6: без bin/obj/publish
git log --oneline | grep lab02                    # 7
```

Усі 7 пунктів виконано.
