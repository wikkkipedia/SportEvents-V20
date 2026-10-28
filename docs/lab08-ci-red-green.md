# ЛР №8 — справжні червоний та зелений GitHub Actions

Репозиторій приватний: https://github.com/wikkkipedia/SportEvents-V20

Pull request №1: https://github.com/wikkkipedia/SportEvents-V20/pull/1

## Червоний запуск (GitHub Actions, Ubuntu, .NET 8)

**Run 40685289951** — https://github.com/wikkkipedia/SportEvents-V20/actions/runs/40685289951

- Commit 36364c4.
- Restore, formatting та Release build: success.
- xUnit: **137 passed / 1 failed**, усього 138.
- Невдалий тест: CiGateDemonstrationTests.CiGuard_Demonstration_WillFailUntilFixed, навмисне Assert.Equal(2, 1).
- Перевірка GitHub Actions завершилася статусом **failure** саме на етапі test, а не на компіляції.

## Зелений запуск (GitHub Actions, Ubuntu, .NET 8)

**Run 40685561478** — https://github.com/wikkkipedia/SportEvents-V20/actions/runs/40685561478

- Commit d31721a.
- Restore, formatting, Release build, xUnit, threshold покриття та завантаження звіту Cobertura: **success**.
- Локальне підтвердження того самого стану: **138/138 tests passed**.
- Реальний GitHub artifact покриття: https://github.com/wikkkipedia/SportEvents-V20/actions/runs/40685561478/artifacts/12815340180
- Рядкове покриття в локальному вимірюванні: **90,59%**; поріг у GitHub Actions — 70% і пройдено.

## Докази Git

- 475e8ea — спеціально додано невдалий тест.
- 412978d — виправлено форматування для Linux.
- 36364c4 — усунуто несумісність IDE0005 з SDK 8, але навмисно невдалий тест залишено.
- 6a6ebd1 — правильний Assert.Equal(2, 1 + 1); окремий коміт виправлення.
- d31721a — зелена гілка PR з усіма виправленнями.

Локально також перевірено: red 137/138; green 138/138. GitHub і локальні результати не змішуються: окремі посилання вище підтверджують онлайн-запуски.
