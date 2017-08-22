Quantumart.QP8.JoinContentAndJoinFieldsMediator = function (joinContentSelectElementId, joinFieldsTreeElementId) {
  const contentPicker = $(`#${joinContentSelectElementId}`).data('entity_data_list_component');
  const entityTreeComponent = Quantumart.QP8.BackendEntityTreeManager.getInstance().getTree(joinFieldsTreeElementId);

  const onRelatedToChanged = function () {
    const selectedContentId = contentPicker.getSelectedEntityIDs()[0];
    entityTreeComponent.set_parentEntityId(selectedContentId);
    entityTreeComponent.set_selectedEntitiesIDs([]);
    entityTreeComponent.refreshTree();
  };

  const dispose = function () {
    $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
  };

  $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

  return {
    dispose
  };
};

Quantumart.QP8.UnionRadioAndSourceContentsListMediator = function (unionSourcePanelElementId, buildParamsPanelElementId, unionSourcesElementId) {
  const $unionSourcePanelElement = $(`#${unionSourcePanelElementId}`);
  const $buildParamsPanelElement = $(`#${buildParamsPanelElementId}`);
  const unionSourcesComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(`${unionSourcesElementId}_list`);

  const onUnionTypeSelected = function () {
    unionSourcesComponent._fixListOverflow();
  };

  const dispose = function () {
    $unionSourcePanelElement.unbind();
    $buildParamsPanelElement.unbind();
  };

  $unionSourcePanelElement.bind('show', onUnionTypeSelected);
  $buildParamsPanelElement.bind('show', onUnionTypeSelected);

  return {
    dispose
  };
};
