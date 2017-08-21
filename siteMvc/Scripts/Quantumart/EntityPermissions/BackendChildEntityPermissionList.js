Quantumart.QP8.BackendChildEntityPermissionList = function (searchBlockElementId) {
  const $searchBlock = jQuery(`#${searchBlockElementId}`);
  const $grid = jQuery('.pep-grid', $searchBlock);
  const gridComponent = $grid.data('tGrid');

  const onApplyFilter = function () {
    gridComponent.ajaxRequest();
  };

  const searchBlockComponent = new Quantumart.QP8.BackendUserAndGroupSearchBlock(
    searchBlockElementId,
    $.proxy(onApplyFilter, this)
  );

  const onDataBinding = function (e) {
    // eslint-disable-next-line no-param-reassign
    e.data = Object.assign({}, e.data, searchBlockComponent.getSearchData());
  };

  const dispose = function () {
    searchBlockComponent.dispose();
    $grid.off();
  };

  const modifyEventArgsContext = function (eventArgsContext) {
    return Object.assign(eventArgsContext || {}, { additionalUrlParameters: searchBlockComponent.getSearchData() });
  };

  $grid.off('dataBinding', gridComponent.onDataBinding).on('dataBinding', onDataBinding);

  return {
    dispose,
    modifyEventArgsContext
  };
};
