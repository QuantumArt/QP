/* eslint max-lines: 'off' */
/* eslint no-empty-function: 'off' */
import { Backend } from './Backend';
import { BackendAggregationList } from './Editor/BackendAggregationList';
import { BackendClassifierField } from './Editor/BackendClassifierField';
import { BackendBrowserHistoryManager } from './Managers/BackendBrowserHistoryManager';
import { BackendEntityDataListManager } from './Managers/BackendEntityDataListManager';
import { BackendEntityTreeManager } from './Managers/BackendEntityTreeManager';
import { BackendEventArgs } from './Common/BackendEventArgs';
import { BackendFileField } from './Editor/BackendFileField';
import { BackendHighlightedTextArea } from './Editor/BackendTextAreaEditor';
import { BackendLibrary } from './Library/BackendLibrary';
import { BackendVisualEditor } from './Editor/BackendVisualEditor';
import { BackendWorkflow } from './Editor/BackendWorkflowEditor';
import { ImageCropResizeClient } from './BackendImageCropResizeClient';
import { $q } from './Utils';


// eslint-disable-next-line no-shadow
export class $c { }

$c.getAllFieldRows = function (parentElement) {
  window.console.error('TODO: SHOULD NOT USE THIS METHOD');
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('dl.row');
};

$c.setFieldRowsVisibility = function (parentElement, fieldNames, visible) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    if (!parentElement) {
      throw new Error($l.Common.parentDomElementNotSpecified);
    }

    const $rows = $q.toJQuery(parentElement).find('dl.row');
    $(fieldNames).each((i, fname) => {
      const $r = $rows.filter(`[data-field_form_name="${fname}"],dl[data-field_name='${fname}']`).first();
      if (visible) {
        $r.show();
      } else {
        $r.hide();
      }
    });
  }
};

// #region Свертывание/развертывание панелей
$c.getAllCheckboxToggles = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('INPUT.checkbox');
};

$c.initAllCheckboxToggles = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $checkboxes = $c.getAllCheckboxToggles(parentElement);
  $checkboxes.each((index, checkboxElem) => {
    $c.initCheckboxToggle(checkboxElem);
  });
};

$c.initCheckboxToggle = function (checkboxElem) {
  let $checkbox = $q.toJQuery(checkboxElem);

  const panelId = $checkbox.data('toggle_for');
  if (!$q.isNullOrWhiteSpace(panelId)) {
    const handler = $c._onCheckboxToggleClickHandler;

    $checkbox.bind('click', handler);
    $checkbox.data('init', true);
    handler.apply($checkbox);
  }

  $checkbox = null;
};

$c.destroyAllCheckboxToggles = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $checkboxes = $c.getAllCheckboxToggles(parentElement);
  $checkboxes.each((index, checkboxElem) => {
    $c.destroyCheckboxToggle(checkboxElem);
  });
};

$c.destroyCheckboxToggle = function (checkboxElem) {
  const $checkbox = $q.toJQuery(checkboxElem);
  $checkbox.unbind('click', $c._onCheckboxToggleClickHandler);
};

$c._onCheckboxToggleClickHandler = function () {
  const $checkbox = $(this);

  const panelIds = $checkbox.data('toggle_for').split(',');
  const isReverse = $q.toBoolean($checkbox.data('reverse'), false);
  const isChecked = $checkbox.is(':checked');
  let state = isChecked;
  if (isReverse) {
    state = !isChecked;
  }
  $.each(panelIds, (index, panelId) => {
    const $panel = $(`#${panelId}`);
    const isReversePanel = $q.toBoolean($panel.data('reverse'), false);
    if ((state && !isReversePanel) || (!state && isReversePanel)) {
      $panel.show();
      $panel.trigger('show');
      $c.fixAllEntityDataListsOverflow($panel);
      $c._refreshAllHta($panel);
      if (!$checkbox.data('init')) {
        $c._setPanelControlsDisabledState($panel, false);
      }
    } else {
      $panel.hide();
      $panel.trigger('hide');
      if (!$checkbox.data('init')) {
        $c._setPanelControlsDisabledState($panel, true);
      }
    }
  });
  $checkbox.removeData('init');
};

$c.getAllSwitcherLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.radioButtonsList, .dropDownList');
};

$c.initDisableControlsPanels = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  $('div', parentElement).each(function () {
    const $panel = $(this);
    if ($panel.css('display') === 'none') {
      $c._setPanelControlsDisabledState($panel, true);
    }
  });
};

$c.initAllSwitcherLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $switcherLists = $c.getAllSwitcherLists(parentElement);
  $switcherLists.each((index, switcherListElem) => {
    $c.initSwitcherList(switcherListElem);
  });

  $c.initDisableControlsPanels(parentElement);
};

$c.initSwitcherList = function (switcherListElem) {
  const $switcherList = $q.toJQuery(switcherListElem);
  const panelIDs = $switcherList.data('switch_for');

  if (!$q.isNullOrEmpty(panelIDs)) {
    const isRadio = $q.toBoolean($switcherList.data('is_radio'), false);
    let handler = null;

    if (isRadio) {
      handler = $c._onRadioButtonSwitcherChangeHandler;
      const $switchers = $switcherList.find(':radio');
      $switchers.bind('change', handler);
      const $checkedSwitchers = $switchers.filter(':checked');
      if ($checkedSwitchers.length > 0) {
        handler.apply($switchers.filter(':checked').get(0));
      }
    } else {
      handler = $c._onDropDownListSwitcherChangeHandler;
      $switcherList.bind('change keyup', handler);
      if ($switcherList.length > 0) {
        handler.apply($switcherList.get(0));
      }
    }
  }
};

$c.destroyAllSwitcherLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $switcherLists = $c.getAllSwitcherLists(parentElement);
  $switcherLists.each((index, switcherListElem) => {
    $c.destroySwitcherList(switcherListElem);
  });
};

$c.destroySwitcherList = function (switcherListElem) {
  const $switcherList = $q.toJQuery(switcherListElem);
  const panelIDs = $switcherList.data('switch_for');

  if (!$q.isNullOrEmpty(panelIDs)) {
    const isRadio = $q.toBoolean($switcherList.data('is_radio'), false);
    if (isRadio) {
      const $switchers = $switcherList.find(':radio');
      $switchers.unbind('change', $c._onRadioButtonSwitcherChangeHandler);
    } else {
      $switcherList.unbind('change keyup', $c._onDropDownListSwitcherChangeHandler);
    }
  }
};

