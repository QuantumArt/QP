# Представления

## Пользовательские представления вида CONTENT_NNN

Строятся для виртуальных контентов. Реализуют запросы в соответствии с типом виртуального контента

## Пользовательские представления вида CONTENT_NNN_LIVE

Строятся для всех контентов. Применяют live-фильтрацию по умолчанию к таблицам или представлениям вида CONTENT_NNN. Используются в LINQ-to-SQL-моделях

## Пользовательские представления вида CONTENT_NNN_STAGE

Строятся для всех контентов. Применяют stage-фильтрацию по умолчанию к таблицам или представлениям вида CONTENT_NNN. Используются в LINQ-to-SQL-моделях

## Пользовательские представления вида CONTENT_NNN_UNITED

Строятся для всех контентов. В отличие от таблиц или представлений типа CONTENT_NNN для расщепленных статей заменяют опубликованную версию статьи текущей.

## Представление ITEM_LINK

Альтернативное представление таблицы ITEM_TO_ITEM. Используется для обратной совместимости

## Представление ITEM_LINK_UNITED

В отличие от представления ITEM_LINK для расщепленных статей заменяет опубликованную версию полей M2M текущими значениями.

## Представление ITEM_LINK_UNITED_FULL

Содержит все значения M2M: и текущие, и расщепленные. Используется для удаления

## Пользовательские представления вида LINK_NN

Представления, созданные на основе ITEM_LINK и отфильтрованные по LINK_ID. Используются в LINQ-to-SQL моделях.

## Пользовательские представления вида LINK_NN_UNITED

Представления, созданные на основе ITEM_LINK_UNITED и отфильтрованные по LINK_ID. Используются в LINQ-to-SQL моделях.

## Представление BACKEND_ACTION_ACCESS_PERMLEVEL

Комбинирует информацию о доступе к действиям (ACTION_ACCESS) с уровнем доступа (PERMISSION_LEVEL)

## Представление CONTENT_ACCESS_PermLevel

Комбинирует информацию о доступе к контентам (CONTENT_ACCESS) с уровнем доступа (PERMISSION_LEVEL)

## Представление CONTENT_ACCESS_PermLevel_site

