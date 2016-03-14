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

    //#region Расширение BackendDocumentContext (Работа с полями)
    Quantumart.QP8.BackendDocumentContext.prototype.toggleField = function (editor, inputName, state, preserveHidden) {
        var $elem = jQuery(editor._formElement).find("dl[data-field_form_name='" + inputName + "']");
        var list = $elem.find(".dataList[data-list_item_name='" + inputName + "']").data("entity_data_list_component");
        if (state) {
            $elem.show();
            if (list) {
                list._fixListOverflow();
            }
        }
        else {
            $elem.hide();
            if (!preserveHidden) {
                if (list) {
                    list.selectEntities();
                }
                else {
                    $elem.val("");
                }
            }
        }
    };

    Quantumart.QP8.BackendDocumentContext.prototype.toggleFields = function (editor, inputNames, state) {
        $.each(inputNames, $.proxy(function (i, inputName) {
            this.toggleField(editor, inputName, state, true);
        }, this));
    };

    Quantumart.QP8.BackendDocumentContext.prototype.getValue = function (editor, inputName) {
        return jQuery(editor._formElement).find("[name='" + inputName + "']").val();
    };

    Quantumart.QP8.BackendDocumentContext.prototype.getBooleanValue = function (editor, inputName) {
        var values = $.grep($c.getAllBooleanValues(editor._formElement), function (e) { return e.fieldName == inputName; });
        if (values.length > 0) {
            return values[0].value;
        }
    };

    Quantumart.QP8.BackendDocumentContext.prototype.setValue = function (editor, fieldValues, disableChangeTracking) {
        var $form = editor._formElement;

        //$c.setAllVisualEditorValues($form, fieldValues);
        $c.setAllSimpleTextBoxValues($form, fieldValues);
        $c.setAllBooleanValues($form, fieldValues);
        $c.setAllNumericBoxValues($form, fieldValues);
        $c.setAllDateTimePickersValues($form, fieldValues);
        $c.setAllRadioListValues($form, fieldValues);
        $c.setAllAggregationListValues($form, fieldValues);
        $c.setAllClassifierFieldValues($form, fieldValues, disableChangeTracking);
        $c.setAllHighlightedTextAreaValues($form, fieldValues);
    };

    Quantumart.QP8.BackendDocumentContext.prototype.getParentContentId = function (editor) {
        var $form = jQuery(editor._formElement);
        return (!this.typeResolverInputName) ? 0 : $form.find("[name='" + this.typeResolverInputName + "']").parent(".singleItemPicker").data("parent_entity_id");
    };

    Quantumart.QP8.BackendDocumentContext.prototype.getLinkedContentId = function (editor, name) {
        var $form = jQuery(editor._formElement);
        return $form.find("[name='" + name + "']").parent(".singleItemPicker").data("parent_entity_id");
    };

    Quantumart.QP8.BackendDocumentContext.prototype.getReadonlyStatus = function (editor, fieldName) {
        /// <summary>
        /// Возвращает статус заблокированности поля. 
        /// работает только с Textbox
        /// </summary>
        var $tboxes = $c.getAllSimpleTextBox(editor._formElement);

        var $tb = $tboxes.filter('[name="' + fieldName + '"]').first();
        if ($tb.length > 0) {
            return $tb.prop("readonly") || $tb.attr("readonly");
        }
    };

    //#endregion

    //#region Расширение BackendDocumentContext (Изменение текста и иконки кнопки)

    Quantumart.QP8.BackendDocumentContext.prototype.changeCustomLinkButton = function (editor, fieldName, suffix, options) {
        // options {url: "", title: ""}
        // todo: скрытие, блокировка кнопки
        var inputId = jQuery(editor._formElement).find("[name='" + fieldName + "']").prop("id");
        var id = inputId + "_" + suffix;

        var $btn = $("#" + id);

        if ($btn.length > 0) {
            if (options.title) {
                $btn.find(".text").html(options.title);
            }

            if (options.url) {
                $btn.find(".icon").css({ "background-image": 'url(' + options.url + ')' });
            }
        }
    };

    //#endregion

    //#region Расширение Quantumart.QP8.ControlHelpers (Работа с полями форм)

    // Readonly текстовых полей
    Quantumart.QP8.ControlHelpers.unlockSimpleTextBoxes = function (parentElement, fieldNames) {
        if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
            var $tboxes = $c.getAllSimpleTextBox(parentElement);
            jQuery(fieldNames).each(function (i, name) {
                var $tb = $tboxes.filter('[name="' + name + '"]').first();
                if ($tb.length > 0) {
                    $tb.prop("readonly", false).removeClass("readonly");
                }
            });
        }
    };

    // заблокирвоать все поля из списка
    Quantumart.QP8.ControlHelpers.makeReadonlyAllFields = function (parentElement, fieldNames) {
        //$c.makeReadonlyVisualEditors(parentElement, fieldNames);
        $c.makeReadonlySimpleTextBoxes(parentElement, fieldNames);
        $c.makeReadonlyBooleans(parentElement, fieldNames);
        $c.makeReadonlyNumericBox(parentElement, fieldNames);
        $c.makeReadonlyDateTimePickers(parentElement, fieldNames);
        $c.makeReadonlyFileFields(parentElement, fieldNames);
        $c.makeReadonlyRadioList(parentElement, fieldNames);
    };

    //#endregion

    //#region Расширение BackendDocumentContext (Кнопка блокировки/разблокировки полей)

    Quantumart.QP8.BackendDocumentContext.prototype.initToggleBlockButtons = function (options, editor) {
        // инициализация во время события загрузки формы
        if (options) {
            var caption = {};
            var isLocked = this.getReadonlyStatus(editor, options.fieldName);

            if (isLocked === true) {
                caption.title = options.unlockTitle;
                caption.url = options.unlockIcon;

                this.addCustomLinkButton({
                    name: options.fieldName,
                    title: caption.title,
                    suffix: "togglelock",
                    "class": "customLinkButton",
                    url: caption.url,
                    onClick: $.proxy(function (options, editor) {
                        var isLocked = this.getReadonlyStatus(editor, options.fieldName);
                        if (isLocked === true) {
                            // unlock
                            $c.unlockSimpleTextBoxes(editor._formElement, [options.fieldName]);
                            this.changeCustomLinkButton(editor, options.fieldName, "togglelock", {
                                title: options.lockTitle,
                                url: options.lockIcon
                            });
                        } else {
                            // lock
                            $c.makeReadonlySimpleTextBoxes(editor._formElement, [options.fieldName]);
                            this.changeCustomLinkButton(editor, options.fieldName, "togglelock", {
                                title: options.unlockTitle,
                                url: options.unlockIcon
                            });
                        }
                    }, this, options, editor)
                });
            }
        }
    };

    Quantumart.QP8.BackendDocumentContext.prototype.addToggleBlockButton = function (fieldName, options) {
        var defaults = {
            unlockIcon: "/Backend/Content/QP8/icons/16x16/unlock.gif",
            lockIcon: "/Backend/Content/QP8/icons/16x16/lock.gif",
            unlockTitle: "Unlock",
            lockTitle: "Lock",
            fieldName: fieldName
        };

        /* Поддерживаются типы полей:
         * * Texbox
         */

        var opts = $.extend(defaults, options);

        this.addInitHandler($.proxy(this.initToggleBlockButtons, this, opts));
    };

    //#endregion

    //#region Настройки виджетной системы

    Quantumart.QP8.BackendDocumentContext.prototype.checkWidgetSystemFields = function (options, editor) {
        // проверяем, какие поля надо скрыть
        var itemType = this.getValue(editor, options.fields.discriminator);
        var isPageFieldValue = this.getBooleanValue(editor, options.fields.isPage);
        var itemDefinitionContentId = this.getLinkedContentId(editor, options.fields.discriminator);

        if (itemType > 0) {
            // выбран тип сущности

            var isPage = $o.getArticleFieldValue(itemDefinitionContentId, options.system.isPageFieldName, itemType);

            this.toggleField(editor, options.fields.isPage, false, true);

            if (isPage != null && isPageFieldValue != isPage) {
                this.setValue(editor, [{ fieldName: options.fields.isPage, value: (isPage == true).toString() }]);
            }

            if (isPage == true) {
                // скрывается набор полей для виджетов
                this.toggleFields(editor, options.widgetsFields, false);
                // отображается набор полей для страниц
                this.toggleFields(editor, options.pagesFields, true);

            } else {
                // скрывается набор полей для страниц
                this.toggleFields(editor, options.pagesFields, false);
                // отображается набор полей для виджетов            
                this.toggleFields(editor, options.widgetsFields, true);
            }

            var extensionId = $o.getArticleFieldValue(itemDefinitionContentId, options.system.preferredContentIdName, itemType);
            var currentExtensionIdValue = this.getValue(editor, options.fields.extensionId);

            if (currentExtensionIdValue != extensionId) {
                Sys.Debug.trace("У статьи неправильный контент-расширение. Ожидается " + extensionId + " на самом деле: " + currentExtensionIdValue);
            } else {
                // блокируется поле extensionId
                $c.makeReadonlyClassifierFields(editor._formElement, [options.fields.extensionId]);
            }

            if (extensionId > 0) {
                if ((currentExtensionIdValue == 0 || editor._acionCode == ACTION_CODE_ADD_NEW_ARTICLE) && currentExtensionIdValue != extensionId) {
                    // если не выставлено значение, то выставляется
                    this.setValue(editor, [{ fieldName: options.fields.extensionId, value: extensionId }]);
                    $c.makeReadonlyClassifierFields(editor._formElement, [options.fields.extensionId]);
                }
            } else {
                // скрывается поле extensionId
                this.toggleField(editor, options.fields.extensionId, false, true);
            }
        }
    };


    Quantumart.QP8.BackendDocumentContext.prototype.initWidgetSystem = function (opts) {

        // настройки
        var options = $.extend({
            fields: {
                name: "", //"field_0000"
                parent: "",
                isPage: "",
                extensionId: "",
                discriminator: ""
            },
            // поля, которые должны отображаться только для виджетов
            widgetsFields: [/*"field_0000",*/],
            // поля, которые должны отображаться только для страниц
            pagesFields: [/*"field_0000",*/],
            system: {
                isPageFieldName: "IsPage",
                preferredContentIdName: "PreferredContentId"
            }
        }, opts || {});

        this.addInitHandler($.proxy(this.checkWidgetSystemFields, this, options));

        this.addValueChangedHandler($.proxy(this.checkWidgetSystemFields, this, options));
    };

    //#endregion
})();