$c._onDropDownListSwitcherChangeHandler = function () {
  const $dropDownList = $(this);
  const $items = $dropDownList.find('OPTION');
  const $item = $items.filter(':selected');
  const panelIDs = $dropDownList.data('switch_for');

  $c._switchPanel($item, panelIDs);
};

$c._onRadioButtonSwitcherChangeHandler = function () {
  const $radioButton = $(this);
  const $radioButtonList = $radioButton.parent().parent().parent();
  const panelIDs = $radioButtonList.data('switch_for');
  $c._switchPanel($radioButton, panelIDs);
};

$c._setPanelControlsDisabledState = function ($panel, state) {
  if ($panel.data('disable_controls')) {
    const $inputs = $(':input', $panel);
    if (state) {
      const $disabled = $inputs.filter(':disabled');
      $disabled.data('realDisabled', true);
      $inputs.prop('disabled', true);
    } else {
      $inputs.prop('disabled', false);
      const $realDisabled = $inputs.filter(function () {
        return !!$(this).data('realDisabled');
      });

      $realDisabled.prop('disabled', true).removeData('realDisabled');
    }
  }
};

$c._switchPanel = function ($selectedSwitcher, panelIDs) {
  // eslint-disable-next-line no-restricted-syntax
  for (const key in panelIDs) {
    if ({}.hasOwnProperty.call(panelIDs, key)) {
      const panelId = panelIDs[key];
      if (panelId) {
        $(panelId).filter(
          function () {
            return $(this).css('display') === 'block'
              || $q.toBoolean($(this).data('depended'), false);
          }
        ).hide()
          .trigger('hide')
          .each(
            function () {
              $c._setPanelControlsDisabledState($(this), true);
            }
          );
      }
    }
  }

  const selectedValue = $selectedSwitcher.val();
  const panelId = panelIDs[selectedValue];
  if (panelId) {
    const $panels = $(panelId);
    $panels.filter(
      function () {
        return !$q.toBoolean($(this).data('depended'), false);
      }
    ).each(
      function () {
        $c._setPanelControlsDisabledState(jQuery(this), false);
      }
    ).show().trigger('show');
    $c.fixAllEntityDataListsOverflow($panels);
    $c._refreshAllHta($panels);
  }
};

// #endregion

// #region Установка значений полей
$c.setValidator = function (input, errors) {
  const message = errors && errors[0] ? errors[0].Message : '';
  const $input = $(input);
  const operation = message ? 'addClass' : 'removeClass';
  if ($input.is(':input')) {
    $input[operation]('input-validation-error');
  }

  const html = message
    ? `<span id="${input.prop('id')}_validator" class="field-validation-error" >${message}</span>`
    : '';

  const $container = $input.closest('dl.row');
  $container.find('em.validators').html(html);
};

$c.getAllSimpleTextBox = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.textbox.simple-text');
};

$c.setAllSimpleTextBoxValues = function (parentElement, fieldValues) {
  if (!$q.isNullOrEmpty(fieldValues)) {
    const $tboxes = $c.getAllSimpleTextBox(parentElement);
    $(fieldValues).each((i, fv) => {
      const $tb = $tboxes.filter(`[name="${fv.fieldName}"]`).first();
      if ($tb.length > 0) {
        $tb.val(fv.value);
        $tb.change();
      }

      $c.setValidator($tb, fv.errors);
    });
  }
};

$c.getAllRadioLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.radioButtonsList');
};

$c.setAllRadioListValues = function (parentElement, fieldValues) {
  if (!$q.isNullOrEmpty(fieldValues)) {
    const $glists = $c.getAllRadioLists(parentElement);
    $(fieldValues).each((i, fv) => {
      const $list = $glists.filter(`[data-field_form_name="${fv.fieldName}"]:first`);
      $list.find(`input:radio[value="${fv.value}"]`).prop('checked', true);
      $c.setValidator($list, fv.errors);
    });
  }
};

$c.getAllBoolean = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('input.checkbox.simple-checkbox:checkbox');
};

$c.setAllBooleanValues = function (parentElement, fieldValues) {
  if (!$q.isNullOrEmpty(fieldValues)) {
    const $tboxes = $c.getAllBoolean(parentElement);
    $(fieldValues).each((i, fv) => {
      const $chbox = $tboxes.filter(`[name="${fv.fieldName}"]`).first();

      // eslint-disable-next-line eqeqeq
      const value = $q.isString(fv.value) ? fv.value == 'true' || fv.value == '1' : fv.value;
      if ($chbox.length > 0) {
        if (value) {
          $chbox.prop('checked', true);
          $chbox.change();
        } else if (!value) {
          $chbox.prop('checked', false);
          $chbox.change();
        }

        $c.setValidator($chbox, fv.errors);
      }
    });
  }
};

$c.setAllNumericBoxValues = function (parentElement, fieldValues) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  if (!$q.isNullOrEmpty(fieldValues)) {
    const $boxes = $c.getAllNumericTextBoxes(parentElement);
    $(fieldValues).each((i, fv) => {
      const $nbox = $boxes.filter(`[name="${fv.fieldName}"]`).first();
      if ($nbox.length > 0) {
        let numericComponent = $nbox.data('tTextBox');
        if (numericComponent) {
          numericComponent.value($q.toInt(fv.value));
          $nbox.change();
        }

        numericComponent = null;
      }

      $c.setValidator($nbox, fv.errors);
    });
  }
};

$c.setAllDateTimePickersValues = function (parentElement, fieldValues) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  if (!$q.isNullOrEmpty(fieldValues)) {
    const $dtPickers = $c.getAllDateTimePickers(parentElement);
    $(fieldValues).each((i, fv) => {
      const $p = $dtPickers.filter(`[name="${fv.fieldName}"]`).first();
      if ($p.length > 0) {
        const picker = $c.getDateTimePickerComponent($p);
        if (picker) {
          if (fv.value) {
            picker.value(fv.value);
          } else {
            picker.value(null);
          }

          $p.change();
        }

        $c.setValidator($p, fv.errors);
      }
    });
  }
};

