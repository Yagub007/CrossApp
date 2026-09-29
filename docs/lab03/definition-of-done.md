# Definition of Done — Лабораторна 3

> Проєкт: **CrossApp**, домен **Замовлення**, .NET 10.
> Перед перевіркою в новому терміналі:
> ```bash
> cd ~/dev/CrossApp
> export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
> ```

| # | Пункт | Статус |
|---|-------|--------|
| 1 | 2–3 record-типи в Core/Dto, nullable лише де виправдано | ✅ |
| 2 | Розбір рядка через switch expression з ≥ 3 різними патернами | ✅ |
| 3 | Імпорт CSV/JSON працює і повертає дані РАЗОМ із помилками | ✅ |
| 4 | data/sample.csv на 10+ рядків, серед них 2–3 пошкоджені | ✅ |
| 5 | Cli виводить кількість, перші записи і пропущені рядки з номерами | ✅ |
| 6 | File-scoped namespace, зайвих using немає | ✅ |
| 7 | Коміт lab03 | ✅ |

---

## 1. Record-типи в Core/Dto 
**Де:** [ProductDto.cs](../../src/Core/Dto/ProductDto.cs), [CustomerDto.cs](../../src/Core/Dto/CustomerDto.cs), [ImportResult.cs](../../src/Core/Dto/ImportResult.cs)
`ProductDto(Id, Name, Price, Category?)`, `CustomerDto(Id, Name, Email?)`, `ImportResult<T>(Items, Errors)`. `?` лише на Category та Email.
```bash
ls src/Core/Dto
```

## 2. switch expression з ≥ 3 патернами 
**Де:** [ProductCsvImporter.cs](../../src/Core/Import/ProductCsvImporter.cs), метод `ParseLine`. Використано 5 патернів: властивості `{ Length: < 4 }`, константний `[_, "", _, _]`, охорона `when`, список зі зв'язуванням `[var id, ...]`, `_`.
```bash
grep -n "switch\|=> new Parse\|when " src/Core/Import/ProductCsvImporter.cs
```

## 3. Імпорт повертає дані РАЗОМ із помилками 
`ImportResult<T>(Items, Errors)` — один пошкоджений рядок не перериває імпорт.
```bash
dotnet run --project src/Cli        # 10 записів + 3 помилки
```

## 4. data/sample.csv: 10+ рядків, 2–3 пошкоджені 
**Де:** [data/sample.csv](../../data/sample.csv) — 10 коректних (P-001…P-010) + 3 пошкоджені (P-011 замало колонок, P-012 нечислова ціна, P-013 порожня назва).
```bash
git check-ignore data/sample.csv || echo "файл у репозиторії"
```

## 5. Cli: кількість + перші записи + пропущені з номерами 
```bash
dotnet run --project src/Cli
```
Виводить `Завантажено записів: 10`, перші 5, `Пропущено рядків: 3` з `рядок 12/13/14` і рядок статистики.

## 6. File-scoped namespace, без зайвих using 
```bash
grep -rL "namespace .*;" src/Core/Dto src/Core/Import || echo "усі file-scoped"
```
У Program.cs немає жодного `Split`/`Parse` — уся логіка в Core:
```bash
grep -nE "\.Split\(|int\.Parse|decimal\.Parse" src/Cli/Program.cs || echo "Cli чистий"
```

## 7. Коміт lab03 
```bash
git log --oneline | grep lab03
```

---
Усі 7 пунктів виконано.
