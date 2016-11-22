// ********************************************************************************************
// *** Контейнерный элемент, в котором располагается содержимое документа         ***
// ********************************************************************************************

//#region Document Host Types
var DOCUMENT_HOST_TYPE_EDITING_DOCUMENT = 1;
var DOCUMENT_HOST_TYPE_POPUP_WINDOW = 2;

//#endregion

var EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED = 'onHostExternalCallerContextsUnbinded';

//#region class BackendDocumentHost
// === Абстрактный класс контейнера, в котором располагается содержимое документа" ===
Quantumart.QP8.BackendDocumentHost = function(eventArgs, options) {
  Quantumart.QP8.BackendDocumentHost.initializeBase(this);

  if ($q.isObject(eventArgs)) {
    this._startedByExternal = eventArgs.get_startedByExternal();
  }

  if ($q.isObject(options)) {
    if (options.hostStateStorage) {
      this._hostStateStorage = options.hostStateStorage;
    }
  }

  this._externalCallerContexts = [];
  this._onSearchHandler = jQuery.proxy(this.onSearch, this);
  this._onContextSwitchingHandler = jQuery.proxy(this.onContextSwitching, this);
  this._onSearchBlockResizeHandler = jQuery.proxy(this.onSearchBlockResize, this);
  this._onGeneralEventHandler = jQuery.proxy(this.onGeneralEvent, this);
};

