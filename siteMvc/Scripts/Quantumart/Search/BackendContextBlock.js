Quantumart.QP8.BackendContextBlock = function (
  searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options
) {
  Quantumart.QP8.BackendContextBlock.initializeBase(
    this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]
  );
  this._onChangeComboHandler = jQuery.proxy(this._onChangeCombo, this);
};

Quantumart.QP8.BackendContextBlock.prototype
= {
    _minSearchBlockHeight: 80,
    _maxSearchBlockHeight: 80,
    _contendGroupListElement: null,
    _siteListElement: null,
    _contentNameElement: null,
    _searchBlockState: null,

    getSearchBlockState() {
      return this._searchBlockState;
    },

    restoreSearchBlockState() {
      if (this._searchBlockState) {
        const fieldValues = jQuery.map(this._searchBlockState, elem => {
          return { fieldName: elem.Name, value: elem.Value };
        });
        $c.setAllEntityDataListValues(this._searchBlockElement, fieldValues);
      }
    },

    _computeSearchBlockState() {
      const result = [];
      $('.contextSwitcher .stateField', this._searchBlockElement).each(function () {
        const $item = $(this);
        result.push({
          Name: $item.prop('name'),
          Value: $item.prop('value'),
          FieldId: $item.closest('.contextSwitcher').data('list_id')
        });
      });
      return result;
    },


    getSearchQuery() {
      return JSON.stringify(this.getSearchBlockState());
    },

    renderSearchBlock() {
      if (!this._isRendered) {
        // получить разметку с сервера
        let serverContent;
        $q.getJsonFromUrl(
          'GET',
          `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}ContextBlock/${this._parentEntityId}`,
          {
            actionCode: this._actionCode,
            hostId: this._hostId
          },
          false,
          false,
          (data, textStatus, jqXHR) => {
            if (data.success) {
              serverContent = data.view;
            } else {
              $q.alertFail(data.message);
            }
          },
          (jqXHR, textStatus, errorThrown) => {
            serverContent = null;
            $q.processGenericAjaxError(jqXHR);
          });
        if (!$q.isNullOrWhiteSpace(serverContent)) {
          $(this._concreteSearchBlockElement).html(serverContent);

          $c.initAllEntityDataLists(this._searchBlockElement);

          this.restoreSearchBlockState();

          const that = this;
          $('.contextSwitcher').each(function () {
            const component = $(this).data('entity_data_list_component');
            component.attachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, that._onChangeComboHandler);
          });
        }

        this._isRendered = true;
      }
    },


    _onChangeCombo() {
      this._searchBlockState = this._computeSearchBlockState();
      $(this._findButtonElement).trigger('click');
    },


    _onFindButtonClick() {
      const state = this.getSearchBlockState();
      let eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, JSON.stringify(state));
      eventArgs.setSearchBlockState(state);
      this.notify(window.EVENT_TYPE_CONTEXT_BLOCK_FIND_START, eventArgs);
      eventArgs = null;
    },

    _onResetButtonClick() {
      // default implementation
    },

    dispose() {
      const that = this;
      $('.contextSwitcher').each(function () {
        const component = $(this).data('entity_data_list_component');
        component.detachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, that._onChangeComboHandler);
      });

      $c.destroyAllEntityDataLists(this._searchBlockElement);
      Quantumart.QP8.BackendContextBlock.callBaseMethod(this, 'dispose');
    }
  };

Quantumart.QP8.BackendContextBlock.registerClass(
  'Quantumart.QP8.BackendContextBlock', Quantumart.QP8.BackendSearchBlockBase
);
