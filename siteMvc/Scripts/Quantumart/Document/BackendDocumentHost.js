/* eslint max-lines: 'off' */
/* eslint-disable no-unused-vars */
import { BackendActionLink } from '../BackendActionLink';
import { BackendActionToolbar } from '../Toolbar/BackendActionToolbar';
import { BackendArticleSearchBlock } from '../Search/BackendArticleSearchBlock';
import { BackendBreadCrumbs } from '../BackendBreadCrumbs';
import { BackendCustomActionHost } from '../CustomAction/BackendCustomActionHost';
import { BackendEntityEditor } from '../Editor/BackendEntityEditor';
import { BackendEntityGrid } from '../BackendEntityGrid';
import { BackendEntityTree } from '../Tree/BackendEntityTree';
import { BackendEntityType } from '../Info/BackendEntityType';
import { BackendEventArgs } from '../Common/BackendEventArgs';
import { BackendLibrary } from '../Library/BackendLibrary';
import { BackendSearchBlockBase } from '../Search/BackendSearchBlockBase';
import { BackendViewToolbar } from '../Toolbar/BackendViewToolbar';
import { FieldSearchState } from '../Search/BackendArticleSearchBlock/FieldSearchBase';
import { Observable } from '../Common/Observable';
import { $a, BackendActionParameters } from '../BackendActionExecutor';
import { $o } from '../Info/BackendEntityObject';
import { $q } from '../Utils';
/* eslint-enable no-unused-vars */

window.DOCUMENT_HOST_TYPE_EDITING_DOCUMENT = 1;
window.DOCUMENT_HOST_TYPE_POPUP_WINDOW = 2;
window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED = 'onHostExternalCallerContextsUnbinded';

export class BackendDocumentHost extends Observable {
  static generateTitleTemplate(
    actionTypeCode,
    entityTypeCode,
    isMultipleEntities,
    showTabNumber
  ) {
    let docHostTitleTemplate = '';
    if (actionTypeCode === window.ACTION_TYPE_CODE_ADD_NEW) {
      docHostTitleTemplate = showTabNumber ? '{0} "{1}" - {2}-{3}' : '{0} "{1}" - {2}';
    } else if (
      actionTypeCode === window.ACTION_TYPE_CODE_LIST
      || actionTypeCode === window.ACTION_TYPE_CODE_LIBRARY
      || actionTypeCode === window.ACTION_TYPE_CODE_READ
      || actionTypeCode === window.ACTION_TYPE_CODE_SEARCH
      || actionTypeCode === window.ACTION_TYPE_CODE_SELECT
      || actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_SELECT
      || actionTypeCode === window.ACTION_TYPE_ACTION_PERMISSION_TREE
    ) {
      docHostTitleTemplate = '{0} "{1}" - {2}';
    } else if (isMultipleEntities) {
      docHostTitleTemplate = '{0} "{1}" - {2} {4}';
    }

    return docHostTitleTemplate;
  }

  // eslint-disable-next-line max-statements, complexity
  static generateTitle(eventArgs, options) {
    const actionCode = eventArgs.get_actionCode();
    const action = $a.getBackendAction(actionCode);
    const actionName = action.ShortName ? action.ShortName : action.Name;
    const actionTypeCode = action.ActionType.Code;
    const isMultipleEntities = eventArgs.get_isMultipleEntities();
    const entities = eventArgs.get_entities();
    const entityTypeCode = eventArgs.get_entityTypeCode();
    let entityId = eventArgs.get_entityId();
    const parentEntityId = eventArgs.get_parentEntityId();
    let tabNumber = 0;

    if ($q.isObject(options)) {
      if ($.isNumeric(options.tabNumber)) {
        ({ tabNumber } = options);
      }
    }

    if (actionTypeCode === window.ACTION_TYPE_CODE_LIST
      && (entityTypeCode === window.ENTITY_TYPE_CODE_SITE
        || entityTypeCode === window.ENTITY_TYPE_CODE_USER
        || entityTypeCode === window.ENTITY_TYPE_CODE_USER_GROUP)) {
      return actionName;
    }

    if (actionTypeCode === window.ACTION_TYPE_CODE_READ
      && (entityTypeCode === window.ENTITY_TYPE_CODE_CUSTOMER_CODE
        || entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE
        || entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE)) {
      return actionName;
    }

    if (actionCode === window.ACTION_CODE_CHILD_CONTENT_PERMISSIONS
      || actionCode === window.ACTION_CODE_CHILD_ARTICLE_PERMISSIONS) {
      return actionName;
    }

    if (actionCode === window.ACTION_CODE_SELECT_USER_GROUP
      || actionCode === window.ACTION_CODE_SELECT_USER
      || actionCode === window.ACTION_CODE_MULTIPLE_SELECT_USER) {
      return actionName;
    }

    if (entityTypeCode === window.ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION) {
      const pet = BackendEntityType.getEntityTypeById(parentEntityId);

      if (pet) {
        if (actionTypeCode === window.ACTION_TYPE_CODE_LIST) {
          return String.format($l.BackendPermission.entityPermissionListTitleFormat, pet.Name);
        } else if (actionCode === window.ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE
          || actionCode === window.ACTION_CODE_NEW_ENTITY_TYPE_PERMISSION) {
          return String.format($l.BackendPermission.entityPermissionPropertyTitleFormat, pet.Name, actionName);
        }
      }
    }

    if (entityTypeCode === window.ENTITY_TYPE_CODE_ACTION_PERMISSION) {
      const pact = $a.getBackendActionById(parentEntityId);
      if (pact) {
        if (actionTypeCode === window.ACTION_TYPE_CODE_LIST) {
          return String.format(
            $l.BackendPermission.actionPermissionListTitleFormat,
            pact.ShortName ? pact.ShortName : pact.Name
          );
        } else if (actionCode === window.ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE
          || actionCode === window.ACTION_CODE_NEW_ACTION_PERMISSION) {
          return String.format($l.BackendPermission.actionPermissionPropertyTitleFormat, pact.Name, actionName);
        }
      }
    }

    const docHostTitleTemplate = BackendDocumentHost.generateTitleTemplate(
      actionTypeCode,
      entityTypeCode,
      isMultipleEntities,
      tabNumber > 1
    );

    const parentInfo = $o.getParentInfo(entityTypeCode, entityId, parentEntityId);
    if (actionTypeCode === window.ACTION_TYPE_CODE_READ && (
      entityTypeCode === window.NTITY_TYPE_CODE_SITE_PERMISSION
      || entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_PERMISSION
      || entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_PERMISSION
      || entityTypeCode === window.ENTITY_TYPE_CODE_WORKFLOW_PERMISSION
      || entityTypeCode === window.ENTITY_TYPE_CODE_SITE_FOLDER_TYPE_PERMISSION
      || entityTypeCode === window.ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION
      || entityTypeCode === window.ENTITY_TYPE_CODE_ACTION_PERMISSION)
    ) {
      return String.format('{0} {1} - {2}', `(${entityId})`, parentInfo[0].EntityTypeName, actionName);
    }

    if ($q.isNullOrEmpty(docHostTitleTemplate)) {
      return actionName;
    }

    if ($q.isNull(entityId) || actionTypeCode === window.ACTION_TYPE_CODE_SELECT) {
      entityId = 0;
    }

    if (!$q.isArray(parentInfo) || parentInfo.length === 0) {
      return actionName;
    }

    const entityListString = isMultipleEntities ? $o.getEntityNamesStringFromEntities(entities) : '';
    return String.format(
      docHostTitleTemplate,
      parentInfo[0].EntityTypeName,
      parentInfo[0].Title,
      actionName,
      tabNumber,
      entityListString
    );
  }

