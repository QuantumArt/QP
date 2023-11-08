# Представления

## Пользовательские представления вида CONTENT_NNN

Строятся для виртуальных контентов. Реализуют запросы в соответствии с типом виртуального контента

## Пользовательские представления вида CONTENT_NNN_LIVE

Строятся для всех контентов. Применяют live-фильтрацию по умолчанию к таблицам или представлениям вида CONTENT_NNN. Используются в LINQ-to-SQL-моделях

## Пользовательские представления вида CONTENT_NNN_STAGE

Строятся для всех контентов. Применяют stage-фильтрацию по умолчанию к таблицам или представлениям вида CONTENT_NNN. Используются в LINQ-to-SQL-моделях

## Пользовательские представления вида CONTENT_NNN_UNITED

Строятся для всех контентов. В отличие от таблиц или представлений типа CONTENT_NNN для расщепленных статей заменяют опубликованную версию статьи текущей.

## Пользовательские представления вида LINK_NN

Представления, созданные на основе ITEM_LINK и отфильтрованные по LINK_ID. Используются в LINQ-to-SQL моделях.

## Пользовательские представления вида LINK_NN_UNITED

Представления, созданные на основе ITEM_LINK_UNITED и отфильтрованные по LINK_ID. Используются в LINQ-to-SQL моделях.

