Quantumart.QP8.BackendDocumentContext.setGlobal("typeIds", {
    "tariff": $o.getArticleIdByFieldValue(349, "Title", "Тариф"),
    "service": $o.getArticleIdByFieldValue(349, "Title", "Услуга"),
    "action": $o.getArticleIdByFieldValue(349, "Title", "Акция"),
    "serviceOnTariff": $o.getArticleIdByFieldValue(368, "Title", "Услуги на тарифе"),
    "mutualGroup": $o.getArticleIdByFieldValue(368, "Title", "Группы несовместимости услуг"),
    "optionPackage": $o.getArticleIdByFieldValue(368, "Title", "Пакеты опций на тарифах")
});

Quantumart.QP8.BackendDocumentContext.setGlobal("typesByProduct", {
    "343": $ctx.getGlobal("typeIds")["tariff"], "385": $ctx.getGlobal("typeIds")["tariff"],
    "403": $ctx.getGlobal("typeIds")["service"], "402": $ctx.getGlobal("typeIds")["service"],
    "419": $ctx.getGlobal("typeIds")["action"], "420": $ctx.getGlobal("typeIds")["action"],
    "404": $ctx.getGlobal("typeIds")["serviceOnTariff"],
    "407": $ctx.getGlobal("typeIds")["optionPackage"],
    "365": $ctx.getGlobal("typeIds")["mutualGroup"]
});

Quantumart.QP8.BackendDocumentContext.setGlobal("regionalLibraryFolders", [
    { folder: "center", macroRegionId: 338941 },
    { folder: "dv", macroRegionId: 338940 },
    { folder: "msk", macroRegionId: 338946 },
    { folder: "psz", macroRegionId: 338944 },
    { folder: "puv", macroRegionId: 338943 },
    { folder: "sibir", macroRegionId: 338945 },
    { folder: "south", macroRegionId: 338939 },
    { folder: "sz", macroRegionId: 338942 },
    { folder: "ural", macroRegionId: 338947 }
]);

