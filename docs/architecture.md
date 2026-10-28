# Архітектура проєкту SportEvents

## Шари та заборони
| Шар | Проєкт і тека | Що робить | Чого не робить |
|---|---|---|---|
| Домен | Core/Domain | Стани, моделі, обчислення віку, власний виняток | Не відкриває файлів, не пише в Console |
| Прикладний | Core/Application | Сценарії, політика місць, інтерфейси сховищ | Не знає формат JSON |
| Інфраструктура | Core/Infrastructure | JSON, файл журналу та системний час | Не обчислює місця |
| Консоль | SportEvents.Console | Меню, введення, текстові повідомлення | Не рахує групи й місця |
| Тести | SportEvents.Tests | Ізольовані тести і фейкові залежності | Не працюють з реальними даними |

## Схема залежностей (рисунок 1)
```text
              SportEvents.Console
                      |
                      v
              Core.Application -----> Core.Domain
                    ^
                    |
             Core.Infrastructure

              SportEvents.Tests ----> Core.Application / Domain
```
Фізично Domain, Application та Infrastructure зібрані в одному проєкті, але прямі імпорти з домену до інфраструктури відсутні. Інфраструктура реалізує `IEventRepository`, `IClock`, `IAuditLogger`, визначені в Application. Console знає лише, які реалізації передати під час старту.

## Композиційний корінь
`SportEvents.Console/Program.cs`, метод `Main`:
```csharp
string folder = Path.Combine(Directory.GetCurrentDirectory(), "data");
IClock clock = new SystemClock();
IEventRepository repo = new JsonEventRepository(Path.Combine(folder, "events.json"));
IAuditLogger log = new FileAuditLogger(Path.Combine(folder, "audit.log"), clock);
EventService service = new(repo, clock, new RankingPolicy(), log);
service.SeedDemoParticipants();
```
Створення реалізацій зібрано в одному місці, а не розкидано по методах предметної області.

## Точки розширення
1. **JSON замість CSV** — створити клас `CsvEventRepository : IEventRepository`, не змінюючи `EventService`.
2. **Інша система місць** — нова реалізація `IResultRanking`, без зміни UI або сховища.
3. **Інше логування** — підставити реалізацію `IAuditLogger` для журналу в іншому форматі.

## Компроміси
Наскрізний проєкт навчальний, тому один файл містить один знімок даних. Не підтримується одночасний запис кількома процесами. Для кількох користувачів знадобиться транзакційна база, але це поза межами курсу.