$c.getAllVisualEditorAreas = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.visualEditor');
};

$c.setAllVisualEditorValues = function (parentElement, fieldValues) {
  if (!$q.isNullOrEmpty(fieldValues)) {
    if (!$q.isNullOrEmpty(fieldValues)) {
      const $tareas = $c.getAllVisualEditorAreas(parentElement);
      $(fieldValues).each((i, fv) => {
        const $ta = $tareas.filter(`[name="${fv.fieldName}"]`).first();
        if ($ta.length > 0) {
          $ta.text(fv.value);
          const $ve = $ta.closest('.visualEditorComponent');
          if ($ve.length > 0) {
            const component = BackendVisualEditor.getComponent($ve);
            if (component && component.getCkEditor()) {
              component.getCkEditor().setData(fv.value);
            }
          }

          $ta.change();
          $c.setValidator($ta, fv.errors);
        }
      });
    }
  }
};

$c.setAllEntityDataListValues = function (parentElement, fieldValues) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  if (!$q.isNullOrEmpty(fieldValues)) {
    const $lists = $c.getAllEntityDataLists(parentElement);
    let $lf = null;
    let listComponent = null;

    $(fieldValues).each((i, fv) => {
      $lf = $lists.filter(`[data-list_item_name="${fv.fieldName}"]`).first();
      if ($lf.length > 0) {
        listComponent = $lf.data('entity_data_list_component');
        if (listComponent) {
          const value = $q.isString(fv.value) ? fv.value.split(',') : fv.value;
          listComponent.selectEntities(value);
        }
      }

      $c.setValidator($lf, fv.errors);
    });
  }
};

$c.setAllClassifierFieldValues = function (parentElement, fieldValues, disableChangeTracking) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  if (!$q.isNullOrEmpty(fieldValues)) {
    const $classifiers = $c.getAllClassifierFields(parentElement);
    let $cl = null;
    let component = null;

    $(fieldValues).each((i, fv) => {
      $cl = $classifiers.filter(`[data-field_name="${fv.fieldName}"]`).first();
      if ($cl.length > 0) {
        component = BackendClassifierField.getComponent($cl);
        if (component) {
          component.setInitFieldValues(fieldValues);
          component.setDisableChangeTracking(disableChangeTracking);
          component.selectContent(fv.value);
        }
      }

      $c.setValidator($cl, fv.errors);
    });
  }
};

$c.setAllAggregationListValues = function (parentElement, fieldValues) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  if (!$q.isNullOrEmpty(fieldValues)) {
    const $lists = $c.getAllAggregationLists(parentElement);
    $(fieldValues).each((i, fv) => {
      const $lf = $lists.filter(`[data-field_name="${fv.fieldName}"]:first`);
      const component = BackendAggregationList.getComponent($lf);
      if (component) {
        component.setItems(fv.value);
      }
    });
  }
};

$c.setAllHighlightedTextAreaValues = function (parentElement, fieldValues) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }
  if (!$q.isNullOrEmpty(fieldValues)) {
    const $htas = $c.getAllHighlightedTextAreas(parentElement);
    $(fieldValues).each((i, fv) => {
      const componentCM = $htas.filter(`[name="${fv.fieldName}"]:first`).data('codeMirror');
      const componentJE = $htas.filter(`[name="${fv.fieldName}"]:first`).data('jsonEditor');
      if (componentCM || componentJE) {
        if ($q.isNullOrEmpty(fv.value)) {
          if (componentCM) {
            componentCM.setValue('');
          } else {
            componentJE.setText('');
          }
        } else if (componentCM) {
          componentCM.setValue(fv.value);
        } else {
          componentJE.setText(fv.value);
        }
      }
    });
  }
};

// #endregion

// #region Получение значения полей
$c.getAllSimpleTextBoxValues = function (parentElement) {
  return $c.getAllSimpleTextBox(parentElement).filter('[name]').map(function () {
    const $tb = $(this);
    return {
      fieldName: $tb.attr('name'),
      value: $tb.val()
    };
  });
};

$c.getAllRadioListValues = function (parentElement) {
  return $c.getAllRadioLists(parentElement).filter('[data-field_form_name]').map(function () {
    const $tb = $(this);
    return {
      fieldName: $tb.data('field_form_name'),
      value: $tb.find('input:radio:checked').val()
    };
  });
};

$c.getAllBooleanValues = function (parentElement) {
  return $c.getAllBoolean(parentElement).filter('[name]').map(function () {
    const $tb = $(this);
    return {
      fieldName: $tb.attr('name'),
      value: $tb.is(':checked')
    };
  });
};

$c.getAllNumericBoxValues = function (parentElement) {
  return $.grep($c.getAllNumericTextBoxes(parentElement)
    .filter('[name]')
    .map(function () {
      const $nbox = $(this);
      const numericComponent = $nbox.data('tTextBox');
      if (numericComponent) {
        return {
          fieldName: $nbox.attr('name'),
          value: numericComponent.value()
        };
      }

      return undefined;
    }), fv => fv);
};

$c.getAllDateTimePickersValues = function (parentElement) {
  return $.grep($c.getAllDateTimePickers(parentElement)
    .filter('[name]')
    .map(function () {
      const $p = $(this);
      const picker = $c.getDateTimePickerComponent($p);
      if (picker) {
        return {
          fieldName: $p.attr('name'),
          value: picker.inputValue
        };
      }

      return undefined;
    }), fv => fv);
};

$c.getAllVisualEditorValues = function (parentElement) {
  return $.grep($c.getAllVisualEditors(parentElement)
    .map(function () {
      const $ve = $(this);
      const $ta = $ve.find('.visualEditor');
      const editor = BackendVisualEditor.getComponent($ve);
      if (editor) {
        if (editor.getCkEditor()) {
          return {
            fieldName: $ta.attr('name'),
            value: editor.getCkEditor().getData()
          };
        }

        return {
          fieldName: $ta.attr('name'),
          value: $ta.text()
        };
      }

      return undefined;
    }), fv => fv);
};