Quantumart.QP8.BackendDocumentContext.prototype.getTypeByProduct = function (product) {
    return this.getGlobal("typesByProduct")[product];
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolverNames = function () {
    if (jQuery.isArray(this.resolverNames))
        return this.resolverNames
    else
        return ["Type"];
}

Quantumart.QP8.BackendDocumentContext.prototype.initHandler = function (editor, $elem) {

    editor._hideFields = jQuery.isArray(editor._hideFields) ? Array.concat(editor._hideFields, this.fieldsToHide) : this.fieldsToHide;

    if (this.changeFilters) {
        var resolverNames = this.getResolverNames();
        var self = this;
        jQuery.each(resolverNames, function (i, resolverName) {
            self.changeFilters(editor, self.getCurrentResolver(editor, resolverName), $elem, self.getFiltersNameProp(resolverName));
        });
    }

    if (this.changeFieldState) {
        this.changeFieldState(editor, null, $elem);
    }
};

Quantumart.QP8.BackendDocumentContext.prototype.fieldValueChangedHandler = function (editor, data) {
    if (this.changeFilters) {
        var resolverNames = this.getResolverNames();
        var self = this;
        jQuery.each(resolverNames, function (i, resolverName) {
            var prop = self.getResolverInputNameProp(resolverName)
            if (prop && data.fieldName == self[prop]) {
                self.changeFilters(editor, self.getCurrentResolver(editor, resolverName), null, self.getFiltersNameProp(resolverName));
            }
        });
    }
    if (this.changeFieldState) {
        this.changeFieldState(editor, data, null);
    }

};

Quantumart.QP8.BackendDocumentContext.prototype.setFilter = function (inputName, value, fieldName, $form) {
  var filter = '';
  if (value) {
    filter = (fieldName) ? 'c.[' + fieldName + '] in (' + value + ')' : 'c.content_item_id in (select linked_item_id from item_link where item_id in (' + value + '))';
  }

  this.applyFilter(inputName, filter, $form);
};

Quantumart.QP8.BackendDocumentContext.prototype.applyFilter = function (inputName, filter, $form) {
  var list = $form.find(".dataList[data-list_item_name='" + inputName + "']").data("entity_data_list_component");
  if (list) {
    list.applyFilter(filter);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.getCurrentResolver = function (editor, name) {
  var $form = jQuery(editor._formElement);
  var prop = this.getResolverInputNameProp(name);
  return (!prop || !this[prop]) ? 0 : this.getResolver(editor, $form.find("[name='" + this[prop] + "']").val(), name);
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolverInputNameProp = function (name) {
  return name.charAt(0).toLowerCase() + name.slice(1) + "ResolverInputName";
};

Quantumart.QP8.BackendDocumentContext.prototype.getFiltersNameProp = function (name) {
  return name.charAt(0).toLowerCase() + name.slice(1) + "Filters";
};

Quantumart.QP8.BackendDocumentContext.prototype.getParentResolverFieldNameProp = function (name) {
  return 'parent' + name + 'FieldName';
};

Quantumart.QP8.BackendDocumentContext.prototype.getParentClassifierFieldNameProp = function (name) {
  return 'parent' + name + 'ClassifierFieldName';
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolverContentId = function (editor, name) {
    var $form = jQuery(editor._formElement);
    var prop = this.getResolverInputNameProp(name);
    return (!prop || !this[prop]) ? 0 : $form.find("[name='" + this[prop] + "']").parent(".singleItemPicker").data("parent_entity_id");
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolver = function (editor, val, name) {
    var prod = 0;
    var parentProp = this.getParentResolverFieldNameProp(name);
    if (parentProp && this[parentProp]) {
        var classifierProp = this.getParentClassifierFieldNameProp(name);
        if (classifierProp && this[classifierProp] && (+val)) {
            var classifier = $o.getArticleFieldValue(this.getResolverContentId(editor, name), this[classifierProp], val);
            var extension = $o.getArticleIdByFieldValue(classifier, "Parent", val);
            prod = $o.getArticleFieldValue(classifier, this[parentProp], extension);
        }
        else
            if (!+val)
                prod = 0;
            else if (+this[parentProp])
                prod = $o.getArticleLinkedItems(+this[parentProp], val);
            else
                prod = $o.getArticleFieldValue(this.getResolverContentId(editor, name), this[parentProp], val);
    }
    else {
        var $form = jQuery(editor._formElement);
        var prop = this.getResolverInputNameProp(name);
        prod = $form.find("[name='" + this[prop] + "']").val();
    }
    if (name == "Type")
        return this.getTypeByProduct(prod);
    else
        return prod;
};

Quantumart.QP8.BackendDocumentContext.prototype.changeFilters = function (editor, value, $elem, filterProp) {
  var $form = $elem || jQuery(editor._formElement);
  if (this[filterProp]) {
    for (var key in this[filterProp]) {
      this.setFilter(key, value, this[filterProp][key], $form);
    }
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.wrapFields = function (editor, inputNames, title) {
  var $form = jQuery(editor._formElement);
  var multiSelector = jQuery.map(inputNames, function (val) { return "dl[data-field_form_name='" + val + "']"; }).join(", ")
  $form.find(multiSelector).wrapAll("<fieldset />")
  $form.find("dl[data-field_form_name='" + inputNames[0] + "']").closest("fieldset").append("<legend>" + title + "</legend>");
};

Quantumart.QP8.BackendDocumentContext.prototype.toggleField = function (editor, inputName, state, preserveHidden) {
    var $elem = jQuery(editor._formElement).find("dl[data-field_form_name='" + inputName + "']");
    var list = $elem.find(".dataList[data-list_item_name='" + inputName + "']").data("entity_data_list_component");
    if (state) {
      $elem.show();
      if (list) {
        list._fixListOverflow();
      }
    } else {
      $elem.hide();
      if (!preserveHidden) {
        if (list) {
          list.selectEntities();
        } else {
          $elem.val("");
        }
      }
    }
};

Quantumart.QP8.BackendDocumentContext.prototype.getValue = function (editor, inputName) {
	return this.getInput(editor, inputName).val();
};

Quantumart.QP8.BackendDocumentContext.prototype.setValue = function (editor, inputName, value) {
	this.getInput(editor, inputName).val(value);
};

Quantumart.QP8.BackendDocumentContext.prototype.getText = function (editor, inputName, value) {
    return this.getInput(editor, inputName).closest("div").find(".title").text();
};

Quantumart.QP8.BackendDocumentContext.prototype.getInput = function (editor, inputName) {
	var $form = (editor instanceof jQuery) ? editor : jQuery(editor._formElement);
    return $form.find("[name='" + inputName + "']");
};

Quantumart.QP8.BackendDocumentContext.prototype.getRegionFolderByRegionId = function (id) {
  if (!id) {
    return null
  };

  var parentIds;
  $q.sendAjax({
    url: '/Backend/Article/GetParentIds',
    async: false,
    data: {
      ids: [id],
      fieldId: 1115,
      filter: ''
    },
    jsendSuccess: function(data) {
      console.log('success: ' + JSON.stringify(data));
      parentIds = data;
    }
  });

  var folderItems = $ctx.getGlobal('regionalLibraryFolders');
  for (var i = 0; i < folderItems.length; i++) {
    var item = folderItems[i];
    if ($.inArray(item.macroRegionId, parentIds) !== -1) {
      return item.folder;
    }
  }

  return null;
};

Quantumart.QP8.BackendDocumentContext.prototype.setFileFieldsSubFolder = function(editor, region, fileFieldIds) {

    var folder = this.getRegionFolderByRegionId(this.getValue(editor, region));
    var fieldNames = jQuery.map(fileFieldIds, function(id) { return "field_" + id; });
    var $fields = $c.getAllFileFields(jQuery(editor._formElement));

    $fields.each(function(index, fieldElem) {
        var $field = $q.toJQuery(fieldElem);
        var currentFieldName = $field.data("field_name");

        if (jQuery.inArray(currentFieldName, fieldNames) != -1) {
            var component = $field.data("file_field_component");
            component.updateUploader(folder);
        }
    });
};

Quantumart.QP8.BackendDocumentContext.prototype.addGenerateMatrixTitleButton = function (inputName)
{
	var self = this;
	this.addCustomLinkButton({
		name: inputName,
		title: "Generate",
		suffix: "generate",
		"class": "customLinkButton",
		url: "/Backend/Content/QP8/icons/16x16/insert_call.gif",
		onClick: function (evt) {
			var resultInput = evt.data.$input;
			var strTemplates = {
				"404":
				{
					template: "Услуга '{0}' для тарифа '{1}' в регионе '{2}'",
					firstField: "field_1690",
					secondField: "field_1691"
				},
				"407":
				{
					template: "Пакет '{0}' для тарифа '{1}' в регионе '{2}'",
					firstField: "field_1706",
					secondField: "field_1703",
					useSecondFieldForRegion: true
				},
				"365":
				{
					template: "Группа '{0}' для региона '{2}'",
					firstField: "field_1443",
					isFirstFieldM2M: true,
				},
				"413":
				{
					template: "Связь между услугами '{0}' и '{1}' в регионе '{2}'",
					firstField: "field_1761",
					secondField: "field_1762"
				},
			}

			var matrixType = self.getValue(evt.data.$form, "field_1417");
			var config = strTemplates[matrixType];
			if (config)
			{
				var firstText = (config.isFirstFieldM2M) ?
					self.getInput(evt.data.$form, config.firstField).first().closest("li").find("label").text() :
					self.getText(evt.data.$form, config.firstField);
				var firstElems = firstText.split(';', 2);

				var secondText = (config.secondField) ? self.getText(evt.data.$form, config.secondField) : undefined;
				var secondElems = (secondText) ? secondText.split(';', 2) : ["", ""];

				var region = (((config.useSecondFieldForRegion) ? secondElems[1] : firstElems[1]) || "").trim().substr(0, 127);

				var title = String.format(config.template, firstElems[0], secondElems[0], region);

				self.setValue(evt.data.$form, inputName, title);
			}
		}
	});
}

Quantumart.QP8.BackendDocumentContext.setGlobal("regionalLibraryFolders", [
    { folder: "center", macroRegionId: 338941 },
    { folder: "dv", macroRegionId: 338940 },
    { folder: "msk", macroRegionId: 338946 },
    { folder: "psz", macroRegionId: 338944 },
    { folder: "puv", macroRegionId: 338943 },
    { folder: "sibir", macroRegionId: 338945 },
    { folder: "south", macroRegionId: 338939 },
    { folder: "sz", macroRegionId: 338942 },
    { folder: "ural", macroRegionId: 338947 }
]);