  static getAllActionLinks(documentWrapperElement) {
    const $wrapper = $q.toJQuery(documentWrapperElement);

    if (!$wrapper) {
      return $q.toJQuery([]);
    }

    return $wrapper.find('.actionLink');
  }

  // eslint-disable-next-line max-statements
  constructor(eventArgs, options) {
    super();

    /** @type {HTMLElement} */
    this._documentWrapperElement = null;
    this._documentWrapperElementId = '';
    this._documentUrl = '';
    /** @type {object} */
    this._documentPostParams = null;
    this._entityTypeCode = '';
    this._entityId = 0;
    this._entityName = '';
    this._parentEntityId = 0;
    this._isMultipleEntities = false;
    this._actionCode = '';
    this._actionTypeCode = '';
    this._isCustomAction = false;
    this._context = null;

    this._useCustomActionToolbar = false;
    this._isContextBlockVisible = false;
    this._selectedParentEntityId = 0;
    this._filter = '';

    this._startedByExternal = false;
    this._isCloseForced = false;
    this._unnamedEntitiesLimit = 1000;

    /** @type {BackendActionToolbar} */
    this._actionToolbarComponent = null;
    /** @type {BackendViewToolbar} */
    this._viewToolbarComponent = null;
    /** @type {BackendSearchBlockBase} */
    this._searchBlockComponent = null;
    /** @type {BackendSearchBlockBase} */
    this._contextBlockComponent = null;
    /** @type {BackendBreadCrumbs} */
    this._breadCrumbsComponent = null;

    this._externalCallerContexts = [];
    this._onSearchHandler = $.proxy(this.onSearch, this);
    this._onContextSwitchingHandler = $.proxy(this.onContextSwitching, this);
    this._onGeneralEventHandler = $.proxy(this.onGeneralEvent, this);

    if ($q.isObject(eventArgs)) {
      this._startedByExternal = eventArgs.get_startedByExternal();
    }

    if ($q.isObject(options)) {
      if (options.hostStateStorage) {
        this._hostStateStorage = options.hostStateStorage;
      }
    }
  }

  // eslint-disable-next-line camelcase
  get_documentWrapperElementId() {
    return this._documentWrapperElementId;
  }

  // eslint-disable-next-line camelcase
  get_documentWrapperElement() {
    return this._documentWrapperElement;
  }

  // eslint-disable-next-line camelcase
  get_documentUrl() {
    return this._documentUrl;
  }

  // eslint-disable-next-line camelcase
  get_selectedEntities() {
    return this._selectedEntities;
  }

  // eslint-disable-next-line camelcase
  get_filter() {
    return this._filter;
  }

  // eslint-disable-next-line camelcase
  get_documentContext() {
    return this._documentContext;
  }

  // eslint-disable-next-line camelcase
  set_documentContext(value) {
    this._documentContext = value;
  }

  // eslint-disable-next-line camelcase
  get_entityTypeCode() {
    return this._entityTypeCode;
  }

