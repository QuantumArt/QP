
/* 
USAGE

var options = new QA.BackendAPI.EntitiesPicker.Options(308);

// options.filter = "c.id in (1,2,3) and c.visible= 1"

options.isMultipleChoice = false;
options.onSelected = function (name, args) {
    var id = args.selectedItems[0].Id;
    var name = $o.getEntityName("article", id)
};
options.onClosed = function (name, args) {
   
};

var component = new QA.BackendAPI.EntitiesPicker.QPEntitiesPicker(options);
component.openDialog();

*/

var QA = QA || { BackendAPI: {} };

//#region EntitiesPicker
QA.BackendAPI.EntitiesPicker = QA.BackendAPI.EntitiesPicker || (function () {
    // #region опции
    Options = function (contentId) {
        this.contentId = contentId;
        this.onSelected = function (name, args) {
            if (console && console.log) {
                console.log(name);
                console.log(args);
            }
        };
        this.onClosed = function (name, args) {
            if (console && console.log) {
                console.log(name);
                console.log(args);
            }
        };
        this.selectedItems = [];
    };
    Options.prototype = {
        contentId: null,
        isMultipleChoice: true,
        selectedItems: null,
        onSelected: null,
        onClosed: null,
        filter: null
    };
    OnSelectedEventArgs = function () { };
    OnSelectedEventArgs.prototype = {
        isSelected: false,
        selectedItems: null
    };

    // #endregion

    // псевдо-класс компонета (конструктор)
    var QPEntitiesPicker = function (options) {
        this.options = options;
        var eventArgs = new Quantumart.QP8.BackendEventArgs();
        eventArgs.set_isMultipleEntities(options.isMultipleChoice);
        eventArgs.set_parentEntityId(options.contentId); // id контента
        eventArgs.set_entityTypeCode("article");

        if (options.isMultipleChoice == true) {
            eventArgs.set_actionCode("multiple_select_article");
        } else {
            eventArgs.set_actionCode("select_article");
        }

        var entities = options.selectedItems;
        if (entities.length > 0) {
            if (options.isMultipleChoice) {
                eventArgs.set_entities(entities);
            }
            else {
                eventArgs.set_entityId(entities[0].Id);
                eventArgs.set_entityName(entities[0].Name);
            }
        }

        this.component = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, {
            filter: options.filter || "c.archive = 0"
        });

        eventArgs = null;
    }

    QPEntitiesPicker.prototype = {
        options: null,
        component: null,
        _onSelectedInternal: function (args0, args1, args2) {
            // destroy component
            this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
            this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
            this.component.closeWindow();
            this.component.dispose();

            // callback
            if (this.options.onSelected) {
                var args = new OnSelectedEventArgs();
                args.isSelected = true;
                args.selectedItems = args2.entities;
                try {
                    this.options.onSelected(args0, args);
                } finally {
                    this.options = null;
                }
            }
        },
        _onClosedInternal: function (args0, args1, args2) {
            // destroy component
            this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
            this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
            this.component.dispose();

            // callback
            if (this.options.onClosed) {
                var args = new OnSelectedEventArgs();
                args.isSelected = false;
                try {
                    this.options.onClosed(args0, args);
                } finally {
                    this.options = null;
                }
            }
        },
        openDialog: function () {
            this.component.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED,
				jQuery.proxy(this._onSelectedInternal, this));

            this.component.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED,
				jQuery.proxy(this._onClosedInternal, this));

            this.component.openWindow();
        }
    };

    return {
        Options: Options,
        QPEntitiesPicker: QPEntitiesPicker
    };
})();

//#endregion

