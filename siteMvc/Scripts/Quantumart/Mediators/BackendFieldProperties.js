import { BackendEntityDataListManager } from '../Managers/BackendEntityDataListManager';
import { $q } from '../Utils';

export class FieldPropertiesMediator {
  constructor(tabId) {
    const $root = $(`#${tabId}_editingForm`);
    const onIntegerClick = function () {
      const isInteger = $root.find("input[name='Data.IsInteger']").prop('checked');
      $root.find("input[name='Data.IsLong']").closest('dl').toggle(isInteger);
      $root.find("input[name='Data.IsDecimal']").closest('dl').toggle(!isInteger);
    };

    const dispose = function () {
      $root.find("input[name='Data.IsInteger']").off('click', onIntegerClick);
    };

    $root.find("input[name='Data.IsInteger']").on('click', onIntegerClick);
    onIntegerClick();

    return {
      dispose
    };
  }
}

export class RelateToAndDisplayFieldMediator {
  constructor(
    relateToSelectElementId, displayFieldSelectElementId, currentFieldIdHiddenElementId, listOrderSelectElementId
  ) {
    const contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
    const $displayFieldSelectElement = $(`#${displayFieldSelectElementId}`);
    const $listOrderSelectElement = $(`#${listOrderSelectElementId}`);
    const currentFieldId = $(`#${currentFieldIdHiddenElementId}`).val();
    const relateableFieldsUrl = `${window.CONTROLLER_URL_CONTENT}_RelateableFields`;

    const onRelatedToChanged = function () {
      const selectedContentId = $(contentPicker.getStateFieldElement()).val();

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
          .done(data => {
            if (data.success) {
              $displayFieldSelectElement.empty();
              $listOrderSelectElement.empty();
              let html, html2;
              if ($q.isNullOrEmpty(data.data)) {
                html = '<option value=""></option>';
                html2 = html;
              } else {
                const htmlBuilder = new $.telerik.stringBuilder();
                $(data.data).each(function () {
                  htmlBuilder
                    .cat('<option value="')
                    .cat(this.id).cat('">')
                    .cat(this.text)
                    .cat('</option>');
                });
                html = htmlBuilder.string();
                html2 = `<option value="">${$l.EntityEditor.selectField}</option>${html}`;
              }
              $displayFieldSelectElement.append(html);
              $listOrderSelectElement.append(html2);
            } else {
              $q.alertError(data.message);
            }
          })
          .fail(jqXHR => {
            $q.processGenericAjaxError(jqXHR);
          });
      }
    };

    const dispose = function () {
      $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    };

    $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

    return {
      dispose
    };
  }
}

export class RelateToAndClassifierFieldMediator {
  constructor(
    relateToSelectElementId, classifierSelectElementId
  ) {
    let contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
    let $classifierSelectElement = $(`#${classifierSelectElementId}`);
    const classifierFieldsUrl = `${window.CONTROLLER_URL_CONTENT}_ClassifierFields`;

    const onRelatedToChanged = function () {
      const selectedContentId = $(contentPicker.getStateFieldElement()).val();
      if (!$q.isNullOrEmpty(selectedContentId)) {
        $q.getJsonFromUrl(
          'GET',
          classifierFieldsUrl,
          {
            contentId: selectedContentId
          },
          true,
          false
        ).done(data => {
          if (data.success) {
            $classifierSelectElement.empty();
            if ($q.isNullOrEmpty(data.data)) {
              $classifierSelectElement.append('<option value=""></option>');
            } else {
              const html = new $.telerik.stringBuilder();
              $(data.data).each(function () {
                html
                  .cat('<option value="')
                  .cat(this.id)
                  .cat('">')
                  .cat(this.text)
                  .cat('</option>');
              });
              $classifierSelectElement.append(html.string());
            }
          } else {
            $q.alertError(data.message);
          }
        })
          .fail(jqXHR => {
            $q.processGenericAjaxError(jqXHR);
          });
      }
    };

    const dispose = function () {
      $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
      contentPicker = null;
      $classifierSelectElement = null;
    };

    $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

    return {
      dispose
    };
  }
}


export class RelateToAndO2MDefaultMediator {
  constructor(
    relateToSelectElementId, O2MPickerListElementId, M2MPickerListElementId
  ) {
    const contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
    const singleItemPickerComponent = BackendEntityDataListManager
      .getInstance().getList(O2MPickerListElementId);
    const multipleItemPickerComponent = BackendEntityDataListManager
      .getInstance().getList(`${M2MPickerListElementId}_list`);

    const onRelatedToChanged = function () {
      const selectedContentId = $(contentPicker.getStateFieldElement()).val();
      multipleItemPickerComponent.removeAllListItems();
      multipleItemPickerComponent.set_parentEntityId(selectedContentId);
      singleItemPickerComponent.set_parentEntityId(selectedContentId);
      singleItemPickerComponent.deselectAllListItems();
    };

    const dispose = function () {
      $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    };

    $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

    return {
      dispose
    };
  }
}


