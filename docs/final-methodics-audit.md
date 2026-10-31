# Повторний аудит 11 методичок — 31.10.2026

Джерела: методички 8 лабораторних та 3 самостійних, вихідний код GitHub, тести .NET та Git-історія. Цей файл не є оцінкою викладача.

| Робота | Перевірені матеріали | Стан |
|---|---|---|
| ЛР 1 | .sln, Core/Console, .editorconfig, StyleCop, CODING_STANDARD.md, README, lab1 tag | Підготовлено |
| ЛР 2 | docs/labs/lab02.md, реєстр, labs/lab02, before/after/diff, Release build | Поведінка без змін |
| ЛР 3 | labs/lab03, guards, CQS, методи з CC<=6 | Перевірено |
| ЛР 4 | domain records, EventService, DI, інваріанти, тести | Перевірено |
| ЛР 5 | docs/errors.md (9 ситуацій), DomainRuleException.RuleCode і innerException, ILogger, тести | Виправлено |
| ЛР 6 | labs/lab06, docs/lab06-completion, docs/smells.md, характеризаційні тести | Перевірено |
| ЛР 7 | справжній red Assert -> green -> refactor, xUnit, Cobertura | Перевірено |
| ЛР 8 | Directory.Build.props, lab08-v0.8.0 tag, CI, red/green PR, checklist | Перевірено |
| СР 1 | .editorconfig, CODING_STANDARD.md, docs/sr01-coding-standard.md | Перевірено |
| СР 2 | три теги baseline/safety-net/after-refactoring, 17 тестів, Git та Cobertura | Перевірено |
| СР 3 | 7 UC, README, 7 ADR, посібник, 145 тестів, Core 96.98%, CI | Бракує ще 4 реальних днів Git-історії |

## Числа СР 3 станом на 31.10.2026

- Фінальна версія коду 1.0.0, після CI необхідно підтвердити тег v1.0.
- xUnit: 145/145 зелених тестів.
- Coverlet: Core 96.98% рядків і 90% гілок, загальне покриття рядків 91.23%.
- Збірка Release: 0 попереджень та 0 помилок.
- scripts/audit_sr03_metrics.py: 1353 непорожніх рядки без однорядкових дужок, найдовший метод 27, максимальна приблизна CC 8, вкладеність 3. Це лексичні оцінки, а не незалежний Roslyn-аналіз.
- Історія Git: 2 різні дати з потрібних 6. Шість днів не можна чесно відтворити за один день.

## Перевірка на захисті

1. Показати публічні GitHub-репозиторії, теги, PR і CI.
2. Виконати dotnet format SportEvents.sln --verify-no-changes.
3. Виконати dotnet build SportEvents.sln -c Release -warnaserror.
4. Виконати dotnet test SportEvents.Tests/SportEvents.Tests.csproj -c Release -p:CollectCoverage=true -p:Threshold=70 -p:ThresholdType=line -p:ThresholdStat=total.
5. Пояснити власний код: TDD, DI, інваріанти, власні винятки, логування, ранжування.

ChatGPT та Remote Desktop Commander використовувалися як помічники при аналізі коду, тестуванні, документації і скриншотах. Важливо вміти пояснити виконані зміни.
