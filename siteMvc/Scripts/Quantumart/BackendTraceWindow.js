// ****************************************************************************
// *** Компонент "Консоль трассировки"										***
// ****************************************************************************

//#region class BackendTraceWindow
// === Класс "Консоль трассировки" ===
Quantumart.QP8.BackendTraceWindow = function() {
	Quantumart.QP8.BackendTraceWindow.initializeBase(this);

	var _self = this;

	this._onTraceToolbarButtonClickedHandler = function (eventType, sender, args) {
		var value = args.get_value();

		if (value == _self.START_BUTTON_CODE) {
			Sys.Debug.startTrace();
			_self._refreshToolbarItems();
		}
		else if (value == _self.STOP_BUTTON_CODE) {
			Sys.Debug.stopTrace();
			_self._refreshToolbarItems();
		}
		else if (value == _self.CLEAR_BUTTON_CODE) {
			Sys.Debug.clearTrace();
		}
	};

	this._onTraceWindowResizeHandler = function () {
		_self._fixConsoleAreaHeight();
	};

	this._onTraceWindowOpenHandler = function () {
		_self._fixConsoleAreaHeight();
	};
};

Quantumart.QP8.BackendTraceWindow.prototype = {
	_toolbarComponent: null, // компонент "Панель управления"
	_windowComponent: null, // компонент "Всплывающее окно"

	_onToolbarButtonClicked: null,
	_onTraceWindowResizeHandler: null,
	_onTraceWindowOpenHandler: null,

	START_BUTTON_CODE: "start",
	STOP_BUTTON_CODE: "stop",
	CLEAR_BUTTON_CODE: "clear",

	initialize: function () {
		this._windowComponent = this._createWindow();
	},

	_createWindow: function () {
		var traceConsoleHtml = new $.telerik.stringBuilder();
		traceConsoleHtml
			.cat('<div id="traceToolbarWrapper" class="toolbarWrapper">\n')
			.cat('	<div id="traceActionToolbarWrapper" class="actionToolbarWrapper"></div>\n')
			.cat('</div>\n')
			.cat('<div id="TraceConsole" class="area"></div>\n')
			;
	
		var windowComponent = $.telerik.window.create({
			title: $l.TraceWindow.windowTitle,
			html: traceConsoleHtml.string(),
			width: 575,
			height: 300,
	        minWidth: 200,
	        minHeight: 50,
	        modal: false,
	        actions: ["Maximize", "Close"],
			resizable: true,
			draggable: true,
			effects: { list: [{ name: "toggle" }, { name: "property", properties: ["opacity"]}], openDuration: "fast", closeDuration: "fast" }
		}).data("tWindow").center();
	
		traceConsoleHtml = null;
	
		var $window = jQuery(windowComponent.element);
		$window
			.addClass("popupWindow")
			.addClass("traceWindow")
			.css("display", "none")
			.bind("open", this._onTraceWindowOpenHandler)
			.bind("resize", this._onTraceWindowResizeHandler)
			;
	
		var $content = $window.find("DIV.t-window-content:first");
		if (jQuery.support.borderRadius) {
			$content.css("paddingBottom", "5px");
		}
	
		$content = null;
		$window = null;
	
		var toolbarComponent = new Quantumart.QP8.BackendToolbar();
		toolbarComponent.set_toolbarElementId("traceToolbar");
		toolbarComponent.set_toolbarContainerElementId("traceActionToolbarWrapper");
		toolbarComponent.initialize();
	
		toolbarComponent.attachObserver(EVENT_TYPE_TOOLBAR_BUTTON_CLICKED, this._onTraceToolbarButtonClickedHandler);
	
		this._toolbarComponent = toolbarComponent;
	
		this._addToolbarItemsToToolbar();
		this._refreshToolbarItems();
	
		return windowComponent;
	},

	toggleWindow: function () {
		var $window = jQuery(this._windowComponent.element);
		if ($window.is(":hidden")) {
			this._openWindow();
		}
		else {
			this._closeWindow();
		}
	
		$window = null;
	},

	_openWindow: function () {
		this._windowComponent.open();
		this._toolbarComponent.showToolbar();
		this._fixConsoleAreaHeight();
	},

	_closeWindow: function () {
		this._toolbarComponent.hideToolbar();
		this._windowComponent.close();
	},

	_addToolbarItemsToToolbar: function () {
		var dataItems = [];
	
		var startButton = {
			Type: TOOLBAR_ITEM_TYPE_BUTTON,
			Value: this.START_BUTTON_CODE,
			Text: "",
			Tooltip: $l.TraceWindow.startButtonTooltip,
			ItemsAffected: 1,
			Icon: "start.gif"
		};
	
		Array.add(dataItems, startButton);
	
		var stopButton = {
			Type: TOOLBAR_ITEM_TYPE_BUTTON,
			Value: this.STOP_BUTTON_CODE,
			Text: "",
			Tooltip: $l.TraceWindow.stopButtonTooltip,
			ItemsAffected: 1,
			Icon: "stop.gif"
		};
	
		Array.add(dataItems, stopButton);
	
		var clearButton = {
			Type: TOOLBAR_ITEM_TYPE_BUTTON,
			Value: this.CLEAR_BUTTON_CODE,
			Text: "",
			Tooltip: $l.TraceWindow.clearButtonTooltip,
			ItemsAffected: 1,
			Icon: "clear.gif"
		};
	
		Array.add(dataItems, clearButton);
	
		this._toolbarComponent.addToolbarItemsToToolbar(dataItems);
	},

	_refreshToolbarItems: function () {
		var stopped = Sys.Debug.isStopped();
		this._toolbarComponent.setEnableState(this.START_BUTTON_CODE, stopped);
		this._toolbarComponent.setEnableState(this.STOP_BUTTON_CODE, !stopped);
	},

	_fixConsoleAreaHeight: function () {
		var $window = jQuery(this._windowComponent.element);
		var $content = $window.find("DIV.t-window-content:first");
		var $toolbar = jQuery(this._toolbarComponent.get_toolbarElement());
		var $area = $content.find("DIV.area:first");
		var areaHeight = $content.height() - $toolbar.outerHeight();
	
		$area.height(areaHeight);
	
		$area = null;
		$toolbar = null;
		$content = null;
		$window = null;
	},

	dispose: function () {
		Quantumart.QP8.BackendTraceWindow.callBaseMethod(this, "dispose");
	
		if (this._toolbarComponent) {
			this._toolbarComponent.dispose();
			this._toolbarComponent = null;
		}
	
		if (this._windowComponent) {
			var windowComponent = this._windowComponent;
			var $window = jQuery(windowComponent.element);
			$window
				.unbind("open", this._onTraceWindowOpenHandler)
				.unbind("resize", this._onTraceWindowResizeHandler)
				;
	
			$window = null;
	
			$c.destroyPopupWindow(windowComponent);
	
			windowComponent = null;
			this._windowComponent = null;
		}
	
		Quantumart.QP8.BackendTraceWindow._instance = null;
	
		$q.collectGarbageInIE();
	}
};

Quantumart.QP8.BackendTraceWindow._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Трассировочная консоль"
Quantumart.QP8.BackendTraceWindow.getInstance = function Quantumart$QP8$BackendTraceWindow$getInstance() {
	if (Quantumart.QP8.BackendTraceWindow._instance == null) {
		Quantumart.QP8.BackendTraceWindow._instance = new Quantumart.QP8.BackendTraceWindow();
		Quantumart.QP8.BackendTraceWindow._instance.initialize();
	}

	return Quantumart.QP8.BackendTraceWindow._instance;
};

// Уничтожает экземпляр класса "Трассировочная консоль"
Quantumart.QP8.BackendTraceWindow.destroyInstance = function Quantumart$QP8$BackendTraceWindow$destroyInstance() {
	if (Quantumart.QP8.BackendTraceWindow._instance) {
		Quantumart.QP8.BackendTraceWindow._instance.dispose();
	}
};

Quantumart.QP8.BackendTraceWindow.registerClass("Quantumart.QP8.BackendTraceWindow", Quantumart.QP8.Observable);
//#endregion