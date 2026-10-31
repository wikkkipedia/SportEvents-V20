# Реєстр дефектів проєктування (ЛР №6, варіант 20)

Номери рядків прив'язані до першого коміту 9288077, файл labs/lab06/SportLegacyProcessor.cs. 8 різних типів дефектів із методички.

| № | Дефект | Місце до змін | Конкретний прояв | Перетворення |
|---|---|---|---|---|
|1|Довгий список параметрів|Handle, рядки 30–33|11 параметрів, частина описує того самого учасника|Introduce Parameter Object|
|2|Вкладені умови|Handle, рядки 35–60|5 рівнів блоків у головному методі|Replace Nested Conditional with Guard Clauses|
|3|Тимчасове поле|SportLegacyProcessor, рядки 26–27|_tmpSum і _tmpDiscount зберігають стан окремого виклику в полях|Remove Temporary Field|
|4|Дублювання|Handle, рядки 71–85 і Preview, рядки 117–135|формули трьох знижок і капування 500 дублюються|Extract Method CalculateDiscount|
|5|Довгий метод|Handle, початок 30|70 непорожніх рядків тіла|Extract Method, Extract Parameter Object|
|6|Великий клас|SportLegacyProcessor, початок 24|в одному класі валідація, суми, доставка, звіт, журнал, опис учасника|Move Method, Extract Method|
|7|Одержимість елементарними типами|Handle, рядки 30–33|стан, валюта, тип учасника — окремі string|Introduce Parameter Object; типізувати в окремій задачі|
|8|Заздрість до чужих даних|DescribeClient, рядки 138–146|використовує шість полів SportsParticipant|Move Method до SportsParticipant|
|9|Магічні числа|Handle, 73–90|0.15m, 0.30m, 500m, 1000m, 60m|Replace Magic Number with Constant (поріг і межа)|
|10|Побудова рядків в циклі|DumpLog, рядки 149–153|для кожного запису створюється новий рядок|Replace String Concatenation with StringBuilder|

Базова версія не редагувалася заднім числом; доступна через git show 9288077:labs/lab06/SportLegacyProcessor.cs.
