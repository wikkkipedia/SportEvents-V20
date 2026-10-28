# SportEvents — варіант 20

Навчальний консольний застосунок для реєстрації учасників і визначення результатів спортивних змагань.

## 1. Автор і навчальна дисципліна
Швець Артем Сергійович, група 472, варіант 20; «Конструювання програмного забезпечення». Проєкт підготовлено як автономну основу для СР №1 і №3. Оригінальний семестровий Git-репозиторій не надано — цей код не видається за його попередню історію.

## 2. Швидкий старт (три команди)
```bash
git clone <АДРЕСА_ВАШОГО_РЕПОЗИТОРІЮ> SportEvents
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
У проєкті використано `coverlet.msbuild`; звіт Cobertura створюється після реального запуску. Покриття в цій копії **не вимірювалося**, адже .NET SDK недоступний у середовищі створення матеріалів. Не підміняйте факт запуску кількістю написаних тестів.

## 6. Технології
.NET 8, C# 12, xUnit, coverlet.msbuild, Roslyn, StyleCop.Analyzers, GitHub Actions, JSON.

## 7. Локальна перевірка, як у CI
```bash
dotnet restore SportEvents.sln && dotnet format SportEvents.sln --verify-no-changes --no-restore && dotnet build SportEvents.sln -c Release -warnaserror && dotnet test SportEvents.Tests/SportEvents.Tests.csproj -c Release --no-restore -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura -p:Threshold=70 -p:ThresholdType=line -p:ThresholdStat=total
```
Конвеєр `.github/workflows/ci.yml` повторює ці перевірки та зберігає звіт як артефакт. Червоний/зелений прогін GitHub не підтверджено: репозиторій не опубліковано.

## 8. Обмеження і борг
Немає графічного інтерфейсу, бази даних і вебсервісу. Фініш зберігається з точністю до мілісекунд; суддівська фотофінішна система не передбачена. Не підтримано командні естафети, дискваліфікацію та імпорт результатів із пристроїв. Журнал містить лише коди подій і номери, а не імена учасників. Потрібно виконати реальний `dotnet test` та перевірити рядкове/гілкове покриття перед здачею.
