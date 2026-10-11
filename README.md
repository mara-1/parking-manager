# ParkingManager

[![CI](https://github.com/mara-1/parking-manager/actions/workflows/ci.yml/badge.svg)](https://github.com/mara-1/parking-manager/actions/workflows/ci.yml)
[![Commit messages](https://github.com/mara-1/parking-manager/actions/workflows/commitlint.yml/badge.svg)](https://github.com/mara-1/parking-manager/actions/workflows/commitlint.yml)

Десктоп-приложение для учёта и управления автомобильной парковкой.
Курсовой проект по дисциплине «Конструирование программного обеспечения».

**Стек:** C#, .NET 8, WPF (MVVM), EF Core + SQLite, QuestPDF, ClosedXML, xUnit.

## Структура репозитория

```
src/ParkingManager/          WPF-приложение: Views, ViewModels (ParkingManager.exe)
src/Parking.Core/            бизнес-логика (Services) и доступ к данным (DataAccess)
tests/Parking.Core.Tests/        модульные тесты бизнес-логики
tests/ParkingManager.Tests/      модульные тесты моделей представления (MVVM)
tests/Parking.IntegrationTests/  интеграционные тесты (EF Core + SQLite)
tests/Parking.UiTests/           тесты от лица пользователя (FlaUI, UI Automation)
tests/tasks.json                 список задач проекта для отчёта о тестировании
scripts/                     создание решения, сбор метрик, отчёт о тестировании
.github/workflows/           пайплайны CI/CD
.githooks/                   локальная проверка сообщений коммитов
```

## Начало работы

```bash
git clone https://github.com/mara-1/parking-manager.git
cd parking-manager
git config core.hooksPath .githooks
dotnet build
dotnet test
```

Правила оформления коммитов, ветвления, слияния и релизов — в [CONTRIBUTING.md](CONTRIBUTING.md).

Перед каждым коммитом рекомендуется выполнять `dotnet format`.

## Тестирование

| Вид | Проект | Что проверяется |
|---|---|---|
| Модульные | `Parking.Core.Tests`, `ParkingManager.Tests` | службы, хеширование паролей, модели представления, конвертеры |
| Интеграционные | `Parking.IntegrationTests` | работа с настоящей базой SQLite |
| От лица пользователя | `Parking.UiTests` | сценарии оператора в запущенном приложении: вход, въезд, выезд |

Каждый тест помечен видом и номером задачи из плана проекта:

```csharp
[Trait("Category", "Unit")]   // Unit | Integration | UI
[Trait("Task", "7")]          // номер задачи в Trello / tests/tasks.json
```

Запуск:

```bash
dotnet test --filter "Category!=UI"   # быстрые тесты
dotnet test tests/Parking.UiTests     # UI-тесты: откроют окна приложения, мышь и клавиатуру не трогать
python scripts/test_report.py         # отчёт по файлам TestResults/**/*.trx
```

UI-тесты запускают собранный `ParkingManager.exe` с отдельной временной базой
(переменная окружения `PARKING_DB_PATH`), поэтому рабочие данные не затрагиваются.

В CI отчёты формируются автоматически: на вкладке **Actions** → запуск → **Summary**
выводятся покрытие кода и сводный отчёт с матрицей «задача → тесты»,
а в артефактах сохраняются `test-results` (результаты и HTML-отчёт о покрытии),
`ui-test-results` (результаты и снимки экрана UI-тестов) и `test-report`.