Комбинирует информацию о доступе к контентам (CONTENT_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID сайта

## Представление CONTENT_ATTRIBUTE_TYPE

Кобинируетинформациюизтаблиц CONTENT_ATTRIBUTE и ATTRIBUTE_TYPE

## Представление content_data_mirror

Не используется

## Представление content_FOLDER_ACCESS_PermLevel

Комбинирует информацию о доступе к папкам библиотеки контента (CONTENT_FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL). В настоящее время не используется

## Представление content_FOLDER_ACCESS_PermLevel_content

Комбинирует информацию о доступе к папкам библиотеки контента (CONTENT_FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID контента. В настоящее время не используется

## Представление content_FOLDER_ACCESS_PermLevel_parent_folder

Комбинирует информацию о доступе к папкам библиотеки контента (CONTENT_FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL), с ID контента и с ID родительской папки. В настоящее время не используется

## Представление CONTENT_FORMAT

Комбинирование информации контентов и PublishingContainer. Не используется в QP8

## Представление CONTENT_GROUP_NAME

Не используется

## Представление CONTENT_ITEM_ACCESS_PermLevel

Комбинирует информацию о доступе к статьям (CONTENT_ITEM_ACCESS) с уровнем доступа (PERMISSION_LEVEL).

## Представление CONTENT_ITEM_ACCESS_PermLevel_content

Комбинирует информацию о доступе к статьям (CONTENT_ITEM_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID контента.

## Представление content_item_group_access

Не используется

## Представление content_item_workflow

Предоставляет информацию об актуальном Workflow для статей

## Представление content_link

Альтернативное представление информации из таблицы CONTENT_TO_CONTENT

## Представление ENTITY_TYPE_ACCESS_PERMLEVEL

Комбинирует информацию о доступе к типам сущностей (ENTITY_TYPE_ACCESS) с уровнем доступа (PERMISSION_LEVEL).

## Представление FOLDER_ACCESS_PermLevel

Комбинирует информацию о доступе к папкам библиотеки сайта (FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL).

## Представление FOLDER_ACCESS_PermLevel_parent_folder

Комбинирует информацию о доступе к папкам библиотеки сайта (FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL), с ID сайта и с ID родительской папки.

## Представление FOLDER_ACCESS_PermLevel_site

Комбинирует информацию о доступе к папкам библиотеки сайта (FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID сайта.

## Представление full_workflow_rules

Дополняет таблицу WORKFLOW_RULES неявным статусом None

## Представление object_attributes

Комбинирует PublishingContainer с полями контента, которые могут в нем применяться. Используется для VisualStudioAdd-In в сайтах, построенных на шаблонах QP.

## Представление objects_status

Комбинирует PublishingContainer со статусами, которые могут в нем применяться. Используется для VisualStudioAdd-In в сайтах, построенных на шаблонах QP.

## Представление PAGE_OBJECT

Комбинирует таблицы OBJECT и OBJECT_TYPE, отфильтровывая объекты страниц. Используется в сайтах, построенных на шаблонах QP

## Представление qp_test_view

Не используется

## Представление qp_ver

Дополняет версии бэкенда столбцом с весом каждой версии. Не используется в QP8

## Представление SITE_ACCESS_PermLevel

Комбинирует информацию о доступе к сайтам (SITE_ACCESS) с уровнем доступа (PERMISSION_LEVEL).

## Представление site_content_item_modified

Список ID статей для сайта вместе с датой модификации

## Представление site_content_link

Альтернативное представление информации из таблицы CONTENT_TO_CONTENT, дополненнное ID сайта

## Представление site_item_link

Альтернативное представление информации из таблицы ITEM_TO_ITEM, дополненнное ID сайта

## Представление site_union_attrs

Дополняет таблицу union_attrs информацией о текущем сайте

## Представление site_union_contents

Дополняет таблицу union_contents информацией о текущем сайте

## Представление site_user_query_contents

Дополняет таблицу user_query_contents информацией о текущем сайте

## Представление TAB_ACCESS_PermLevel

Комбинирует информацию о доступе ко вкладкам (TAB_ACCESS) с уровнем доступа (PERMISSION_LEVEL). Не используется в QP8

## Представление TEMPLATE_OBJECT

Комбинирует таблицы OBJECT и OBJECT_TYPE, отфильтровывая объекты шаблонов. Используется в сайтах, построенных на шаблонах QP

## Представление USER_GROUP_TREE

Объединяет таблицы USER_GROUP и GROUP_TO_GROUP для показа дерева групп

## Представление V_USER_QUERY_ATTRS

Выдает информацию о полях-источниках для виртуальных контентов типа USERQUERY

## Представление VIRTUAL_ATTR_BASE_ATTR_RELATION

Выдает информацию о полях-источниках для виртуальных контентов любых типов

## Представление VIRTUAL_CONTENT_RELATION

Выдает информацию о контентах-источниках для виртуальных контентов

## Представление VW_ACTION_STATUS_VIRTUAL

Не используется

## Представление VW_ACTION_TOOLBAR_BUTTON_VIRTUAL

Не используется

## Представление VW_ENTITY_TYPE

Не используется

## Представление VW_TREE_NODE_VIRTUAL

Не используется

## Представление WORKFLOW_ACCESS_PermLevel

Комбинирует информацию о доступе к Workflow (WORKFLOW_ACCESS) с уровнем доступа (PERMISSION_LEVEL).

## Представление WORKFLOW_ACCESS_PermLevel_site

Комбинирует информацию о доступе к Workflow (WORKFLOW_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID сайта

## Представление workflow_max_statuses

Выдает информацию о максимальном статусе для каждого Workflow