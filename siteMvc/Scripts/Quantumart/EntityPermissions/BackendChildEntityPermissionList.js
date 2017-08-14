Quantumart.QP8.BackendChildEntityPermissionList = function (searchBlockElementId) {
  let $searchBlock = jQuery(`#${searchBlockElementId}`);
  let $grid = jQuery('.pep-grid', $searchBlock);
  let gridComponent = $grid.data('tGrid');

  let onApplyFilter = function () {
    gridComponent.ajaxRequest();
  };

  let searchBlockComponent = new Quantumart.QP8.BackendUserAndGroupSearchBlock(searchBlockElementId, $.proxy(onApplyFilter, this));
  let onDataBinding = function (e) {
    e.data = Object.assign({}, e.data, searchBlockComponent.getSearchData());
  };

  let dispose = function () {
    searchBlockComponent.dispose();
    $grid.off();
  };

  let modifyEventArgsContext = function (eventArgsContext) {
    return Object.assign(eventArgsContext || {}, { additionalUrlParameters: searchBlockComponent.getSearchData() });
  };

  $grid.off('dataBinding', gridComponent.onDataBinding).on('dataBinding', onDataBinding);

  return {
    dispose: dispose,
    modifyEventArgsContext: modifyEventArgsContext
  };
};
