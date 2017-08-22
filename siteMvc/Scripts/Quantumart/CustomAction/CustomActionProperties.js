Quantumart.QP8.CustomActionEntityTypesObserver = function (entityTypesElementId, actionsElementId, contentsElementId) {
  const $entityTypesElement = $(`#${entityTypesElementId}`);
  const $actionsElement = $(`#${actionsElementId}`);
  const $contents = $(`#${contentsElementId}_list`);

  const setFilter = function () {
    const id = $entityTypesElement.val();
    if (id) {
      const code = Quantumart.QP8.BackendEntityType.getEntityTypeById(id).Code;
      const testCodes = [window.ENTITY_TYPE_CODE_VIRTUAL_CONTENT, window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE, window.ENTITY_TYPE_CODE_VIRTUAL_FIELD];
      const filter = $.inArray(code, testCodes) > -1 ? 'c.virtual_type <> 0' : 'c.virtual_type = 0';
      const obj = $contents.data('entity_data_list_component');
      if (obj) {
        const oldFilter = obj.getFilter();
        obj.setFilter(filter);
        if (oldFilter != '' && oldFilter != filter) {
          obj.removeAllListItems();
        }
      }
    }
  };

  const updateActionList = function (selectedActions) {
    const $list = $('ul', $actionsElement);
    $list.empty();
    const actions = selectedActions ? selectedActions.split(',') : null;

    const html = new $.telerik.stringBuilder();
    const entityTypeId = $('option:selected', $entityTypesElement).val();
    const dictionary = Quantumart.QP8.BackendEntityType.getEntityTypeIdToActionListItemDictionary();
    const pair = $.grep(dictionary, item => item.EntityTypeId == entityTypeId);
    if (pair && pair[0]) {
      const listItems = pair[0].ActionItems;
      $.each(listItems, (i, item) => {
        html.cat(String.format('<li><input id="SelectedActions[{0}]" class="checkbox chb-list-item qp-notChangeTrack" type="checkbox" value="{1}" name="SelectedActions[{0}]" ', i, item.Value))
          .catIf(' checked="checked" ', actions && actions.indexOf(item.Value) != -1)
          .cat('/> ')
          .cat(String.format('<input type="hidden" value="false" name="SelectedActions[{0}]" />', i))
          .cat(`${String.format('<label for="SelectedActions[{0}]">', item.Value) + item.Text}</label>`)
          .cat('</li>');
      });
    }

    $list.html(html.string());
    $list.show();
  };

  const onEntityTypeChanged = function () {
    updateActionList();
    setFilter();
  };

  $entityTypesElement.bind('change keyup', onEntityTypeChanged);
  setFilter();

  return {
    updateActionList,
    dispose() {
      $entityTypesElement.unbind();
    }
  };
};

Quantumart.QP8.CustomActionIsInterfaceSelectorObserver = function (isInterfaceElementId, actionWindowPanelElementId, preActionPanelElementId) {
  const $isInterface = $(`#${isInterfaceElementId}`);
  const $actionWindowPanel = $(`#${actionWindowPanelElementId}`);
  const $preActionPanel = $(`#${preActionPanelElementId}`);

  const onIsInterfaceClicked = function () {
    if ($isInterface.is(':checked')) {
      $actionWindowPanel.show();
      $preActionPanel.hide();
    } else {
      $actionWindowPanel.hide();
      $preActionPanel.show();
    }
  };

  $isInterface.click(onIsInterfaceClicked);

  return {
    show: onIsInterfaceClicked,
    dispose() {
      $isInterface.unbind();
    }
  };
};
