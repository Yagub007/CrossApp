# Definition of Done — Лабораторна 5

> Проєкт: **CrossApp**, домен **Замовлення**, .NET 10.
> Перед перевіркою:
> ```bash
> cd ~/dev/CrossApp
> export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
> ```

| # | Пункт | Статус |
|---|-------|--------|
| 1 | Інтерфейс сховища в Core | ✅ |
| 2 | Дві робочі реалізації (пам'ять + файл) | ✅ |
| 3 | Сервіс залежить лише від інтерфейсу, ін'єкція через конструктор | ✅ |
| 4 | Перемикання реалізації аргументом Cli; конкретні класи лише в Program.cs | ✅ |
| 5 | Сценарій: додати, змінити, показати список, знайти за id | ✅ |
| 6 | Помилка (дубль/відсутній id) з людським повідомленням | ✅ |
| 7 | Microsoft.Extensions.DependencyInjection не підключено | ✅ |
| 8 | README оновлено, коміт lab05 | ✅ |

---

## 1. Інтерфейс сховища в Core ✅
**Де:** [IOrderStore.cs](../../src/Core/Abstractions/IOrderStore.cs) — `List / GetById / Add / Update / Remove`.
```bash
cat src/Core/Abstractions/IOrderStore.cs
```

## 2. Дві реалізації ✅
**Де:** [InMemoryOrderStore.cs](../../src/Core/Storage/InMemoryOrderStore.cs) (словник), [FileOrderStore.cs](../../src/Core/Storage/FileOrderStore.cs) (JSON + кеш + Flush).
```bash
ls src/Core/Storage
```

## 3. Сервіс залежить лише від інтерфейсу ✅
Тип параметра конструктора — `IOrderStore`, не клас.
```bash
grep -n "OrderService(IOrderStore" src/Core/Services/OrderService.cs
grep -rn "FileOrderStore\|InMemoryOrderStore" src/Core/Services/ || echo "у сервісі конкретних сховищ немає"
```

## 4. Перемикання через Cli; конкретні класи лише в Program.cs ✅
```bash
grep -n "new FileOrderStore\|new InMemoryOrderStore" src/Cli/Program.cs
```
Поза Program.cs конкретні сховища згадуються лише у власних файлах і (дод.) у `StoreFactory`.

## 5. Сценарій: додати / змінити / список / знайти ✅
```bash
dotnet run --project src/Cli
```
`CreateOrder` → `AddLine` → `Confirm` → `All()` (список) → `Find(id)`.

## 6. Помилка з людським повідомленням ✅
У виводі блок «Сценарії відмови»:
- `Confirm неіснуючого id → Немає замовлення з id=...`
- `дубль id → Замовлення з id=... уже існує.`

## 7. DI-контейнер не підключено ✅
```bash
grep -rn "Microsoft.Extensions.DependencyInjection" src/*/*.csproj src/**/*.cs || echo "контейнер не підключено"
```
Ін'єкція — звичайним `new` у Program.cs (ручний DI).

## 8. README + коміт lab05 ✅
```bash
grep -n "Сервісний шар" README.md
git log --oneline | grep lab05
```

---

## Перевірка persistence (ключова демонстрація)
```bash
dotnet run --project src/Cli -- --file    # РУН 1: на старті 0
dotnet run --project src/Cli -- --file    # РУН 2: на старті 1 — дані збереглися
cat src/Cli/bin/Debug/net10.0/data/catalog.json
```

---

## Додаткові завдання (виконано)
- **1. Декоратор** — [CachingOrderStore.cs](../../src/Core/Storage/CachingOrderStore.cs): приймає інший `IOrderStore`, той самий контракт, кешує `List()` до першої зміни. Демо: «Кеш-звернень: 2».
- **2. Пошук із предикатом** — `OrderService.Search(Func<Order,bool>)` ([OrderService.cs](../../src/Core/Services/OrderService.cs)). Демо: «Підтверджених замовлень: 8».
- **3. Фабрика** — [StoreFactory.cs](../../src/Core/Storage/StoreFactory.cs): `Create(args, path)` будує сховище; порівняння з «усе в Program.cs» — у звіті.

```bash
dotnet run --project src/Cli    # блоки «Додаткове 1/2/3» у виводі
```

---
Усі 8 пунктів + додаткові виконано.
