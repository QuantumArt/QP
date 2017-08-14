Quantumart.QP8.FieldPropertiesMediator = function (tabId) {
  let $root = $(`#${tabId}_editingForm`);
  $root.find("input[name='Data.IsInteger']").on('click', onIntegerClick);
  onIntegerClick();

  function onIntegerClick() {
    let isInteger = $root.find("input[name='Data.IsInteger']").prop('checked');
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
  let contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
  let $displayFieldSelectElement = $(`#${displayFieldSelectElementId}`);
  let $listOrderSelectElement = $(`#${listOrderSelectElementId}`);
  let currentFieldId = $(`#${currentFieldIdHiddenElementId}`).val();
  let relateableFieldsUrl = `${window.CONTROLLER_URL_CONTENT}_RelateableFields`;

  function onRelatedToChanged() {
    let selectedContentId = $(contentPicker.getStateFieldElement()).val();

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
            let html, html2;
            if (!$q.isNullOrEmpty(data.data)) {
              let htmlBuilder = new $.telerik.stringBuilder();
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
  }

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

  return {
    dispose: dispose
  };
};

Quantumart.QP8.RelateToAndClassifierFieldMediator = function (relateToSelectElementId, classifierSelectElementId, aggregatedElementId, multiplePickerId) {
  let contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
  let $classifierSelectElement = $(`#${classifierSelectElementId}`);
  let $aggregatedElement = $(`#${aggregatedElementId}`);
  let classifierFieldsUrl = `${window.CONTROLLER_URL_CONTENT}_ClassifierFields`;

  function onRelatedToChanged() {
    let selectedContentId = $(contentPicker.getStateFieldElement()).val();
    if (!$q.isNullOrEmpty(selectedContentId)) {
      $q.getJsonFromUrl(
        'GET',
        classifierFieldsUrl,
        {
          contentId: selectedContentId
        },
        true,
        false
      ).done((data) => {
          if (data.success) {
            $classifierSelectElement.empty();
            if (!$q.isNullOrEmpty(data.data)) {
              let html = new $.telerik.stringBuilder();
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
  let contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
  let singleItemPickerComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(O2MPickerListElementId);
  let multipleItemPickerComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(`${M2MPickerListElementId}_list`);

  function onRelatedToChanged() {
    let selectedContentId = $(contentPicker.getStateFieldElement()).val();
    multipleItemPickerComponent.removeAllListItems();
    multipleItemPickerComponent.set_parentEntityId(selectedContentId);
    singleItemPickerComponent.set_parentEntityId(selectedContentId);
    singleItemPickerComponent.deselectAllListItems();
  }

  function dispose() {
    $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
  }

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

  return {
    dispose: dispose
  };
};


Quantumart.QP8.FieldTypeFileDefaultMediator = function (fieldTypeSelectElementId, fileFieldElementId) {
  let $fieldTypeSelectElement = $(`#${fieldTypeSelectElementId}`);
  let $fileFieldElement = $(`#${fileFieldElementId}`);
  let fileFieldComponent = $fileFieldElement.data('file_field');

  function onFieldTypeChanged() {
    let fieldType = $('option:selected', $fieldTypeSelectElement).val();
    if (fieldType == window.FILE_FIELD_TYPE) {
      fileFieldComponent.set_isImage(false);
    } else if (fieldType == window.IMAGE_FIELD_TYPE) {
      fileFieldComponent.set_isImage(true);
    }
  }

  function dispose() {
    $fieldTypeSelectElement.unbind();
  }

  $fieldTypeSelectElement.bind('change keyup', onFieldTypeChanged);

  return {
    dispose: dispose
  };
};

Quantumart.QP8.RelateToAndPanelsMediator = function (relateToSelectElementId, panelsSelector, fieldContentID) {
  let contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
  let $panels = $(panelsSelector);

  function onRelatedToChanged() {
    let selectedContentId = $(contentPicker.getStateFieldElement()).val();
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
  }

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

  return {
    refresh: onRelatedToChanged,
    dispose: dispose
  };
};
