// ****************************************************************************
// *** Компонент "Редактируемый документ"									***
// ****************************************************************************

//#region class BackendEditingDocument
// === Класс "Редактируемый документ" ===
Quantumart.QP8.BackendEditingDocument = function (tabId, editingArea, eventArgs, options) {
	if (!tabId) {
		throw new Error($l.EditingArea.tabIdNotSpecifiedInConstructor);
	}

	if (!editingArea) {
		throw new Error($l.EditingArea.editingAreaNotSpecifiedInConstructor);
	}

	Quantumart.QP8.BackendEditingDocument.initializeBase(this, [eventArgs, options]);

	this._editingArea = editingArea;
	this._tabId = tabId;
	this._id = this.getDocumentIdByTabId(this._tabId);
	if ($q.isObject(eventArgs)) {
	    this._applyEventArgs(eventArgs, true);
	    this.bindExternalCallerContext(eventArgs);
		this._selectedEntities = [];
		if (eventArgs.get_context() && eventArgs.get_context().additionalUrlParameters) {
			this._additionalUrlParameters = eventArgs.get_context().additionalUrlParameters;
		}
	}
};

Quantumart.QP8.BackendEditingDocument.prototype = {
	_id: "", // идентификатор редактируемого объекта
	_tabId: "", // идентификатор таба
	_editingArea: null, // область редактирования
	_additionalUrlParameters: null, // дополнительные параметры URL

	get_id: function () {
		return this._id;
	},

	get_tabId: function () {
		return this._tabId;
	},

	set_tabId: function (value) {
		this._tabId = value;
	},

	get_editingArea: function () {
		return this._editingArea;
	},

	get_hostType: function () {
		return DOCUMENT_HOST_TYPE_EDITING_DOCUMENT;
	},

	get_zIndex: function () {
	    return 0;
	},


	initialize: function (callback) {
		this.generateDocumentUrl();
		this.createPanels();
		this.renderPanels();
		this.addNewDocumentWrapper();
		this._loadDefaultSearchBlockState();
		this.loadHtmlContentToDocumentWrapper(callback);
	},

	getDocumentIdByTabId: function (tabId) {
		return this._editingArea.getDocumentIdByTabId(tabId);
	},

	generateDocumentWrapperId: function () {
		return String.format("doc_{0}", this._tabId);
	},

	addNewDocumentWrapper: function () {
		var documentWrapperElementId = this._documentWrapperElementId;
		if ($q.isNullOrWhiteSpace(documentWrapperElementId)) {
			documentWrapperElementId = this.generateDocumentWrapperId();
		}

		var $documentWrapper = jQuery("#" + documentWrapperElementId);
		if ($q.isNullOrEmpty($documentWrapper)) {
			$documentWrapper = jQuery("<div />", { "id": documentWrapperElementId, "class": "documentWrapper" })

			var $documentsContainer = jQuery(this._editingArea.get_documentsContainerElement());
			$documentsContainer.append($documentWrapper);

			$documentsContainer = null;
		}

		this._documentWrapperElementId = documentWrapperElementId;
		this._documentWrapperElement = $documentWrapper.get(0);

		$documentWrapper = null;
		return documentWrapperElementId;
	},

	generateDocumentUrl: function (options) {
		var entityIDs = (this._isMultipleEntities) ? $o.getEntityIDsFromEntities(this._entities) : [this._entityId];

		var extraOptions = {
			additionalUrlParameters: this._additionalUrlParameters,
			controllerActionUrl: this.getCurrentViewActionUrl()
		};
		if (this.get_isBindToExternal() === true) {
			extraOptions.additionalUrlParameters = jQuery.extend(extraOptions.additionalUrlParameters, { boundToExternal: true });
		}

		options = (!$q.isObject(options)) ? extraOptions : jQuery.extend(options, extraOptions);

		var url = $a.generateActionUrl(this._isMultipleEntities, entityIDs, this._parentEntityId, this._tabId, this.getCurrentAction(), options);
		this._documentUrl = url;

		var params = {};
		if (this._isMultipleEntities || this._isCustomAction) { params.IDs = entityIDs; }
		if (this._isCustomAction) { params.actionCode = this._actionCode; }
		this._documentPostParams = params;
	},

	htmlLoadingMethod: function () {
		return (this._isMultipleEntities || this._isCustomAction) ? "POST" : "GET";
	},

	showErrorMessageInDocumentWrapper: function (status) {
		var $documentWrapper = jQuery(this._documentWrapperElement); // скрытый документ
		$documentWrapper.html($q.generateErrorMessageText());
	},

	removeDocumentWrapper: function (callback) {
		var $documentWrapper = jQuery(this._documentWrapperElement);
		$documentWrapper
			.empty()
			.remove()
			;

		$q.callFunction(callback);
	},

	createPanels: function () {
		var action = this.getCurrentAction();

		// Создаем хлебные крошки
		var breadCrumbsComponent = Quantumart.QP8.BackendBreadCrumbsManager.getInstance().createBreadCrumbs("breadCrumbs_" + this._tabId,
			{
				"documentHost": this,
				"breadCrumbsContainerElementId": this._editingArea.get_breadCrumbsContainerElementId()
			}
		);

		breadCrumbsComponent.attachObserver(EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, this._onGeneralEventHandler);
		breadCrumbsComponent.attachObserver(EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, this._onGeneralEventHandler);
		this._breadCrumbsComponent = breadCrumbsComponent;

		// Создаем панель инструментов для действий
		var actionToolbarOptions = {
			"toolbarContainerElementId": this._editingArea.get_actionToolbarContainerElementId()
		};
		var eventArgsAdditionalData = this.get_eventArgsAdditionalData();
		if (eventArgsAdditionalData && eventArgsAdditionalData.disabledActionCodes) {
			actionToolbarOptions.disabledActionCodes = eventArgsAdditionalData.disabledActionCodes;
		}
		var actionToolbarComponent = new Quantumart.QP8.BackendActionToolbar("actionToolbar_" + this._tabId, this._actionCode, this._parentEntityId, actionToolbarOptions);
		actionToolbarComponent.initialize();
		actionToolbarComponent.attachObserver(EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED, this._onGeneralEventHandler);
		this._actionToolbarComponent = actionToolbarComponent;

	    // Создаем панель инструментов для представлений
		var viewToolbarOptions = {
		    "toolbarContainerElementId": this._editingArea.get_viewToolbarContainerElementId()
		};
		var state = this.loadHostState();
		if (state && state.viewTypeCode) {
		    viewToolbarOptions.viewTypeCode = state.viewTypeCode;
		}
		var viewToolbarComponent = new Quantumart.QP8.BackendViewToolbar("viewToolbar_" + this._tabId, this._actionCode, viewToolbarOptions);
		viewToolbarComponent.initialize();

		viewToolbarComponent.attachObserver(EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED, this._onGeneralEventHandler);
		viewToolbarComponent.attachObserver(EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED, this._onGeneralEventHandler);
		viewToolbarComponent.attachObserver(EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED, this._onGeneralEventHandler);

		this._viewToolbarComponent = viewToolbarComponent;
	},

	showPanels: function (callback) {

		var $breadCrumbsContainer = jQuery(this._editingArea.get_breadCrumbsContainerElement());
		$breadCrumbsContainer.find("> *.breadCrumbs:visible").hide(0);
		this._breadCrumbsComponent.showBreadCrumbs();

		$breadCrumbsContainer = null;

		var $actionToolbarContainer = jQuery(this._editingArea.get_actionToolbarContainerElement());
		$actionToolbarContainer.find("> *.toolbar:visible").hide(0);
		this._actionToolbarComponent.showToolbar(callback);

		$actionToolbarContainer = null;

		var $viewToolbarContainer = jQuery(this._editingArea.get_viewToolbarContainerElement());
		$viewToolbarContainer.find("> *.toolbar:visible").hide(0);
		this._viewToolbarComponent.showToolbar();

		$viewToolbarContainer = null;

		this.fixActionToolbarWidth();

		if (this.get_isSearchBlockVisible() && this._searchBlockComponent) {
			var $searchBlockContainer = jQuery(this._editingArea.get_searchBlockContainerElement());
			$searchBlockContainer.find("> *.searchBlock:visible").hide(0);
			this._searchBlockComponent.showSearchBlock();
		}

		if (this._isContextBlockVisible && this._contextBlockComponent) {
		    var $contextBlockContainer = jQuery(this._editingArea.get_contextBlockContainerElement());
		    $contextBlockContainer.find("> *.contextBlock:visible").hide(0);
		    this._contextBlockComponent.showSearchBlock();
		}
	},

	hidePanels: function () {
		this._breadCrumbsComponent.hideBreadCrumbs();
		this._actionToolbarComponent.hideToolbar();
		this._viewToolbarComponent.hideToolbar();
		if (this.get_isSearchBlockVisible() && this._searchBlockComponent) {
			this._searchBlockComponent.hideSearchBlock();
		}

		if (this._isContextBlockVisible && this._contextBlockComponent) {
		    this._contextBlockComponent.hideSearchBlock();
		}
	},

	createSearchBlock: function () {
		var searchBlockComponent = Quantumart.QP8.BackendSearchBlockManager.getInstance()
			.createSearchBlock("searchBlock_" + this._tabId, this._entityTypeCode, this._parentEntityId, this,
				{
					"searchBlockContainerElementId": this._editingArea.get_searchBlockContainerElementId(),
					"tabId": this._tabId,
					"actionCode": this._actionCode,
					"searchBlockState": this.getHostStateProp("searchBlockState")
				});

		searchBlockComponent.attachObserver(EVENT_TYPE_SEARCH_BLOCK_FIND_START, this._onSearchHandler);
		searchBlockComponent.attachObserver(EVENT_TYPE_SEARCH_BLOCK_RESET_START, this._onSearchHandler);
		searchBlockComponent.attachObserver(EVENT_TYPE_SEARCH_BLOCK_RESIZED, this._onSearchBlockResizeHandler);

		this._searchBlockComponent = searchBlockComponent;
	},

	createContextBlock: function () {
	    var contextBlockComponent = Quantumart.QP8.BackendSearchBlockManager.getInstance()
			.createSearchBlock("contextBlock_" + this._tabId, this._entityTypeCode, this._parentEntityId, this,
				{
				    "searchBlockContainerElementId": this._editingArea.get_contextBlockContainerElementId(),
				    "tabId": this._tabId,
				    "actionCode": this._actionCode,
				    "contextSearch": true,
				    "hideButtons": true,
				    "searchBlockState": this.get_contextState()
				});
	    contextBlockComponent.initialize();

	    contextBlockComponent.attachObserver(EVENT_TYPE_CONTEXT_BLOCK_FIND_START, this._onContextSwitchingHandler);

	    this._contextBlockComponent = contextBlockComponent;
	},

	destroySearchBlock: function () {
		var searchBlockComponent = this._searchBlockComponent;

		if (searchBlockComponent) {
			searchBlockComponent.hideSearchBlock()
			searchBlockComponent.detachObserver(EVENT_TYPE_SEARCH_BLOCK_FIND_START, this._onSearchHandler);
			searchBlockComponent.detachObserver(EVENT_TYPE_SEARCH_BLOCK_RESET_START, this._onSearchHandler);
			searchBlockComponent.detachObserver(EVENT_TYPE_SEARCH_BLOCK_RESIZED, this._onSearchBlockResizeHandler);

			var searchBlockElementId = searchBlockComponent.get_searchBlockElementId();
			Quantumart.QP8.BackendSearchBlockManager.getInstance().destroySearchBlock(searchBlockElementId);

			this._searchBlockComponent = null;
		}
	},

	destroyContextBlock: function () {
	    var contextBlockComponent = this._contextBlockComponent;

	    if (contextBlockComponent) {
	        contextBlockComponent.hideSearchBlock()
	        contextBlockComponent.detachObserver(EVENT_TYPE_CONTEXT_BLOCK_FIND_START, this._onContextSwitchingHandler);

	        var contextBlockElementId = contextBlockComponent.get_searchBlockElementId();
	        Quantumart.QP8.BackendSearchBlockManager.getInstance().destroySearchBlock(contextBlockElementId);

	        this._contextBlockComponent = null;
	        this._isContextBlockVisible = false;
	    }
	},

	showLoadingLayer: function () {
		if (this._editingArea) {
			if (this.isSelected())
				this._editingArea.showAjaxLoadingLayer();
		}
	},

	hideLoadingLayer: function () {
		if (this._editingArea) {
			if (this.isSelected())
				this._editingArea.hideAjaxLoadingLayer();
		}
	},

	destroyPanels: function () {
		if (this._breadCrumbsComponent) {
			this._breadCrumbsComponent.detachObserver(EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, this._onGeneralEventHandler);
			this._breadCrumbsComponent.detachObserver(EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, this._onGeneralEventHandler);
			var breadCrumbsElementId = this._breadCrumbsComponent.get_breadCrumbsElementId();
			Quantumart.QP8.BackendBreadCrumbsManager.getInstance().destroyBreadCrumbs(breadCrumbsElementId);
			this._breadCrumbsComponent = null;
		}

		if (this._actionToolbarComponent) {
			this._actionToolbarComponent.detachObserver(EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED, this._onGeneralEventHandler);
			this._actionToolbarComponent.dispose();
			this._actionToolbarComponent = null;
		}

		if (this._viewToolbarComponent) {
			this._viewToolbarComponent.detachObserver(EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED, this._onGeneralEventHandler);
			this._viewToolbarComponent.detachObserver(EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED, this._onGeneralEventHandler);
			this._viewToolbarComponent.detachObserver(EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED, this._onGeneralEventHandler);
			this._viewToolbarComponent.dispose();
			this._viewToolbarComponent = null;
		}

		var editingArea = this._editingArea;
		if (editingArea) {

			if (!$q.isNullOrWhiteSpace(this._id)) {
				this._editingArea.removeDocument(this._id);
			}

			editingArea = null;
			this._editingArea = null;
		}
	},

	isSelected: function () {
		var isSelected = (this._id == this._editingArea.getSelectedDocumentId());

		return isSelected;
	},

	updateTitle: function (eventArgs) {
		this._editingArea.updateTab(this.get_tabId(), eventArgs);
	},

	onDocumentError: function () {
	    this._isCloseForced = false;
		this._editingArea.onDocumentError(this._id);
	},

	onChangeContent: function (eventType, sender, eventArgs) {
		var tabId = this._editingArea.getExistingTabId(eventArgs);
		if (tabId != 0) {
		    var selectedDocument = this._editingArea.selectDocument(tabId);
		    if (selectedDocument) {
		        selectedDocument.onSelectedThroughExecution(eventArgs);
		    }
		}
		else {
			this.changeContent(eventArgs);
		}
	},

	saveSelectionContext: function (eventArgs) {
		this._selectedParentEntityId = eventArgs.get_parentEntityId();
	},

	onActionExecuting: function (eventArgs) {
	    this._copyCurrentContextToEventArgs(eventArgs);
		return this._editingArea.onActionExecuting(eventArgs);
	},

	onEntityReaded: function (eventArgs) {
		return this._editingArea.onEntityReaded(eventArgs);
	},

	onDocumentChanging: function (isLocal) {
		this._editingArea.onDocumentLoading(this._id, isLocal);
	},

	onDocumentChanged: function (isLocal) {
		this._editingArea.onDocumentLoaded(this._id, isLocal);
	},

	onNeedUp: function (eventArgs) {
		var tabId = 0;
		if (this._callerContext && this._callerContext.eventArgs) {
			tabId = this._editingArea.getExistingTabId(this._callerContext.eventArgs);
			if (tabId != 0) {
				this._editingArea.selectDocument(tabId);
				this._editingArea.closeDocument(this.get_tabId(), false, true);
				return;
			}
		}

		if (tabId == 0) {
			var bcItem = this._breadCrumbsComponent.getLastItemButOne();
			if (bcItem) {
				var bcEventArgs = this._breadCrumbsComponent.getItemActionEventArgs(bcItem);
				if (bcEventArgs) {
					tabId = this._editingArea.getExistingTabId(bcEventArgs);
				}
			}
			if (tabId != 0) {
				this._editingArea.selectDocument(tabId);
				this._editingArea.closeDocument(this.get_tabId(), false, true);
			}
			else {
				this.changeContent(bcEventArgs, true);
			}
		}
	},

	saveAndCloseRequest: function (eventArgs) {
		Sys.Debug.trace("saveAndCloseRequest: " + eventArgs._tabId);

	    var context = this.get_documentContext();
	    if (context && context._options.saveAndCloseActionCode) {
	        var main = this.get_mainComponent();
	        if (main != null && Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main) && main.isFieldsChanged()) {
	            this._isCloseForced = true;
	            this.executeAction(context._options.saveAndCloseActionCode);
	        }
	    }
	        //if (main != null && Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main) && !main.isFieldsChanged()) {
	        //    if (!context.get_mainComponent()._formHasErrors) {
	        //        this._editingArea.closeDocument(this.get_tabId(), true);
	        //    }
	        //}
	        //else{
	        //    this._isCloseForced = true;
	        //    this.executeAction(context._options.saveAndCloseActionCode);
	        //}
	    //}
	    //else {
	    //    this._editingArea.closeDocument(this.get_tabId(), true);
	    //}
	},

	onSaveAndClose: function () {
	    if (this._isCloseForced) {
	        this._isCloseForced = false;
	        var context = this.get_documentContext();
	        if (context && context.get_mainComponentType() == $e.MainComponentType.Editor && !context.get_mainComponent()._formHasErrors){
	            this._editingArea.closeDocument(this.get_tabId(), true);
	        }
	    }
	},

	resetSelectedEntities: function () {
		$q.clearArray(this._selectedEntities);
	},

	_onLibraryResized: function () {
		var $docContainer = jQuery(this._editingArea.get_documentsContainerElement());
		var $docWrp = jQuery(this._documentWrapperElement);
		$docWrp.height($docContainer.height());
		$docContainer = null;
		$docWrp = null;
	},

	_onExternalCallerContextsUnbinded: function (unbindingEventArgs) {
		this._editingArea.hostExternalCallerContextsUnbinded(unbindingEventArgs);
	},

	_isWindow: function() { return false; },

	dispose: function () {
		Quantumart.QP8.BackendEditingDocument.callBaseMethod(this, "dispose");

		this.destroyPanels();
		this.destroySearchBlock();
		this.destroyContextBlock();

		this.removeDocumentWrapper();

		$q.clearArray(this._entities);
		$q.clearArray(this._selectedEntities);

		this._documentContext = null;
		this._documentWrapperElement = null;

		$q.collectGarbageInIE();
	}
};

Quantumart.QP8.BackendEditingDocument.registerClass("Quantumart.QP8.BackendEditingDocument", Quantumart.QP8.BackendDocumentHost);
//#endregion

