# Описание БД QP8.CMS

## Общие сведения

Данный документ описывает структуру БД системы **QP8.CMS** (далее QP8). Система поддерживает следующие СУБД:

* SQL Server 2012 (или выше)
* PostgreSQL (или PostgresPro) 12 (или выше)

### Права доступа

#### SQL Server

Пользователь, под которым работает QP8, должен либо входить в роль `db_owner`, либо одновременно входить в роли `db_datareader`, `db_datawriter` и иметь права доступа `EXECUTE` на все хранимые процедуры и функции, определенные в системе.

Если в системе определены виртуальные контенты, получающие информацию из внешних источников, следует иметь в виду, что запросы к этим базам выполняются под тем же логином, под которым производится соединение с основной базой.

#### Postgres

Если для работы QP8.CMS и сайтов на его основе используется пользователь PG, отличный от `postgres`, необходимо дать следующие права на базу:

``` sql
GRANT CONNECT, TEMP ON DATABASE;
GRANT USAGE, CREATE ON SCHEMA qp
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA qp
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA qp
GRANT EXECUTE ON ALL ROUTINES IN SCHEMA qp
ALTER DEFAULT PRIVILEGES IN SCHEMA qp GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES
ALTER DEFAULT PRIVILEGES IN SCHEMA qp GRANT USAGE, SELECT ON SEQUENCES 
ALTER DEFAULT PRIVILEGES IN SCHEMA qp GRANT EXECUTE ON ROUTINES
```

Также необходимо сменить:

* `search_path` на значение `qp,public` либо в свойствах роли пользователя для всех баз, либо в строке подключения для конкретной базы.
* владельца у [пользовательских таблиц и представлений](db/customer.md), выполнив хранимую процедуру `qp_change_contents_ownership` (только для пользователя, под которым работает QP8.CMS).

## Разделы описания

* [Основные таблицы и поля](db/main.md)
* [Пользовательские таблицы и представления](db/customer.md)
* [Дополнительные таблицы и поля для работы с контентами](db/extra.md)
* [Таблицы и поля, описывающие структуру бэкенда](db/structure.md)
* [Таблицы и поля, реализующие права доступа и аудит](db/access.md)
* [Прочие таблицы](db/other.md)
* [Хранимые процедуры](db/procedures.md)
* [Пользовательские функции](db/functions.md)
* [Представления](db/views.md)
