Quantumart.QP8.CustomActionEntityTypesObserver = function (entityTypesElementId, actionsElementId, contentsElementId) {
  let $entityTypesElement = $(`#${entityTypesElementId}`);
  let $actionsElement = $(`#${actionsElementId}`);
  let $contents = $(`#${contentsElementId}_list`);

  let onEntityTypeChanged = function () {
    updateActionList();
    setFilter();
  };

  let setFilter = function () {
    let id = $entityTypesElement.val();
    if (id) {
      let code = Quantumart.QP8.BackendEntityType.getEntityTypeById(id).Code;
      let testCodes = [window.ENTITY_TYPE_CODE_VIRTUAL_CONTENT, window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE, window.ENTITY_TYPE_CODE_VIRTUAL_FIELD];
      let filter = $.inArray(code, testCodes) > -1 ? 'c.virtual_type <> 0' : 'c.virtual_type = 0';
      let obj = $contents.data('entity_data_list_component');
      if (obj) {
        let oldFilter = obj.getFilter();
        obj.setFilter(filter);
        if (oldFilter != '' && oldFilter != filter) {
          obj.removeAllListItems();
        }
      }
    }
  };

  let updateActionList = function (selectedActions) {
    let $list = $('ul', $actionsElement);
    $list.empty();
    let actions = selectedActions ? selectedActions.split(',') : null;

    let html = new $.telerik.stringBuilder();
    let entityTypeId = $('option:selected', $entityTypesElement).val();
    let dictionary = Quantumart.QP8.BackendEntityType.getEntityTypeIdToActionListItemDictionary();
    let pair = $.grep(dictionary, (item) => {
      return item.EntityTypeId == entityTypeId;
    });
    if (pair && pair[0]) {
      let listItems = pair[0].ActionItems;
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

  $entityTypesElement.bind('change keyup', onEntityTypeChanged);
  setFilter();

  return {
    updateActionList: updateActionList,
    dispose: function () {
      $entityTypesElement.unbind();
    }
  };
};

Quantumart.QP8.CustomActionIsInterfaceSelectorObserver = function (isInterfaceElementId, actionWindowPanelElementId, preActionPanelElementId) {
  let $isInterface = $(`#${isInterfaceElementId}`);
  let $actionWindowPanel = $(`#${actionWindowPanelElementId}`);
  let $preActionPanel = $(`#${preActionPanelElementId}`);

  let onIsInterfaceClicked = function () {
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
    dispose: function () {
      $isInterface.unbind();
    }
  };
};
