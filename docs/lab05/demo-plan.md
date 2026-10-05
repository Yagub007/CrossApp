# План демонстрації на захисті — Лабораторна 5

> **Перед стартом:**
> ```bash
> cd ~/dev/CrossApp
> export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
> ```

---

## 1. Інтерфейс + сервіс + composition root
```bash
cat src/Core/Abstractions/IOrderStore.cs
grep -n "OrderService(IOrderStore" src/Core/Services/OrderService.cs
grep -n -A4 "IOrderStore store = useFile" src/Cli/Program.cs
```
**Що сказати:** «Ось контракт `IOrderStore` — 5 операцій. Конструктор сервісу приймає **інтерфейс**, не клас. А в Program.cs (composition root) тернарним оператором обираю реалізацію за `--file` — це єдине місце з конкретними класами.»

## 2. Запуск без --file; вихід; повторний запуск — даних немає
```bash
dotnet run --project src/Cli
dotnet run --project src/Cli
```
**Що сказати:** «Режим InMemory. Щоразу 15 зразкових замовлень + одне створене. Дані живуть лише в пам'яті — після виходу зникають, тому обидва запуски однакові.»

## 3. Запуск з --file; відкрити catalog.json; повторний запуск — дані на місці
```bash
dotnet run --project src/Cli -- --file          # РУН 1: на старті 0
cat src/Cli/bin/Debug/net10.0/data/catalog.json # файл з'явився
dotnet run --project src/Cli -- --file          # РУН 2: на старті 1
```
**Що сказати:** «Той самий сервіс, інша реалізація. Файлове сховище створило `catalog.json` і зберігає дані між запусками — на другому запуску на старті вже 1 замовлення. Це і є взаємозамінність реалізацій без зміни сервісу.»

## 4. Сценарій відмови
```bash
dotnet run --project src/Cli    # дивитись блок "Сценарії відмови"
```
**Що сказати:** «`Confirm` неіснуючого id і додавання дубля id дають `InvalidOperationException` зі зрозумілим текстом — виняток ловиться в Cli (try/catch), stack trace не сиплеться.»

---

## Бонус — додаткові завдання
```bash
dotnet run --project src/Cli    # блоки "Додаткове 1/2/3"
```
- **Дод. 1 (декоратор):** `CachingOrderStore` обгортає будь-який `IOrderStore` — «Кеш-звернень: 2» (3 виклики List → 1 реальний).
- **Дод. 2 (пошук):** `Search(o => o.Status == Confirmed)` → «Підтверджених: 8».
- **Дод. 3 (фабрика):** `StoreFactory.Create(args)` повертає ту саму реалізацію, що й composition root.

---

## Відповіді на 2–3 питання (готові)
**П: Що таке composition root?** Єдине місце, де створюються конкретні об'єкти й зв'язуються між собою. У нас — `Program.cs`.
**П: Чому сервіс не створює сховище сам?** Щоб (1) можна було замінити файл на БД без зміни сервісу, (2) тестувати сервіс з InMemory-сховищем без диска.
**П: Dependency Inversion vs Dependency Injection?** Inversion — *принцип*: сервіс залежить від абстракції, не від класу. Injection — *прийом*: залежність передають ззовні (у нас через конструктор).

### Чек-лист
- [ ] `export DOTNET_ROOT=...` виконано
- [ ] для чистої демонстрації file-режиму можна видалити старий `catalog.json`