$c.getAllEntityDataListValues = function (parentElement) {
  return $.grep($c.getAllEntityDataLists(parentElement).filter('[data-list_item_name]').map(function () {
    const $that = $(this);
    const listComponent = $that.data('entity_data_list_component');
    if (listComponent) {
      return {
        fieldName: $that.data('list_item_name'),
        value: listComponent.getSelectedEntityIDs()
      };
    }

    return undefined;
  }), fv => fv);
};

$c.getAllClassifierFieldValues = function (parentElement) {
  return $c.getAllClassifierFields(parentElement).filter('[data-field_name]').map(function () {
    const $that = $(this);
    const component = BackendClassifierField.getComponent($that);
    if (component) {
      return {
        fieldName: $that.data('field_name'),
        value: component.getSelectedContent()
      };
    }

    return undefined;
  });
};

$c.getAllAggregationListValues = function (parentElement) {
  return $c.getAllAggregationLists(parentElement).filter('[data-field_name]').map(function () {
    const $that = $(this);
    const component = BackendAggregationList.getComponent($that);
    if (component) {
      return {
        fieldName: $that.data('field_name'),
        value: component.getItems()
      };
    }

    return undefined;
  });
};

$c.getAllHighlightedTextAreaValues = function (parentElement) {
  return $c.getAllHighlightedTextAreas(parentElement).filter('[name]').map(function () {
    const $that = $(this);
    const componentCM = $that.data('codeMirror');
    const componentJE = $that.data('jsonEditor');
    if (componentCM || componentJE) {
      return {
        fieldName: $that.prop('name'),
        value: componentCM ? componentCM.getValue() : componentJE.getText()
      };
    }

    return undefined;
  });
};

$c.getAllFieldValues = function (parentElement) {
  const $ch = $c;
  const result = $.merge([], $ch.getAllSimpleTextBoxValues(parentElement));
  $.merge(result, $ch.getAllRadioListValues(parentElement));
  $.merge(result, $ch.getAllBooleanValues(parentElement));
  $.merge(result, $ch.getAllNumericBoxValues(parentElement));
  $.merge(result, $ch.getAllDateTimePickersValues(parentElement));
  $.merge(result, $ch.getAllVisualEditorValues(parentElement));
  $.merge(result, $ch.getAllEntityDataListValues(parentElement));
  $.merge(result, $ch.getAllClassifierFieldValues(parentElement));
  $.merge(result, $ch.getAllAggregationListValues(parentElement));
  $.merge(result, $ch.getAllHighlightedTextAreaValues(parentElement));
  return result;
};

// #endregion

// #region Readonly полей
$c.makeReadonlySimpleTextBoxes = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $tboxes = $c.getAllSimpleTextBox(parentElement);
    $(fieldNames).each((i, fname) => {
      const $tb = $tboxes.filter(`[name="${fname}"]`).first();
      if ($tb.length > 0) {
        $tb.prop('readonly', true).addClass('readonly');
      }
    });
  }
};

$c.makeReadonlyBooleans = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $chboxes = $c.getAllBoolean(parentElement);
    $(fieldNames).each((i, fname) => {
      const $cb = $chboxes.filter(`[name="${fname}"]`).first();
      if ($cb.length > 0) {
        $cb.siblings(`input[name="${fname}"]`).filter(':hidden').first().val($cb.prop('checked'));
        $cb.prop('disabled', true);
      }
    });
  }
};

$c.makeReadonlyRadioList = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $glists = $c.getAllRadioLists(parentElement);
    $(fieldNames).each((i, fname) => {
      const $gl = $glists.filter(`[data-field_form_name="${fname}"]:first`);
      const cv = $gl.find('input:radio:checked').val();
      $gl.find('input:radio').prop('disabled', true);
      if (cv) {
        const $hdn = $gl.find(`input:hidden[name="${fname}"]`);
        if ($hdn.length === 0) {
          const htmlHidden = new $.telerik.stringBuilder();
          htmlHidden
            .cat('<input type="hidden" name="')
            .cat(fname)
            .cat('" value="')
            .cat(cv)
            .cat('" />');

          $gl.append(htmlHidden.string());
        } else {
          $hdn.prop('value', cv);
        }
      }
    });
  }
};

$c.makeReadonlyNumericBox = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $boxes = $c.getAllNumericTextBoxes(parentElement);
    $(fieldNames).each((i, fname) => {
      const $nbox = $boxes.filter(`[name="${fname}"]`).first();
      if ($nbox.length > 0) {
        let numericComponent = $nbox.data('tTextBox');
        if (numericComponent) {
          numericComponent.disable();
          $nbox.prop('disabled', false)
            .prop('readonly', true)
            .addClass('readonly');
        }
        numericComponent = null;
      }
    });
  }
};

$c.makeReadonlyDateTimePickers = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $dtPickers = $c.getAllDateTimePickers(parentElement);
    $(fieldNames).each((i, fname) => {
      const $p = $dtPickers.filter(`[name="${fname}"]`).first();
      if ($p.length > 0) {
        let picker = $c.getDateTimePickerComponent($p);
        if (picker) {
          picker.disable();
          $p.prop('disabled', false)
            .prop('readonly', true)
            .addClass('readonly');
        }
        picker = null;
      }
    });
  }
};

$c.makeReadonlyVisualEditors = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $tareas = $c.getAllVisualEditorAreas(parentElement);
    $(fieldNames).each((i, fname) => {
      $tareas.filter(`[name="${fname}"]`).first().data('is_readonly', true);
    });
  }
};

$c.makeReadonlyFileFields = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $fileFields = $q.toJQuery(parentElement).find('div.fileField');
    $(fieldNames).each((i, fname) => {
      const $ff = $fileFields.filter(`[data-field_name="${fname}"]`).first();
      if ($ff.length > 0) {
        $c.makeReadonlySimpleTextBoxes($ff, [fname]);
        $ff.find('.l-html-uploader, .l-sl-uploader, .libraryButton').hide();
      }
    });
  }
};

$c.makeReadonlyEntityDataList = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $lists = $c.getAllEntityDataLists(parentElement);
    let $lf = null;
    let listComponent = null;
    $(fieldNames).each((i, fname) => {
      $lf = $lists.filter(`[data-list_item_name="${fname}"]`).first();
      if ($lf.length > 0) {
        listComponent = $lf.data('entity_data_list_component');
        if (listComponent) {
          listComponent.makeReadonly();
        }
      }
    });
  }
};

