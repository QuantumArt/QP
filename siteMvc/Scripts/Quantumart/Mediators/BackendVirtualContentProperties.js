Quantumart.QP8.JoinContentAndJoinFieldsMediator = function (joinContentSelectElementId, joinFieldsTreeElementId) {
    var contentPicker = jQuery("#" + joinContentSelectElementId).data("entity_data_list_component"),
        entityTreeComponent = Quantumart.QP8.BackendEntityTreeManager.getInstance().getTree(joinFieldsTreeElementId);

	function onRelatedToChanged() {
	    var selectedContentId = contentPicker.getSelectedEntityIDs()[0];
	    entityTreeComponent.set_parentEntityId(selectedContentId);
	    entityTreeComponent.set_selectedEntitiesIDs([]);
	    entityTreeComponent.refreshTree();
	}

	function dispose() {
	    jQuery(contentPicker.getStateFieldElement()).off("change", onRelatedToChanged);
	    contentPicker = null;
        entityTreeComponent = null;
	}

	jQuery(contentPicker.getStateFieldElement()).on("change", onRelatedToChanged);

	return {
		dispose: dispose
	};
};

Quantumart.QP8.UnionRadioAndSourceContentsListMediator = function (unionSourcePanelElementId, buildParamsPanelElementId, unionSourcesElementId) {
	var $unionSourcePanelElement = jQuery("#" + unionSourcePanelElementId),
        $buildParamsPanelElement = jQuery("#" + buildParamsPanelElementId),
        unionSourcesComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(unionSourcesElementId + "_list");

    function onUnionTypeSelected(){
            unionSourcesComponent._fixListOverflow();
    }

    function dispose() {
        $unionSourcePanelElement.unbind();
        $buildParamsPanelElement.unbind();

		$unionSourcePanelElement = null;
        $buildParamsPanelElement = null;
        unionSourcesComponent = null;
	}

    $unionSourcePanelElement.bind('show', onUnionTypeSelected);
    $buildParamsPanelElement.bind('show', onUnionTypeSelected);

    return {
		dispose: dispose
	};
};
