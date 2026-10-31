# SportEvents — варіант 20

[![SportEvents quality](https://github.com/wikkkipedia/SportEvents-V20/actions/workflows/ci.yml/badge.svg)](https://github.com/wikkkipedia/SportEvents-V20/actions/workflows/ci.yml)

Навчальний консольний застосунок для реєстрації учасників і визначення результатів спортивних змагань.

## 1. Автор і навчальна дисципліна
Швець Артем Сергійович, група 472, варіант 20; «Конструювання програмного забезпечення». Проєкт підготовлено як автономну основу для СР №1 і №3. Оригінальний семестровий Git-репозиторій не надано — цей код не видається за його попередню історію.

## 2. Швидкий старт (три команди)
```bash
git clone https://github.com/wikkkipedia/SportEvents-V20.git SportEvents
cd SportEvents
dotnet run --project SportEvents.Console
```
Перед запуском встановіть .NET SDK 8 і, якщо проєкт передано ZIP-архівом, розпакуйте його та почніть із другої команди.

## 3. Структура
- `SportEvents.Core/Domain` — сутності, стани та вікові межі.
- `SportEvents.Core/Application` — сценарії використання, контракти, обчислення місць.
- `SportEvents.Core/Infrastructure` — JSON-сховище, журнал і системний годинник.
- `SportEvents.Console` — меню та читання вводу, тільки взаємодія з користувачем.
- `SportEvents.Tests` — модульні тести та ручні дублери залежностей.
- `docs/` — специфікація, архітектура, посібник, рішення, самооцінка та стандарт кодування.

## 4. Можливості
UC-01 — додати учасника; UC-02 — створити змагання; UC-03 — зареєструвати на старт; UC-04 — внести результат; UC-05 — відкрити, завершити чи скасувати змагання; UC-06 — знайти змагання; UC-07 — сформувати протокол. Деталі: [специфікація](docs/spec.md).

## 5. Перевірка та покриття
```bash
dotnet restore SportEvents.sln
dotnet build SportEvents.sln -c Release -warnaserror
dotnet test SportEvents.Tests/SportEvents.Tests.csproj -c Release -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura
```
У проєкті використано coverlet.msbuild. Реально перевірено на Windows 11, .NET SDK 9.0.318 із TargetFramework net8.0: **145 із 145 тестів зелені**, рядкове покриття всього рішення **91,23%**, гілок **83,76%**. Для основної бібліотеки SportEvents.Core — 96,98% рядків. Логи збережено у docs/lab06-completion, а попередня перевірка — verification.

## 6. Технології
.NET 8, C# 12, xUnit, coverlet.msbuild, Roslyn, StyleCop.Analyzers, GitHub Actions, JSON.

## 7. Локальна перевірка, як у CI
```bash
dotnet restore SportEvents.sln && dotnet format SportEvents.sln --verify-no-changes --no-restore && dotnet build SportEvents.sln -c Release -warnaserror && dotnet test SportEvents.Tests/SportEvents.Tests.csproj -c Release --no-restore -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura -p:Threshold=70 -p:ThresholdType=line -p:ThresholdStat=total
```
Конвеєр `.github/workflows/ci.yml` повторює ці перевірки та зберігає звіт як артефакт. Локальний конвеєр проходить (restore, dotnet format, Release build, тести). Реальний [червоний GitHub Actions](https://github.com/wikkkipedia/SportEvents-V20/actions/runs/40685289951) і [зелений GitHub Actions](https://github.com/wikkkipedia/SportEvents-V20/actions/runs/40685561478) підтверджені в pull request №1. Результати та докази: [docs/lab08-ci-red-green.md](docs/lab08-ci-red-green.md).

## 8. Обмеження і борг
Немає графічного інтерфейсу, бази даних і вебсервісу. Фініш зберігається з точністю до мілісекунд; суддівська фотофінішна система не передбачена. Не підтримано командні естафети, дискваліфікацію та імпорт результатів із пристроїв. Журнал містить лише коди подій і номери, а не імена учасників. Реальний dotnet test виконано, а відсотки покриття наведено вище. Старі успадковані навчальні модулі у каталозі labs збережено для перевірки ЛР №2, №3 і №6; вони не використовуються в основній консолі.
