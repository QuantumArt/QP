Quantumart.QP8.BackendUserAndGroupSearchBlock = function (searchBlockElementId, onApplyFilter) {

  const $searchBlock = $(`#${searchBlockElementId}`);
  const $btnSearch = $('.pep-search-button', $searchBlock);
  const $radioGroup = $('.radioButtonsList', $searchBlock);

  const userPicker = $('.pep-user-selector', $searchBlock).data('entity_data_list_component');
  const groupPicker = $('.pep-group-selector', $searchBlock).data('entity_data_list_component');

  const getSearchData = function () {
    const users = userPicker.getSelectedEntities();
    const groups = groupPicker.getSelectedEntities();
    const type = $('li input:checked', $radioGroup).val();

    if (type == 1 && users[0]) {
      return { userId: users[0].Id };
    } else if (type == 2 && groups[0]) {
      return { groupId: groups[0].Id };
    }

    return {};
  };

  const dispose = function () {
    $(userPicker.getStateFieldElement()).off('change', onApplyFilter);
    $(groupPicker.getStateFieldElement()).off('change', onApplyFilter);
    $('li input', $radioGroup).off();
    $btnSearch.off();
  };

  $c.initAllSwitcherLists($searchBlock);
  $c.initAllEntityDataLists($searchBlock);

  $(userPicker.getStateFieldElement()).on('change', onApplyFilter);
  $(groupPicker.getStateFieldElement()).on('change', onApplyFilter);
  $('li input', $radioGroup).on('change', onApplyFilter);
  $btnSearch.on('click', onApplyFilter);

  return {
    getSearchData,
    dispose
  };
};
