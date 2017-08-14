Quantumart.QP8.BackendContextBlock = function (searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
  Quantumart.QP8.BackendContextBlock.initializeBase(this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]);
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

    get_searchBlockState: function () {
      return this._searchBlockState;
    },

    _restore_searchBlockState: function () {
      if (this._searchBlockState) {
        var fieldValues = jQuery.map(this._searchBlockState, (elem) => {
          return { fieldName: elem.Name, value: elem.Value };
        });
        $c.setAllEntityDataListValues(this._searchBlockElement, fieldValues);
      }
    },

    _compute_searchBlockState: function () {
      var result = [];
      $('.contextSwitcher .stateField', this._searchBlockElement).each(function () {
        var $item = $(this);
        result.push({
          Name: $item.prop('name'),
          Value: $item.prop('value'),
          FieldId: $item.closest('.contextSwitcher').data('list_id')
        });
      });
      return result;
    },


    get_searchQuery: function () {
      return JSON.stringify(this.get_searchBlockState());
    },

    renderSearchBlock: function () {
      if (!this._isRendered) {
        // получить разметку с сервера
        var serverContent;
        $q.getJsonFromUrl(
          'GET',
          `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK  }ContextBlock/${  this._parentEntityId}`,
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

          this._restore_searchBlockState();

          var self = this;
          $('.contextSwitcher').each(function () {
            var component = $(this).data('entity_data_list_component');
            component.attachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, self._onChangeComboHandler);
          });
        }

        this._isRendered = true;
      }
    },


    _onChangeCombo: function () {
      this._searchBlockState = this._compute_searchBlockState();
      $(this._findButtonElement).trigger('click');
    },


    _onFindButtonClick: function () {
      var state = this.get_searchBlockState();
      var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, JSON.stringify(state));
      eventArgs.set_searchBlockState(state);
      this.notify(window.EVENT_TYPE_CONTEXT_BLOCK_FIND_START, eventArgs);
      eventArgs = null;
    },

    _onResetButtonClick: function () {

    },

    dispose: function () {
      var self = this;
      $('.contextSwitcher').each(function () {
        var component = $(this).data('entity_data_list_component');
        component.detachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, self._onChangeComboHandler);
      });

      $c.destroyAllEntityDataLists(this._searchBlockElement);
      Quantumart.QP8.BackendContextBlock.callBaseMethod(this, 'dispose');
    }
  };

Quantumart.QP8.BackendContextBlock.registerClass('Quantumart.QP8.BackendContextBlock', Quantumart.QP8.BackendSearchBlockBase);
