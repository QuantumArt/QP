window.CONTENT_CHANGE_TRACK_SELECTORS = '.containerContentSelector .singleItemPicker';
Quantumart.QP8.BackendTemplateObjectPropertiesMediator = function (rootElementId) {
  const $componentElem = jQuery(`#${rootElementId}`);
  const $parentObjectSelector = $componentElem.find('.parentTemplateObjectsSelector');
  const $nameField = $componentElem.find('.name');
  const $netNameField = $componentElem.find('.netName');
  const $overrideChkbx = $componentElem.find('.overrideChkbx');
  const $globalChkbx = $componentElem.find('.globalChkbx');
  const $typeSelector = $componentElem.find('.typeDlist');
  const $statusSelector = $componentElem.find('.multipleItemPicker');

  const onContentValueChanged = function (e, data) {
    if (data.value) {
      $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_PAGE_TEMPLATE}GetFieldsByContentId`,
        {
          contentId: data.value
        },
        true, false).done(
        info => {
          const newFields = info.fields.split(',');
          const newStatuses = info.statuses;
          const vm = $componentElem
            .find('.sortingItems .aggregationList')
            .data('component')
            ._viewModel;

          if (vm.fields) {
            vm.fields.removeAll();
            newFields.forEach(fv => vm.fields.push(fv));
          }

          $statusSelector.data('entity_data_list_component').removeAllListItems();

          if (info.hasWorkflow) {
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

  const manageGlobalVisibility = function () {
    if ($globalChkbx.get(0) && $globalChkbx.data('visibletypes').split(',').indexOf($typeSelector.val()) !== -1) {
      $globalChkbx.parent('.field').show();
    } else {
      $globalChkbx.parent('.field').hide();
    }
  };

  const onParentTemplateObjectChanged = function () {
    if ($overrideChkbx.is(':checked') && $parentObjectSelector.children('option').size()) {
      const objId = $parentObjectSelector.val();
      const [targetObj] = $(this.data('objects')).filter(function () {
        $q.warnIfEqDiff(this.id, objId);
        return this.Id === objId;
      });

      $nameField.val(targetObj.Name);
      $netNameField.val(targetObj.NetName);
    } else {
      $nameField.val('');
      $netNameField.val('');
    }
  };

  $componentElem.on(
    window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, window.CONTENT_CHANGE_TRACK_SELECTORS, onContentValueChanged
  );
  $parentObjectSelector.change($.proxy(onParentTemplateObjectChanged, $parentObjectSelector));
  $overrideChkbx.click($.proxy(onParentTemplateObjectChanged, $parentObjectSelector));

  $typeSelector.change(manageGlobalVisibility);
  $statusSelector.find(`.multi-picker-item[value="${$statusSelector.data('published-id')}"]`).attr('checked', true);
  if ($statusSelector.data('has-workflow') === 'False') {
    $statusSelector.data('entity_data_list_component').disableList();
  }

  manageGlobalVisibility();

  const dispose = function () {
    $componentElem.unbind();
    $parentObjectSelector.unbind();
  };

  return {
    dispose
  };
};
