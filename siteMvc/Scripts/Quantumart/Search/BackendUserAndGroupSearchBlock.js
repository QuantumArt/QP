Quantumart.QP8.BackendUserAndGroupSearchBlock = function (searchBlockElementId, onApplyFilter) {
	function getSearchData() {
		var users = userPicker.getSelectedEntities(),
		groups = groupPicker.getSelectedEntities(),
		type = jQuery('li input:checked', $radioGroup).val();

		if (type == 1 && users[0]) {
			return { userId: users[0].Id };
		}
		else if (type == 2 && groups[0]) {
			return { groupId: groups[0].Id };
		}
		else
			return {};
	}

	function dispose() {
		jQuery(userPicker.getStateFieldElement()).off("change", onApplyFilter);
		jQuery(groupPicker.getStateFieldElement()).off("change", onApplyFilter);
		jQuery('li input', $radioGroup).off();

		$searchBlock = null;
		$radioGroup = null;
		userPicker = null;
		groupPicker = null;

		$btnSearch.off();
		$btnSearch = null;
	};

	var $searchBlock = jQuery('#' + searchBlockElementId),
		$btnSearch = jQuery('.pep-search-button', $searchBlock),
		$radioGroup = jQuery('.radioButtonsList', $searchBlock);

	$c.initAllSwitcherLists($searchBlock);
	$c.initAllEntityDataLists($searchBlock);

	var userPicker = jQuery('.pep-user-selector', $searchBlock).data('entity_data_list_component'),
		groupPicker = jQuery('.pep-group-selector', $searchBlock).data('entity_data_list_component');

	jQuery(userPicker.getStateFieldElement()).on("change", onApplyFilter);
	jQuery(groupPicker.getStateFieldElement()).on("change", onApplyFilter);
	jQuery('li input', $radioGroup).on("change", onApplyFilter);
	$btnSearch.on("click", onApplyFilter);

	return {
		getSearchData: getSearchData,
		dispose: dispose
	};
}
