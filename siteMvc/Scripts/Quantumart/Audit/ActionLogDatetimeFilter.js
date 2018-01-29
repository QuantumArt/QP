import { ActionLogFilterBase } from './ActionLogFilterBase';
import { DateOrTimeRangeFieldSearch } from '../Search/BackendArticleSearchBlock/DateOrTimeRangeFieldSearch';

export class ActionLogDatetimeFilter extends ActionLogFilterBase {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor(filterContainer) {
    super(filterContainer);
  }

  initialize() {
    this.$container.addClass('fieldSearchContainerContent');
    this.dtFieldSearch = new DateOrTimeRangeFieldSearch(
      this.$container, 0, 0, '', '', $e.ArticleFieldSearchType.DateTimeRange
    );
    this.dtFieldSearch.initialize();
  }

  getFilterDetails() {
    return this.dtFieldSearch.getFilterDetails();
  }

  getValue() {
    const sq = this.dtFieldSearch.getSearchQuery();
    if (!sq.QueryParams[3]) {
      return {
        from: sq.QueryParams[1],
        to: sq.QueryParams[2]
      };
    }
    return {
      from: sq.QueryParams[1]
    };
  }


  dispose() {
    this.dtFieldSearch.dispose();
    this.dtFieldSearch = null;
    super.dispose();
  }
}


Quantumart.QP8.ActionLogDatetimeFilter = ActionLogDatetimeFilter;
