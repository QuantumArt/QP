window.CONTENT_CHANGE_TRACK_SELECTORS = '.containerContentSelector .singleItemPicker';
Quantumart.QP8.BackendTemplateObjectPropertiesMediator = function (rootElementId) {
  let $componentElem = jQuery(`#${rootElementId}`);
  let $parentObjectSelector = $componentElem.find('.parentTemplateObjectsSelector');
  let $nameField = $componentElem.find('.name');
  let $netNameField = $componentElem.find('.netName');
  let $overrideChkbx = $componentElem.find('.overrideChkbx');
  let $globalChkbx = $componentElem.find('.globalChkbx');
  let $typeSelector = $componentElem.find('.typeDlist');
  let $statusSelector = $componentElem.find('.multipleItemPicker');
  let $selectionIsStarting = $componentElem.find('.selection-is-starting .radioButtonsList');
  let $selectionIncludes = $componentElem.find('.selection-includes .radioButtonsList');

  let onContentValueChanged = function (e, data) {
    if (data.value) {
      $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_PAGE_TEMPLATE}GetFieldsByContentId`,
        {
          contentId: data.value
        },
        true, false).done(
        data => {
          let newFields = data.fields.split(',');
          let newStatuses = data.statuses;
          let publishedStatusId = $statusSelector.data('published-id');
          let vm = $componentElem.find('.sortingItems .aggregationList').data('component')._viewModel;

          if (vm.fields) {
            vm.fields.removeAll();
            for (let i in newFields) {
              vm.fields.push(newFields[i]);
            }
          }

          $statusSelector.data('entity_data_list_component').removeAllListItems();

          if (data.hasWorkflow == true) {
            $statusSelector.data('entity_data_list_component').selectEntities(newStatuses);
            $statusSelector.data('entity_data_list_component').deselectAllListItems();
            $statusSelector.data('entity_data_list_component').enableList();
          } else {
            $statusSelector.data('entity_data_list_component').selectEntities([$statusSelector.data('published-id')]);
            $statusSelector.data('entity_data_list_component').disableList();
          }
        });
    }
  };

  let manageGlobalVisibility = function () {
    if ($globalChkbx.get(0) && $globalChkbx.data('visibletypes').split(',').indexOf($typeSelector.val()) != -1) {
      $globalChkbx.parent('.field').show();
    } else {
      $globalChkbx.parent('.field').hide();
    }
  };

  let onParentTemplateObjectChanged = function () {
    if ($overrideChkbx.is(':checked') && $parentObjectSelector.children('option').size()) {
      let objId = $parentObjectSelector.val();
      let targetObj = $(this.data('objects')).filter(function () {
        return this.Id == objId;
      })[0];
      $nameField.val(targetObj.Name);
      $netNameField.val(targetObj.NetName);
    } else {
      $nameField.val('');
      $netNameField.val('');
    }
  };

  $componentElem.on(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, window.CONTENT_CHANGE_TRACK_SELECTORS, onContentValueChanged);
  $parentObjectSelector.change($.proxy(onParentTemplateObjectChanged, $parentObjectSelector));
  $overrideChkbx.click($.proxy(onParentTemplateObjectChanged, $parentObjectSelector));

  $typeSelector.change(manageGlobalVisibility);
  $statusSelector.find(`.multi-picker-item[value="${$statusSelector.data('published-id')}"]`).attr('checked', true);
  if ($statusSelector.data('has-workflow') == 'False') {
    $statusSelector.data('entity_data_list_component').disableList();
  }

  manageGlobalVisibility();

  let dispose = function () {
    $componentElem.unbind();
    $parentObjectSelector.unbind();
  };

  return {
    dispose: dispose
  };
};
