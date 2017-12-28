import { BackendSearchBlockBase, BackendSearchBlockEventArgs } from './BackendSearchBlockBase';
import { $q } from '../Utils';

export class BackendUserSearchBlock extends BackendSearchBlockBase {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor(
    searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
    super(
      searchBlockGroupCode,
      searchBlockElementId,
      entityTypeCode,
      parentEntityId,
      options
    );
  }

  _minSearchBlockHeight = 145;
  _maxSearchBlockHeight = 145;

  getSearchQuery() {
    const $root = $(this._concreteSearchBlockElement);

    const login = $('.login', $root).val();
    const email = $('.email', $root).val();
    const firstName = $('.firstName', $root).val();
    const lastName = $('.lastName', $root).val();

    const query = JSON.stringify({
      Login: login,
      Email: email,
      FirstName: firstName,
      LastName: lastName
    });

    return query;
  }

  renderSearchBlock() {
    if (!this.get_isRendered()) {
      $q.getJsonFromUrl(
        'GET',
        `${window.CONTROLLER_URL_USER}SearchBlock/${this._parentEntityId}`,
        {
          hostId: this._hostId
        },
        false,
        false
      ).done($.proxy(function (data) {
        if (data.success) {
          const serverContent = data.view;
          $(this._concreteSearchBlockElement).html(serverContent);
          this.set_isRendered(true);
        } else {
          $q.alertFail(data.message);
        }
      }, this))
        .fail(jqXHR => {
          $q.processGenericAjaxError(jqXHR);
        });
    }
  }

  _onFindButtonClick() {
    const eventArgs = new BackendSearchBlockEventArgs(0, this.getSearchQuery());
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
  }

  _onResetButtonClick() {
    $('.csFilterTextbox', this._concreteSearchBlockElement).val('');
    const eventArgs = new BackendSearchBlockEventArgs(0, null);
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_RESET_START, eventArgs);
  }

  dispose() {
    super.dispose();
  }
}


Quantumart.QP8.BackendUserSearchBlock = BackendUserSearchBlock;
