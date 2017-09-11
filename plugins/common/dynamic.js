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

    editor._hideFields = jQuery.isArray(editor._hideFields) ? editor._hideFields.concat(this.fieldsToHide) : this.fieldsToHide;

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
            if (prop && (data.fieldName == self[prop] || data.contentFieldName == self[prop])) {
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
  var $elem = this.getRow($form, inputName);
  var list = $elem.find(".dataList").data("entity_data_list_component");
  if (list) {
    list.applyFilter(filter);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.getCurrentResolver = function (editor, name) {
  var $form = jQuery(editor._formElement);
  var prop = this.getResolverInputNameProp(name);
  return (!prop || !this[prop]) ? 0 : this.getResolver(editor, this.getValue(editor, this[prop]), name);
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
    return (!prop || !this[prop]) ? 0 : this.getElem(editor, this[prop]).parent(".singleItemPicker").data("parent_entity_id");
}

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
        prod = this.getValue(editor, this[prop]);
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
  var multiSelector = jQuery.map(inputNames, function (val) { return this.getRowSelector(val); }).join(", ")
  $form.find(multiSelector).wrapAll("<fieldset />")
  this.getRow(editor, inputNames[0]).closest("fieldset").append("<legend>" + title + "</legend>");
};

Quantumart.QP8.BackendDocumentContext.prototype.toggleField = function (editor, inputName, state, preserveHidden) {
    var $elem = this.getRow(editor, inputName);
    var list = $elem.find(".dataList").data("entity_data_list_component");
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
    this.getInput(editor, inputName).val(value).trigger("change");
};

Quantumart.QP8.BackendDocumentContext.prototype.getText = function (editor, inputName, value) {
    return this.getInput(editor, inputName).closest("div").find(".title").text();
};

Quantumart.QP8.BackendDocumentContext.prototype.getInput = function (editor, inputName) {
    var $form = (editor instanceof jQuery) ? editor : jQuery(editor._formElement);
    return $form.find("[name='" + inputName + "'],[data-content_field_name='" + inputName + "']");
};

Quantumart.QP8.BackendDocumentContext.prototype.getRow = function(editor, inputName) {
  var $form = (editor instanceof jQuery) ? editor : jQuery(editor._formElement);
  return $form.find(this.getRowSelector(inputName));
};

Quantumart.QP8.BackendDocumentContext.prototype.getRowSelector = function(inputName) {
  return "dl[data-field_form_name='" + inputName + "'],dl[data-field_name='" + inputName + "']";
};

//# sourceURL=dynamic.js