$c.makeReadonlyClassifierFields = function (parentElement, fieldNames) {
  if ($q.isArray(fieldNames) && !$q.isNullOrEmpty(fieldNames)) {
    const $classifiers = $c.getAllClassifierFields(parentElement);
    let $cl = null;
    let component = null;

    $(fieldNames).each((i, fname) => {
      $cl = $classifiers.filter(`[data-field_name="${fname}"]`).first();
      if ($cl.length > 0) {
        component = BackendClassifierField.getComponent($cl);
        if (component) {
          component.makeReadonly();
        }
      }
    });
  }
};

// #endregion

// #region Работа с DateTimePicker`ами
$c.getAllDateTimePickers = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.t-picker-wrap INPUT.t-input');
};

$c.initAllDateTimePickers = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $fields = $c.getAllDateTimePickers(parentElement);
  $fields.each((index, fieldElem) => {
    $c.initDateTimePicker(fieldElem);
  });
};

$c.initDateTimePicker = function (fieldElem) {
  const $field = $q.toJQuery(fieldElem);
  $field.bind('valueChange', function () {
    const $t = $(this);
    $t.addClass(window.CHANGED_FIELD_CLASS_NAME);

    let value;
    if ($t.hasClass('time')) {
      value = $t.data('tTimePicker').value();
    } else if ($t.hasClass('datetime')) {
      value = $t.data('tDateTimePicker').value();
    } else if ($t.hasClass('date')) {
      value = $t.data('tDatePicker').value();
    }
    $t.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
      fieldName: $t.attr('name'),
      value,
      contentFieldName: $t.closest('dl').data('field_name')
    });
  });
};

$c.getDateTimePickerComponent = function (fieldElem) {
  const $fieldElem = $q.toJQuery(fieldElem);
  let $picker = $fieldElem.data('tDatePicker');
  if (!$picker) {
    $picker = $fieldElem.data('tDateTimePicker');
  }

  if (!$picker) {
    $picker = $fieldElem.data('tTimePicker');
  }

  return $picker;
};

$c.disableDateTimePicker = function (fieldElem) {
  const $picker = $c.getDateTimePickerComponent(fieldElem);
  if ($picker) {
    $picker.disable();
  }
};

$c.enableDateTimePicker = function (fieldElem) {
  const $picker = $c.getDateTimePickerComponent(fieldElem);
  if ($picker) {
    $picker.enable();
  }
};

$c.getDateTimePickerValue = function (fieldElem) {
  const $picker = $c.getDateTimePickerComponent(fieldElem);
  if ($picker && $picker.value()) {
    return $q.toJQuery(fieldElem).val();
  }

  return null;
};

$c.setDateTimePickerValue = function (fieldElem, value) {
  const $picker = $c.getDateTimePickerComponent(fieldElem);
  if ($picker) {
    if ($q.isNull(value)) {
      $q.toJQuery(fieldElem).val(null);
    } else {
      $q.toJQuery(fieldElem).val(value);
    }
  }
};

$c.destroyAllDateTimePickers = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $fields = $c.getAllDateTimePickers(parentElement);
  $fields.each((index, fieldElem) => {
    $c.destroyDateTimePicker(fieldElem);
  });
};

$c.destroyDateTimePicker = function (fieldElem) {
  $q.toJQuery(fieldElem)
    .unbind()
    .removeData('tDatePicker')
    .removeData('tDateTimePicker')
    .removeData('tTimePicker');
};

// #endregion

$c.getAllNumericTextBoxes = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.t-numerictextbox INPUT.t-input');
};

$c.initAllNumericTextBoxes = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $textBoxes = $c.getAllNumericTextBoxes(parentElement);
  $textBoxes.each(index => {
    $c.initNumericTextBox($textBoxes.eq(index));
  });
};

$c.initNumericTextBox = function (textBoxElement) {
  const $textBox = $q.toJQuery(textBoxElement);
  $textBox
    .bind('valueChange', function () {
      const $t = $(this);
      $t.addClass(window.CHANGED_FIELD_CLASS_NAME);
      $t.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
        fieldName: $t.attr('name'),
        value: $t.data('tTextBox').value(),
        contentFieldName: $t.closest('dl').data('field_name')
      });
    }).parent().find('.t-formatted-value').css({
      color: '',
      'font-family': '',
      'font-size': '',
      'font-weight': '',
      'line-height': '',
      'text-decoration': ''
    }
    );
};

$c.destroyAllNumericTextBoxes = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $textBoxes = $c.getAllNumericTextBoxes(parentElement);
  $textBoxes.each(index => {
    $c.destroyNumericTextBox($textBoxes.eq(index));
  });
};

$c.destroyNumericTextBox = function (textBoxElement) {
  const $textBox = $q.toJQuery(textBoxElement);
  $textBox
    .unbind()
    .removeData('tTextBox');
};

// #region Работа с файловыми полями
$c.getAllFileFields = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.fileField');
};

$c.initAllFileFields = function (parentElement, actionExecutingHandler) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $fields = $c.getAllFileFields(parentElement);
  $fields.each((index, fieldElem) => {
    $c.initFileField(fieldElem, actionExecutingHandler);
  });
};

$c.initFileField = function (fieldElem, actionExecutingHandler) {
  const $field = $q.toJQuery(fieldElem);
  if (!$q.isNullOrEmpty($field)) {
    const wrapperId = $field.attr('id');
    const fieldId = $field.data('field_id');
    const entityId = +$field.data('entity_id') || 0;
    const allowFileUpload = $q.toBoolean($field.data('allow_file_upload'), false);

    const options = {};
    options.entityId = entityId;
    options.allowFileUpload = allowFileUpload;
    options.isVersion = $q.toBoolean($field.data('is_version'), false);

    options.useSiteLibrary = $q.toBoolean($field.data('use_site_library'), false);
    options.libraryEntityId = +$field.data('library_entity_id') || 0;
    options.libraryParentEntityId = +$field.data('library_parent_entity_id') || 0;
    options.subFolder = $field.data('subfolder');
    options.libraryPath = $q.toString($field.data('library_path'), '');
    options.libraryUrl = $q.toString($field.data('library_url'), '');
    options.renameMatched = $q.toBoolean($field.data('rename_matched'), false);
    options.isImage = $q.toBoolean($field.data('is_image'), false);

    const fileField = new BackendFileField(fieldId, wrapperId, options);

    fileField.initialize();

    if ($q.isFunction(actionExecutingHandler) && fileField._allowFileUpload) {
      fileField._uploaderComponent.attachObserver(window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED, actionExecutingHandler);
    }

    $field.data('file_field_component', fileField);
  }
};

