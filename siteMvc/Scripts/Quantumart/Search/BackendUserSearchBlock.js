Quantumart.QP8.BackendUserSearchBlock = function (searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
  Quantumart.QP8.BackendUserSearchBlock.initializeBase(this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]);
};

Quantumart.QP8.BackendUserSearchBlock.prototype = {
  _minSearchBlockHeight: 145,
  _maxSearchBlockHeight: 145,

  get_searchQuery () {
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
  },

  renderSearchBlock () {
    if (this.get_isRendered() !== true) {
      $q.getJsonFromUrl(
        'GET',
        `${window.CONTROLLER_URL_USER}SearchBlock/${this._parentEntityId}`,
        {
          hostId: this._hostId
        },
        false,
        false
      )
        .done($.proxy(function (data) {
          if (data.success) {
            const serverContent = data.view;
            $(this._concreteSearchBlockElement).html(serverContent);
            this.set_isRendered(true);
          } else {
            $q.alertFail(data.message);
          }
        }, this))
        .fail((jqXHR, textStatus, errorThrown) => {
          $q.processGenericAjaxError(jqXHR);
        });
    }
  },

  _onFindButtonClick () {
    const eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, this.get_searchQuery());
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
  },

  _onResetButtonClick () {
    $('.csFilterTextbox', this._concreteSearchBlockElement).val('');
    const eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, null);
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_RESET_START, eventArgs);
  },

  dispose () {
    Quantumart.QP8.BackendUserSearchBlock.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendUserSearchBlock.registerClass('Quantumart.QP8.BackendUserSearchBlock', Quantumart.QP8.BackendSearchBlockBase);