  // eslint-disable-next-line camelcase
  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  }

  // eslint-disable-next-line camelcase
  get_entityId() {
    return this._entityId;
  }

  // eslint-disable-next-line camelcase
  set_entityId(value) {
    this._entityId = value;
  }

  // eslint-disable-next-line camelcase
  get_entityName() {
    return this._entityName;
  }

  // eslint-disable-next-line camelcase
  set_entityName(value) {
    this._entityName = value;
  }

  // eslint-disable-next-line camelcase
  get_parentEntityId() {
    return this._parentEntityId;
  }

  // eslint-disable-next-line camelcase
  set_parentEntityId(value) {
    this._parentEntityId = value;
  }

  // eslint-disable-next-line camelcase
  get_isMultipleEntities() {
    return this._isMultipleEntities;
  }

  // eslint-disable-next-line camelcase
  set_isMultipleEntities(value) {
    this._isMultipleEntities = value;
  }

  // eslint-disable-next-line camelcase
  get_isCustomAction() {
    return this._isCustomAction;
  }

  // eslint-disable-next-line camelcase
  set_isCustomAction(value) {
    this._isCustomAction = value;
  }

  // eslint-disable-next-line camelcase
  get_entities() {
    return this._entities;
  }

  // eslint-disable-next-line camelcase
  set_entities(value) {
    this._entities = value;
  }

  // eslint-disable-next-line camelcase
  get_actionCode() {
    return this._actionCode;
  }

  // eslint-disable-next-line camelcase
  set_actionCode(value) {
    this._actionCode = value;
  }

  // eslint-disable-next-line camelcase
  get_actionTypeCode() {
    return this._actionTypeCode;
  }

  // eslint-disable-next-line camelcase
  set_actionTypeCode(value) {
    this._actionTypeCode = value;
  }

  // eslint-disable-next-line camelcase
  get_externalCallerContexts() {
    return this._externalCallerContexts;
  }

  // eslint-disable-next-line camelcase
  get_eventArgsAdditionalData() {
    return this._eventArgsAdditionalData;
  }

  // eslint-disable-next-line camelcase
  set_eventArgsAdditionalData(value) {
    this._eventArgsAdditionalData = value;
  }

  // eslint-disable-next-line camelcase
  get_context() {
    return this._context;
  }

  // eslint-disable-next-line camelcase
  set_context(value) {
    this._context = value;
  }

  /** @abstract */
  showLoadingLayer() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  hideLoadingLayer() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {Function} [_callback]
   */
  showPanels(_callback) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {Function} [_callback]
   */
  hidePanels(_callback) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {string} _eventType
   * @param {object} _sender
   * @param {any} _eventArgs
   */
  _onLibraryResized(_eventType, _sender, _eventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {object} _unbindingEventArgs
   */
  _onExternalCallerContextsUnbinded(_unbindingEventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {string} _eventType
   * @param {object} _sender
   * @param {any} _eventArgs
   */
  onChangeContent(_eventType, _sender, _eventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {any} _eventArgs
   */
  onActionExecuting(_eventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {any} _eventArgs
   */
  onEntityReaded(_eventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {any} _eventArgs
   */
  onNeedUp(_eventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {boolean} [_isLocal]
   */
  onDocumentChanging(_isLocal) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {boolean} [_isLocal]
   */
  onDocumentChanged(_isLocal) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @virtual */
  onSaveAndClose() {
    // default implementation
  }

  /** @abstract */
  createSearchBlock() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  destroySearchBlock() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  createContextBlock() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  destroyContextBlock() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {object} [_options]
   */
  generateDocumentUrl(_options) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {object} _eventArgs
   */
  updateTitle(_eventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @returns {'GET' | 'POST'}
   */
  htmlLoadingMethod() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  resetSelectedEntities() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {any} _eventArgs
   */
  saveSelectionContext(_eventArgs) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {number} _xhrStatus
   */
  showErrorMessageInDocumentWrapper(_xhrStatus) {
    throw new Error($l.Common.methodNotImplemented);
  }

  _applyEventArgs(eventArgs, saveCallerContext) {
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
      this.set_context(eventArgs.get_context());
      if (saveCallerContext) {
        this._callerContext = eventArgs.get_callerContext();
      }
    }
  }

  _getAppliedEventArgs() {
    const eventArgs = new BackendEventArgs();

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
    eventArgs.set_context(this.get_context());
    return eventArgs;
  }

  bindExternalCallerContext(eventArgs) {
    if (eventArgs) {
      const bindedContext = eventArgs.get_externalCallerContext();
      if (bindedContext) {
        if (!$.grep(this._externalCallerContexts, ctx => ctx.hostUID === bindedContext.hostUID).length) {
          this._externalCallerContexts.push(bindedContext);
        }
      }
    }
  }

  unbindExternalCallerContexts(reason) {
    const externalCallerContexts = this._externalCallerContexts;
    this._externalCallerContexts = [];
    if (!$q.isNullOrEmpty(externalCallerContexts)) {
      this._onExternalCallerContextsUnbinded({
        reason,
        externalCallerContexts
      });
    }

    if (reason !== 'close'
      && $q.isFunction(this._actionToolbarComponent.getDisabledActionCodes)
      && !$q.isNullOrEmpty(this._actionToolbarComponent.getDisabledActionCodes())) {
      this.hidePanels();
      this._actionToolbarComponent.setDisabledActionCodes(null);
      this.renderPanels();
    }
  }

  // eslint-disable-next-line camelcase
  get_isBindToExternal() {
    return !$q.isNullOrEmpty(this._externalCallerContexts);
  }

  loadHostState() {
    if (this._hostStateStorage) {
      const result = this._hostStateStorage.loadHostState({
        entityId: this.get_entityId(),
        parentEntityId: this.get_parentEntityId(),
        actionCode: this.get_actionCode()
      });

      return result;
    }
    return undefined;
  }

  getHostStateProp(propName) {
    const hostState = this.loadHostState();
    if (hostState && Object.prototype.hasOwnProperty.call(hostState, propName)) {
      return hostState[propName];
    }

    return undefined;
  }

  saveHostState(state) {
    if (this._hostStateStorage) {
      this._hostStateStorage.saveHostState({
        entityId: this.get_entityId(),
        parentEntityId: this.get_parentEntityId(),
        actionCode: this.get_actionCode()
      }, state);
    }
  }

  fixActionToolbarWidth() {
    const $actionToolbar = $(this._actionToolbarComponent.get_toolbarElement()).parents('#actionToolbar');
    const $viewToolbar = $(this._viewToolbarComponent.get_toolbarElement()).parents('#viewToolbar');
    $actionToolbar.width(
      $actionToolbar.parents('#toolbar').width()
      - $viewToolbar.width()
      - $viewToolbar.paddingLeft()
      - $actionToolbar.paddingRight()
      - 1
    );
  }

  restoreHostState() {
    const state = this.loadHostState();
    if (!$q.isNullOrEmpty(state) && (!$.isEmptyObject(state.searchBlockState) || this.get_isSearchBlockVisible())) {
      if (!this._searchBlockComponent) {
        this.createSearchBlock();
        this._searchBlockComponent.renderSearchBlock();
      }

      if (this.get_isSearchBlockVisible() && this.documentWrapperIsVisible()) {
        this._searchBlockComponent.showSearchBlock();
      } else {
        this._searchBlockComponent.hideSearchBlock();
      }
    }
  }

  _loadDefaultSearchBlockState() {
    if ($q.isNullOrEmpty(this.loadHostState())) {
      let filterStates;
      const actionTypeCode = this.get_actionTypeCode();
      const entityTypeCode = this.get_entityTypeCode();

      if ((actionTypeCode === window.ACTION_TYPE_CODE_LIST
        || actionTypeCode === window.ACTION_TYPE_CODE_SELECT
        || actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_SELECT
      ) && (entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE
        || entityTypeCode === window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE)
      ) {
        // synchronous AJAX call (sic!)
        $q.getJsonFromUrl(
          'GET',
          `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}DefaultFilterStates`,
          {
            actionCode: this.get_actionCode(),
            contentId: this.get_parentEntityId()
          },
          false,
          false,
          data => {
            if (data.success) {
              ({ filterStates } = data);
            } else {
              $q.alertFail(data.message);
            }
          },
          jqXHR => {
            filterStates = null;
            $q.processGenericAjaxError(jqXHR);
          }
        );
      }

      if (!$q.isNullOrEmpty(filterStates) && $q.isArray(filterStates)) {
        // @ts-ignore filterStates is assigned in synchronous AJAX call
        const fieldSearchBlockState = filterStates.map(item =>
          new FieldSearchState(
            item.SearchType,
            item.FieldId,
            item.ContentId,
            item.FieldColumnName,
            item.FieldName,
            item.FieldGroup,
            item.ReferenceFieldId,
            {
              isEntity: true,
              entities: item.SelectedEntities
            }
          )
        );

        // @ts-ignore filterStates is assigned in synchronous AJAX call
        const searchQuery = filterStates.map(item => BackendArticleSearchBlock.createFieldSearchQuery(
          item.SearchType,
          item.FieldId,
          item.FieldColumnName,
          null,
          item.SelectedEntities.map(ent => ent.Id),
          false,
          false
        ));

        const defaultSearchBlockState = {
          searchQuery: JSON.stringify(searchQuery),
          searchBlockState: {
            fieldSearchBlockState,
            defaultFieldSearchBlockState: fieldSearchBlockState
          }
        };

        this.saveHostState(defaultSearchBlockState);
        return defaultSearchBlockState;
      }
    }

    return undefined;
  }

  showDocumentWrapper() {
    const $documentWrapper = $(this._documentWrapperElement);
    $documentWrapper.show();
  }

  hideDocumentWrapper() {
    const $documentWrapper = $(this._documentWrapperElement);
    $documentWrapper.hide();
  }

  documentWrapperIsVisible() {
    const $documentWrapper = $(this._documentWrapperElement);
    return $documentWrapper.is(':visible');
  }

  // eslint-disable-next-line max-statements, complexity
  executeAction(actionCode) {
    const action = $a.getBackendActionByCode(actionCode);
    if (!action) {
      $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      return;
    }

    let entityId = this._entityId;
    let entityName = this._entityName;
    const isMultiple = action.ActionType.IsMultiple;
    const isEditor = this.getMainComponent() instanceof BackendEntityEditor;
    const isCustomActionHost = this.getMainComponent() instanceof BackendCustomActionHost;
    const selectedEntities = !isEditor
      && !isCustomActionHost
      ? this._selectedEntities
      : [{ Id: this._entityId, Name: this._entityName }];

    let entities = null;
    if (isMultiple) {
      entities = this._isMultipleEntities ? this._entities.slice() : selectedEntities;
    }

    const actionTypeCode = action.ActionType.Code;
    if (isMultiple) {
      if (entities.length === 0) {
        $q.alertError($l.DocumentHost.noEntitiesToExecuteActionErrorMessage);
        this.refresh();
        return;
      }

      const unnamedEntities = $.grep(entities, elem => !elem.Name);
      if (unnamedEntities.length > 0 && unnamedEntities.length <= this._unnamedEntitiesLimit) {
        let entityTypeCode = this._entityTypeCode;
        if (entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_PERMISSION) {
          entityTypeCode = window.ENTITY_TYPE_CODE_CONTENT;
        } else if (entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_PERMISSION) {
          entityTypeCode = window.ENTITY_TYPE_CODE_ARTICLE;
        }

        const dataItems = $o.getSimpleEntityList(
          entityTypeCode,
          this._parentEntityId,
          0,
          0,
          Quantumart.QP8.Enums.ListSelectionMode.OnlySelectedItems,
          $o.getEntityIDsFromEntities(unnamedEntities)
        );

        const namedEntities = dataItems.map(item => ({ Id: $q.toInt(item.Value), Name: item.Text }));
        $.each(namedEntities, (index, elem) => {
          for (let i = 0; i < entities.length; i++) {
            const entity = entities[i];
            if (entity.Id === elem.Id) {
              entity.Name = elem.Name;
              break;
            }
          }
        });
      } else if (unnamedEntities.length > this._unnamedEntitiesLimit) {
        for (let i = 0; i < unnamedEntities.length; i++) {
          unnamedEntities[i].Name = unnamedEntities[i].Id;
        }
      }
    }

    if (!isMultiple
      && (
        this._actionTypeCode === window.ACTION_TYPE_CODE_LIST
        || this._actionTypeCode === window.ACTION_TYPE_CODE_LIBRARY
      ) && this._selectedEntities.length === 1) {
      entityId = this._selectedEntities[0].Id;
      entityName = this._selectedEntities[0].Name;
    }

    const isCustomAction = action.IsCustom;
    const isMultistepAction = action.IsMultistep;

    if (!isCustomAction && !isMultistepAction && actionTypeCode === window.ACTION_TYPE_CODE_REFRESH) {
      const main = this.getMainComponent();
      if (!main || !(main instanceof BackendEntityEditor) || main.confirmRefresh()) {
        this.refresh();
      }
    } else if (!isCustomAction && !isMultistepAction
      && (actionTypeCode === window.ACTION_TYPE_CODE_SAVE
        || actionTypeCode === window.ACTION_TYPE_CODE_UPDATE
        || actionTypeCode === window.ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE
        || actionTypeCode === window.ACTION_TYPE_CODE_SAVE_AND_UP
        || actionTypeCode === window.ACTION_TYPE_CODE_UPDATE_AND_UP
        || (actionTypeCode === window.ACTION_TYPE_CODE_RESTORE
          && action.EntityType.Code === window.ENTITY_TYPE_CODE_ARTICLE_VERSION)
      )
    ) {
      const main = this.getMainComponent();
      if (main && (main instanceof BackendEntityEditor)) {
        main.saveEntity(actionCode);
      }
    } else {
      const params = new BackendActionParameters({
        entityTypeCode: this._entityTypeCode,
        parentEntityId: this._selectedParentEntityId || this._parentEntityId,
        entities,
        entityId,
        entityName,
        context: this.get_context(),
        // @ts-ignore FIXME
        forceOpenWindow: this.isWindow
      });

      params.correct(action);

      const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
      eventArgs.set_startedByExternal(this._startedByExternal);
      this.onActionExecuting(eventArgs);
    }
  }


  getMainComponent() {
    return $q.isObject(this._documentContext) ? this._documentContext.getMainComponent() : null;
  }

  getViewTypeCode() {
    return this._viewToolbarComponent ? this._viewToolbarComponent.getSelectedViewTypeCode() : '';
  }

  getSearchQuery() {
    let result = '';
    if (this._searchBlockComponent) {
      result = this._searchBlockComponent.getSearchQuery();
    } else {
      const state = this.loadHostState();
      if (!$q.isNullOrEmpty(state) && !$q.isNullOrWhiteSpace(state.searchQuery)) {
        result = state.searchQuery;
      }
    }

    return result;
  }

  // eslint-disable-next-line camelcase
  get_contextQuery() {
    let result = '';
    if (this._contextBlockComponent) {
      result = this._contextBlockComponent.getSearchQuery();
    } else if (this._eventArgsAdditionalData) {
      result = this._eventArgsAdditionalData.contextQuery;
    }

    return result;
  }

  // eslint-disable-next-line camelcase
  get_contextState() {
    const query = this.get_contextQuery();
    if ($q.isNullOrEmpty(query)) {
      return null;
    }

    return JSON.parse(query);
  }

  // eslint-disable-next-line camelcase
  get_currentPage() {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendEntityGrid)) {
      return main.get_currentPage();
    }

    const state = this.loadHostState();
    if (!$q.isNullOrEmpty(state) && !$q.isNullOrWhiteSpace(state.gridCurrentPage)) {
      return state.gridCurrentPage;
    }

    return undefined;
  }

  // eslint-disable-next-line camelcase
  get_orderBy() {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendEntityGrid)) {
      return main.get_orderBy();
    }

    const state = this.loadHostState();
    if (!$q.isNullOrEmpty(state) && !$q.isNullOrWhiteSpace(state.gridOrderBy)) {
      return state.gridOrderBy;
    }

    return undefined;
  }

  markPanelsAsBusy() {
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
  }

  unmarkPanelsAsBusy() {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.unmarkBreadCrumbsAsBusy();
    }

    if (this._actionToolbarComponent) {
      this._actionToolbarComponent.unmarkToolbarAsBusy();
    }

    if (this._viewToolbarComponent) {
      this._viewToolbarComponent.unmarkToolbarAsBusy();
    }

    this.hideLoadingLayer();
  }

  loadHtmlContentToDocumentWrapper(callback, options) {
    const that = this;
    let async = true;

    if (options) {
      async = options.asyncReq;
    }

    $q.getJsonFromUrl(
      this.htmlLoadingMethod(),
      this._documentUrl,
      this._documentPostParams,
      async,
      false,
      data => {
        that.loadReadyHtmlContent(data);
        if ($q.isFunction(callback)) {
          callback(data.success);
        }
      },
      jqXHR => {
        that.onDocumentError();
        if ($q.isFunction(callback)) {
          callback(false);
        }

        $q.processGenericAjaxError(jqXHR);
        that.showErrorMessageInDocumentWrapper(jqXHR.status);
      }
    );
  }

  _saveSelectedEntitiesContext(eventArgs) {
    this._selectedEntitiesContext = eventArgs.get_context();
  }

  refresh() {
    this.onDocumentChanging();
    this.resetSelectedEntities();
    this.refreshPanels();
    this.generateDocumentUrl();

    const that = this;
    this.loadHtmlContentToDocumentWrapper(() => {
      $(that._documentWrapperElement).scrollTo(0);
      that.onDocumentChanged();
    });
  }

  changeView(controllerActionUrl) {
    this.onDocumentChanging();
    this.generateDocumentUrl({ controllerActionUrl });

    const that = this;
    this.loadHtmlContentToDocumentWrapper(() => {
      $(that._documentWrapperElement).scrollTo(0);
      that.onDocumentChanged();
    });
  }

  changeViewForSettings(options) {
    this.onDocumentChanging();
    this.generateDocumentUrl(options);

    const that = this;
    this.loadHtmlContentToDocumentWrapper(() => {
      $(that._documentWrapperElement).scrollTo(0);
      that.onDocumentChanged();
    });
  }

  changeContent(eventArgs, isSaveAndUp) {
    if (this.allowChange()) {
      this.onDocumentChanging();
      this.markMainComponentAsBusy();
      if (!isSaveAndUp) {
        this.cancel();
      }

      this._copyContextQueryToEventArgs(eventArgs);

      const appliedEventArgs = this._getAppliedEventArgs();
      const externalCallerContexts = this._externalCallerContexts;

      this._externalCallerContexts = [];
      this.updateDocument(eventArgs);
      this._externalCallerContexts = externalCallerContexts;

      this._loadDefaultSearchBlockState();
      this.loadHtmlContentToDocumentWrapper(success => {
        this.unmarkMainComponentAsBusy();
        if (success) {
          this.unbindExternalCallerContexts('changed');
        } else {
          this.updateDocument(appliedEventArgs);
        }

        $(this._documentWrapperElement).scrollTo(0);
        this.onDocumentChanged();

        if (eventArgs.fromHistory) {
          eventArgs.finishExecution();
        }
      });
    } else if (eventArgs.fromHistory) {
      eventArgs.finishExecution(false);
    }
  }

  updateDocument(eventArgs) {
    const oldActionCode = this.get_actionCode();
    const newActionCode = eventArgs.get_actionCode();

    this._applyEventArgs(eventArgs);
    this.generateDocumentUrl();
    this.updateTitle(eventArgs);

    if (oldActionCode !== newActionCode) {
      this.hidePanels();
      this.destroySearchBlock();
      this.destroyContextBlock();
      this.renderPanels();
    }
  }

  loadReadyHtmlContent(data) {
    const $documentWrapper = $(this._documentWrapperElement);
    const visible = $documentWrapper.is(':visible');

    if (data.success) {
      const scrollData = $documentWrapper.data('scroll_position');

      this.onDocumentUnloaded();
      $documentWrapper.data('scroll_position', scrollData);

      if (visible) {
        this.hideDocumentWrapper();
      }

      $documentWrapper.empty().html(data.view);
      if (visible) {
        this.showDocumentWrapper();
      }

      this.onDocumentLoaded();
    } else {
      $q.alertError(data.message);
      this.onDocumentError();
    }
  }

  onLoadMainComponent() {
    const main = this.getMainComponent();
    if (main) {
      if (main instanceof BackendEntityEditor) {
        main.onLoad();
      } else if (main instanceof BackendLibrary) {
        main.attachObserver(window.EVENT_TYPE_LIBRARY_RESIZED, $.proxy(this._onLibraryResized, this));
        main.onLoad();
      } else if (main instanceof BackendEntityGrid) {
        main.onLoad();
      }
    }
  }

  onSelectMainComponent() {
    const main = this.getMainComponent();

    if (main) {
      if (main instanceof BackendEntityEditor) {
        main.onSelect();
      } else if (main instanceof BackendLibrary) {
        main.resize();
      } else if (main instanceof BackendCustomActionHost) {
        main.onSelect();
      }
    }
  }

  // Вызывается после того как уже открытый хост был выбран в результате выполнения action
  onSelectedThroughExecution(eventArgs) {
    this.bindExternalCallerContext(eventArgs);
    this.renderPanels();

    if (this._documentContext) {
      this._documentContext.execSelect(eventArgs);
    }
  }

  unmarkMainComponentAsBusy() {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendEntityGrid)) {
      main.unmarkGridAsBusy();
    }
  }

  markMainComponentAsBusy() {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendEntityGrid)) {
      main.markGridAsBusy();
    }
  }

  onContextSwitching(eventType, sender, args) {
    const main = this.getMainComponent();
    if (main) {
      if (main instanceof BackendEntityGrid) {
        main.resetGrid({ contextQuery: args.getSearchQuery() });
      } else if (main instanceof BackendEntityEditor) {
        main.applyContext(args.getSearchBlockState());
      }
    }
  }

  _getComponentSearchProcessor(options) {
    const main = this.getMainComponent();
    const isGrid = main instanceof BackendEntityGrid;
    const isTree = main instanceof BackendEntityTree;
    if (main && (isGrid || isTree)) {
      if ($q.isNullOrEmpty(options.state.searchBlockState)) {
        $q.removeProperty(options.state, 'searchQuery');
      } else {
        Object.assign(options.state, {
          searchQuery: options.searchQuery()
        });
      }

      if (isGrid) {
        return {
          process() {
            main.resetGrid({ searchQuery: options.searchQuery() });
          }
        };
      }

      if (isTree) {
        return {
          process() {
            main.searchByTerm({ searchQuery: options.searchQuery() });
          }
        };
      }

      throw new Error('Undefined search processor');
    }

    return undefined;
  }

  onSearch(eventType, sender, args) {
    const state = this.loadHostState() || {};
    const searchBlockState = args.getSearchBlockState();

    if ($q.isNullOrEmpty(searchBlockState)) {
      $q.removeProperty(state, 'searchBlockState');
    } else {
      state.searchBlockState = searchBlockState;
    }

    this._getComponentSearchProcessor({
      state,
      searchQuery: args.getSearchQuery.bind(args)
    }).process();

    this.saveHostState(state);
  }

  onViewChanging(eventType, sender, args) {
    let state = this.loadHostState();
    if ($q.isNullOrEmpty(state)) {
      state = {};
    }

    const code = args.getCode();
    state.viewTypeCode = code;
    this.saveHostState(state);

    const main = this.getMainComponent();
    if (main && (main instanceof BackendLibrary)) {
      main.changeViewType(code);
    } else {
      this.changeView(args.getControllerActionUrl());
    }
  }

  onEntityLoaded(eventArgs) {
    if (+eventArgs.get_entityId() !== 0) {
      this.updateDocument(eventArgs);
      this.onEntityReaded(eventArgs);
      if (eventArgs.get_isRestored() && eventArgs.get_entityTypeCode() === window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
        const message = String.format($l.EntityEditor.versionSuccessfullyRestoredMessage, eventArgs.get_entityId());
        $q.alertError(message);
      }
    } else if (eventArgs.get_actionTypeCode() === window.ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE) {
      this.onEntityReaded(eventArgs);
    }
  }

  onEntitySubmitting() {
    this.onDocumentChanging();
  }

  onEntitySubmitted(eventArgs) {
    this.loadReadyHtmlContent(eventArgs.get_data());
    this.onDocumentChanged();
    this.saveAndUp();

    if (this.onSaveAndClose) {
      this.onSaveAndClose();
    }
  }

  onEntitySubmittedError() {
    this._isCloseForced = false;
    this.onDocumentChanged();
  }

  saveAndUp() {
    const context = this.get_documentContext();
    if (context && context.needUp()) {
      this.onNeedUp(context.getEventArgs());
    }
  }

  onDocumentUnloaded() {
    this.destroyAllActionLinks();
    const main = this.getMainComponent();
    if (main && (main instanceof BackendLibrary)) {
      main.detachObserver(window.EVENT_TYPE_LIBRARY_RESIZED);
    }

    if (this._documentContext) {
      this._documentContext.dispose();
    }
  }

  onDocumentLoaded() {
    this.initAllActionLinks();
    this.restoreHostState();
    this.onLoadMainComponent();
    if (this.get_eventArgsAdditionalData() && this.get_eventArgsAdditionalData().restoring) {
      const argData = this.get_eventArgsAdditionalData();
      argData.restoring = false;
      argData.initFieldValues = null;
    }

    const context = this.get_documentContext();
    if (context) {
      const state = context.getState();
      const mainComponentType = context.getMainComponentType();
      if (mainComponentType === $e.MainComponentType.Editor && state !== $e.DocumentContextState.Error) {
        const eventArgs = context.getEventArgs();
        this._copyContextQueryToEventArgs(eventArgs);
        this.onEntityLoaded(context.getEventArgs());
      }

      context.onHostLoaded();
    }
  }

  onDocumentError() {
    this._isCloseForced = false;
  }

  onSelectionChanged(eventArgs) {
    const selectedEntities = eventArgs.get_entities().slice();
    this._selectedEntities = selectedEntities;
    this._context = eventArgs._context;
    this.saveSelectionContext(eventArgs);
    this._saveSelectedEntitiesContext(eventArgs);
    this.refreshPanels();
  }

  onDataBinding() {
    this.onDocumentChanging(true);
  }

  onDataBound(eventType, sender, eventArgs) {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendEntityGrid)) {
      let state = this.loadHostState();
      if ($q.isNullOrEmpty(state)) {
        state = {};
      }

      if ($q.isNullOrEmpty(sender.get_currentPage())) {
        $q.removeProperty(state, 'gridCurrentPage');
      } else {
        state.gridCurrentPage = main.get_currentPage();
      }

      if ($q.isNullOrEmpty(sender.get_orderBy())) {
        $q.removeProperty(state, 'gridOrderBy');
      } else {
        state.gridOrderBy = main.get_orderBy();
      }

      this.saveHostState(state);
    }

    this.onSelectionChanged(eventArgs);
    this.onDocumentChanged(true);
  }

  // eslint-disable-next-line complexity
  onGeneralEvent(eventType, sender, eventArgs) {
    if (eventType === window.EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING
      || eventType === window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING
      || eventType === window.EVENT_TYPE_LIBRARY_ACTION_EXECUTING
      || eventType === window.EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING
      || eventType === window.EVENT_TYPE_ACTION_LINK_CLICK
      || eventType === window.EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING
      || eventType === window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED
      || eventType === window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK
      || eventType === window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CONTEXT_CLICK) {
      this.onActionExecuting(eventArgs);
    } else if (eventType === window.EVENT_TYPE_EXTERNAL_ACTION_EXECUTING) {
      if (eventArgs.get_externalCallerContext().data.changeCurrentTab && eventArgs.get_isInterface()) {
        this.onChangeContent(eventType, sender, eventArgs);
      } else {
        this.onActionExecuting(eventArgs);
      }
    } else if (eventType === window.EVENT_TYPE_ENTITY_GRID_DATA_BINDING) {
      this.onDataBinding();
    } else if (eventType === window.EVENT_TYPE_ENTITY_GRID_DATA_BOUND
      || eventType === window.EVENT_TYPE_LIBRARY_DATA_BOUND
      || eventType === window.EVENT_TYPE_ENTITY_TREE_DATA_BOUND) {
      this.onDataBound(eventType, sender, eventArgs);
    } else if (eventType === window.EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED
      || eventType === window.EVENT_TYPE_LIBRARY_ENTITY_SELECTED
      || eventType === window.EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED) {
      this.onSelectionChanged(eventArgs);
    } else if (eventType === window.EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK
      || eventType === window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK
      || eventType === window.EVENT_TYPE_ACTION_LINK_SELF_CLICK) {
      this.onChangeContent(eventType, sender, eventArgs);
    } else if (eventType === window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING) {
      this.onEntitySubmitting();
    } else if (eventType === window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED) {
      this.onEntitySubmitted(eventArgs);
    } else if (eventType === window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR) {
      this.onEntitySubmittedError();
    } else if (eventType === window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING) {
      this.refresh();
    } else if (eventType === window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED) {
      this.onViewChanging(eventType, sender, eventArgs);
    } else if (eventType === window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED) {
      this.onActionToolbarButtonClicked(eventType, sender, eventArgs);
    } else if (eventType === window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED) {
      this.onSearchViewToolbarButtonClicked();
    } else if (eventType === window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED) {
      this.onContextViewToolbarButtonClicked();
    } else if (eventType === window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED) {
      this.initAllActionLinks(eventArgs.articleWrapper);
    } else if (eventType === window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING) {
      this.destroyAllActionLinks(eventArgs.articleWrapper);
    }
  }

  getCurrentAction() {
    return $a.getBackendActionByCode(this._actionCode);
  }

  cancel() {
    const action = this.getCurrentAction();
    const cancelActionCode = action ? action.EntityType.CancelActionCode : '';
    if (!$q.isNullOrWhiteSpace(cancelActionCode)
      && action.ActionType.Code === window.ACTION_TYPE_CODE_READ
      && $o.checkEntityExistence(this._entityTypeCode, this._entityId)) {
      this.executeAction(cancelActionCode);
    }
  }

  allowChange() {
    const main = this.getMainComponent();
    return !main || !(main instanceof BackendEntityEditor) || main.confirmChange();
  }

  allowClose() {
    const main = this.getMainComponent();
    return !main || !(main instanceof BackendEntityEditor) || main.confirmClose();
  }

  selectAllEntities() {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendEntityGrid)) {
      main.selectAllRows();
    } else {
      $q.alertError($l.Toolbar.selectAllIsNotAllowed);
    }
  }

  deselectAllEntities() {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendEntityGrid)) {
      main.deselectAllRows();
    } else {
      $q.alertError($l.Toolbar.selectAllIsNotAllowed);
    }
  }

  initAllActionLinks(documentWrapperElement = this._documentWrapperElement) {
    const that = this;
    BackendDocumentHost.getAllActionLinks(documentWrapperElement).each((index, linkElem) => {
      const $link = $q.toJQuery(linkElem);
      if (!$q.isNullOrEmpty($link) && $q.isNull($link.data('action_link_component'))) {
        const linkId = $link.attr('id');
        const entityId = +$link.data('entity_id') || 0;
        const entityName = $link.data('entity_name');
        const parentEntityId = $q.toInt($link.data('parent_entity_id'), null);
        const actionTypeCode = $link.data('action_type_code');
        const actionCode = $link.data('action_code');
        const actionAlias = $link.data('action_alias');
        const actionTargetType = $q.toInt($link.data('action_target_type'), null);
        const context = $link.data('context');
        const actionLink = new BackendActionLink(linkId, {
          entityId,
          entityName,
          parentEntityId,
          actionTypeCode,
          actionCode,
          actionTargetType,
          context,
          actionAlias
        });

        actionLink.initialize();
        actionLink.attachObserver(window.EVENT_TYPE_ACTION_LINK_SELF_CLICK, that._onGeneralEventHandler);
        actionLink.attachObserver(window.EVENT_TYPE_ACTION_LINK_CLICK, that._onGeneralEventHandler);
        $link.data('action_link_component', actionLink);
      }
    });
  }

  destroyAllActionLinks(documentWrapperElement = this._documentWrapperElement) {
    const that = this;
    BackendDocumentHost.getAllActionLinks(documentWrapperElement).each((index, linkElem) => {
      const $link = $q.toJQuery(linkElem);
      if (!$q.isNullOrEmpty($link)) {
        const actionLink = $link.data('action_link_component');
        if ($q.isObject(actionLink)) {
          actionLink.detachObserver(window.EVENT_TYPE_ACTION_LINK_SELF_CLICK, that._onGeneralEventHandler);
          actionLink.detachObserver(window.EVENT_TYPE_ACTION_LINK_CLICK, that._onGeneralEventHandler);
          actionLink.dispose();
        }

        $link.removeData('action_link_component');
      }
    });
  }

  onActionToolbarButtonClicked(eventType, sender, eventArgs) {
    this.executeAction(this._actionToolbarComponent.getToolbarItemValue(eventArgs.get_value()));
  }

  onSearchViewToolbarButtonClicked() {
    if (!this.get_isSearchBlockVisible()) {
      if (!this._searchBlockComponent) {
        this.createSearchBlock();
      }

      this._searchBlockComponent.renderSearchBlock();
      this._searchBlockComponent.showSearchBlock();
      this.set_isSearchBlockVisible(true);
    } else if (this._searchBlockComponent) {
      this._searchBlockComponent.hideSearchBlock();
      this.set_isSearchBlockVisible(false);
    }
  }

  // eslint-disable-next-line camelcase
  get_isSearchBlockVisible() {
    const state = this.loadHostState();
    if (!$q.isNullOrEmpty(state)) {
      return $q.toBoolean(state.isSearchBlockVisible, false);
    }

    return false;
  }

  // eslint-disable-next-line camelcase
  set_isSearchBlockVisible(value) {
    let state = this.loadHostState();
    if ($q.isNullOrEmpty(state)) {
      state = {};
    }

    state.isSearchBlockVisible = $q.toBoolean(value, false);
    this.saveHostState(state);
  }

  onContextViewToolbarButtonClicked() {
    if (!this._isContextBlockVisible) {
      if (!this._contextBlockComponent) {
        this.createContextBlock();
      }

      this._contextBlockComponent.renderSearchBlock();
      this._contextBlockComponent.showSearchBlock();
      this._isContextBlockVisible = true;
    } else if (this._contextBlockComponent) {
      this._contextBlockComponent.hideSearchBlock();
      this._isContextBlockVisible = false;
    }
  }

  getEventArgs() {
    const action = this.getCurrentAction();
    let eventArgs = null;
    if ($q.isObject(action)) {
      eventArgs = $a.getEventArgsFromAction(action);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entityId(this._entityId);
      eventArgs.set_entityName(this._entityName);
      eventArgs.set_parentEntityId(this._parentEntityId);
    } else {
      eventArgs = new BackendEventArgs();
    }

    return eventArgs;
  }

  renderPanels() {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.addItemsToBreadCrumbs();
    }

    const selected = this._getSelectedEntities();

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
  }

  refreshPanels() {
    const selected = this._getSelectedEntities();

    if (this._actionToolbarComponent) {
      if (selected.length === 1 && selected.Id !== this._entityId) {
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
  }

  _getSelectedEntities() {
    if (this._actionTypeCode === window.ACTION_TYPE_CODE_LIST
      || this._actionTypeCode === window.ACTION_TYPE_CODE_LIBRARY) {
      return this._selectedEntities;
    }

    return [{ Id: this._entityId, Name: this._entityName }];
  }

  getCurrentViewActionUrl() {
    const state = this.loadHostState();
    return state && state.viewTypeCode
      ? $a.getActionViewByViewTypeCode(this.getCurrentAction(), state.viewTypeCode).ControllerActionUrl
      : '';
  }

  _copyCurrentContextToEventArgs(eventArgs) {
    const context = this.get_documentContext();

    if (context) {
      eventArgs.set_context(context.modifyEventArgsContext(eventArgs.get_context()));
    }

    eventArgs.set_callerContext({
      eventArgs: this._getAppliedEventArgs()
    });

    this._copyContextQueryToEventArgs(eventArgs);
  }

  _copyContextQueryToEventArgs(eventArgs) {
    const query = this.get_contextQuery();
    if (query) {
      const addData = eventArgs.get_additionalData();
      if (addData) {
        addData.contextQuery = query;
      } else {
        eventArgs.set_additionalData({ contextQuery: query });
      }
    }
  }
}

Quantumart.QP8.BackendDocumentHost = BackendDocumentHost;
