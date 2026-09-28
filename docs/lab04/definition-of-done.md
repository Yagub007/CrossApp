# Definition of Done — Лабораторна 4

> Проєкт: **CrossApp**, домен **Замовлення**, .NET 10.
> Перед перевіркою:
> ```bash
> cd ~/dev/CrossApp
> export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$HOME/.dotnet:$PATH"
> ```

| # | Пункт | Статус |
|---|-------|--------|
| 1 | Сутності в Core/Domain; records тижня 3 лишились як DTO | ✅ |
| 2 | Стан інкапсульовано: приватні поля / private set, змінює лише метод | ✅ |
| 3 | Є фабричний метод, публічного конструктора немає | ✅ |
| 4 | ≥ 3 інваріанти з осмисленими повідомленнями винятків | ✅ (7) |
| 5 | Правильний вибір типу винятку (Argument* / InvalidOperation*) | ✅ |
| 6 | Є ToDto / FromDto (мапінг сутність ↔ DTO) | ✅ |
| 7 | Cli демонструє успіх і відмову; stack trace не сиплеться | ✅ |
| 8 | Колекції назовні — тільки для читання | ✅ |
| 9 | Перелік інваріантів у README, коміт lab04 | ✅ |

---

## 1. Сутності в Core/Domain, DTO лишились ✅
**Де:** [Order.cs](../../src/Core/Domain/Order.cs), [OrderLine.cs](../../src/Core/Domain/OrderLine.cs), [OrderStatus.cs](../../src/Core/Domain/OrderStatus.cs); DTO — [Core/Dto](../../src/Core/Dto).
```bash
ls src/Core/Domain src/Core/Dto
```

## 2. Інкапсуляція ✅
Приватне поле `_lines`, властивості `{ get; }` / `Status { get; private set; }`. Пряме присвоєння ззовні не компілюється.
```bash
grep -n "private readonly\|get; private set;\|=> _quantity\|{ get; }" src/Core/Domain/Order.cs
```

## 3. Фабрика, без публічного конструктора ✅
```bash
grep -n "private Order(\|private OrderLine(\|public static Order Create\|public static OrderLine Create" src/Core/Domain/*.cs
grep -n "public Order(\|public OrderLine(" src/Core/Domain/*.cs || echo "публічних конструкторів немає"
```

## 4. ≥ 3 інваріанти з повідомленнями ✅ (реалізовано 7)
```bash
grep -n "throw new" src/Core/Domain/Order.cs src/Core/Domain/OrderLine.cs
```
Повний перелік — у таблиці [README](../../README.md) та у звіті.

## 5. Правильні типи винятків ✅
`ArgumentException` / `ArgumentOutOfRangeException` — некоректний вхід; `InvalidOperationException` — операція неможлива в поточному стані. Видно у виводі сценарію 2.

## 6. ToDto / FromDto ✅
```bash
grep -n "ToDto\|FromDto" src/Core/Domain/Order.cs src/Core/Domain/OrderLine.cs
```
`FromDto` відновлює стан через `Create`/`AddLine`/`Confirm` — ті самі інваріанти.

## 7. Cli: успіх + відмова, без stack trace ✅
```bash
dotnet run --project src/Cli
```
Сценарій 1 — успіх; сценарій 2 — try/catch виводить лише `тип — Message`.

## 8. Колекції назовні лише для читання ✅
```bash
grep -n "AsReadOnly\|IReadOnlyList" src/Core/Domain/Order.cs
```

## 9. Інваріанти в README + коміт lab04 ✅
```bash
grep -n "Інваріанти" README.md
git log --oneline | grep lab04
```

---

## Додаткові завдання (виконано)
- **1. Зв'язок із тижнем 3** — [OrderAssembler.cs](../../src/Core/Domain/OrderAssembler.cs): `BuildLines(ImportResult)` → доменні рядки + перелік тих, що не пройшли інваріанти.
- **2. Інваріант між двома сутностями** — пояснено, чому такі правила виносять у сервіс тижня 5 (сутність бачить лише себе; крос-сутнісне правило потребує стану кількох об'єктів/сховища).
- **3. Явний стан + переходи** — [OrderStatus.cs](../../src/Core/Domain/OrderStatus.cs) + `Order.RequireTransition` зі switch expression: недопустимий перехід кидає `InvalidOperationException`.

```bash
dotnet run --project src/Cli    # у виводі є блоки «Додаткове 1» і «Додаткове 3»
```

---
Усі 9 пунктів + додаткові виконано.
