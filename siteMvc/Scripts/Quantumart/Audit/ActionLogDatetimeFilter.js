Quantumart.QP8.ActionLogDatetimeFilter = function (filterContainer) {
  Quantumart.QP8.ActionLogDatetimeFilter.initializeBase(this, [filterContainer]);
};

Quantumart.QP8.ActionLogDatetimeFilter.prototype = {
  initialize () {
    this.$container.addClass('fieldSearchContainerContent');
    this.dtFieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch(this.$container, 0, 0, '', '', $e.ArticleFieldSearchType.DateTimeRange);
    this.dtFieldSearch.initialize();
  },

  get_filterDetails () {
    return this.dtFieldSearch.get_filterDetails();
  },

  get_value () {
    const sq = this.dtFieldSearch.get_searchQuery();
    if (!sq.QueryParams[3]) {
      return {
        from: sq.QueryParams[1],
        to: sq.QueryParams[2]
      };
    }
    return {
      from: sq.QueryParams[1]
    };

  },


  dispose () {
    this.dtFieldSearch.dispose();
    this.dtFieldSearch = null;
    Quantumart.QP8.ActionLogDatetimeFilter.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.ActionLogDatetimeFilter.registerClass('Quantumart.QP8.ActionLogDatetimeFilter', Quantumart.QP8.ActionLogFilterBase);
