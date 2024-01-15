# Nuget-пакет Quantumart.QP8.BLL

## Назначение

API, предоставляющий доступ к функциональности QP8. Используется для создания админок, встраиваемых в QP8.

## Репозиторий

https://nuget.qsupport.ru/packages/Quantumart.QP8.BLL

## Quantumart.QP8.BLL 3.x

### Quantumart.QP8.BLL.3.3.11

* Убрана зависимость от схемы public в PostgreSQL (#173717)
* Обновление Quantumart.AspNetCore до 6.0.13

### Quantumart.QP8.BLL.3.3.10

* Обновление Quantumart.AspNetCore до 6.0.11 (#173120)

### Quantumart.QP8.BLL.3.3.9

* Обновление Quantumart.AspNetCore до 6.0.8 (#172194)

### Quantumart.QP8.BLL.3.3.8

* Обновление Quantumart.AspNetCore до 6.0.7

### Quantumart.QP8.BLL.3.3.7

* Обновление Quantumart.AspNetCore до 6.0.6

### Quantumart.QP8.BLL.3.3.6

* Обновление Quantumart.AspNetCore до 6.0.5

### Quantumart.QP8.BLL.3.3.5

* Обновление Quantumart.AspNetCore до 6.0.3

### Quantumart.QP8.BLL.3.3.4

* Исправление ошибки с конверсией типов #171240

### Quantumart.QP8.BLL.3.3.3

* Обновление nuget-зависимостей

### Quantumart.QP8.BLL.3.3.2

* Обновление nuget-зависимостей

### Quantumart.QP8.BLL.3.3.1

* Обновление nuget-зависимостей

### Quantumart.QP8.BLL.3.3.0

* Обновление EF.Core и NpgSql до 6.x

### Quantumart.QP8.BLL.3.2.1

* Обновление nuget-зависимостей (устранение пререлизов)

### Quantumart.QP8.BLL.3.2.0

* Обновление EF.Core и NpgSql до 5.x

### Quantumart.QP8.BLL.3.1.1

* Обновление Nuget-пакетов

### Quantumart.QP8.BLL.3.1.0

* Обновление Nuget-пакетов (до версий для .NET Core 3.1)

### Quantumart.QP8.BLL.3.0.2

* Убраны лишние Exceptions в XAML-валидации

### Quantumart.QP8.BLL.3.0.1

* Добавлена поддержка локализации в XAML-валидации

### Quantumart.QP8.BLL.3.0.0

* Добавлена поддержка PostgreSQL, переход на EF.Core

## Quantumart.QP8.BLL 2.x

### Quantumart.QP8.BLL.2.9.3

* Обновлена версия EF

### Quantumart.QP8.BLL.2.9.2

* Добавлена поддержка версий в `BatchUpdate`

### Quantumart.QP8.BLL.2.9.1

* Исправлены ошибки в запросах в `ArticleMatchService` #131609

### Quantumart.QP8.BLL.2.9.0

* Обновление nuget-пакетов

### Quantumart.QP8.BLL.2.8.7

* Исправлена работа `BatchUpdate` (ошибка с репликацией статей)

### Quantumart.QP8.BLL.2.8.6

* Исправлена ошибка, возникающая при пустом результате в методе `Ids`

### Quantumart.QP8.BLL.2.8.5

* Исправлена ошибка с пустым списком ID в методе получения списка в `ArticleService`

### Quantumart.QP8.BLL.2.8.4

* Новый метод Ids в ArticleService

### Quantumart.QP8.BLL.2.8.3

* Исправлена валидация XAML для контентов-расширений

### Quantumart.QP8.BLL.2.8.2

* Исправления загрузки виртуальных контентов через BLL (часть 2)

### Quantumart.QP8.BLL.2.8.1

* Исправления загрузки виртуальных контентов через BLL (часть 1)

### Quantumart.QP8.BLL.2.8.0

* Поддержка дополнительной фильтрации в методе `List` класса `ArticleService`

### Quantumart.QP8.BLL.2.7.0

* Поддержка опции сохранения результата (обогащение статей с помощью XAML) в методе `ValidateXamlById` класса `ArticleService`

### Quantumart.QP8.BLL.2.6.0

* Множественная загрузка связей в ArticleService
* Опция фильтрация архивных статей для всех методов ArticleService

### Quantumart.QP8.BLL.2.5.0

* Внутренний рефакторинг

### Quantumart.QP8.BLL.2.4.0

* Обновлены версии зависимостей (мультиплатформенная Quantumart.dll)

### Quantumart.QP8.BLL.2.3.0

* Переход на .NET Framework 4.7

### Quantumart.QP8.BLL.2.2.7

* Передача customer code при валидации XAML

### Quantumart.QP8.BLL.2.2.6

* Переход на .NET Framework 4.5.2

### Quantumart.QP8.BLL.2.1.3

* Исправлен дефект #94310: в BatchUpdate не работало обнуление ссылок M2M
