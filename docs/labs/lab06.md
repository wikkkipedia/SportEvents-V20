# ЛР6: рефакторинг ResultTable.Rank, варіант SportEvents

Збережений вихідний демонстраційний фрагмент: `lab06_ranking_before.cs.txt`.
Після: `SportEvents.Core/Application/ResultTable.cs`.
Реєстр дефектів: 1 складний булевий вираз; 2 вкладені цикли;
3 змінювання вхідного списку; 4 відсутня перевірка null;
5 дубльовані умови порівняння; 6 нечітке ім'я r; 7 нечіткі i/j;
8 залежність від змінного List; 9 неявна умова стабільності сортування.

Застосовано: Extract Variable (розбір ключів); Extract Method (Compare);
Rename (rows); Replace Nested Loop with Query; Introduce Guard Clause;
Replace Primitive Comparison with Enum Compare; Hide Mutable Collection
(повернення IReadOnlyList); Remove Side Effect (новий масив).

У цьому архіві це окремий навчальний адаптований приклад, а НЕ точне
рефакторування 149-рядкового LegacyProcessor із методички ЛР6.
Перш ніж заявляти повне виконання ЛР6, необхідно зіставити оригінальні
149 рядків, виміряти метрики й показати green tests на .NET.
