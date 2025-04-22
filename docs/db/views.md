# Представления

| Имя представления | Описание |
|-------------------|----------|
| ITEM_LINK | Альтернативное представление [таблицы ITEM_TO_ITEM](main.md#таблица-item_to_item). Используется для обратной совместимости. Доступно во всех типах подерживаемых БД |
| ITEM_LINK_UNITED | В отличие от представления ITEM_LINK для расщепленных статей заменяет опубликованную версию полей M2M текущими значениями. Доступно во всех типах подерживаемых БД |
| ITEM_LINK_UNITED_FULL | Содержит все значения M2M: и текущие, и расщепленные. Используется для удаления. Не реализовано в Postgres |
| BACKEND_ACTION_ACCESS_PERMLEVEL | Комбинирует информацию о доступе к действиям ([таблица ACTION_ACCESS](access.md#таблица-action_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Доступно во всех типах подерживаемых БД |
| CONTENT_ACCESS_PermLevel | Комбинирует информацию о доступе к контентам ([таблица CONTENT_ACCESS](access.md#таблица-content_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Доступно во всех типах подерживаемых БД |
| CONTENT_ACCESS_PermLevel_site | Комбинирует информацию о доступе к контентам ([таблица CONTENT_ACCESS](access.md#таблица-CONTENT_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)) и с ID сайта. Доступно во всех типах подерживаемых БД |
| CONTENT_ATTRIBUTE_TYPE | Кобинирует информацию из таблиц [CONTENT_ATTRIBUTE](main.md#таблица-content_attribute) и [ATTRIBUTE_TYPE](extra.md#таблица-attribute_type). Доступно во всех типах подерживаемых БД |
| content_data_mirror | Не используется |
| content_FOLDER_ACCESS_PermLevel | Комбинирует информацию о доступе к папкам библиотеки контента ([CONTENT_FOLDER_ACCESS](access.md#таблица-content_folder_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). В настоящее время не используется |
| content_FOLDER_ACCESS_PermLevel_content | Комбинирует информацию о доступе к папкам библиотеки контента ([CONTENT_FOLDER_ACCESS](access.md#таблица-content_folder_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)) и с ID контента. В настоящее время не используется |
| content_FOLDER_ACCESS_PermLevel_parent_folder | Комбинирует информацию о доступе к папкам библиотеки контента ([CONTENT_FOLDER_ACCESS](access.md#таблица-content_folder_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)), с ID контента и с ID родительской папки. В настоящее время не используется |
| CONTENT_FORMAT | Комбинирование информации контентов и PublishingContainer. Не используется в QP8 |
| CONTENT_GROUP_NAME | Не используется |
| CONTENT_ITEM_ACCESS_PermLevel | Комбинирует информацию о доступе к статьям ([CONTENT_ITEM_ACCESS](access.md#таблица-content_item_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Доступно во всех типах подерживаемых БД |
| CONTENT_ITEM_ACCESS_PermLevel_content | Комбинирует информацию о доступе к статьям ([CONTENT_ITEM_ACCESS](access.md#таблица-content_item_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)) и с ID контента. Доступно во всех типах подерживаемых БД |
| content_item_group_access | Не используется |
| content_item_workflow | Предоставляет информацию об актуальном Workflow для статей. Доступно во всех типах подерживаемых БД |
| content_link | Альтернативное представление информации из [таблицы CONTENT_TO_CONTENT](main.md#таблица-content_to_content). Не реализовано в Postgres |
| ENTITY_TYPE_ACCESS_PERMLEVEL | Комбинирует информацию о доступе к типам сущностей ([ENTITY_TYPE_ACCESS](access.md#таблица-entity_type_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Доступно во всех типах подерживаемых БД |
| FOLDER_ACCESS_PermLevel | Комбинирует информацию о доступе к папкам библиотеки сайта ([таблица FOLDER_ACCESS](access.md#таблица-folder_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Доступно во всех типах подерживаемых БД |
| FOLDER_ACCESS_PermLevel_parent_folder | Комбинирует информацию о доступе к папкам библиотеки сайта ([таблица FOLDER_ACCESS](access.md#таблица-folder_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)), с ID сайта и с ID родительской папки. Не реализовано в Postgres |
| FOLDER_ACCESS_PermLevel_site | Комбинирует информацию о доступе к папкам библиотеки сайта ([таблица FOLDER_ACCESS](access.md#таблица-folder_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)) и с ID сайта. Доступно во всех типах подерживаемых БД |
| full_workflow_rules | Дополняет [таблицу WORKFLOW_RULES](extra.md#таблица-workflow_rules) неявным статусом None. Доступно во всех типах подерживаемых БД |
| object_attributes | Комбинирует PublishingContainer с полями контента, которые могут в нем применяться. Используется для VisualStudioAdd-In в сайтах, построенных на шаблонах QP. |
| objects_status | Комбинирует PublishingContainer со статусами, которые могут в нем применяться. Используется для VisualStudioAdd-In в сайтах, построенных на шаблонах QP. |
| PAGE_OBJECT | Комбинирует таблицы OBJECT и OBJECT_TYPE, отфильтровывая объекты страниц. Используется в сайтах, построенных на шаблонах QP |
| qp_test_view | Не используется |
| qp_ver | Дополняет версии бэкенда столбцом с весом каждой версии. Не используется в QP8 |
| SITE_ACCESS_PermLevel | Комбинирует информацию о доступе к сайтам ([таблица SITE_ACCESS](access.md#таблица-access.md#таблица-site_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Доступно во всех типах подерживаемых БД |
| site_content_item_modified | Список ID статей для сайта вместе с датой модификации. Не реализовано в Postgres |
| site_content_link | Альтернативное представление информации из [таблицы CONTENT_TO_CONTENT](main.md#таблица-content_to_content), дополненнное ID сайта. Не реализовано в Postgres |
| site_item_link | Альтернативное представление информации из [таблицы ITEM_TO_ITEM](main.md#таблица-item_to_item), дополненнное ID сайта. Не реализовано в Postgres |
| site_union_attrs | Дополняет таблицу union_attrs информацией о текущем сайте. Не реализовано в Postgres |
| site_union_contents | Дополняет таблицу union_contents информацией о текущем сайте. Не реализовано в Postgres |
| site_user_query_contents | Дополняет таблицу user_query_contents информацией о текущем сайте. Не реализовано в Postgres |
| status_type_new | Альтернативное представление информации из [таблицы STATUS_TYPE](extra.md#таблица-status_type). Используется в EF и EF.Core. Доступно во всех типах поддерживаемых БД |
| TAB_ACCESS_PermLevel | Комбинирует информацию о доступе ко вкладкам ([таблица TAB_ACCESS](access.md#таблица-tab_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Не используется в QP8 |
| TEMPLATE_OBJECT | Комбинирует таблицы OBJECT и OBJECT_TYPE, отфильтровывая объекты шаблонов. Используется в сайтах, построенных на шаблонах QP. Доступно во всех типах подерживаемых БД |
| user_new | Альтернативное представление информации из [таблицы USER](access.md#таблица-user). Используется в EF и EF.Core. Доступно во всех типах поддерживаемых БД |
| user_group_new | Альтернативное представление информации из [таблицы USER_GROUP](access.md#таблица-user_group). Используется в EF и EF.Core. Доступно во всех типах поддерживаемых БД |
| user_group_bind_new | Альтернативное представление информации из [таблицы USER_GROUP_BIND](access.md#таблица-user_group_bind). Используется в EF и EF.Core. Доступно во всех типах поддерживаемых БД |
| user_group_bind_recursive | Альтернативное представление информации из [таблицы USER_GROUP_BIND](access.md#таблица-user_group_bind), в котором иерархия групп разворачивается в плоский список. Доступно во всех типах поддерживаемых БД |
| USER_GROUP_TREE | Объединяет таблицы [USER_GROUP](access.md#таблица-user_group) и [GROUP_TO_GROUP](access.md#таблица-group_to_group) для показа дерева групп. Доступно во всех типах подерживаемых БД |
| V_USER_QUERY_ATTRS | Выдает информацию о полях-источниках для виртуальных контентов типа USERQUERY. Доступно во всех типах подерживаемых БД |
| VIRTUAL_ATTR_BASE_ATTR_RELATION | Выдает информацию о полях-источниках для виртуальных контентов любых типов. Доступно во всех типах подерживаемых БД |
| VIRTUAL_CONTENT_RELATION | Выдает информацию о контентах-источниках для виртуальных контентов. Доступно во всех типах подерживаемых БД |
| VW_ACTION_STATUS_VIRTUAL | Не используется |
| VW_ACTION_TOOLBAR_BUTTON_VIRTUAL | Не используется |
| VW_ENTITY_TYPE | Не используется |
| VW_TREE_NODE_VIRTUAL | Не используется |
| WORKFLOW_ACCESS_PermLevel | Комбинирует информацию о доступе к Workflow ([таблица WORKFLOW_ACCESS](access.md#таблица-workflow_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)). Доступно во всех типах подерживаемых БД |
| WORKFLOW_ACCESS_PermLevel_site | Комбинирует информацию о доступе к Workflow ([таблица WORKFLOW_ACCESS](access.md#таблица-workflow_access)) с уровнем доступа ([таблица PERMISSION_LEVEL](access.md#таблица-permission_level)) и с ID сайта. Доступно во всех типах подерживаемых БД |
|  workflow_max_statuses | Выдает информацию о максимальном статусе для каждого Workflow. Доступно во всех типах подерживаемых БД |
