# ЛР4: класова модель та SOLID

Сутності: `Participant`, `SportsEvent`, `EventRegistration`, `FinishResult`, `CompetitionDistance`.
`CompetitionDistance` має незмінне значення та інваріант Meters > 0.
`SportsEvent` є моделлю документа, зміна стану виконується через EventService.
`EventService` залежить від IEventRepository, IClock, IResultRanking, IAuditLogger;
всі залежності передані через конструктор, без Service Locator.

Аудит SOLID (приклади ризиків у моноліті, усунені або розділені у рішення):
SRP — змішування збереження й розрахунку; OCP — умови для всіх типів ranking;
LSP — непотрібний спадкоємець, що кидає виняток; ISP — єдиний інтерфейс для всього;
DIP — пряме new JsonEventRepository всередині EventService.

Висновок: розрахунок ранжування у RankingPolicy, збереження у JsonEventRepository,
поточна дата у IClock, логування через IAuditLogger. Зауваження: повна інкапсуляція
всіх record-моделей лишається завданням для поглиблення.
