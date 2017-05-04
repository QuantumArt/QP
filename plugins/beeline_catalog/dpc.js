Quantumart.QP8.BackendDocumentContext.prototype.getTypeByProduct = function(product) {
  return this.getGlobal("typesByProduct")[product];
};

Quantumart.QP8.BackendDocumentContext.prototype.initHandler = function(editor, $elem) {
  if (this.changeFilters) {
    this.changeFilters(editor, this.getCurrentType(editor), $elem);
  }

  if (this.changeFieldState) {
    this.changeFieldState(editor, null, $elem);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.fieldValueChangedHandler = function(editor, data) {
  if ((data.fieldName === this.typeResolverInputName || data.contentFieldName === this.typeResolverInputName) && this.changeFilters) {
    this.changeFilters(editor, this.getCurrentType(editor));
  }

  if (this.changeFieldState) {
    this.changeFieldState(editor, data, null);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.setFilter = function(inputName, value, fieldName, editor) {
  var filter = (fieldName) ?
    "c.[" + fieldName + "] = " + value :
    "c.content_item_id in (select linked_item_id from item_link where item_id = " + value + ")";
  var list = this.getRow(editor, inputName).find(".dataList").data("entity_data_list_component");
  if (list) {
    list.applyFilter(filter);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.getParentContentId = function(editor) {
  return (!this.typeResolverInputName) ? 0 : this.getElem(editor, this.typeResolverInputName).parent(".singleItemPicker").data("parent_entity_id");
};

Quantumart.QP8.BackendDocumentContext.prototype.getCurrentType = function(editor) {
  if (!this.typeResolverInputName) {
    return 0;
  }
  else {
    var currentTypeResolverValue = this.getElem(editor, this.typeResolverInputName).val();
    return this.getType(editor, currentTypeResolverValue);
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.getType = function(editor, val) {
  var prod = 0;
  if (this.parentTypeFieldName) {
    prod = (!+val) ? 0 :$o.getArticleFieldValue(this.getParentContentId(editor), this.parentTypeFieldName, val);
  }
  else {
    prod = this.getValue(editor, this.typeResolverInputName);
  }
  return this.getTypeByProduct(prod);
};

Quantumart.QP8.BackendDocumentContext.prototype.changeFilters = function(editor, value, $elem) {
    if (value && this.typeFilters) {
      for (var key in this.typeFilters) {
        this.setFilter(key, value, this.typeFilters[key], editor);
      }
    }
};

Quantumart.QP8.BackendDocumentContext.prototype.toggleVeButtons = function(editor, inputName, state) {
   var $elem = this.getRow(editor, inputName);
   var $button = $elem.find(".visualEditorToolbar");
   if (state) {
       $button.show();
   }
   else {
       $button.hide();
   }
};

Quantumart.QP8.BackendDocumentContext.prototype.toggleField = function(editor, inputName, state, preserveHidden) {
  var $elem = this.getRow(editor, inputName);
  var list = $elem.find(".dataList").data("entity_data_list_component");
  if (state) {
    $elem.show();
    if (list) {
      list._fixListOverflow();
    }
  }
  else
  {
    $elem.hide();
    if (!preserveHidden)
    {
      if (list) {
        list.selectEntities();
      } else {
        $elem.val("");
      }
    }
  }
};

Quantumart.QP8.BackendDocumentContext.prototype.getValue = function(editor, inputName) {
  return this.getElem(editor, inputName).val();
};

Quantumart.QP8.BackendDocumentContext.prototype.getElem = function(editor, inputName) {
  var $form = (editor instanceof jQuery) ? editor : jQuery(editor._formElement);
  return $form.find("[name='" + inputName + "'],[data-content_field_name='" + inputName + "']");
};

Quantumart.QP8.BackendDocumentContext.prototype.getRow = function(editor, inputName) {
  var $form = (editor instanceof jQuery) ? editor : jQuery(editor._formElement);
  return $form.find("dl[data-field_form_name='" + inputName + "'],dl[data-field_name='" + inputName + "']");
};


//# sourceURL=dpc.js