| Имя представления | Описание |
|-------------------|----------|
| ITEM_LINK | Альтернативное представление таблицы ITEM_TO_ITEM. Используется для обратной совместимости |
| ITEM_LINK_UNITED | В отличие от представления ITEM_LINK для расщепленных статей заменяет опубликованную версию полей M2M текущими значениями. |
| ITEM_LINK_UNITED_FULL | Содержит все значения M2M: и текущие, и расщепленные. Используется для удаления |
| BACKEND_ACTION_ACCESS_PERMLEVEL | Комбинирует информацию о доступе к действиям (ACTION_ACCESS) с уровнем доступа (PERMISSION_LEVEL) |
| CONTENT_ACCESS_PermLevel | Комбинирует информацию о доступе к контентам (CONTENT_ACCESS) с уровнем доступа (PERMISSION_LEVEL) |
| CONTENT_ACCESS_PermLevel_site | Комбинирует информацию о доступе к контентам (CONTENT_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID сайта |
| CONTENT_ATTRIBUTE_TYPE | Кобинируетинформациюизтаблиц CONTENT_ATTRIBUTE и ATTRIBUTE_TYPE |
| content_data_mirror | Не используется |
| content_FOLDER_ACCESS_PermLevel | Комбинирует информацию о доступе к папкам библиотеки контента (CONTENT_FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL). В настоящее время не используется |
| content_FOLDER_ACCESS_PermLevel_content | Комбинирует информацию о доступе к папкам библиотеки контента (CONTENT_FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID контента. В настоящее время не используется |
| content_FOLDER_ACCESS_PermLevel_parent_folder | Комбинирует информацию о доступе к папкам библиотеки контента (CONTENT_FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL), с ID контента и с ID родительской папки. В настоящее время не используется |
| CONTENT_FORMAT | Комбинирование информации контентов и PublishingContainer. Не используется в QP8 |
| CONTENT_GROUP_NAME | Не используется |
| CONTENT_ITEM_ACCESS_PermLevel | Комбинирует информацию о доступе к статьям (CONTENT_ITEM_ACCESS) с уровнем доступа (PERMISSION_LEVEL). |
| CONTENT_ITEM_ACCESS_PermLevel_content | Комбинирует информацию о доступе к статьям (CONTENT_ITEM_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID контента. |
| content_item_group_access | Не используется |
| content_item_workflow | Предоставляет информацию об актуальном Workflow для статей |
| content_link | Альтернативное представление информации из таблицы CONTENT_TO_CONTENT |
| ENTITY_TYPE_ACCESS_PERMLEVEL | Комбинирует информацию о доступе к типам сущностей (ENTITY_TYPE_ACCESS) с уровнем доступа (PERMISSION_LEVEL). |
| FOLDER_ACCESS_PermLevel | Комбинирует информацию о доступе к папкам библиотеки сайта (FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL). |
| FOLDER_ACCESS_PermLevel_parent_folder | Комбинирует информацию о доступе к папкам библиотеки сайта (FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL), с ID сайта и с ID родительской папки. |
| FOLDER_ACCESS_PermLevel_site | Комбинирует информацию о доступе к папкам библиотеки сайта (FOLDER_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID сайта. |
| full_workflow_rules | Дополняет таблицу WORKFLOW_RULES неявным статусом None |
| object_attributes | Комбинирует PublishingContainer с полями контента, которые могут в нем применяться. Используется для VisualStudioAdd-In в сайтах, построенных на шаблонах QP. |
| objects_status | Комбинирует PublishingContainer со статусами, которые могут в нем применяться. Используется для VisualStudioAdd-In в сайтах, построенных на шаблонах QP. |
| PAGE_OBJECT | Комбинирует таблицы OBJECT и OBJECT_TYPE, отфильтровывая объекты страниц. Используется в сайтах, построенных на шаблонах QP |
| qp_test_view | Не используется |
| qp_ver | Дополняет версии бэкенда столбцом с весом каждой версии. Не используется в QP8 |
| SITE_ACCESS_PermLevel | Комбинирует информацию о доступе к сайтам (SITE_ACCESS) с уровнем доступа (PERMISSION_LEVEL). |
| site_content_item_modified | Список ID статей для сайта вместе с датой модификации |
| site_content_link | Альтернативное представление информации из таблицы CONTENT_TO_CONTENT, дополненнное ID сайта |
| site_item_link | Альтернативное представление информации из таблицы ITEM_TO_ITEM, дополненнное ID сайта |
| site_union_attrs | Дополняет таблицу union_attrs информацией о текущем сайте |
| site_union_contents | Дополняет таблицу union_contents информацией о текущем сайте |
| site_user_query_contents | Дополняет таблицу user_query_contents информацией о текущем сайте |
| TAB_ACCESS_PermLevel | Комбинирует информацию о доступе ко вкладкам (TAB_ACCESS) с уровнем доступа (PERMISSION_LEVEL). Не используется в QP8 |
| TEMPLATE_OBJECT | Комбинирует таблицы OBJECT и OBJECT_TYPE, отфильтровывая объекты шаблонов. Используется в сайтах, построенных на шаблонах QP |
| USER_GROUP_TREE | Объединяет таблицы USER_GROUP и GROUP_TO_GROUP для показа дерева групп |
| V_USER_QUERY_ATTRS | Выдает информацию о полях-источниках для виртуальных контентов типа USERQUERY |
| VIRTUAL_ATTR_BASE_ATTR_RELATION | Выдает информацию о полях-источниках для виртуальных контентов любых типов |
| VIRTUAL_CONTENT_RELATION | Выдает информацию о контентах-источниках для виртуальных контентов |
| VW_ACTION_STATUS_VIRTUAL | Не используется |
| VW_ACTION_TOOLBAR_BUTTON_VIRTUAL | Не используется |
| VW_ENTITY_TYPE | Не используется |
| VW_TREE_NODE_VIRTUAL | Не используется |
| WORKFLOW_ACCESS_PermLevel | Комбинирует информацию о доступе к Workflow (WORKFLOW_ACCESS) с уровнем доступа (PERMISSION_LEVEL). |
| WORKFLOW_ACCESS_PermLevel_site | Комбинирует информацию о доступе к Workflow (WORKFLOW_ACCESS) с уровнем доступа (PERMISSION_LEVEL) и с ID сайта |
|  workflow_max_statuses | Выдает информацию о максимальном статусе для каждого Workflow |
