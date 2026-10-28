# ЛР2: аудит іменування та форматування SportEvents

Донор: `docs/labs/lab02_before.cs.txt` (навчальний зразок, не частина виконуваної збірки).
Готовий код: `SportEvents.Core/Domain/Models.cs`, `Application/EventService.cs`,
`Application/RankingPolicy.cs`, `Application/ElapsedTimePolicy.cs`.

## Реєстр порушень (конкретний донор)
1. rez - назва класу не PascalCase.
2. nm - назва поля незрозуміла.
3. id - публічне змінне поле.
4. ds - назва поля не пояснює змісту.
5. sp - сирий список масивів рядків.
6. st - число стану замість enum.
7. Add - без змісту аргументів.
8. a - незрозумілий параметр.
9. b - незрозумілий параметр.
10. c - незрозумілий параметр.
11. Calc - нечітка назва операції.
12. f - булевий прапорець без змістовної назви.
13. t - нечітка локальна змінна.
14. n - лічильник без контексту.
15. Дробові тривалості зберігаються як double замість TimeSpan.
16. Небезпечний double.Parse з культури системи.
17. Магічні пороги 1000, 10, 100 без пояснення.
18. Форматування: кілька операторів на одному рядку.
19. Коментар 'час і все що треба' не пояснює рішення.
20. Коментар із мертвим кодом Old.
21. Безумовне присвоєння t=t.
22. Публічний змінний список sp порушує інкапсуляцію.

## Перейменування
rez -> ElapsedTimePolicy; nm -> EventName; id -> EventId; ds -> Distance;
sp -> Splits; st -> EventStatus; Add -> AddSplit; a -> participantId;
b -> lapIndex; c -> rawTime; Calc -> SumSplits; f -> hasPenalty;
t -> totalDuration; n -> splitCount; i -> splitIndex;
Old -> вилучено; 'стан 0' -> EventStatus.Planned.

Метрики й перевірки, що потребують dotnet: очікують виконання на ПК із .NET SDK.