$c.destroyAllFileFields = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $fields = $c.getAllFileFields(parentElement);
  $fields.each((index, fieldElem) => {
    $c.destroyFileField(fieldElem);
  });
};

$c.destroyFileField = function (fieldElem) {
  const $field = $q.toJQuery(fieldElem);
  if (!$q.isNullOrEmpty($field)) {
    let fileField = $field.data('file_field_component');
    if (fileField) {
      fileField.dispose();
      fileField = null;
    }

    $field.removeData('file_field_component');
  }
};

// #endregion

// #region Работа с компонентом "поле-классификатор"
$c.getAllClassifierFields = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.classifierComponent');
};

$c.initAllClassifierFields = function (parentElement, actionExecutingHandler, editorOptions, onClassifierEventHandler) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  $c.getAllClassifierFields(parentElement).each(function () {
    $c.initClassifierField(this, actionExecutingHandler, editorOptions, onClassifierEventHandler);
  });
};

$c.initClassifierField = function (componentElem, actionExecutingHandler, editorOptions, onClassifierEventHandler) {
  const component = new BackendClassifierField(componentElem, actionExecutingHandler, editorOptions);
  component.initialize();
  if ($q.isFunction(onClassifierEventHandler)) {
    component.attachObserver(window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED, onClassifierEventHandler);
    component.attachObserver(window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING, onClassifierEventHandler);
  }
};

$c.destroyAllClassifierFields = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  $c.getAllClassifierFields(parentElement).each(function () {
    $c.destroyClassifierField(this);
  });
};

$c.destroyClassifierField = function (componentElem) {
  const component = BackendClassifierField.getComponent(componentElem);
  if (component) {
    component.detachObserver(window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED);
    component.dispose();
  }
};

// #endregion

// #region Работа с визуальными редакторами
$c.getAllVisualEditors = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.visualEditorComponent');
};

$c.initAllVisualEditors = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  $c.getAllVisualEditors(parentElement).each((index, editorElem) => {
    $c.initVisualEditor(editorElem);
  });
};

$c.initVisualEditor = function (editorElem) {
  const editor = new BackendVisualEditor(editorElem);
  editor.initialize();
};

$c.destroyAllVisualEditors = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  $c.getAllVisualEditors(parentElement).each((index, editorElem) => {
    $c.destroyVisualEditor(editorElem);
  });

  $q.collectGarbageInIE();
};

$c.destroyVisualEditor = function (editorElem) {
  let editor = BackendVisualEditor.getComponent(editorElem);
  if (editor) {
    editor.dispose();
    editor = null;
  }
};

$c.saveDataOfAllVisualEditors = function (parentElement) {
  $c.getAllVisualEditors(parentElement).each((index, editorElem) => {
    $c.saveVisualEditorData(editorElem);
  });
};

$c.saveVisualEditorData = function (editorElem) {
  let editor = BackendVisualEditor.getComponent(editorElem);
  if (editor) {
    editor.saveVisualEditorData();
    editor = null;
  }
};

// #endregion

// #region Работа с Highlighted TextArea
$c.getAllHighlightedTextAreas = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.highlightedTextarea');
};

$c.initAllHighlightedTextAreas = function (parentElem) {
  if (!parentElem) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  $c.getAllHighlightedTextAreas(parentElem).each(function () {
    $c.initHighlightedTextArea($(this));
  });
};

$c.initHighlightedTextArea = function (editorElem) {
  let area = new BackendHighlightedTextArea(editorElem);
  area.initialize();
  area = null;
};

$c.saveDataOfAllHighlightedTextAreas = function (parentElem) {
  $c.getAllHighlightedTextAreas(parentElem).each(function () {
    const area = new BackendHighlightedTextArea($(this));
    area.saveData();
  });
};

$c.destroyAllHighlightedTextAreas = function (parentElem) {
  $c.getAllHighlightedTextAreas(parentElem).each(function () {
    $c.destroyHighlightedTextArea($(this));
  });
};

$c.destroyHighlightedTextArea = function (editorElem) {
  let area = new BackendHighlightedTextArea(editorElem);
  area.destroy();
  area = null;
};

// #endregion

// #region Работа с AggregationLists
$c.getAllAggregationLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.aggregationList');
};

$c.initAllAggregationLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $lists = $c.getAllAggregationLists(parentElement);
  $lists.each(function () {
    $c.initAggregationList($(this));
  });
};

$c.initAggregationList = function (editorElem) {
  let list = new BackendAggregationList(editorElem);
  list.initialize();
  list = null;
};

$c.destroyAllAggregationLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $lists = $c.getAllAggregationLists(parentElement);
  $lists.each(function () {
    $c.destroyAggregationList($(this));
  });
};

$c.destroyAggregationList = function (editorElem) {
  (new BackendAggregationList(editorElem)).destroyAggregationList();
};

$c.saveDataOfAllAggregationLists = function (parentElement) {
  const $lists = $c.getAllAggregationLists(parentElement);
  $lists.each(function () {
    $c.saveAggregationListData($(this));
  });
};

$c.saveAggregationListData = function (editorElem) {
  (new BackendAggregationList(editorElem)).saveAggregationListData();
};

// #endregion

// #region Работа с Workflows
$c.getAllWorkflows = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.workflow_control');
};

$c.initAllWorkflows = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $workflows = $c.getAllWorkflows(parentElement);
  $workflows.each(function () {
    $c.initWorkflow($(this));
  });
};

$c.initWorkflow = function (editorElem) {
  const workflow = new BackendWorkflow(editorElem);
  workflow.initialize();
};

$c.saveDataOfAllWorkflows = function (editorElem) {
  window.console.error('TODO: SHOULD NOT USE THIS METHOD');
  const $workflows = $c.getAllWorkflows(editorElem);
  $workflows.each(() => {
    $c.saveWorkflowData();
  });
};