Quantumart.QP8.BackendDocumentHost.prototype = {
  _documentWrapperElementId: '', // идентификатор DOM-элемента, образующего документ
  _documentWrapperElement: null, // DOM-элемент, образующий документ
  _documentUrl: '', // URL документа
  _documentPostParams: null, // параметры, которые должны передаваться через POST
  _documentContext: null, // изменяемый контекст документа

  _entityTypeCode: '', // код типа сущности
  _entityId: 0, // идентификатор сущности
  _entityName: '', // название сущности
  _parentEntityId: 0, // идентификатор родительской сущности;
  _isMultipleEntities: false, // признак привязки к нескольким сущностям
  _entities: null, // список сущностей
  _actionCode: '', // код действия
  _actionTypeCode: '', // код типа действия
  _isCustomAction: false, // является ли действие пользовательским

  _breadCrumbsComponent: null, // компонент "Хлебные крошки"
  _actionToolbarComponent: null, // компонент "Панель инструментов" для действий
  _useCustomActionToolbar: false,
  _viewToolbarComponent: null, // компонент "Панель инструментов" для представлений
  _searchBlockComponent: null, // компонент "Блок поиска"
  _oldSearchBlockHeight: 0, // старая высота блока поиска
  _isContextBlockVisible: false, // признак, разрешающий отображение блока контекста
  _selectedEntities: null, // выбранные сущности
  _selectedParentEntityId: 0, // ID родителя для выбранных сущностей
  _filter: '', // фильтр сущностей

  _hostStateStorage: null, // хранилище состояния хоста
  _callerContext: null, // контекст порождающего хоста
  _externalCallerContexts: null, // контексты порождающих CustomAction-хостов
  _eventArgsAdditionalData: null,
  _selectedEntitiesContext: null, // контекст выбора

  _onSearchHandler: null,
  _onContextSwitchingHandler: null,
  _onSearchBlockResizeHandler: null,
  _onGeneralEventHandler: null,

  _startedByExternal: false,

  _isCloseForced: false,

  _unnamedEntitiesLimit: 1000,

  get_documentWrapperElementId: function() {
    return this._documentWrapperElementId;
  },
  get_documentWrapperElement: function() {
    return this._documentWrapperElement;
  },
  get_documentUrl: function() {
    return this._documentUrl;
  },
  get_selectedEntities: function() {
    return this._selectedEntities;
  },
  get_filter: function() {
    return this._filter;
  },

  get_hostType: $c.notImplemented,
  get_zIndex: $c.notImplemented,

  get_documentContext: function() {
    return this._documentContext;
  },
  set_documentContext: function(value) {
    this._documentContext = value;
  },

  get_entityTypeCode: function() {
    return this._entityTypeCode;
  },
  set_entityTypeCode: function(value) {
    this._entityTypeCode = value;
  },
  get_entityId: function() {
    return this._entityId;
  },
  set_entityId: function(value) {
    this._entityId = value;
  },
  get_entityName: function() {
    return this._entityName;
  },
  set_entityName: function(value) {
    this._entityName = value;
  },
  get_parentEntityId: function() {
    return this._parentEntityId;
  },
  set_parentEntityId: function(value) {
    this._parentEntityId = value;
  },
  get_isMultipleEntities: function() {
    return this._isMultipleEntities;
  },
  set_isMultipleEntities: function(value) {
    this._isMultipleEntities = value;
  },
  get_isCustomAction: function() {
    return this._isCustomAction;
  },
  set_isCustomAction: function(value) {
    this._isCustomAction = value;
  },
  get_entities: function() {
    return this._entities;
  },
  set_entities: function(value) {
    this._entities = value;
  },
  get_actionCode: function() {
    return this._actionCode;
  },
  set_actionCode: function(value) {
    this._actionCode = value;
  },
  get_actionTypeCode: function() {
    return this._actionTypeCode;
  },
  set_actionTypeCode: function(value) {
    this._actionTypeCode = value;
  },
  get_externalCallerContexts: function() {
    return this._externalCallerContexts;
  },

  get_eventArgsAdditionalData: function() {
    return this._eventArgsAdditionalData;
  },
  set_eventArgsAdditionalData: function(value) {
    this._eventArgsAdditionalData = value;
  },

  _applyEventArgs: function(eventArgs, saveCallerContext) {
    if ($q.isObject(eventArgs)) {
      this.set_entityTypeCode(eventArgs.get_entityTypeCode());
      this.set_entityId(eventArgs.get_entityId());
      this.set_entityName(eventArgs.get_entityName());
      this.set_parentEntityId(eventArgs.get_parentEntityId());
      this.set_isMultipleEntities(eventArgs.get_isMultipleEntities());
      this.set_isCustomAction(eventArgs.get_isCustomAction());
      this.set_entities(eventArgs.get_entities());
      this.set_actionCode(eventArgs.get_actionCode());
      this.set_actionTypeCode(eventArgs.get_actionTypeCode());
      this.set_eventArgsAdditionalData(eventArgs.get_additionalData());
      if (saveCallerContext) {
        this._callerContext = eventArgs.get_callerContext();
      }
    }
  },

  _getAppliedEventArgs: function() {
    var eventArgs = new Quantumart.QP8.BackendEventArgs();

    eventArgs.set_entityTypeCode(this.get_entityTypeCode());
    eventArgs.set_entityId(this.get_entityId());
    eventArgs.set_entityName(this.get_entityName());
    eventArgs.set_parentEntityId(this.get_parentEntityId());
    eventArgs.set_isMultipleEntities(this.get_isMultipleEntities());
    eventArgs.set_isCustomAction(this.get_isCustomAction());
    eventArgs.set_entities(this.get_entities());
    eventArgs.set_actionCode(this.get_actionCode());
    eventArgs.set_actionTypeCode(this.get_actionTypeCode());
    eventArgs.set_additionalData(this.get_eventArgsAdditionalData());
    return eventArgs;
  },

  bindExternalCallerContext: function(eventArgs) {
    if (eventArgs) {
      var bindedContext = eventArgs.get_externalCallerContext();

      if (bindedContext) {
        if (jQuery.grep(this._externalCallerContexts, function(c) {
          return c.hostUID == bindedContext.hostUID;
        }).length == 0) {
          this._externalCallerContexts.push(bindedContext);
        }
      }
    }
  },

  unbindExternalCallerContexts: function(reason) {
    var externalCallerContexts = this._externalCallerContexts;

    this._externalCallerContexts = [];

    // Если есть родительские внешние документы, то уведомить их об отвязке
    if (!$q.isNullOrEmpty(externalCallerContexts)) {
      this._onExternalCallerContextsUnbinded(
        {
          reason: reason,
          externalCallerContexts: externalCallerContexts
        });
    }

    if (reason != 'close' && $q.isFunction(this._actionToolbarComponent.getDisabledActionCodes) && !$q.isNullOrEmpty(this._actionToolbarComponent.getDisabledActionCodes())) {
      this.hidePanels();
      this._actionToolbarComponent.setDisabledActionCodes(null);
      this.renderPanels();
    }

    externalCallerContexts = null;
  },

  get_isBindToExternal: function() {
    return !$q.isNullOrEmpty(this._externalCallerContexts);
  },

  _onExternalCallerContextsUnbinded: $c.notImplemented,

  loadHostState: function() {
    if (this._hostStateStorage) {
      var result = this._hostStateStorage.loadHostState({
        entityId: this.get_entityId(),
        parentEntityId: this.get_parentEntityId(),
        actionCode: this.get_actionCode()
      });

      return result;
    }
  },
  getHostStateProp: function(name) {
    var hostState = this.loadHostState();

    if (hostState && hostState.hasOwnProperty(name)) {
      return hostState[name];
    }
  },
  saveHostState: function(state) {
    if (this._hostStateStorage) {
      this._hostStateStorage.saveHostState({
        entityId: this.get_entityId(),
        parentEntityId: this.get_parentEntityId(),
        actionCode: this.get_actionCode()
      }, state);
    }
  },

  fixActionToolbarWidth: function() {
    var $actionToolbar = jQuery(this._actionToolbarComponent.get_toolbarElement()).parents('#actionToolbar');
    var $viewToolbar = jQuery(this._viewToolbarComponent.get_toolbarElement()).parents('#viewToolbar');

    $actionToolbar.width($actionToolbar.parents('#toolbar').width() - $viewToolbar.width() - $viewToolbar.paddingLeft() - $actionToolbar.paddingRight() - 1);
  },

  restoreHostState: function() {
    var state = this.loadHostState();

    if (!$q.isNullOrEmpty(state) &&
    (
    !jQuery.isEmptyObject(state.searchBlockState) ||
    this.get_isSearchBlockVisible() === true
    )
    ) {
      if (!this._searchBlockComponent) {
        this.createSearchBlock();
        this._searchBlockComponent.renderSearchBlock();
      }

      if (this.get_isSearchBlockVisible() === true && this.documentWrapperIsVisible()) {
        this._searchBlockComponent.showSearchBlock();
      } else {
        this._searchBlockComponent.hideSearchBlock();
      }
    }
  },

  _loadDefaultSearchBlockState: function() {
    if ($q.isNullOrEmpty(this.loadHostState())) {
      var filterStates;
      var actionTypeCode = this.get_actionTypeCode();
      var entityTypeCode = this.get_entityTypeCode();

      if ((actionTypeCode == ACTION_TYPE_CODE_LIST || actionTypeCode == ACTION_TYPE_CODE_SELECT || actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_SELECT)
                  && (entityTypeCode == ENTITY_TYPE_CODE_ARTICLE || entityTypeCode == ENTITY_TYPE_CODE_VIRTUAL_ARTICLE)) {
        $q.getJsonFromUrl(
                            'GET',
                            CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + 'DefaultFilterStates',
                    {
                      actionCode: this.get_actionCode(),
                      contentId: this.get_parentEntityId()
                    },
                    false,
                    false,
                    function(data) {
                      if (data.success) {
                        filterStates = data.filterStates;
                      } else {
                        $q.alertFail(data.message);
                      }
                    },
                    function(jqXHR) {
                      filterStates = null;
                      $q.processGenericAjaxError(jqXHR);
                    }
                );
      }

      if (!$q.isNullOrEmpty(filterStates) && $q.isArray(filterStates)) {
        var fieldSearchBlockState = jQuery.map(filterStates, function(item) {
          return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(
          item.SearchType,
          item.FieldId,
                                  item.ContentId,
          item.FieldColumnName,
          item.FieldName,
                                  item.FieldGroup,
            {
              entities: item.SelectedEntities
            });
        });

        var searchQuery = jQuery.map(filterStates, function(item) {
          var ids = jQuery.map(item.SelectedEntities, function(ent) {
            return ent.Id;
          });

          return new Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(item.SearchType, item.FieldId, item.FieldColumnName, null, ids, false, false);
        });

        var defaultSearchBlockState = {
          searchBlockState: {
            fieldSearchBlockState: fieldSearchBlockState,
            defaultFieldSearchBlockState: fieldSearchBlockState
          },
          searchQuery: JSON.stringify(searchQuery)
        };

        this.saveHostState(defaultSearchBlockState);
        return defaultSearchBlockState;
      }
    }
  },

  showDocumentWrapper: function() {
    var $documentWrapper = jQuery(this._documentWrapperElement); // скрытый документ
    $documentWrapper.show();
    $documentWrapper = null;
  },

  hideDocumentWrapper: function() {
    var $documentWrapper = jQuery(this._documentWrapperElement); // скрытый документ
    $documentWrapper.hide();
    $documentWrapper = null;
  },

  documentWrapperIsVisible: function() {
    var $documentWrapper = jQuery(this._documentWrapperElement); // скрытый документ
    var isVisible = $documentWrapper.is(':visible');

    $documentWrapper = null;
    return isVisible;
  },

  executeAction: function(actionCode) {
    var action = $a.getBackendActionByCode(actionCode);

    if (action == null) {
      $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
    } else {
      var entityId = this._entityId;
      var entityName = this._entityName;
      var isMultiple = action.ActionType.IsMultiple;
      var isEditor = Quantumart.QP8.BackendEntityEditor.isInstanceOfType(this.get_mainComponent());
      var isCustomActionHost = Quantumart.QP8.BackendCustomActionHost.isInstanceOfType(this.get_mainComponent());
      var selectedEntities = (!isEditor && !isCustomActionHost) ? this._selectedEntities : [{ Id: this._entityId, Name: this._entityName }];
      var entities = (!isMultiple) ? null : ((!this._isMultipleEntities) ? selectedEntities : Array.clone(this._entities));

      var actionTypeCode = action.ActionType.Code;

      if (isMultiple) {
        var filteredEntities = this._filterEntities(entities);

        if (filteredEntities.length != entities.length) {
          entities = filteredEntities;
        }

        filteredEntities = null;

        if (entities.length == 0) {
          $q.alertError($l.DocumentHost.noEntitiesToExecuteActionErrorMessage);
          this.refresh();
          return;
        }

        var unnamedEntities = jQuery.grep(entities, function(elem) {
          return !elem.Name;
        });

        if (unnamedEntities.length > 0 && unnamedEntities.length <= this._unnamedEntitiesLimit) {
          var entityTypeCode = this._entityTypeCode;

          if (entityTypeCode == ENTITY_TYPE_CODE_CONTENT_PERMISSION)
          entityTypeCode = ENTITY_TYPE_CODE_CONTENT;
          else if (entityTypeCode == ENTITY_TYPE_CODE_ARTICLE_PERMISSION)
          entityTypeCode = ENTITY_TYPE_CODE_ARTICLE;

          var dataItems = $o.getSimpleEntityList(entityTypeCode, this._parentEntityId, 0, 0, Quantumart.QP8.Enums.ListSelectionMode.OnlySelectedItems, $o.getEntityIDsFromEntities(unnamedEntities));
          var namedEntities = jQuery.map(dataItems, function(item) {
            return { Id: $q.toInt(item.Value), Name: item.Text };
          });

          jQuery.each(namedEntities, function(index, elem) {
            for (var i = 0; i < entities.length; i++) {
              var entity = entities[i];

              if (entity.Id == elem.Id) {
                entity.Name = elem.Name;
                break;
              }
            }
          });
        } else if (unnamedEntities.length > this._unnamedEntitiesLimit) {
          for (var i = 0; i < unnamedEntities.length; i++) {
            unnamedEntities[i].Name = unnamedEntities[i].Id;
          }
        }
      }

      if (!isMultiple && (this._actionTypeCode == ACTION_TYPE_CODE_LIST || this._actionTypeCode == ACTION_TYPE_CODE_LIBRARY) && this._selectedEntities.length == 1) {
        entityId = this._selectedEntities[0].Id;
        entityName = this._selectedEntities[0].Name;
      }

      var isCustomAction = action.IsCustom;
      var isMultistepAction = action.IsMultistep;

      if (!isCustomAction && !isMultistepAction && actionTypeCode == ACTION_TYPE_CODE_REFRESH) {
        var main = this.get_mainComponent();

        if (main == null || !Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main) || main.confirmRefresh()) {
          this.refresh();
        }
      } else if (!isCustomAction && !isMultistepAction &&
      (actionTypeCode == ACTION_TYPE_CODE_SAVE ||
      actionTypeCode == ACTION_TYPE_CODE_UPDATE ||
      actionTypeCode == ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE ||
      actionTypeCode == ACTION_TYPE_CODE_SAVE_AND_UP ||
      actionTypeCode == ACTION_TYPE_CODE_UPDATE_AND_UP ||
                           actionTypeCode == ACTION_TYPE_CODE_RESTORE && action.EntityType.Code == ENTITY_TYPE_CODE_ARTICLE_VERSION
      )
      ) {
        var main = this.get_mainComponent();

        if (main != null && Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main)) {
          main.saveEntity(actionCode);
        }
      } else {
        var params = new Quantumart.QP8.BackendActionParameters({
          entityTypeCode: this._entityTypeCode,
          parentEntityId: this._selectedParentEntityId || this._parentEntityId,
          entities: entities,
          entityId: entityId,
          entityName: entityName,
          forceOpenWindow: this._isWindow()
        });

        params.correct(action);

        var eventArgs = $a.getEventArgsFromActionWithParams(action, params);

        eventArgs.set_startedByExternal(this._startedByExternal);
        this.onActionExecuting(eventArgs);

        params = null;
        eventArgs = null;
      }
    }
  },

  _filterEntities: function(entities) {
    if (this._selectedEntitiesContext && this._selectedEntitiesContext.url) {
      var queryData = jQuery.extend({
        page: 1,
        size: 0,
        onlyIds: true
      }, this._selectedEntitiesContext.dataQueryParams);

      for (var i = 0; i < entities.length; i++) {
        queryData['filterIds[' + i + ']'] = entities[i].Id;
      }

      rowsData = null;
      eventArgs = null;

      $q.postDataToUrl(this._selectedEntitiesContext.url, queryData, false, function(data) {
        rowsData = data;
      }, function(jqXHR) {
        $q.processGenericAjaxError(jqXHR);
      });

      return jQuery.map(rowsData.data, function(item) {
        return { Id: item.CONTENT_ITEM_ID };
      });
    }

    return entities;
  },

  get_mainComponent: function() {
    return ($q.isObject(this._documentContext)) ? this._documentContext.get_mainComponent() : null;
  },

  get_viewTypeCode: function() {
    return (this._viewToolbarComponent) ? this._viewToolbarComponent.getSelectedViewTypeCode() : '';
  },

  get_searchQuery: function() {
    var result = '';

    if (this._searchBlockComponent) {
      result = this._searchBlockComponent.get_searchQuery();
    } else {
      var state = this.loadHostState();

      if (!$q.isNullOrEmpty(state) && !$q.isNullOrWhiteSpace(state.searchQuery)) {
        result = state.searchQuery;
      }
    }

    return result;
  },

  get_contextQuery: function() {
    var result = '';

    if (this._contextBlockComponent) {
      result = this._contextBlockComponent.get_searchQuery();
    } else if (this._eventArgsAdditionalData) {
      result = this._eventArgsAdditionalData.contextQuery;
    }

    return result;
  },

  get_contextState: function() {
    var query = this.get_contextQuery();

    if ($q.isNullOrEmpty(query)) {
      return null;
    } else {
      return JSON.parse(query);
    }
  },

  get_currentPage: function() {
    var main = this.get_mainComponent();

    if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
      return main.get_currentPage();
    } else {
      var state = this.loadHostState();

      if (!$q.isNullOrEmpty(state) && !$q.isNullOrWhiteSpace(state.gridCurrentPage)) {
        return state.gridCurrentPage;
      }
    }
  },
  get_orderBy: function() {
    var main = this.get_mainComponent();

    if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
      return main.get_orderBy();
    } else {
      var state = this.loadHostState();

      if (!$q.isNullOrEmpty(state) && !$q.isNullOrWhiteSpace(state.gridOrderBy)) {
        return state.gridOrderBy;
      }
    }
  },

  markPanelsAsBusy: function() {
    this.showLoadingLayer();

    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.markBreadCrumbsAsBusy();
    }

    if (this._actionToolbarComponent) {
      this._actionToolbarComponent.markToolbarAsBusy();
    }

    if (this._viewToolbarComponent) {
      this._viewToolbarComponent.markToolbarAsBusy();
    }
  },

  unmarkPanelsAsBusy: function(hideLoadingLayer) {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.unmarkBreadCrumbsAsBusy();
    }

    if (this._actionToolbarComponent) {
      this._actionToolbarComponent.unmarkToolbarAsBusy();
    }

    if (this._viewToolbarComponent) {
      this._viewToolbarComponent.unmarkToolbarAsBusy();
    }

    this.hideLoadingLayer(hideLoadingLayer);
  },

  showLoadingLayer: $c.notImplemented,
  hideLoadingLayer: $c.notImplemented,
  onChangeContent: $c.notImplemented,
  onActionExecuting: $c.notImplemented,
  onEntityReaded: $c.notImplemented,
  onNeedUp: $c.notImplemented,
  htmlLoadingMethod: $c.notImplemented,
  resetSelectedEntities: $c.notImplemented,
  saveSelectionContext: $c.notImplemented,
  showErrorMessageInDocumentWrapper: $c.notImplemented,
  _onLibraryResized: $c.notImplemented,
  loadHtmlContentToDocumentWrapper: function(callback, options) {
    var self = this;
    var async = true;

    if (options) {
      async = options.asyncReq;
    }

    $q.getJsonFromUrl(
    this.htmlLoadingMethod(),
    this._documentUrl,
    this._documentPostParams,
    async,
    false,
      function(data) {
        self.loadReadyHtmlContent(data);
        if (Quantumart.QP8.Utils.isFunction(callback)) {
          callback(data.success);
        }

        data = null;
      },
      function(jqXHR) {
        // Оповещаем об ошибке при загрузке документа
        self.onDocumentError();
        if (Quantumart.QP8.Utils.isFunction(callback)) {
          callback(false);
        }

        $q.processGenericAjaxError(jqXHR);

        self.showErrorMessageInDocumentWrapper(jqXHR.status);
      }
    );
  },

  _saveSelectedEntitiesContext: function(eventArgs) {
    this._selectedEntitiesContext = eventArgs.get_context();
  },

  refresh: function() {
    this.onDocumentChanging();
    this.resetSelectedEntities();
    this.refreshPanels();
    this.generateDocumentUrl();
    var self = this;

    this.loadHtmlContentToDocumentWrapper(function() {
      jQuery(self._documentWrapperElement).scrollTo(0);
      self.onDocumentChanged();
    });
  },

  changeView: function(controllerActionUrl) {
    this.onDocumentChanging();
    this.generateDocumentUrl({ controllerActionUrl: controllerActionUrl });
    var self = this;

    this.loadHtmlContentToDocumentWrapper(function() {
      jQuery(self._documentWrapperElement).scrollTo(0);
      self.onDocumentChanged();
    });
  },
  changeViewForSettings: function(options) {
    this.onDocumentChanging();
    this.generateDocumentUrl(options);
    var self = this;

    this.loadHtmlContentToDocumentWrapper(function() {
      jQuery(self._documentWrapperElement).scrollTo(0);
      self.onDocumentChanged();
    });
  },
  changeContent: function(eventArgs, isSaveAndUp) {
    if (this.allowChange()) {
      this.onDocumentChanging();
      this.markMainComponentAsBusy();
      if (!isSaveAndUp) {
        this.cancel();
      }

      this._copyContextQueryToEventArgs(eventArgs);

      var appliedEventArgs = this._getAppliedEventArgs();

      var externalCallerContexts = this._externalCallerContexts;

      this._externalCallerContexts = [];
      this.updateDocument(eventArgs);
      this._externalCallerContexts = externalCallerContexts;

      var self = this;

      this._loadDefaultSearchBlockState();
      this.loadHtmlContentToDocumentWrapper(function(success) {
        self.unmarkMainComponentAsBusy();

        if (!success) {
          self.updateDocument(appliedEventArgs);
        } else {
          self.unbindExternalCallerContexts('changed');
        }

        jQuery(self._documentWrapperElement).scrollTo(0);
        self.onDocumentChanged();
      });
    }
  },

  updateDocument: function(eventArgs) {
    var oldActionCode = this.get_actionCode();
    var newActionCode = eventArgs.get_actionCode();

    this._applyEventArgs(eventArgs);
    this.generateDocumentUrl();
    this.updateTitle(eventArgs);

    if (oldActionCode != newActionCode) {
      this.hidePanels();
      this.destroySearchBlock();
      this.destroyContextBlock();
      this.renderPanels();
    }
  },

  _fixDocumentWrapperHeight: function(searchBlockHeight) {
    var $documentWrapper = jQuery(this._documentWrapperElement);
    var oldDocumentWrapperHeight = parseInt($documentWrapper.height(), 10);
    var oldSearchBlockHeight = this._oldSearchBlockHeight;
    var newSearchBlockHeight = oldSearchBlockHeight;
    var newDocumentWrapperHeight = 0;

    if (!$q.isNull(searchBlockHeight)) {
      newSearchBlockHeight = searchBlockHeight;
    }

    if (newSearchBlockHeight > oldSearchBlockHeight) {
      newDocumentWrapperHeight = oldDocumentWrapperHeight - (newSearchBlockHeight - oldSearchBlockHeight);
    } else {
      newDocumentWrapperHeight = oldDocumentWrapperHeight + (oldSearchBlockHeight - newSearchBlockHeight);
    }

    $documentWrapper.height(newDocumentWrapperHeight);
    this._oldSearchBlockHeight = newSearchBlockHeight;
  },

  loadReadyHtmlContent: function(data) {
    var $documentWrapper = jQuery(this._documentWrapperElement);
    var visible = $documentWrapper.is(':visible');

    if (data.success) {
      var scrollData = $documentWrapper.data('scroll_position');

      this.onDocumentUnloaded();
      $documentWrapper.data('scroll_position', scrollData);

      if (visible) {
        $documentWrapper.hide();
      }

      $documentWrapper.empty().html(data.view);
      if (visible) {
        $documentWrapper.show();
      }

      this.onDocumentLoaded();
    } else {
      window.alert(data.message);
      this.onDocumentError();
    }
  },

  onLoadMainComponent: function() {
    var main = this.get_mainComponent();
    if (main) {
      if (Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main)) {
        main.onLoad();
      } else if (Quantumart.QP8.BackendLibrary.isInstanceOfType(main)) {
        main.attachObserver(EVENT_TYPE_LIBRARY_RESIZED, jQuery.proxy(this._onLibraryResized, this));
        main.onLoad();
      } else if (Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
        main.onLoad();
      }
    }
  },

  onSelectMainComponent: function() {
    var main = this.get_mainComponent();

    if (main) {
      if (Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main)) {
        main.onSelect();
      } else if (Quantumart.QP8.BackendLibrary.isInstanceOfType(main)) {
        main.resize();
      } else if (Quantumart.QP8.BackendCustomActionHost.isInstanceOfType(main)) {
        main.onSelect();
      }
    }
  },

  // Вызывается после того как уже открытый хост был выбран в результате выполнения action
  onSelectedThroughExecution: function(eventArgs) {
    this.bindExternalCallerContext(eventArgs);
    this.renderPanels();

    if (this._documentContext) {
      this._documentContext.execSelect(eventArgs);
    }
  },

  unmarkMainComponentAsBusy: function() {
    var main = this.get_mainComponent();
    if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
      main.unmarkGridAsBusy();
    }
  },

  markMainComponentAsBusy: function() {
    var main = this.get_mainComponent();
    if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
      main.markGridAsBusy();
    }
  },

  onSearchBlockResize: function(eventType, sender, args) {
    this._fixDocumentWrapperHeight(args.get_searchBlockHeight());
  },

  onContextSwitching: function(eventType, sender, args) {
    var main = this.get_mainComponent();
    if (main) {
      if (Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
        main.resetGrid({ contextQuery: args.get_searchQuery() });
      } else if (Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main)) {
        main.applyContext(args.get_searchBlockState());
      }
    }
  },

  _getComponentSearchProcessor: function(options) {
    var main = this.get_mainComponent();
    var isGrid = Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main);
    var isTree = Quantumart.QP8.BackendEntityTree.isInstanceOfType(main);

    if (main && (isGrid || isTree)) {
      if (!$q.isNullOrEmpty(options.state.searchBlockState)) {
        options.state.searchQuery = options.searchQuery();
      } else {
        $q.removeProperty(options.state, 'searchQuery');
      }

      if (isGrid) {
        return {
          process: function() {
            main.resetGrid({ searchQuery: options.searchQuery() });
          }
        };
      }

      if (isTree) {
        return {
          process: function() {
            main.searchByTerm({ searchQuery: options.searchQuery() });
          }
        };
      }

      throw new Error('Undefined search processor');
    }
  },

  onSearch: function(eventType, sender, args) {
    var state = this.loadHostState() || {};
    var searchBlockState = args.get_searchBlockState();

    if (!$q.isNullOrEmpty(searchBlockState)) {
      state.searchBlockState = searchBlockState;
    } else {
      $q.removeProperty(state, 'searchBlockState');
    }

    this._getComponentSearchProcessor({
      state: state,
      searchQuery: args.get_searchQuery.bind(args)
    }).process();

    this.saveHostState(state);
  },

  // onSearch: function (eventType, sender, args) {
  //  var main = this.get_mainComponent();
  //  var state = this.loadHostState();
  //  if ($q.isNullOrEmpty(state)) {
  //    state = {};
  //  }
  //  var searchBlockState = args.get_searchBlockState();
  //  if (!$q.isNullOrEmpty(searchBlockState)) {
  //    state.searchBlockState = searchBlockState;
  //  }
  //  else {
  //    $q.removeProperty(state, "searchBlockState");
  //  }
  //  if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
  //    if (!$q.isNullOrEmpty(state.searchBlockState)) {
  //      state.searchQuery = args.get_searchQuery();
  //    }
  //    else {
  //      $q.removeProperty(state, "searchQuery");
  //    }
  //    main.resetGrid({ "searchQuery": args.get_searchQuery() })
  //  }
  //  this.saveHostState(state);
  // },

  onViewChanging: function(eventType, sender, args) {
    var state = this.loadHostState();

    if ($q.isNullOrEmpty(state)) {
      state = {};
    }

    var code = args.get_code();

    state.viewTypeCode = code;
    this.saveHostState(state);

    var main = this.get_mainComponent();

    if (main && Quantumart.QP8.BackendLibrary.isInstanceOfType(main)) {
      main.changeViewType(code);
    } else {
      this.changeView(args.get_controllerActionUrl());
    }
  },

  onEntityLoaded: function(eventArgs) {
    if (eventArgs.get_entityId() != 0) {
      this.updateDocument(eventArgs);
      this.onEntityReaded(eventArgs);
      if (eventArgs.get_isRestored() && eventArgs.get_entityTypeCode() == ENTITY_TYPE_CODE_ARTICLE_VERSION) {
        var message = String.format($l.EntityEditor.versionSuccessfullyRestoredMessage, eventArgs.get_entityId());
        window.alert(message);
      }
    } else if (eventArgs.get_actionTypeCode() == ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE) {
      this.onEntityReaded(eventArgs);
    }
  },

  onEntitySubmitting: function() {
    this.onDocumentChanging();
  },

  onEntitySubmitted: function(eventArgs) {
    this.loadReadyHtmlContent(eventArgs.get_data());
    this.onDocumentChanged();
    this.saveAndUp();
    this.onSaveAndClose();
  },

  onEntitySubmittedError: function() {
    this._isCloseForced = false;
    this.onDocumentChanged();
  },

  saveAndCloseRequest: function() { },
  onSaveAndClose: function() {},

  saveAndUp: function() {
    var context = this.get_documentContext();

    if (context && context.needUp()) {
      this.onNeedUp(context.get_eventArgs());
    }
  },

  onDocumentUnloaded: function() {
    this.destroyAllActionLinks();

    var main = this.get_mainComponent();

    if (main && Quantumart.QP8.BackendLibrary.isInstanceOfType(main)) {
      main.detachObserver(EVENT_TYPE_LIBRARY_RESIZED);
    }

    main = null;

    if (this._documentContext) {
      this._documentContext.dispose();
      this._documentContext = null;
    }
  },

  onDocumentLoaded: function() {
    this.initAllActionLinks();
    this.restoreHostState();
    this.onLoadMainComponent();
    if (this.get_eventArgsAdditionalData() && this.get_eventArgsAdditionalData().restoring === true) {
      var argData = this.get_eventArgsAdditionalData();

      argData.restoring = false;
      argData.initFieldValues = null;
    }

    var context = this.get_documentContext();

    if (context) {
      var state = context.get_state();
      var mainComponentType = context.get_mainComponentType();

      if (mainComponentType == $e.MainComponentType.Editor && state != $e.DocumentContextState.Error) {
        var eventArgs = context.get_eventArgs();

        this._copyContextQueryToEventArgs(eventArgs);
        this.onEntityLoaded(context.get_eventArgs());
      }

      context.onHostLoaded();
    }
  },

  onDocumentError: function() {
    this._isCloseForced = false;
  },

  onSelectionChanged: function(eventArgs) {
    var selectedEntities = Array.clone(eventArgs.get_entities());

    this._selectedEntities = selectedEntities;
    this.saveSelectionContext(eventArgs);
    this._saveSelectedEntitiesContext(eventArgs);
    this.refreshPanels();
  },

  onDataBinding: function() {
    this.onDocumentChanging(true);
  },

  onDataBound: function(eventType, sender, eventArgs) {
    var main = this.get_mainComponent();

    if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
      var state = this.loadHostState();

      if ($q.isNullOrEmpty(state)) {
        state = {};
      }

      if (!$q.isNullOrEmpty(sender.get_currentPage())) {
        state.gridCurrentPage = main.get_currentPage();
      } else {
        $q.removeProperty(state, 'gridCurrentPage');
      }

      if (!$q.isNullOrEmpty(sender.get_orderBy())) {
        state.gridOrderBy = main.get_orderBy();
      } else {
        $q.removeProperty(state, 'gridOrderBy');
      }

      this.saveHostState(state);
    }

    this.onSelectionChanged(eventArgs);
    this.onDocumentChanged(true);
  },

  onGeneralEvent: function(eventType, sender, eventArgs) {
    if (eventType == EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING
      || eventType == EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING
      || eventType == EVENT_TYPE_LIBRARY_ACTION_EXECUTING
      || eventType == EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING
      || eventType == EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK
      || eventType == EVENT_TYPE_ACTION_LINK_CLICK
      || eventType == EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING
      || eventType == EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED) {
      this.onActionExecuting(eventArgs);
    } else if (eventType == EVENT_TYPE_EXTERNAL_ACTION_EXECUTING) {
      if (eventArgs.get_externalCallerContext().data.changeCurrentTab && eventArgs.get_isInterface()) {
        this.onChangeContent(eventType, sender, eventArgs);
      } else {
        this.onActionExecuting(eventArgs);
      }
    } else if (eventType == EVENT_TYPE_ENTITY_GRID_DATA_BINDING) {
      this.onDataBinding(eventArgs);
    } else if (eventType == EVENT_TYPE_ENTITY_GRID_DATA_BOUND || eventType == EVENT_TYPE_LIBRARY_DATA_BOUND || eventType == EVENT_TYPE_ENTITY_TREE_DATA_BOUND) {
      this.onDataBound(eventType, sender, eventArgs);
    } else if (eventType == EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED || eventType == EVENT_TYPE_LIBRARY_ENTITY_SELECTED || eventType == EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED) {
      this.onSelectionChanged(eventArgs);
    } else if (eventType == EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK || eventType == EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK || eventType == EVENT_TYPE_ACTION_LINK_SELF_CLICK) {
      this.onChangeContent(eventType, sender, eventArgs);
    } else if (eventType == EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING) {
      this.onEntitySubmitting();
    } else if (eventType == EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED) {
      this.onEntitySubmitted(eventArgs);
    } else if (eventType == EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR) {
      this.onEntitySubmittedError();
    } else if (eventType == EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING) {
      this.refresh();
    } else if (eventType == EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED) {
      this.onViewChanging(eventType, sender, eventArgs);
    } else if (eventType == EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED) {
      this.onActionToolbarButtonClicked(eventType, sender, eventArgs);
    } else if (eventType == EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED) {
      this.onSearchViewToolbarButtonClicked(eventType, sender, eventArgs);
    } else if (eventType == EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED) {
      this.onContextViewToolbarButtonClicked(eventType, sender, eventArgs);
    } else if (eventType == EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED) {
      this.initAllActionLinks(eventArgs.articleWrapper);
    } else if (eventType == EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING) {
      this.destroyAllActionLinks(eventArgs.articleWrapper);
    }
  },

  getCurrentAction: function() {
    return $a.getBackendActionByCode(this._actionCode);
  },

  cancel: function() {
    var action = this.getCurrentAction();
    var cancelActionCode = (action) ? action.EntityType.CancelActionCode : '';

    if (!$q.isNullOrWhiteSpace(cancelActionCode) && action.ActionType.Code == ACTION_TYPE_CODE_READ && $o.checkEntityExistence(this._entityTypeCode, this._entityId)) {
      this.executeAction(cancelActionCode);
    }
  },

  allowChange: function() {
    var main = this.get_mainComponent();

    return (main == null || !Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main) || main.confirmChange());
  },

  allowClose: function() {
    var main = this.get_mainComponent();

    return (main == null || !Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main) || main.confirmClose());
  },

  selectAllEntities: function() {
    var main = this.get_mainComponent();
    if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
      main.selectAllRows();
    } else {
      window.alert($l.Toolbar.selectAllIsNotAllowed);
    }
  },

  deselectAllEntities: function() {
    var main = this.get_mainComponent();
    if (main && Quantumart.QP8.BackendEntityGrid.isInstanceOfType(main)) {
      main.deselectAllRows();
    } else {
      window.alert($l.Toolbar.selectAllIsNotAllowed);
    }
  },

  getAllActionLinks: function(documentWrapperElement) {
    var $wrapper = $q.toJQuery(documentWrapperElement);
    var result;

    if ($wrapper) {
      result = $wrapper.find('.actionLink');
    } else {
      result = [];
    }

    return result;
  },

  initAllActionLinks: function(documentWrapperElement) {
    var self = this;

    if ($q.isNull(documentWrapperElement)) {
      documentWrapperElement = this._documentWrapperElement;
    }

    this.getAllActionLinks(documentWrapperElement).each(
      function(index, linkElem) {
        var $link = $q.toJQuery(linkElem);

        if (!$q.isNullOrEmpty($link) && $q.isNull($link.data('action_link_component'))) {
          var linkId = $link.attr('id');
          var entityId = $q.toInt($link.data('entity_id'), 0);
          var entityName = $link.data('entity_name');
          var parentEntityId = $q.toInt($link.data('parent_entity_id'), null);
          var actionTypeCode = $link.data('action_type_code');
          var actionCode = $link.data('action_code');
          var actionTargetType = $q.toInt($link.data('action_target_type'), null);
          var context = $link.data('context');

          var actionLink = new Quantumart.QP8.BackendActionLink(linkId,
            {
              entityId: entityId,
              entityName: entityName,
              parentEntityId: parentEntityId,
              actionTypeCode: actionTypeCode,
              actionCode: actionCode,
              actionTargetType: actionTargetType,
              context: context
            }
          );

          actionLink.initialize();
          actionLink.attachObserver(EVENT_TYPE_ACTION_LINK_SELF_CLICK, self._onGeneralEventHandler);
          actionLink.attachObserver(EVENT_TYPE_ACTION_LINK_CLICK, self._onGeneralEventHandler);
          $link.data('action_link_component', actionLink);
        }
      }
    );
  },

  destroyAllActionLinks: function(documentWrapperElement) {
    var self = this;

    if ($q.isNull(documentWrapperElement)) {
      documentWrapperElement = this._documentWrapperElement;
    }

    this.getAllActionLinks(documentWrapperElement).each(
      function(index, linkElem) {
        var $link = $q.toJQuery(linkElem);

        if (!$q.isNullOrEmpty($link)) {
          var actionLink = $link.data('action_link_component');

          if ($q.isObject(actionLink)) {
            actionLink.detachObserver(EVENT_TYPE_ACTION_LINK_SELF_CLICK, self._onGeneralEventHandler);
            actionLink.detachObserver(EVENT_TYPE_ACTION_LINK_CLICK, self._onGeneralEventHandler);
            actionLink.dispose();
            actionLink = null;
          }

          $link.removeData('action_link_component');
        }
      }
    );
  },

  onActionToolbarButtonClicked: function(eventType, sender, eventArgs) {
    this.executeAction(this._actionToolbarComponent.getToolbarItemValue(eventArgs.get_value()));
  },

  onSearchViewToolbarButtonClicked: function() {
    if (this.get_isSearchBlockVisible() == false) {
      if (!this._searchBlockComponent) {
        this.createSearchBlock();
      }

      this._searchBlockComponent.renderSearchBlock();
      this._searchBlockComponent.showSearchBlock();
      this.set_isSearchBlockVisible(true);
    } else {
      if (this._searchBlockComponent) {
        this._searchBlockComponent.hideSearchBlock();
        this.set_isSearchBlockVisible(false);
      }
    }
  },

  get_isSearchBlockVisible: function() {
    var state = this.loadHostState();

    if (!$q.isNullOrEmpty(state)) {
      return $q.toBoolean(state.isSearchBlockVisible, false);
    } else {
      return false;
    }
  },

  set_isSearchBlockVisible: function(value) {
    var state = this.loadHostState();

    if ($q.isNullOrEmpty(state)) {
      state = {};
    }

    state.isSearchBlockVisible = $q.toBoolean(value, false);
    this.saveHostState(state);
  },

  onContextViewToolbarButtonClicked: function() {
    if (this._isContextBlockVisible == false) {
      if (!this._contextBlockComponent) {
        this.createContextBlock();
      }

      this._contextBlockComponent.renderSearchBlock();
      this._contextBlockComponent.showSearchBlock();
      this._isContextBlockVisible = true;
    } else {
      if (this._contextBlockComponent) {
        this._contextBlockComponent.hideSearchBlock();
        this._isContextBlockVisible = false;
      }
    }
  },

  getEventArgs: function() {
    var action = this.getCurrentAction();
    var eventArgs = null;

    if (!$q.isObject(action)) {
      eventArgs = new Quantumart.QP8.BackendEventArgs();
    } else {
      eventArgs = $a.getEventArgsFromAction(action);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entityId(this._entityId);
      eventArgs.set_entityName(this._entityName);
      eventArgs.set_parentEntityId(this._parentEntityId);
    }

    return eventArgs;
  },

  renderPanels: function() {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.addItemsToBreadCrumbs();
    }

    var selected = this._getSelectedEntities();

    if (this._actionToolbarComponent && !this._useCustomActionToolbar) {
      this._actionToolbarComponent.set_actionCode(this._actionCode);
      this._actionToolbarComponent.set_entityId(this._entityId);
      this._actionToolbarComponent.set_parentEntityId(this._parentEntityId);
      this._actionToolbarComponent.set_isBindToExternal(this.get_isBindToExternal());
      this._actionToolbarComponent.addToolbarItemsToToolbar(selected.length);
    }

    if (this._viewToolbarComponent) {
      this._viewToolbarComponent.set_actionCode(this._actionCode);
      this._viewToolbarComponent.set_parentEntityId(this._parentEntityId);
      this._viewToolbarComponent.addToolbarItemsToToolbar(selected.length);
    }

    this.showPanels();
  },

  refreshPanels: function() {
    var selected = this._getSelectedEntities();

    if (this._actionToolbarComponent) {
      if (selected.length == 1 && selected.Id != this._entityId) {
        this._actionToolbarComponent.tuneToolbarItems(selected[0].Id, this._parentEntityId);
      } else {
        this._actionToolbarComponent.setVisibleStateForAll(true);
        this._actionToolbarComponent.setEnableStateForAll(true);
      }

      this._actionToolbarComponent.refreshToolbarItems(selected.length);
    }

    if (this._viewToolbarComponent) {
      this._viewToolbarComponent.refreshToolbarItems(selected.length);
    }

    this.fixActionToolbarWidth();
  },

  _getSelectedEntities: function() {
    if (this._actionTypeCode == ACTION_TYPE_CODE_LIST || this._actionTypeCode == ACTION_TYPE_CODE_LIBRARY) {
      return this._selectedEntities;
    } else {
      return [{ Id: this._entityId, Name: this._entityName }];
    }
  },

  getCurrentViewActionUrl: function() {
    var state = this.loadHostState();

    return (state && state.viewTypeCode) ? $a.getActionViewByViewTypeCode(this.getCurrentAction(), state.viewTypeCode).ControllerActionUrl : '';
  },

  _copyCurrentContextToEventArgs: function(eventArgs) {
    var context = this.get_documentContext();

    if (context) {
      eventArgs.set_context(context.modifyEventArgsContext(eventArgs.get_context()));
    }

    eventArgs.set_callerContext({
      eventArgs: this._getAppliedEventArgs()
    });

    this._copyContextQueryToEventArgs(eventArgs);
  },

  _copyContextQueryToEventArgs: function(eventArgs) {
    var query = this.get_contextQuery();

    if (query) {
      var addData = eventArgs.get_additionalData();

      if (!addData)
      eventArgs.set_additionalData({ contextQuery: query });
      else
      addData.contextQuery = query;
    }
  },

  dispose: function() {
  }
};

