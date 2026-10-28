# ЛР3: декомпозиція підпрограм

Метод-доноp: `docs/labs/lab03_before.cs.txt` (адаптований стислий демонстраційний зразок; не точна 92-рядкова заготовка методички).
Рішення: `LapSummaryCalculator.Calculate`, `ElapsedTimePolicy.SumSplits`,
`LapSummaryRequest`, `LapSummaryResult`.

Розбиття відповідальності: перевірка запиту, перевірка часу, підрахунок,
формування результату, консольний вивід окремо в `SportEvents.Console/Program.cs`.
Замість чотирьох параметрів — один тип-запит.
Класифікація CQS: `Calculate` — query; `RecordResult` — command;
`GenerateProtocol` — query; `OpenEvent` — command.

Порівняння оцінюється лише на зазначеному зразку і НЕ є звітом про метрики
методичного 92-рядкового лістингу. Перевірка dotnet test лишається відкритою.
