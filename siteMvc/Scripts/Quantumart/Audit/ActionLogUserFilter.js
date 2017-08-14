Quantumart.QP8.ActionLogUserFilter = function (filterContainer) {
	Quantumart.QP8.ActionLogUserFilter.initializeBase(this, [filterContainer]);
};

Quantumart.QP8.ActionLogUserFilter.prototype = {
	initialize: function () {
		this.$container.addClass("fieldSearchContainerContent");
		this.userSearch = new Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch(this.$container, 0, -4, "", "", $e.ArticleFieldSearchType.M2MRelation);
		this.userSearch.initialize();
	},

	get_filterDetails: function () {
		return this.userSearch.get_filterDetails();
	},

	get_value: function () {
		return this.userSearch.get_searchQuery().QueryParams[0];
	},

	onOpen: function () {
		if (this.userSearch) {
			this.userSearch.onOpen();
		}
	},


	dispose: function () {
		this.userSearch.dispose();
		this.userSearch = null;
		Quantumart.QP8.ActionLogUserFilter.callBaseMethod(this, "dispose");
	}
};

Quantumart.QP8.ActionLogUserFilter.registerClass("Quantumart.QP8.ActionLogUserFilter", Quantumart.QP8.ActionLogFilterBase);