$c.saveWorkflowData = function () {
  window.console.error('TODO: SHOULD NOT USE THIS METHOD');
};

// #endregion

// #region preview и download
$c.preview = function (testUrl) {
  let win = null;
  const queryResult = $q.getJsonSync(testUrl);
  if (queryResult.proceed) {
    win = $c.openPreviewWindow(queryResult.url, queryResult.width, queryResult.height);
  } else {
    $q.alertError(queryResult.msg);
  }

  return win;
};

$c.crop = function (testUrl, urlParams) {
  let win = null;
  const queryResult = $q.getJsonSync(testUrl);
  if (queryResult.proceed) {
    win = $c.openCropWindow(queryResult.url, queryResult.folderUrl, urlParams);
  } else {
    $q.alertError(queryResult.msg);
  }

  return win;
};

$c.openPreviewWindow = function (url, width, height) {
  const urlWithTime = `${url}?t=${new Date().getTime()}`;
  const html = new $.telerik.stringBuilder();
  html
    .cat('<div class="previewImage">')
    .cat(`<img src="${urlWithTime}" width="${width}" height="${height}" />`)
    .cat('</div>');

  const _backendBrowserHistoryManager = BackendBrowserHistoryManager.getInstance();

  const win = $.telerik.window.create({
    title: $l.FileField.previewWindowTitle,
    html: html.string(),
    height: $c.correctPreviewSize(height, 125, $(window).height()),
    width: $c.correctPreviewSize(width, 215, $(window).width()),
    onOpen: _backendBrowserHistoryManager.handleModalWindowOpen,
    onClose: _backendBrowserHistoryManager.handleModalWindowClose
  }).data('tWindow');

  win.center().open();
  return win;
};

$c.openCropWindow = function (url, folderUrl, urlParams) {
  const imgCropResize = ImageCropResizeClient.create({
    sourceImageUrl: url,
    resultImageFolder: folderUrl,
    onCompleteCallback() {
      imgCropResize.closeWindow();
      imgCropResize.dispose();

      const newEventArgs = new BackendEventArgs();
      newEventArgs.set_entityTypeCode(urlParams.entityTypeCode);
      newEventArgs.set_actionTypeCode(window.ACTION_TYPE_CODE_FILE_CROPPED);
      const actionCode = urlParams.entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
        ? window.ACTION_CODE_UPDATE_SITE_FILE
        : window.ACTION_CODE_UPDATE_CONTENT_FILE;

      newEventArgs.set_actionCode(actionCode);
      newEventArgs.set_parentEntityId(urlParams.id);
      Backend.getInstance()._onActionExecuted(newEventArgs);
    }
  });

  imgCropResize.openWindow();
  return imgCropResize;
};

$c.correctPreviewSize = function (size, minSize, maxSize) {
  let newSize = size;
  const offset = 50;
  if (size < minSize) {
    newSize = minSize;
  } else if (size > (maxSize - offset)) {
    newSize = maxSize - offset;
  }

  return newSize;
};

$c.downloadFile = function (url) {
  const id = 'hiddenDownloader';
  let iframe = $(`#${id}`).get(0);
  if (!iframe) {
    iframe = $('<iframe>').attr({
      id,
      width: 0,
      height: 0
    }).appendTo(document.body).get(0);
  }

  // @ts-ignore HTMLIFrameElement.src
  iframe.src = url;
};

$c.downloadFileWithChecking = function (checkUrl, fileName) {
  const result = $q.getJsonSync(checkUrl);
  if (result.proceed) {
    const urlParams = { id: result.key, fileName: encodeURIComponent(fileName) };
    const url = BackendLibrary.generateActionUrl('DownloadFile', urlParams);
    $c.downloadFile(url);
  } else {
    $q.alertError(result.msg);
  }
};

// #endregion

// #region select popup
$c.getListItemCollectionFromEntities = function (entities) {
  const dataItems = [];
  $.each(entities, (index, entity) => {
    Array.add(dataItems, { Value: entity.Id, Text: entity.Name, Selected: false });
  });

  return dataItems;
};

$c.getEntitiesFromListItemCollection = function (dataItems) {
  const entities = [];
  $.each(dataItems, (index, dataItem) => {
    Array.add(entities, { Id: dataItem.Value, Name: dataItem.Text });
  });

  return entities;
};

// #endregion

// #region Работа со всплывающими окнами
$c.setPopupWindowTitle = function (windowComponent, titleText) {
  if (windowComponent) {
    $(windowComponent.element).find('.t-window-titlebar > .t-window-title').text(titleText);
  }
};

$c.destroyPopupWindow = function (windowComponent) {
  if (windowComponent) {
    const $window = $(windowComponent.element);
    if (!$q.isNullOrEmpty($window)) {
      $window.removeData('tWindow').empty();
    }

    windowComponent.destroy();
  }
};

$c.closePopupWindow = function (windowComponent) {
  const $window = $(windowComponent.element);
  if ($window.is(':visible')) {
    windowComponent.overlayOnClose();
    $.telerik.fx.rewind(windowComponent.effects, $window, null, () => {
      $window.hide();
    });
  }

  if (windowComponent.isMaximized) {
    $('html, body').css('overflow', '');
    if (windowComponent._documentScrollTop && windowComponent._documentScrollTop > 0) {
      $(document).scrollTop(windowComponent._documentScrollTop);
    }
  }
};

// #endregion

// #region Работа с упрощенными списками сущностей
$c.getAllEntityDataLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.dataList');
};

$c.initAllEntityDataLists = function (parentElement, actionExecutingHandler, editorOptions) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $dataLists = $c.getAllEntityDataLists(parentElement);
  $dataLists.each((index, dataListElem) => {
    $c.initEntityDataList(dataListElem, actionExecutingHandler, editorOptions);
  });
};