(function () {
    //#region Расширение BackendDocumentContext (Регистрация и выполнение обработчиков событий формы)
    Quantumart.QP8.BackendDocumentContext.prototype.addInitHandler = function (callback) {
        if (typeof (callback) == "function") {
            if (!Array.isArray(this._initHandlerCallbacks)) {
                this._initHandlerCallbacks = [];
            }
            this._initHandlerCallbacks.push(callback);
        }
    };


    Quantumart.QP8.BackendDocumentContext.prototype.addValueChangedHandler = function (callback) {
        if (typeof (callback) == "function") {
            if (!Array.isArray(this._fieldValueChangedHandlerCallbacks)) {
                this._fieldValueChangedHandlerCallbacks = [];
            }
            this._fieldValueChangedHandlerCallbacks.push(callback);
        }
    };

    Quantumart.QP8.BackendDocumentContext.prototype.fieldValueChangedHandler = function (editor, data, $rootElem) {
        // выполняем зарегистрированные обработчики
        if (Array.isArray(this._fieldValueChangedHandlerCallbacks)) {
            $.each(this._fieldValueChangedHandlerCallbacks, function (i, handler) {
                if (typeof (handler) == "function") {
                    handler(editor);
                }
            });
        }
    };

    Quantumart.QP8.BackendDocumentContext.prototype.initHandler = function (editor, $elem) {
        Sys.Debug.trace("form initializer");
        // выполняем зарегистрированные обработчики
        if (Array.isArray(this._initHandlerCallbacks)) {
            $.each(this._initHandlerCallbacks, function (i, handler) {
                if (typeof (handler) == "function") {
                    Sys.Debug.trace("execute init handler");
                    handler(editor);
                }
            });
        }
    };


    //#endregion


    Quantumart.QP8.BackendDocumentContext.prototype.getValue = function (editor, inputName) {
        return jQuery(editor._formElement).find("[name='" + inputName + "']").val();
    };

    Quantumart.QP8.BackendDocumentContext.prototype.getBooleanValue = function (editor, inputName) {
        var values = $.grep($c.getAllBooleanValues(editor._formElement), function (e) { return e.fieldName == inputName; });
        if (values.length > 0) {
            return values[0].value;
        }
    };

    Quantumart.QP8.BackendDocumentContext.prototype.addShowRelatedArticlesButton = function (fieldName, options) {
        var defaults = {
            icon: "/Backend/Content/QP8/icons/16x16/version.gif",
            contentId: 0,
            title: "Показать историю заказа",
            filterBy: null,
            // имя поля, значение которого используется в фильтрации
            // в текущей версии не реализовано.
            filterValue: null
        };

        defaults.fieldName = fieldName;

        var opts = $.extend(defaults, options);

        if (!(opts.contentId > 0)) {
            Sys.Debug.trace("Valid virtual content Id should be supplied. Fill 'contentId' property.");
            return;
        }

        if (!opts.filterBy || opts.filterBy == "") {
            Sys.Debug.trace("Fill 'filterBy' property.");
            return;
        }

        this.addInitHandler($.proxy(this.initShowRelatedArticlesButton, this, opts));
    };


    Quantumart.QP8.BackendDocumentContext.prototype.initShowRelatedArticlesButton = function (options, editor) {

        Sys.Debug.trace("call initShowRelatedArticlesButton");

        var articleId = editor.get_entityId();

        var contentId = options.contentId;

        if (articleId && articleId > 0) {
            this.addCustomLinkButton({
                name: options.fieldName,
                title: options.title,
                suffix: "ShowRelatedArticles",
                "class": "customLinkButton",
                url: options.icon,
                onClick: $.proxy(function (options, editor) {
                    var pickerOptions = new QA.BackendAPI.EntitiesPicker.Options(contentId);
                    pickerOptions.filter = "c.visible = 1 and c.archive = 0";
                    if (articleId)
                        pickerOptions.filter += " and c." + options.filterBy + " = " + articleId;

                    pickerOptions.isMultipleChoice = false;

                    var component = new QA.BackendAPI.EntitiesPicker.QPEntitiesPicker(pickerOptions);
                    component.openDialog();

                }, this, options, editor)
            });
        }
    };

})();