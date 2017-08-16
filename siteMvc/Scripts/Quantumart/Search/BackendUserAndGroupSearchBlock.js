Quantumart.QP8.BackendUserAndGroupSearchBlock = class BackendUserAndGroupSearchBlock {
  constructor(searchBlockElementId, onApplyFilter) {
    this.onApplyFilter = onApplyFilter;
    this.$searchBlock = $(`#${searchBlockElementId}`);
    this.$btnSearch = $('.pep-search-button', this.$searchBlock);
    this.$radioGroup = $('.radioButtonsList', this.$searchBlock);

    this.initPickers();
    this.bindEvents();
  }

  initPickers() {
    $c.initAllSwitcherLists(this.$searchBlock);
    $c.initAllEntityDataLists(this.$searchBlock);
    this.userPicker = $('.pep-user-selector', this.$searchBlock).data('entity_data_list_component');
    this.groupPicker = $('.pep-group-selector', this.$searchBlock).data('entity_data_list_component');
  }

  bindEvents() {
    $(this.userPicker.getStateFieldElement()).on('change', this.onApplyFilter);
    $(this.groupPicker.getStateFieldElement()).on('change', this.onApplyFilter);
    $('li input', this.$radioGroup).on('change', this.onApplyFilter);
    this.$btnSearch.on('click', this.onApplyFilter);
  }

  getSearchData() {
    const users = this.userPicker.getSelectedEntities();
    const groups = this.groupPicker.getSelectedEntities();
    const type = $('li input:checked', this.$radioGroup).val();

    if (type === 1 && users[0]) {
      return { userId: users[0].Id };
    } else if (type === 2 && groups[0]) {
      return { groupId: groups[0].Id };
    }

    return {};
  }

  dispose() {
    $(this.userPicker.getStateFieldElement()).off('change', this.onApplyFilter);
    $(this.groupPicker.getStateFieldElement()).off('change', this.onApplyFilter);
    $('li input', this.$radioGroup).off();
    this.$btnSearch.off();
  }
};
