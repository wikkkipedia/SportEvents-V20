# GitHub Pull Request №1 — CI та контроль якості SportEvents

Pull request: https://github.com/wikkkipedia/SportEvents-V20/pull/1

## Мета змін

Додано ручний запуск workflow_dispatch, обмежено дозволи конвеєра до contents: read, налаштовано Restore → dotnet format → Release збірку → xUnit + вимогу 70% покриття → artifact Cobertura. Оновлено .gitattributes для LF і враховано розбіжність Roslyn SDK 8/9.

## Самоогляд

- [x] Бізнес-логіку програми не змінювали спеціально для CI; додано лише контрольний тест.
- [x] Секретів, токенів або персональних даних у код не додано.
- [x] Локальний Release: 0 помилок, 0 попереджень.
- [x] Локально 138 із 138 тестів, загальне рядкове покриття 90,59%.
- [x] Червоний GitHub Actions підтверджено: https://github.com/wikkkipedia/SportEvents-V20/actions/runs/40685289951
- [x] Зелений GitHub Actions підтверджено: https://github.com/wikkkipedia/SportEvents-V20/actions/runs/40685561478
- [x] В README є адресу справжнього репозиторію та GitHub Actions badge.
- [ ] Після злиття PR перевірити зелений запуск від push у main.

## Ризики і обмеження

GitHub CI пройшов на Ubuntu + .NET SDK 8. Локально перевірено Windows із SDK 9 та TargetFramework net8.0. Усі названі вище онлайн-результати мають окремі URL. Попередні невдалі CI-запуски через форматування та IDE0005 також збережено у GitHub history; це виправлені несумісності, а не приховані неуспіхи.
