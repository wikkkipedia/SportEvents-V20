# ЛР5: помилки, Result, логування

Барикада: RaceTimeInput.TryParse перетворює string на TimeSpan до передавання
в EventService.RecordResult; некоректне введення повертає OperationResult.Fail,
а не аварію. Неприпустимі переходи станів позначає DomainRuleException.
Для журналювання застосовано IAuditLogger та FileAuditLogger із рівнями
Information / Warning / Error. Ресурси файлів закриваються викликами
File.ReadAllText / File.WriteAllText або using у відповідному сценарії.

Матриця 9 випадків: програміст — null у non-null ctor, відсутній об'єкт,
індекс поза масивом; середовище — диск повний, файл заблокований, нема прав;
користувач — неформатний час, від'ємна дистанція, повторна реєстрація.

Старий антипатерн: `catch (Exception) { return null; }`; заміна — окремий
OperationResult для очікуваного вводу, доменний виняток для порушення інваріантів,
обробка IOException / UnauthorizedAccessException у зовнішньому шарі.

Команди перевірки: dotnet build, dotnet test --filter RaceTimeInputTests.
Вони не виконані в середовищі без SDK.