$c.initEntityDataList = function (dataListElem, actionExecutingHandler, editorOptions) {
  const $dataList = $q.toJQuery(dataListElem);
  if (!$q.isNullOrEmpty($dataList)) {
    let listType = Quantumart.QP8.Enums.DataListType.None;
    if ($dataList.hasClass('dropDownList')) {
      listType = Quantumart.QP8.Enums.DataListType.DropDownList;
    } else if ($dataList.hasClass('radioButtonsList')) {
      listType = Quantumart.QP8.Enums.DataListType.RadioButtonList;
    } else if ($dataList.hasClass('checkboxsList')) {
      listType = Quantumart.QP8.Enums.DataListType.CheckBoxList;
    } else if ($dataList.hasClass('singleItemPicker')) {
      listType = Quantumart.QP8.Enums.DataListType.SingleItemPicker;
    } else if ($dataList.hasClass('multipleItemPicker')) {
      listType = Quantumart.QP8.Enums.DataListType.MultipleItemPicker;
    }

    const options = {
      listId: +$dataList.data('list_id') || 0,
      listItemName: $q.toString($dataList.data('list_item_name'), ''),
      addNewActionCode: $q.toString($dataList.data('add_new_action_code'), window.ACTION_CODE_NONE),
      readActionCode: $q.toString($dataList.data('read_action_code'), window.ACTION_CODE_NONE),
      selectActionCode: $q.toString($dataList.data('select_action_code'), window.ACTION_CODE_NONE),
      maxListWidth: +$dataList.data('max_list_width') || 0,
      maxListHeight: +$dataList.data('max_list_height') || 0,
      showIds: $q.toBoolean($dataList.data('show_ids'), false),
      filter: $q.toString($dataList.data('filter'), ''),
      hostIsWindow: $q.toBoolean(editorOptions ? editorOptions.hostIsWindow : false, false),
      isCollapsable: $q.toBoolean($dataList.data('is_collapsable'), false),
      enableCopy: $q.toBoolean($dataList.data('enable_copy'), true),
      readDataOnInsert: $q.toBoolean($dataList.data('read_data_on_insert'), false),
      countLimit: $q.toInt($dataList.data('count_limit'), 1)
    };

    const entityDataList = BackendEntityDataListManager.getInstance().createList(
      $dataList.attr('id'),
      $dataList.data('entity_type_code'),
      $dataList.data('parent_entity_id'),
      $dataList.data('entity_id'),
      listType,
      options
    );

    if ($q.toBoolean($dataList.data('list_enabled'), true)) {
      entityDataList.enableList();
    } else {
      entityDataList.disableList();
    }

    if ($q.isFunction(actionExecutingHandler)) {
      entityDataList.attachObserver(window.EVENT_TYPE_ENTITY_LIST_ACTION_EXECUTING, actionExecutingHandler);
    }

    $dataList.data('entity_data_list_component', entityDataList);
  }
};

$c.fixAllEntityDataListsOverflow = function ($container) {
  const $dataLists = $c.getAllEntityDataLists($container);
  $dataLists.each(function () {
    const $dataList = $(this);
    const component = $dataList.data('entity_data_list_component');
    if (component) {
      component._fixListOverflow();
    }
  });
};

$c._refreshAllHta = function ($container) {
  const $htas = $c.getAllHighlightedTextAreas($container);
  $htas.each(function () {
    const $hta = $(this);
    const component = $hta.data('codeMirror');
    if (component) {
      component.refresh();
    }
  });
};

$c.destroyAllEntityDataLists = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $dataLists = $c.getAllEntityDataLists(parentElement);
  $dataLists.each((index, dataListElem) => {
    $c.destroyEntityDataList(dataListElem);
  });
};

$c.destroyEntityDataList = function (dataListElem) {
  const $dataList = $q.toJQuery(dataListElem);
  if (!$q.isNullOrEmpty($dataList)) {
    const entityDataList = $dataList.data('entity_data_list_component');
    if (entityDataList) {
      entityDataList.detachObserver(window.EVENT_TYPE_ENTITY_LIST_ACTION_EXECUTING);
      entityDataList.dispose();
    }
  }
};

// #endregion

// #region Работа c деревьями сущностей
$c.getAllEntityDataTrees = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.checkboxTree');
};

$c.initAllEntityDataTrees = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $dataTrees = $c.getAllEntityDataTrees(parentElement);
  $dataTrees.each((index, dataTreeElem) => {
    $c.initEntityDataTree(dataTreeElem);
  });
};

$c.initEntityDataTree = function (dataTreeElem) {
  const $dataTree = $q.toJQuery(dataTreeElem);
  if (!$q.isNullOrEmpty($dataTree)) {
    const treeElementId = $dataTree.attr('id');
    const treeName = $dataTree.data('tree_name');
    const entityTypeCode = $dataTree.data('entity_type_code');
    const parentEntityId = $dataTree.data('parent_entity_id');
    const actionCode = $dataTree.data('read_action_code');
    const allowGlobalSelection = $dataTree.data('allow_global_selection');
    const allowMultipleNodeSelection = $dataTree.data('show_checkbox');
    const selectedIDsString = $dataTree.data('selected_ids');
    const virtualContentId = $dataTree.data('virtual_content_id');
    let selectedEntitiesIDs;

    if (!$q.isNullOrWhiteSpace(selectedIDsString)) {
      selectedEntitiesIDs = selectedIDsString.toString().split(';');
    }

    const options = {
      treeName,
      allowGlobalSelection,
      allowMultipleNodeSelection,
      selectedEntitiesIDs,
      virtualContentId
    };

    const entityDataTree = BackendEntityTreeManager
      .getInstance()
      .createTree(treeElementId, entityTypeCode, parentEntityId, actionCode, options);

    entityDataTree.initialize();
    $dataTree.data('entity_data_tree_component', entityDataTree);
  }
};

$c.destroyAllEntityDataTrees = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  const $dataTrees = $c.getAllEntityDataTrees(parentElement);
  $dataTrees.each((index, dataTreeElem) => {
    $c.destroyEntityDataTree(dataTreeElem);
  });
};

$c.destroyEntityDataTree = function (dataTreeElem) {
  const $dataTree = $q.toJQuery(dataTreeElem);
  if (!$q.isNullOrEmpty($dataTree)) {
    const entityDataTree = $dataTree.data('entity_data_tree_component');
    if (entityDataTree) {
      entityDataTree.dispose();
    }
  }
};

// #endregion

export const ControlHelpers = $c;

window.$c = $c;

Quantumart.QP8.ControlHelpers = ControlHelpers;
