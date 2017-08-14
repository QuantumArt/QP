window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED = 'OnSelectPopupWindowResultSelected';
window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED = 'OnSelectPopupWindowClosed';

Quantumart.QP8.BackendSelectPopupWindow = function (eventArgs, options) {
	Quantumart.QP8.BackendSelectPopupWindow.initializeBase(this);
	var manager = Quantumart.QP8.BackendPopupWindowManager.getInstance();
	this._popupWindowId = manager.generatePopupWindowId();
	this._isMultipleEntities = eventArgs.get_isMultipleEntities();
	this._actionCode = eventArgs.get_actionCode();
	this._entityTypeCode = eventArgs.get_entityTypeCode();
	this._parentEntityId = eventArgs.get_parentEntityId();
	this._popupWindowToolbarComponent = this._createToolbar();


	options = Object.assign({}, options, {
		popupWindowId: this._popupWindowId,
		showBreadCrumbs: false,
		saveSelectionWhenChangingView: true,
		customActionToolbarComponent: this._popupWindowToolbarComponent,
		isModal: true,
		allowResize: false,
		allowDrag: false
	});

	eventArgs.set_actionTypeCode(Quantumart.QP8.BackendActionType.getActionTypeCodeByActionCode(this._actionCode));
	this._popupWindowComponent = manager.createPopupWindow(eventArgs, options);
	this._popupWindowComponent.attachObserver(window.EVENT_TYPE_POPUP_WINDOW_CLOSED, $.proxy(this._onClosed, this));
	this._popupWindowComponent.attachObserver(window.EVENT_TYPE_POPUP_WINDOW_CLOSED, $.proxy(this._popupWindowClosedHandler, this));
};

Quantumart.QP8.BackendSelectPopupWindow.prototype = {
	_isMultipleEntities: false,
	_popupWindowId: '',
	_popupWindowToolbarComponent: null,
	_popupWindowComponent: null,
	_allowMultipleItemSelection: false,

	SELECT_BUTTON_CODE: 'select',
	SELECT_ALL_BUTTON_CODE: 'select_all',
	DESELECT_ALL_BUTTON_CODE: 'deselect_all',
	REFRESH_BUTTON_CODE: 'refresh',

	_onPopupWindowToolbarButtonClicked: function (eventType, sender, args) {
		if (this._popupWindowComponent) {
			var value = args.get_value();
			if (value == this.SELECT_BUTTON_CODE) {
				var selectedEntities = this._popupWindowComponent.get_selectedEntities();
				var context = this._popupWindowComponent.get_selectionContext();
				this.notify(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, { entities: selectedEntities, context: context, entityTypeCode: this._entityTypeCode, parentEntityId: this._parentEntityId });
			} else if (value == this.SELECT_ALL_BUTTON_CODE) {
				this._popupWindowComponent.selectAllEntities();
			} else if (value == this.DESELECT_ALL_BUTTON_CODE) {
				this._popupWindowComponent.deselectAllEntities();
			} else if (value == this.REFRESH_BUTTON_CODE) {
				this._popupWindowComponent.refresh();
			}
		}
	},

	_onClosed: function (eventType, sender, args) {
		this._popupWindowComponent = null;
		var eventArgs = new Quantumart.QP8.BackendEventArgs();
		this.notify(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, eventArgs);
		eventArgs = null;
	},

	_createToolbar: function () {
		var toolbar = new Quantumart.QP8.BackendToolbar();
		toolbar.set_toolbarElementId('popupWindowToolbar_' + this._popupWindowId);
		toolbar.initialize();
		toolbar.attachObserver(window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED, $.proxy(this._onPopupWindowToolbarButtonClicked, this));
		toolbar.addToolbarItemsToToolbar(this._getToolbarItems());
		return toolbar;
	},

	_getToolbarItems: function () {
		var dataItems = [];

		var selectButton = {
			Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
			Value: this.SELECT_BUTTON_CODE,
			Text: $l.EntityDataList.selectPopupWindowButtonText,
			Tooltip: $l.EntityDataList.selectPopupWindowButtonText,
			AlwaysEnabled: true,
			Icon: 'multiple_pick.gif'
		};

		Array.add(dataItems, selectButton);

		if (this._isMultipleEntities) {
			var selectAllButton = {
				Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
				Value: this.SELECT_ALL_BUTTON_CODE,
				Text: $l.EntityDataList.selectAllPopupWindowButtonText,
				Tooltip: $l.EntityDataList.selectAllPopupWindowButtonText,
				AlwaysEnabled: true,
				Icon: 'select_all.gif'
			};

			Array.add(dataItems, selectAllButton);

			var deselectAllButton = {
				Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
				Value: this.DESELECT_ALL_BUTTON_CODE,
				Text: $l.EntityDataList.deselectAllPopupWindowButtonText,
				Tooltip: $l.EntityDataList.deselectAllPopupWindowButtonText,
				AlwaysEnabled: true,
				Icon: 'deselect_all.gif'
			};

			Array.add(dataItems, deselectAllButton);
		}

		var refreshButton = {
			Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
			Value: this.REFRESH_BUTTON_CODE,
			Text: $l.EntityDataList.refreshPopupWindowButtonText,
			Tooltip: $l.EntityDataList.refreshPopupWindowButtonText,
			AlwaysEnabled: true,
			Icon: 'refresh.gif'
		};

		Array.add(dataItems, refreshButton);
		return dataItems;
	},

	openWindow: function () {
	    if (this._popupWindowComponent) {
			this._popupWindowComponent.openWindow();
		}
	},

	closeWindow: function () {
		if (this._popupWindowComponent) {
			this._popupWindowComponent.closeWindow();
		}
	},
	_popupWindowClosedHandler: function () {
	    this.dispose();
	},
	dispose: function () {
		if (this._popupWindowToolbarComponent) {
			this._popupWindowToolbarComponent.detachObserver(window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED);
			this._popupWindowToolbarComponent.dispose();
			this._popupWindowToolbarComponent = null;
		}

		if (this._popupWindowComponent) {
			this._popupWindowComponent.detachObserver(window.EVENT_TYPE_POPUP_WINDOW_CLOSED);
			this._popupWindowComponent.dispose();
			this._popupWindowComponent = null;
		}
	}

};

Quantumart.QP8.BackendSelectPopupWindow.registerClass('Quantumart.QP8.BackendSelectPopupWindow', Quantumart.QP8.Observable);
