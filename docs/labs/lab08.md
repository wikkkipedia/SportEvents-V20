# ЛР8: Git, складання і CI

Гілка `feature/ci-pipeline` дійсно створена локально.
`Directory.Build.props` встановлює .NET 8, nullable, аналізатори, warnings as errors.
`.github/workflows/ci.yml` містить restore, format verify, Release build,
unit tests, поріг покриття 70%, upload coverage artifacts.

Контрольні команди для запуску на комп'ютері:
`dotnet restore SportEvents.sln`
`dotnet format SportEvents.sln --verify-no-changes`
`dotnet build SportEvents.sln -c Release -warnaserror`
`dotnet test SportEvents.sln -c Release --collect:"XPlat Code Coverage"`
`dotnet list SportEvents.sln package --vulnerable --include-transitive`

Обов'язкові докази, яких немає: посилання на GitHub, зелений і червоний
CI run, PR з самооглядом, злиття PR. Їх неможливо створити без
підключеного GitHub і фактичного запуску CI, і не слід підміняти симуляціями.
