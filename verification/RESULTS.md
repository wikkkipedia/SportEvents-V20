# Перевірка SportEvents, варіант 20 — 28.10.2026

Перевірка виконана на Windows 11 із .NET SDK 9.0.318 для проєкту, що цілиться на .NET 8. Усі результати нижче отримано запуском команд, а не взято зі зразка звіту.

| Перевірка | Команда | Фактичний результат |
|---|---|---|
| Відновлення пакетів | `dotnet restore SportEvents.sln` | успішно (exit code 0) |
| Форматування й аналіз стилю | `dotnet format SportEvents.sln --verify-no-changes --no-restore --verbosity minimal` | успішно (exit code 0) |
| Складання Release | `dotnet build SportEvents.sln -c Release --no-restore -v:q` | успішно; 0 попереджень, 0 помилок |
| Модульні тести | `dotnet test SportEvents.Tests/SportEvents.Tests.csproj -c Release --no-build --no-restore -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura -p:Threshold=70 -p:ThresholdType=line -p:ThresholdStat=total -v:q` | 122 з 122 успішно |
| Рядкове покриття | Coverlet | 96,56 % |
| Покриття розгалужень | Coverlet | 89,68 % |
| Покриття методів | Coverlet | 96,36 % |
| Консольний застосунок | `dotnet run --project SportEvents.Console/SportEvents.Console.csproj -c Release --no-build` | меню відобразилося, 60 демоучасників, завершення за `0` без помилки |

Вивід команд у файлах `restore.log`, `format.log`, `build.log`, `tests.log`. Під час виправлення застосовано автоматичне форматування та явні винятки для частини правил StyleCop/CA в `.editorconfig` з коментарями. Аналізатори **не вимкнені повністю**. Суворе складання (`TreatWarningsAsErrors=true`) залишено.

## Що це НЕ підтверджує

- Реального зеленого та червоного запусків GitHub Actions і закритого pull request немає, оскільки немає під'єднаного віддаленого GitHub-репозиторію.
- Не перевірялися справжні метрики «до» та «після» для пакета чужого коду самостійної роботи №2 — вихідного пакета викладача немає.
- Вісім лабораторних містять підготовлені звіти та приклади, але ці випробування не є підтвердженням виконання всіх історичних кроків кожної лабораторної у заданому порядку.
- Наявні 22 локальні коміти походять із попередньо підготовленого архіву і не підтверджують роботу упродовж шести різних днів.
