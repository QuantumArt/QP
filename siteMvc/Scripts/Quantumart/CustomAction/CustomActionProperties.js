Quantumart.QP8.CustomActionEntityTypesObserver = function (entityTypesElementId, actionsElementId, contentsElementId) {

  function onEntityTypeChanged() {
    updateActionList();
    setFilter();
  }

  function setFilter()
  {
    var id = $entityTypesElement.val();
    if (id)
    {
      var code = Quantumart.QP8.BackendEntityType.getEntityTypeById(id).Code;
      var testCodes = [ENTITY_TYPE_CODE_VIRTUAL_CONTENT, ENTITY_TYPE_CODE_VIRTUAL_ARTICLE, ENTITY_TYPE_CODE_VIRTUAL_FIELD];
      var filter = (jQuery.inArray(code, testCodes) > -1) ? "c.virtual_type <> 0" : "c.virtual_type = 0";
      var obj = $contents.data("entity_data_list_component");
      if (obj)
      {
        var oldFilter = obj.getFilter();
        obj.setFilter(filter);
        if (oldFilter != "" && oldFilter != filter)
          {obj.removeAllListItems();}
      }

    }
  }

  function updateActionList(selectedActions) {
    var $list = jQuery('ul', $actionsElement);
    $list.empty();
    var actions = (selectedActions) ? selectedActions.split(",") : null;

    var html = new $.telerik.stringBuilder();
    var entityTypeId = jQuery("option:selected", $entityTypesElement).val();
    var dictionary = Quantumart.QP8.BackendEntityType.getEntityTypeIdToActionListItemDictionary();
    var pair = jQuery.grep(dictionary, function (item) { return (item.EntityTypeId == entityTypeId); });
    if (pair && pair[0]) {
      var listItems = pair[0].ActionItems;
      jQuery.each(listItems, function (i, item) {
        html.cat(String.format('<li><input id="SelectedActions[{0}]" class="checkbox chb-list-item qp-notChangeTrack" type="checkbox" value="{1}" name="SelectedActions[{0}]" ', i, item.Value))
          .catIf(' checked="checked" ', actions && actions.indexOf(item.Value) != -1)
          .cat('/> ')
          .cat(String.format('<input type="hidden" value="false" name="SelectedActions[{0}]" />', i))
          .cat(String.format('<label for="SelectedActions[{0}]">', item.Value) + item.Text + "</label>")
          .cat('</li>');
      });
    }

    $list.html(html.string());
    $list.show();

  }

  var $entityTypesElement = jQuery("#" + entityTypesElementId),
        $actionsElement = jQuery("#" + actionsElementId),
    $contents = jQuery("#" + contentsElementId + "_list");

  $entityTypesElement.bind("change keyup", onEntityTypeChanged);
  setFilter();

  return {
    updateActionList: updateActionList,
    dispose: function () {
      $entityTypesElement.unbind();

      $entityTypesElement = null;
      $actionsElement = null;
    }
  };
};

Quantumart.QP8.CustomActionIsInterfaceSelectorObserver = function (isInterfaceElementId, actionWindowPanelElementId, preActionPanelElementId) {


  function onIsInterfaceClicked() {
    if ($isInterface.is(':checked')) {
      $actionWindowPanel.show();
      $preActionPanel.hide();
    }
    else {
      $actionWindowPanel.hide();
      $preActionPanel.show();
    }
  }

  var $isInterface = jQuery('#' + isInterfaceElementId),
  $actionWindowPanel = jQuery('#' + actionWindowPanelElementId),
  $preActionPanel = jQuery('#' + preActionPanelElementId);

  $isInterface.click(onIsInterfaceClicked);

  return {
    show: onIsInterfaceClicked,
    dispose: function () {
      $isInterface.unbind();

      $isInterface = null;
      $actionWindowPanel = null;
      $preActionPanel = null;
    }
  };
};
