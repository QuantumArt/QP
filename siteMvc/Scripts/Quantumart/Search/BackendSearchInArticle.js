import { BackendEntityGridManager } from '../Managers/BackendEntityGridManager';

export class BackendSearchInArticle {
  /** @type {JQuery} */
  $searchInput = null;
  /** @type {JQuery} */
  $searchButton = null;

  /**
   * @param {string} gridElementId
   * @param {string} searchBlockElementId
   * @param {string} initQuery
   * @param {Quantumart.QP8.BackendDocumentContext} documentContext
   */
  constructor(gridElementId, searchBlockElementId, initQuery, documentContext) {
    this._gridElementId = gridElementId;
    this._searchBlockElementId = searchBlockElementId;
    this._onSearchButtonClick = this._onSearchButtonClick.bind(this);
    this._initQuery = initQuery;
    this._documentContext = documentContext;
  }

  initialize() {
    this.$searchInput = $(`#${this._searchBlockElementId}`).find('input.textbox');
    this.$searchButton = $(`#${this._searchBlockElementId}`).find('.button');
    this.$searchButton.bind('click', this._onSearchButtonClick);
    this._refreshQuery(this._initQuery);
  }

  _onSearchButtonClick() {
    const searchQuery = this.$searchInput.val();
    BackendEntityGridManager.getInstance().resetGrid(this._gridElementId, {
      searchQuery
    });
  }

  /**
   * @param {Quantumart.QP8.BackendEventArgs} eventArgs
   */
  refreshQuery(eventArgs) {
    const context = eventArgs.get_context();
    const query
      = context && context.additionalUrlParameters
        ? context.additionalUrlParameters.query
        : '';
    this._documentContext.getHost()._additionalUrlParameters = null;
    this._refreshQuery(query);
  }

  /**
   * @param {string} query
   */
  _refreshQuery(query) {
    if (query) {
      this.$searchInput.val(query);
      this.$searchButton.trigger('click');
    }
  }

  dispose() {
    this.$searchButton.unbind('click');
  }
}

Quantumart.QP8.BackendSearchInArticle = BackendSearchInArticle;