export class FieldTypeFileDefaultMediator {
  constructor(fieldTypeSelectElementId, fileFieldElementId) {
    const $fieldTypeSelectElement = $(`#${fieldTypeSelectElementId}`);
    const $fileFieldElement = $(`#${fileFieldElementId}`);
    const fileFieldComponent = $fileFieldElement.data('file_field');

    const onFieldTypeChanged = function () {
      const fieldType = $('option:selected', $fieldTypeSelectElement).val();
      if (fieldType === window.FILE_FIELD_TYPE) {
        fileFieldComponent.setIsImage(false);
      } else if (fieldType === window.IMAGE_FIELD_TYPE) {
        fileFieldComponent.setIsImage(true);
      }
    };

    const dispose = function () {
      $fieldTypeSelectElement.unbind();
    };

    $fieldTypeSelectElement.bind('change keyup', onFieldTypeChanged);

    return {
      dispose
    };
  }
}

export class RelateToAndPanelsMediator {
  constructor(relateToSelectElementId, panelsSelector, fieldContentID) {
    const contentPicker = $(`#${relateToSelectElementId}`).data('entity_data_list_component');
    const $panels = $(panelsSelector);

    const onRelatedToChanged = function () {
      const selectedContentId = $(contentPicker.getStateFieldElement()).val();
      if (!selectedContentId) {
        $panels.hide();
      } else if ($q.isNullOrEmpty(selectedContentId)) {
        $panels.hide();
      } else {
        $panels.show();
        $q.warnIfEqDiff(selectedContentId, fieldContentID);
        if (selectedContentId === fieldContentID) {
          $panels.filter('[showforcurrent]').show();
          $panels.filter('[hideforcurrent]').hide();
        } else {
          $panels.filter('[showforcurrent]').hide();
          $panels.filter('[hideforcurrent]').show();
        }
      }
    };

    const dispose = function () {
      $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    };

    $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

    return {
      refresh: onRelatedToChanged,
      dispose
    };
  }
}

export class BackRelationSelectAndFieldMediator {
  constructor(BackRelationSelectElementId, listOrderSelectElementId) {
    const $fieldSelectElement = $(`#${BackRelationSelectElementId}`);
    const $listOrderSelectElement = $(`#${listOrderSelectElementId}`);
    const relateableFieldsUrl = `${window.CONTROLLER_URL_CONTENT}_RelateableFields`;

    const onRelatedToChanged = function () {
      const currentFieldId = $('option:selected', $fieldSelectElement).val();

      if (!$q.isNullOrEmpty(currentFieldId)) {
        $q.getJsonFromUrl(
          'GET',
          relateableFieldsUrl,
          {
            contentId: 0,
            fieldId: currentFieldId
          },
          true,
          false
        )
          .done(data => {
            if (data.success) {
              $listOrderSelectElement.empty();
              let html, html2;
              if ($q.isNullOrEmpty(data.data)) {
                html = '<option value=""></option>';
                html2 = html;
              } else {
                const htmlBuilder = new $.telerik.stringBuilder();
                $(data.data).each(function () {
                  htmlBuilder
                    .cat('<option value="')
                    .cat(this.id).cat('">')
                    .cat(this.text)
                    .cat('</option>');
                });
                html = htmlBuilder.string();
                html2 = `<option value="">${$l.EntityEditor.selectField}</option>${html}`;
              }
              $listOrderSelectElement.append(html2);
            } else {
              $q.alertError(data.message);
            }
          })
          .fail(jqXHR => {
            $q.processGenericAjaxError(jqXHR);
          });
      }
    };

    const dispose = function () {
      $fieldSelectElement.off('change', onRelatedToChanged);
    };

    $fieldSelectElement.on('change', onRelatedToChanged);

    return {
      dispose
    };
  }
}


Quantumart.QP8.FieldPropertiesMediator = FieldPropertiesMediator;
Quantumart.QP8.RelateToAndDisplayFieldMediator = RelateToAndDisplayFieldMediator;
Quantumart.QP8.RelateToAndClassifierFieldMediator = RelateToAndClassifierFieldMediator;
Quantumart.QP8.RelateToAndO2MDefaultMediator = RelateToAndO2MDefaultMediator;
Quantumart.QP8.FieldTypeFileDefaultMediator = FieldTypeFileDefaultMediator;
Quantumart.QP8.RelateToAndPanelsMediator = RelateToAndPanelsMediator;
Quantumart.QP8.BackRelationSelectAndFieldMediator = BackRelationSelectAndFieldMediator;
