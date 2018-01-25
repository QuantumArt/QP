import { ActionLogFilterBase } from './ActionLogFilterBase';
import { RelationFieldSearch } from '../Search/BackendArticleSearchBlock/RelationFieldSearch';

export class ActionLogUserFilter extends ActionLogFilterBase {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor(filterContainer) {
    super(filterContainer);
  }

  initialize() {
    this.$container.addClass('fieldSearchContainerContent');
    this.userSearch = new RelationFieldSearch(
      this.$container, 0, -4, '', '', $e.ArticleFieldSearchType.M2MRelation
    );
    this.userSearch.initialize();
  }

  getFilterDetails() {
    return this.userSearch.getFilterDetails();
  }

  getValue() {
    return this.userSearch.getSearchQuery().QueryParams[0];
  }

  onOpen() {
    if (this.userSearch) {
      this.userSearch.onOpen();
    }
  }


  dispose() {
    this.userSearch.dispose();
    this.userSearch = null;
    super.dispose();
  }
}


Quantumart.QP8.ActionLogUserFilter = ActionLogUserFilter;
