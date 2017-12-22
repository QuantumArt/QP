Quantumart.QP8.BackendContentSearchBlock = function (
  searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options
) {
  Quantumart.QP8.BackendContentSearchBlock.initializeBase(
    this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]
  );
  this._onChangeComboHandler = jQuery.proxy(this._onChangeCombo, this);
};

Quantumart.QP8.BackendContentSearchBlock.prototype = {
  _minSearchBlockHeight: 80,
  _maxSearchBlockHeight: 80,
  _contentGroupListElement: null,
  _siteListElement: null,
  _contentNameElement: null,

  getSearchQuery() {
    let groupId = null;
    let siteId = null;
    let contentName = null;

    if (this._contentGroupListElement) {
      groupId = $(this._contentGroupListElement).find('option:selected').val();
    }
    if (this._siteListElement) {
      siteId = $(this._siteListElement).find('option:selected').val();
    }
    if (this._contentNameElement) {
      contentName = $(this._contentNameElement).val();
    }

    return JSON.stringify({
      GroupId: groupId,
      SiteId: siteId,
      ContentName: contentName
    });
  },

  renderSearchBlock() {
    if (!this.get_isRendered()) {
      let serverContent;
      $q.getJsonFromUrl(
        'GET',
        `${window.CONTROLLER_URL_CONTENT_SEARCH_BLOCK}SearchBlock/${this._parentEntityId}`,
        {
          actionCode: this._actionCode,
          hostId: this._hostId
        },
        false,
        false,
        data => {
          if (data.success) {
            serverContent = data.view;
          } else {
            $q.alertFail(data.message);
          }
        },
        jqXHR => {
          serverContent = null;
          $q.processGenericAjaxError(jqXHR);
        });
      if (!$q.isNullOrWhiteSpace(serverContent)) {
        $(this._concreteSearchBlockElement).html(serverContent);

        this._contentGroupListElement = $('.contentGroupList', this._searchBlockElement).get(0);
        this._siteListElement = $('.siteList', this._searchBlockElement).get(0);
        this._contentNameElement = $('.contentNameText', this._searchBlockElement).get(0);
        $('.csFilterCombo', this._searchBlockElement).bind('change', this._onChangeComboHandler);
      }

      this.set_isRendered(true);
    }
  },

  _onChangeCombo() {
    $(this._findButtonElement).trigger('click');
  },

  _onFindButtonClick() {
    let eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, this.getSearchQuery());
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
    eventArgs = null;
  },

  _onResetButtonClick() {
    $('.csFilterCombo', this._searchBlockElement).find("option[value='']").prop('selected', true);
    $('.csFilterTextbox', this._searchBlockElement).val('');

    let eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, null);
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_RESET_START, eventArgs);
    eventArgs = null;
  },

  dispose() {
    $('.csFilterCombo', this._searchBlockElement).unbind();

    this._contentGroupListElement = null;
    this._siteListElement = null;
    this._contentNameElement = null;
    this._onChangeComboHandler = null;

    Quantumart.QP8.BackendContentSearchBlock.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendContentSearchBlock.registerClass(
  'Quantumart.QP8.BackendContentSearchBlock', Quantumart.QP8.BackendSearchBlockBase
);