// Генерирует заголовок таба
Quantumart.QP8.BackendDocumentHost.generateTitle = function(eventArgs, options) {
  var actionCode = eventArgs.get_actionCode();
  var action = $a.getBackendAction(actionCode);
  var actionName = (action.ShortName) ? action.ShortName : action.Name;
  var actionTypeCode = action.ActionType.Code;
  var isMultipleEntities = eventArgs.get_isMultipleEntities();
  var entities = eventArgs.get_entities();
  var entityTypeCode = eventArgs.get_entityTypeCode();
  var entityId = eventArgs.get_entityId();
  var parentEntityId = eventArgs.get_parentEntityId();
  var isTab = false;
  var tabNumber = 0;

  if ($q.isObject(options)) {
    if ($q.isBoolean(options.isTab)) {
      isTab = options.isTab;
    }

    if ($q.isInt(options.tabNumber)) {
      tabNumber = options.tabNumber;
    }
  }

  var docHostTitle = (isTab) ? $l.Common.untitledTabText : $l.Common.untitledWindowTitle;

  if (actionTypeCode == ACTION_TYPE_CODE_LIST && (entityTypeCode == ENTITY_TYPE_CODE_SITE || entityTypeCode == ENTITY_TYPE_CODE_USER || entityTypeCode == ENTITY_TYPE_CODE_USER_GROUP)) {
    return actionName;
  }

  if (actionTypeCode == ACTION_TYPE_CODE_READ && (entityTypeCode == ENTITY_TYPE_CODE_CUSTOMER_CODE || entityTypeCode == ENTITY_TYPE_CODE_SITE_FILE || entityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE)) {
    return actionName;
  }

  if (actionCode == ACTION_CODE_CHILD_CONTENT_PERMISSIONS || actionCode == ACTION_CODE_CHILD_ARTICLE_PERMISSIONS) {
    return actionName;
  }

  if (actionCode == ACTION_CODE_SELECT_USER_GROUP || actionCode == ACTION_CODE_SELECT_USER || actionCode == ACTION_CODE_MULTIPLE_SELECT_USER) {
    return actionName;
  }

  if (entityTypeCode == ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION) {
    var pet = Quantumart.QP8.BackendEntityType.getEntityTypeById(parentEntityId);

    if (pet) {
      if (actionTypeCode == ACTION_TYPE_CODE_LIST) {
        return String.format($l.BackendPermission.entityPermissionListTitleFormat, pet.Name);
      } else if (actionCode == ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE || actionCode == ACTION_CODE_NEW_ENTITY_TYPE_PERMISSION) {
        return String.format($l.BackendPermission.entityPermissionPropertyTitleFormat, pet.Name, actionName);
      }
    }
  }

  if (entityTypeCode == ENTITY_TYPE_CODE_ACTION_PERMISSION) {
    var pact = $a.getBackendActionById(parentEntityId);

    if (pact) {
      if (actionTypeCode == ACTION_TYPE_CODE_LIST) {
        return String.format($l.BackendPermission.actionPermissionListTitleFormat, (pact.ShortName) ? pact.ShortName : pact.Name);
      } else if (actionCode == ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE || actionCode == ACTION_CODE_NEW_ACTION_PERMISSION) {
        return String.format($l.BackendPermission.actionPermissionPropertyTitleFormat, pact.Name, actionName);
      }
    }
  }

  var docHostTitleTemplate = Quantumart.QP8.BackendDocumentHost.generateTitleTemplate(actionTypeCode, entityTypeCode, isMultipleEntities, tabNumber > 1);

  var parentInfo = $o.getParentInfo(entityTypeCode, entityId, parentEntityId);

  if (actionTypeCode == ACTION_TYPE_CODE_READ &&
  (
  entityTypeCode == ENTITY_TYPE_CODE_SITE_PERMISSION ||
  entityTypeCode == ENTITY_TYPE_CODE_CONTENT_PERMISSION ||
  entityTypeCode == ENTITY_TYPE_CODE_ARTICLE_PERMISSION ||
  entityTypeCode == ENTITY_TYPE_CODE_WORKFLOW_PERMISSION ||
  entityTypeCode == ENTITY_TYPE_CODE_SITE_FODER_TYPE_PERMISSION ||
  entityTypeCode == ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION ||
  entityTypeCode == ENTITY_TYPE_CODE_ACTION_PERMISSION
  )
  ) {
    return String.format('{0} {1} - {2}', '(' + entityId + ')', parentInfo[0].EntityTypeName, actionName);
  }

  if ($q.isNullOrEmpty(docHostTitleTemplate)) {
    return actionName;
  }

  if ($q.isNull(entityId) || actionTypeCode == ACTION_TYPE_CODE_SELECT) {
    entityId = 0;
  }

  if (!$q.isArray(parentInfo) || parentInfo.length == 0) {
    return actionName;
  }

  var entityListString = (isMultipleEntities) ? $o.getEntityNamesStringFromEntities(entities) : '';

  return String.format(docHostTitleTemplate, parentInfo[0].EntityTypeName, parentInfo[0].Title, actionName, tabNumber, entityListString);
};

