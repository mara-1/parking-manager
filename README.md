# ParkingManager

[![CI](https://github.com/mara-1/parking-manager/actions/workflows/ci.yml/badge.svg)](https://github.com/mara-1/parking-manager/actions/workflows/ci.yml)
[![Commit messages](https://github.com/mara-1/parking-manager/actions/workflows/commitlint.yml/badge.svg)](https://github.com/mara-1/parking-manager/actions/workflows/commitlint.yml)

Десктоп-приложение оператора автомобильной парковки: учёт мест по категориям,
регистрация въезда и выезда, расчёт оплаты, бронирование, история стоянок
и отчёты с выгрузкой в Excel и PDF.

Курсовой проект по дисциплине «Конструирование программного обеспечения»
(УлГТУ, группа ПИбд-43, Аринина М. А.).

**Стек:** C# 12, .NET 8, WPF (MVVM), EF Core 8 + SQLite, ClosedXML, QuestPDF, xUnit, FlaUI.

## Возможности

| Функция | Что умеет |
|---|---|
| Вход оператора | логин и пароль, пароли хранятся в виде хеша PBKDF2-SHA256 с солью |
| Учёт мест | две категории: 450 обычных мест и 50 мест для инвалидов (не менее 10 %), счётчики свободных мест, схема занятости |
| Въезд и выезд | по государственному номеру (формат РФ, `А123ВС73`), поиск по части номера, проверка повторного въезда и наличия мест |
| Оплата | 60 ₽ за каждый начатый час (минимум 1 час), места для инвалидов бесплатно; сумма фиксируется при выезде |
| Бронирование | бронь на сегодня по номеру и категории; место удерживается до времени прибытия + 15 минут, затем бронь снимается |
| История | стоянки за период с поиском по номеру и суммой |
| Отчёты | итоги за период, разбивка по категориям и по дням, брони; сохранение в Excel и PDF |
| Настройки | вместимость категорий и стоимость часа |

Подробно — в [руководстве оператора](docs/user-guide.md).

## Установка и запуск

**Требования:** Windows 10 или 11 (x64), [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).

1. Откройте [Releases](https://github.com/mara-1/parking-manager/releases) и скачайте архив
   `ParkingManager-vX.Y.Z.zip` последней версии.
2. Распакуйте архив в любую папку и запустите `ParkingManager.exe`.
3. Войдите под демо-оператором: логин **admin**, пароль **admin123**.

База данных создаётся при первом запуске в файле
`%LOCALAPPDATA%\ParkingManager\parking.db`. Другой путь можно задать
переменной окружения `PARKING_DB_PATH`.

## Сборка из исходного кода

Нужны .NET SDK 8 (версия закреплена в `global.json`) и Visual Studio 2022 или `dotnet` CLI.

```bash
git clone https://github.com/mara-1/parking-manager.git
cd parking-manager
git config core.hooksPath .githooks
dotnet build
dotnet run --project src/ParkingManager
```

## Документация

| Документ | Содержание |
|---|---|
| [Руководство оператора](docs/user-guide.md) | работа с каждой вкладкой, правила, сообщения об ошибках |
| [Архитектура](docs/architecture.md) | слои и проекты, MVVM, модель данных, бизнес-правила, CI/CD и метрики |
| [CONTRIBUTING.md](CONTRIBUTING.md) | коммиты, ветвление Git Flow, слияние, релизы |
| [CHANGELOG.md](CHANGELOG.md) | изменения по версиям |

## Структура репозитория

```
src/ParkingManager/              WPF-приложение: Views, ViewModels, Converters (ParkingManager.exe)
src/Parking.Core/                бизнес-логика (Services), модели (Models), доступ к данным (DataAccess)
tests/Parking.Core.Tests/        модульные тесты бизнес-логики
tests/ParkingManager.Tests/      модульные тесты моделей представления (MVVM)
tests/Parking.IntegrationTests/  интеграционные тесты (SQLite, выгрузка в Excel и PDF)
tests/Parking.UiTests/           тесты от лица пользователя (FlaUI, UI Automation)
tests/tasks.json                 задачи проекта для отчёта о тестировании
tests/coverage.runsettings       настройки сбора покрытия
docs/                            документация
scripts/                         создание решения, сбор метрик, отчёт о тестировании
.github/workflows/               пайплайны CI/CD
.githooks/                       локальная проверка сообщений коммитов
```

## Тестирование

| Вид | Проект | Что проверяется |
|---|---|---|
| Модульные | `Parking.Core.Tests`, `ParkingManager.Tests` | службы, правила тарификации и броней, формат номера, модели представления |
| Интеграционные | `Parking.IntegrationTests` | работа с настоящей базой SQLite, файлы Excel и PDF |
| От лица пользователя | `Parking.UiTests` | сценарии оператора в запущенном приложении |

Каждый тест помечен видом и номером задачи проекта:

```csharp
[Trait("Category", "Unit")]   // Unit | Integration | UI
[Trait("Task", "7")]          // номер задачи в Trello / tests/tasks.json
```

```bash
dotnet test --filter "Category!=UI"   # модульные и интеграционные тесты
dotnet test tests/Parking.UiTests     # UI-тесты: откроют окна приложения, мышь и клавиатуру не трогать
python scripts/test_report.py         # отчёт по файлам TestResults/**/*.trx
```

UI-тесты запускают собранный `ParkingManager.exe` с отдельной временной базой
(`PARKING_DB_PATH`), поэтому рабочие данные не затрагиваются.

В CI на вкладке **Actions** → запуск → **Summary** выводятся покрытие кода
и сводный отчёт с матрицей «задача → тесты»; в артефактах сохраняются
`test-results` (результаты и HTML-отчёт о покрытии), `ui-test-results`
(результаты и снимки экрана UI-тестов) и `test-report`.

## Метрики качества

При каждом релизе workflow `Release` собирает метрики (предупреждения анализаторов,
цикломатическая сложность, покрытие, тесты, активность в репозитории, CI) и публикует
их в Summary запуска. Сгенерированный при сборке код (XAML, регулярные выражения)
из покрытия и сложности исключён. Значения по этапам — в [CHANGELOG.md](CHANGELOG.md).
