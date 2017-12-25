### ActionLogFilterBase.js
* Заменил `$q.defineAbstractMethods` на типизированные объявления методов (!)

### Backend.js
* Убрал использование `$q.bindProxies` (!)  
  (иначе переменыне вида `_fooHandler` остаются необъявленными)

### BackendActionExecutor.js
* `$.Deferred()` - не конструктор, а фабрика
* Лишний аргумент `errorThrown` в вызове `errorCallback`
* Объявление как константа (для навигации по коду)
* Приведение типов

### BackendActionPermissionTree.js
* Приведение типов

### BackendBaseUploader.js
* Заменил `$q.defineAbstractMethods` на типизированные объявления методов (!)

### BackendBreadCrumbs.js
* Убрал использование `$q.bindProxies` (!)
* Добавил в прототип поле `_contextMenuComponent`

### BackendBreadCrumbsManager.js
* Лишние аргументы в вызове `super`

### BackendEntityGrid.js
* Убрал использование `$q.bindProxies` (!)

### BackendEntityDataListBase.js
* `Quantumart.QP8.Enums.DataListType` - переделал в plain object
* Лишний аргумент `this._listWrapperElement` в вызове `$(...).unwrap`
* Добавил типизированные объявления абстрактных методов

### BackendDireclLinkExecutor.js
* `$.Deferred()` - не конструктор, а фабрика

### BackendDocumentContext.js
* Исправил опечатку `loadHanlder` => `loadHandler` (!)

### BackendDocumentHost.js
* Заменил `$q.defineAbstractMethods` на типизированные объявления методов (!)
* Добавил пропущенные абстрактные методы (!)
* Добавил определения виртуальных полей (задаваемых в классах-наследниках) (!)
* Лишний аргумент `eventArgs` в вызове `onDataBinding`
* Лишние аргументы в вызове `onSearchViewToolbarButtonClicked`
* Лишние аргументы в вызове `onContextViewToolbarButtonClicked`
* Лишний аргумент `hideLoadingLayer` в вызове `this.hideLoadingLayer`
* Неиспользуемый аргумент `hideLoadingLayer` в методе `this.unmarkPanelsAsBusy`
* Приведение типов

### BackendEditingArea.js
* Приведение типов

### BackendEditorsAutoSaver.js
* `$.Deferred()` - не конструктор, а фабрика

### BackendEntityEditor.js
* Лишний аргумент `$form` в вызове `_disposeContextModelField`
* Лишний аргумент `$form` в вызове `_disposeVariationsModelField`
* Лишний аргумент `$form` в вызове `_disposeErrorModelField`
* Приведение типов

### BackendEntityGrid.js
* Приведение типов
* Заменил ES6 `Set` на `Array.distinct` (!)

### BackendEntityObject.js
* Объявление как константа (для навигации по коду)
* Приведение типов

### BackendEntityTree.js
* Убрал использование `$q.bindProxies` (!)

### BackendEntityTreeManager.js
* Неиспользуемый аргумент `options` в сигнатуре `getInstance`

### BackendEventArgs.js
* Удалил вызов `Object.getType().inheritsFrom()` так как вызов `Type.isInstanceOfType` (!)  
  уже содержит в себе вызов `Type.inheritsFrom`

### BackendFieldSearch.js
* Убрал использование `$q.bindProxies` (!)

### BackendImageCropResizeClient.js
* `$.ajax().success` (deprecated) => `$.ajax().done`
* Приведение типов

### BackendMultistepActionWindow.js
* Приведение типов

### MultistepActionExportSettings.js
* Заменил spread на `Array.from` для `querySelectorAll` (!)

### BackendMultistepImportSettings.js
* `$q.alert` отсутствует - заменил на `$q.alertSuccess` (!)

### BackendPopupWindow.js
* Приведение типов

### BackendPopupWindowManager.js
* Неиспользуемый аргумент `options` в сигнатуре `getInstance`

### BackendProgressBar.jquery.js
* `settings.total()` => `settings.total` (это число, а не метод) (!)
* Неиспользуемый аргумент `dfr` в сигнатуре `setTotal`
* Лишний аргумент `dfr` в вызове `setValue`
* `BackendProgressBarComponent` - не конструктор, а фабрика

### BackendSearchBlockBase.js
* Добавил абстрактный метод `getSearchQuery`
* Приведение типов

### BackendSettingsPopupWindow.js
* Перенес вызов базового конструктора в начало метода (!)

### BackendTabStrip.js
* Убрал использование `$q.bindProxies` (!)
* Приведение типов
* Добавил поле `isExpandRequested` в класс `BackendTabEventArgs`

### BackendTemplateObjectProperties.js
* Приведение типов

### BackendTextAreaEditor.js
* Убрал ошибочный вызов `$.proxy` вокруг вызова `this._insertLibraryTag(url)` (!)
* Лишний аргумент `tArea` в вызове `this.getMode`
* Лишний аргумент `cm` в вызове `this.initTemplateToolbar`
* Форматирование кода
* `_onInsertCall()` - заменил проверку на пустое значение для `valToInsert` (!)  
  (Александр допустил ошибку при рефакторинге, заменив `==` на `===`) 

### BackendTreeBase.js
* Убрал использование `$q.bindProxies` (!)
* Исправил объявления абстрактных методов (!)
* Добавил типизацию для виртуальных методов

### BackendTreeMenu.js
* Убрал использование `$q.bindProxies` (!)

### BackendTreeMenuContextMenuManager.js
* Неиспользуемый аргумент `options` в сигнатуре `getInstance`

### BackendVirtualFieldTree.js
* Переписал конструктор: убрал `[,,,,, options]`

### BackendVisualEditor.js
* Замена `window.CKEDITOR` на `CKEDITOR`

### BackendWorkflowEditor.js
* `this` должен передаваться вторым аргументом в `jQuery.proxy` а не в `$(...).each` (!)

### ClassifierFieldSearch.js
* Убрал лишний параметр `searchType` из вызова конструктора `FieldSearchBase` (!
* Убрал вызов отсутствующего метода `this._onLoadHandler` (!)

### ControlHelpers.js
* Объявление как класс (для навигации по коду)
* Лишний аргумент в вызове `$c.saveWorkflowData`

### FieldSearchBase.js
* Заменил ES6 `Set` на `Array.distinct` (!)

### jQueryExtensions.js
* Добавлены названия CSS Colors (!)
* Проверка ESLint
* Добавлены сигнатуры методов для TypeScript
* Приведение типов
* Переместил из `vendors.js` в `app.js`

### RelationFieldSearch.js
* Добавил в прототип определения методов  (!)
  `_onLoadHandler`, `_onSelectorChangeHandler`, `_onListContentChangedHandler`
* Заменил ES6 `Set` на `Array.distinct` (!)

### Utils.js
* Объявление как класс (для навигации по коду)
* Лишний аргумент в вызове `console.groupEnd()`
* `Window.status` (строка состояния браузера) => `jqXHR.status` (HTTP статус-код) (!)
* Приведение типов

### vanilla.helpers.js
* Убрал неиспользуемый namespace `window.Global`
* Объявление `Url` как константы (для навигации по коду)

### ckeditor/codemirror/plugin.js
* Необъявленные переменные `i, len`