// Генерирует шаблон заголовока таба
Quantumart.QP8.BackendDocumentHost.generateTitleTemplate = function(actionTypeCode, entityTypeCode, isMultipleEntities, showTabNumber) {
  var docHostTitleTemplate = '';

  if (actionTypeCode == ACTION_TYPE_CODE_ADD_NEW) {
    docHostTitleTemplate = (showTabNumber) ? '{0} "{1}" - {2}-{3}' : '{0} "{1}" - {2}';
  } else if (
  actionTypeCode == ACTION_TYPE_CODE_LIST
  || actionTypeCode == ACTION_TYPE_CODE_LIBRARY
  || actionTypeCode == ACTION_TYPE_CODE_READ
  || actionTypeCode == ACTION_TYPE_CODE_SEARCH
  || actionTypeCode == ACTION_TYPE_CODE_SELECT
  || actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_SELECT
  || actionTypeCode == ACTION_TYPE_ACTION_PERMISSION_TREE
  ) {
    docHostTitleTemplate = '{0} "{1}" - {2}';
  } else if (isMultipleEntities) {
    docHostTitleTemplate = '{0} "{1}" - {2} {4}';
  }

  return docHostTitleTemplate;
};

Quantumart.QP8.BackendDocumentHost.registerClass('Quantumart.QP8.BackendDocumentHost', Quantumart.QP8.Observable);

//#endregion
