Quantumart.QP8.BackendChildEntityPermissionList = function (searchBlockElementId) {
	function onDataBinding(e) {
		e.data = jQuery.extend(e.data, searchBlockComponent.getSearchData());
	}


	function onApplyFilter() {
		gridComponent.ajaxRequest();
	}

	function dispose() {
		searchBlockComponent.dispose();
		searchBlockComponent = null;

		$grid.off();
		$grid = null;
		gridComponent = null;
	}

	function modifyEventArgsContext(eventArgsContext) {
		return jQuery.extend(eventArgsContext, { additionalUrlParameters: searchBlockComponent.getSearchData() });
	}

	var searchBlockComponent = new Quantumart.QP8.BackendUserAndGroupSearchBlock(searchBlockElementId, jQuery.proxy(onApplyFilter, this));
	var $searchBlock = jQuery('#' + searchBlockElementId);
	var $grid = jQuery('.pep-grid', $searchBlock);
	var gridComponent = $grid.data("tGrid");

	$grid.off("dataBinding", gridComponent.onDataBinding)
		 .on("dataBinding", onDataBinding);

	return {
		dispose: dispose,
		modifyEventArgsContext: modifyEventArgsContext
	};
};
