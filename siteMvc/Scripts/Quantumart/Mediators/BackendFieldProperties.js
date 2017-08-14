Quantumart.QP8.FieldPropertiesMediator = function (tabId) {
  var $root = $(`#${tabId}_editingForm`);
  $root.find("input[name='Data.IsInteger']").on('click', onIntegerClick);
  onIntegerClick();

  function onIntegerClick() {
    var isInteger = $root.find("input[name='Data.IsInteger']").prop('checked');
    $root.find("input[name='Data.IsLong']").closest('dl').toggle(isInteger);
    $root.find("input[name='Data.IsDecimal']").closest('dl').toggle(!isInteger);
  }

  function dispose() {
    $root.find("input[name='Data.IsInteger']").off('click', onIntegerClick);
  }

  return  {
    dispose: dispose
  };
};

Quantumart.QP8.RelateToAndDisplayFieldMediator = function (relateToSelectElementId, displayFieldSelectElementId, currentFieldIdHiddenElementId, listOrderSelectElementId) {
  var contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component'),
    $displayFieldSelectElement = $(`#${displayFieldSelectElementId}`),
    $listOrderSelectElement = $(`#${listOrderSelectElementId}`),
    currentFieldId = $(`#${currentFieldIdHiddenElementId}`).val(),
    relateableFieldsUrl = `${window.CONTROLLER_URL_CONTENT}_RelateableFields`;

  function onRelatedToChanged() {
    var selectedContentId = $(contentPicker.getStateFieldElement()).val();

    if (!$q.isNullOrEmpty(selectedContentId)) {
      $q.getJsonFromUrl(
        'GET',
        relateableFieldsUrl,
        {
          contentId: selectedContentId,
          fieldId: currentFieldId
        },
        true,
        false
      )
        .done((data) => {
          if (data.success) {
            $displayFieldSelectElement.empty();
            $listOrderSelectElement.empty();
            var html, html2;
            if (!$q.isNullOrEmpty(data.data)) {
              var htmlBuilder = new $.telerik.stringBuilder();
              $(data.data).each(function () {
                htmlBuilder
                  .cat('<option value="')
                  .cat(this.id).cat('">')
                  .cat(this.text)
                  .cat('</option>');
              });
              html = htmlBuilder.string();
              html2 = `<option value="">${$l.EntityEditor.selectField}</option>${html}`;
            } else {
              html = '<option value=""></option>';
              html2 = html;
            }
            $displayFieldSelectElement.append(html);
            $listOrderSelectElement.append(html2);
          } else {
            $q.alertError(data.message);
          }
        })
        .fail((jqXHR, textStatus, errorThrown) => {
          $q.processGenericAjaxError(jqXHR);
        });
    }
  }


  function dispose() {
    $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    contentPicker = null;
    $displayFieldSelectElement = null;
  }

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);


  return {
    dispose: dispose
  };
};

Quantumart.QP8.RelateToAndClassifierFieldMediator = function (relateToSelectElementId, classifierSelectElementId, aggregatedElementId, multiplePickerId) {
  var contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component'),
    $classifierSelectElement = $(`#${classifierSelectElementId}`),
    $aggregatedElement = $(`#${aggregatedElementId}`),
    classifierFieldsUrl = `${window.CONTROLLER_URL_CONTENT}_ClassifierFields`;

  function onRelatedToChanged() {
    var selectedContentId = $(contentPicker.getStateFieldElement()).val();
    if (!$q.isNullOrEmpty(selectedContentId)) {
      $q.getJsonFromUrl(
        'GET',
        classifierFieldsUrl,
        {
          contentId: selectedContentId
        },
        true,
        false
      )
        .done((data) => {
          if (data.success) {
            $classifierSelectElement.empty();
            if (!$q.isNullOrEmpty(data.data)) {
              var html = new $.telerik.stringBuilder();
              $(data.data).each(function () {
                html
                  .cat('<option value="')
                  .cat(this.id)
                  .cat('">')
                  .cat(this.text)
                  .cat('</option>');
              });
              $classifierSelectElement.append(html.string());
            } else {
              $classifierSelectElement.append('<option value=""></option>');
            }
          } else {
            $q.alertError(data.message);
          }
        })
        .fail((jqXHR, textStatus, errorThrown) => {
          $q.processGenericAjaxError(jqXHR);
        });
    }
  }

  function dispose() {
    $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    contentPicker = null;
    $classifierSelectElement = null;
    $aggregatedElement = null;
  }

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

  return {
    dispose: dispose
  };
};


Quantumart.QP8.RelateToAndO2MDefaultMediator = function (relateToSelectElementId, O2MPickerListElementId, M2MPickerListElementId) {
  var contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component'),
    singleItemPickerComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(O2MPickerListElementId),
    multipleItemPickerComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(`${M2MPickerListElementId}_list`);


  function onRelatedToChanged() {
    var selectedContentId = $(contentPicker.getStateFieldElement()).val();
    multipleItemPickerComponent.removeAllListItems();
    multipleItemPickerComponent.set_parentEntityId(selectedContentId);
    singleItemPickerComponent.set_parentEntityId(selectedContentId);
    singleItemPickerComponent.deselectAllListItems();
  }


  function dispose() {
    $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    contentPicker = null;
    singleItemPickerComponent = null;
  }

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

  return {
    dispose: dispose
  };
};


Quantumart.QP8.FieldTypeFileDefaultMediator = function (fieldTypeSelectElementId, fileFieldElementId) {
  var $fieldTypeSelectElement = $(`#${fieldTypeSelectElementId}`),
    $fileFieldElement = $(`#${fileFieldElementId}`),
    fileFieldComponent = $fileFieldElement.data('file_field');

  function onFieldTypeChanged() {
    var fieldType = $('option:selected', $fieldTypeSelectElement).val();
    if (fieldType == window.FILE_FIELD_TYPE) {
      fileFieldComponent.set_isImage(false);
    } else if (fieldType == window.IMAGE_FIELD_TYPE) {
      fileFieldComponent.set_isImage(true);
    }
  }

  function dispose() {
    $fieldTypeSelectElement.unbind();
    $fieldTypeSelectElement = null;
    $fileFieldElement = null;
    fileFieldComponent = null;
  }

  $fieldTypeSelectElement.bind('change keyup', onFieldTypeChanged);

  return {
    dispose: dispose
  };
};

// Показывает/скрывает панели при выборе контента на который ссылается поле
Quantumart.QP8.RelateToAndPanelsMediator = function (relateToSelectElementId, panelsSelector, fieldContentID) {
  var contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component'),
    $panels = $(panelsSelector);

  function onRelatedToChanged() {
    var selectedContentId = $(contentPicker.getStateFieldElement()).val();

    if (!selectedContentId) {
      $panels.hide();
    } else if ($q.isNullOrEmpty(selectedContentId)) {
      $panels.hide();
    } else {
      $panels.show();
      if (selectedContentId == fieldContentID) {
        $panels.filter('[showforcurrent]').show();
        $panels.filter('[hideforcurrent]').hide();
      } else {
        $panels.filter('[showforcurrent]').hide();
        $panels.filter('[hideforcurrent]').show();
      }
    }
  }


  function dispose() {
    $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    contentPicker = null;
    $panels = null;
  }

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

  return {
    refresh: onRelatedToChanged,
    dispose: dispose
  };
};
