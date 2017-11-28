/* eslint no-var: 0 */
/* eslint prefer-template: 0 */
/* eslint prefer-arrow-callback: 0 */
/* eslint vars-on-top: 0 */
/* eslint no-inner-declarations: 0 */
/* eslint spaced-comment: 0 */

Quantumart.QP8.BackendDocumentContext.prototype.getTypeByProduct = function (product) {
  return this.getGlobal('typesByProduct')[product];
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolverNames = function () {
  return jQuery.isArray(this.resolverNames) ? this.resolverNames : ['Type'];
};

Quantumart.QP8.BackendDocumentContext.prototype.initHandler = function (editor, $elem) {
  editor._hideFields = jQuery.isArray(editor._hideFields) // eslint-disable-line no-param-reassign
    ? editor._hideFields.concat(this.fieldsToHide) : this.fieldsToHide;

  if (this.changeFilters) {
    var resolverNames = this.getResolverNames();
    var that = this;
    jQuery.each(resolverNames, function (i, resolverName) {
      var resolver = that.getCurrentResolver(editor, resolverName);
      var filtersProp = that.getFiltersNameProp(resolverName);
      that.changeFilters(editor, resolver, $elem, filtersProp);
    });
  }

  if (this.changeFieldState) {
    this.changeFieldState(editor, null, $elem);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.fieldValueChangedHandler = function (editor, data) {
  if (this.changeFilters) {
    var resolverNames = this.getResolverNames();
    var that = this;
    jQuery.each(resolverNames, function (i, resolverName) {
      var prop = that.getResolverInputNameProp(resolverName);
      if (prop && (data.fieldName === that[prop] || data.contentFieldName === that[prop])) {
        var resolver = that.getCurrentResolver(editor, resolverName);
        var filtersProp = that.getFiltersNameProp(resolverName);
        that.changeFilters(editor, resolver, null, filtersProp);
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
    filter = fieldName ? 'c.[' + fieldName + '] in (' + value + ')'
      : 'c.content_item_id in (select linked_item_id from item_link where item_id in (' + value + '))';
  }

  this.applyFilter(inputName, filter, $form);
};

Quantumart.QP8.BackendDocumentContext.prototype.applyFilter = function (inputName, filter, $form) {
  var $elem = this.getRow($form, inputName);
  var list = $elem.find('.dataList').data('entity_data_list_component');
  if (list) {
    list.applyFilter(filter);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.getCurrentResolver = function (editor, resolverName) {
  var prop = this.getResolverInputNameProp(resolverName);
  return !prop || !this[prop] ? 0 : this.getResolver(editor, this.getValue(editor, this[prop]), resolverName);
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolverInputNameProp = function (resolverName) {
  return resolverName.charAt(0).toLowerCase() + resolverName.slice(1) + 'ResolverInputName';
};

Quantumart.QP8.BackendDocumentContext.prototype.getFiltersNameProp = function (resolverName) {
  return resolverName.charAt(0).toLowerCase() + resolverName.slice(1) + 'Filters';
};

Quantumart.QP8.BackendDocumentContext.prototype.getParentResolverFieldNameProp = function (resolverName) {
  return 'parent' + resolverName + 'FieldName';
};

Quantumart.QP8.BackendDocumentContext.prototype.getParentClassifierFieldNameProp = function (resolverName) {
  return 'parent' + resolverName + 'ClassifierFieldName';
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolverContentId = function (editor, resolverName) {
  var prop = this.getResolverInputNameProp(resolverName);
  return !prop || !this[prop] ? 0
    : this.getInput(editor, this[prop]).parent('.singleItemPicker').data('parent_entity_id');
};

Quantumart.QP8.BackendDocumentContext.prototype.getResolver = function (editor, val, resolverName) {
  var prod = 0;
  var parentProp = this.getParentResolverFieldNameProp(resolverName);
  if (parentProp && this[parentProp]) {
    var classifierProp = this.getParentClassifierFieldNameProp(resolverName);
    var resolverContentId = this.getResolverContentId(editor, resolverName);
    if (classifierProp && this[classifierProp] && +val) {
      var classifier = $o.getArticleFieldValue(resolverContentId, this[classifierProp], val);
      var extension = $o.getArticleIdByFieldValue(classifier, 'Parent', val);
      prod = $o.getArticleFieldValue(classifier, this[parentProp], extension);
    } else if (!+val) {
      prod = 0;
    } else if (+this[parentProp]) {
      prod = $o.getArticleLinkedItems(+this[parentProp], val);
    } else {
      prod = $o.getArticleFieldValue(resolverContentId, this[parentProp], val);
    }
  } else {
    var prop = this.getResolverInputNameProp(resolverName);
    prod = this.getValue(editor, this[prop]);
  }

  return resolverName === 'Type' ? this.getTypeByProduct(prod) : prod;
};

Quantumart.QP8.BackendDocumentContext.prototype.changeFilters = function (editor, value, $elem, filterProp) {
  var $form = $elem || jQuery(editor._formElement);
  if (this[filterProp]) {
    var that = this;
    jQuery.each(this[filterProp], function (key, propName) {
      that.setFilter(key, value, propName, $form);
    });
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.wrapFields = function (editor, inputNames, title) {
  var $form = jQuery(editor._formElement);
  var that = this;
  var multiSelector = inputNames.map(function (val) {
    return that.getRowSelector(val);
  }).join(', ');
  $form.find(multiSelector).wrapAll('<fieldset />');
  var $fieldSet = this.getRow(editor, inputNames[0]).closest('fieldset');
  $fieldSet.append('<legend>' + title + '</legend>');
};

Quantumart.QP8.BackendDocumentContext.prototype.toggleField = function (editor, inputName, state, preserveHidden) {
  var $elem = this.getRow(editor, inputName);
  var list = $elem.find('.dataList').data('entity_data_list_component');
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
        $elem.val('');
      }
    }
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.getValue = function (editor, inputName) {
  return this.getInput(editor, inputName).val();
};

Quantumart.QP8.BackendDocumentContext.prototype.getIds = function (editor, inputName) {
  var $elem = this.getRow(editor, inputName);
  var entities = $elem.find('.dataList').data('entity_data_list_component').getSelectedEntities();
  return entities.map(function (elem) {
    return elem.Id;
  });
};

Quantumart.QP8.BackendDocumentContext.prototype.setValue = function (editor, inputName, value) {
  this.getInput(editor, inputName).val(value).trigger('change');
};

Quantumart.QP8.BackendDocumentContext.prototype.getText = function (editor, inputName) {
  return this.getInput(editor, inputName).closest('div').find('.title').text();
};

Quantumart.QP8.BackendDocumentContext.prototype.getInput = function (editor, inputName) {
  var $form = editor instanceof jQuery ? editor : jQuery(editor._formElement);
  return $form.find("[name='" + inputName + "'],[data-content_field_name='" + inputName + "']");
};

Quantumart.QP8.BackendDocumentContext.prototype.getRow = function (editor, inputName) {
  var $form = editor instanceof jQuery ? editor : jQuery(editor._formElement);
  return $form.find(this.getRowSelector(inputName));
};

Quantumart.QP8.BackendDocumentContext.prototype.getRowSelector = function (inputName) {
  return "dl[data-field_form_name='" + inputName + "'],dl[data-field_name='" + inputName + "']";
};

//# sourceURL=dynamic.js

