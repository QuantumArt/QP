Quantumart.QP8.BackendUserAndGroupSearchBlock = function (searchBlockElementId, onApplyFilter) {
  let userPicker = $('.pep-user-selector', $searchBlock).data('entity_data_list_component');
  let groupPicker = $('.pep-group-selector', $searchBlock).data('entity_data_list_component');

  let $searchBlock = $(`#${searchBlockElementId}`);
  let $btnSearch = $('.pep-search-button', $searchBlock);
  let $radioGroup = $('.radioButtonsList', $searchBlock);

  let getSearchData = function () {
    let users = userPicker.getSelectedEntities();
    let groups = groupPicker.getSelectedEntities();
    let type = $('li input:checked', $radioGroup).val();

    if (type == 1 && users[0]) {
      return { userId: users[0].Id };
    } else if (type == 2 && groups[0]) {
      return { groupId: groups[0].Id };
    }

    return {};
  }

  let dispose = function () {
    $(userPicker.getStateFieldElement()).off('change', onApplyFilter);
    $(groupPicker.getStateFieldElement()).off('change', onApplyFilter);
    $('li input', $radioGroup).off();
    $btnSearch.off();
  }

  $c.initAllSwitcherLists($searchBlock);
  $c.initAllEntityDataLists($searchBlock);

  $(userPicker.getStateFieldElement()).on('change', onApplyFilter);
  $(groupPicker.getStateFieldElement()).on('change', onApplyFilter);
  $('li input', $radioGroup).on('change', onApplyFilter);
  $btnSearch.on('click', onApplyFilter);

  return {
    getSearchData: getSearchData,
    dispose: dispose
  };
};
