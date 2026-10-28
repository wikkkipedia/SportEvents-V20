# Лабораторна робота №1 - середовище та стандарт

Варіант 20 SportEvents; проєкти: SportEvents.Core, SportEvents.Console,
SportEvents.Tests. Основа рішення імпортована з попереднього локального архіву.
Базова структура містить .editorconfig, Directory.Build.props, README,
XML-документацію, GitHub workflow, .gitignore.

Команди для перевірки: `dotnet --version`, `dotnet build SportEvents.sln`,
`dotnet test SportEvents.sln`, `dotnet format SportEvents.sln --verify-no-changes`.
Запуск .NET у цьому середовищі НЕ підтверджено: dotnet SDK відсутній.

Ключові сутності: SportsEvent (документ), EventRegistration (позиція),
Participant (учасник), FinishResult (об'єкт обліку/результат), EventStatus.
Стан: Planned -> Open -> Finished, Planned/Open -> Cancelled.

Локальна історія Git побудована зараз для відтворення виконаної роботи;
вона НЕ є історією попереднього семестру і НЕ відтворює минулі дати.
