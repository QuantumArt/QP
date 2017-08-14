window.EVENT_TYPE_SPLITTER_INITIALIZED = 'OnSplitterInitialized';
window.EVENT_TYPE_SPLITTER_RESIZED = 'OnSplitterResized';
window.EVENT_TYPE_SPLITTER_DRAG_START = 'OnSplitterDragStart';
window.EVENT_TYPE_SPLITTER_DROP = 'OnSplitterDrop';

Quantumart.QP8.BackendSplitter = function (splitterElementId, options) {
	Quantumart.QP8.BackendSplitter.initializeBase(this);

	this._splitterElementId = splitterElementId;
	if ($q.isObject(options)) {
		if (options.firstPaneWidth) {
			this._firstPaneWidth = options.firstPaneWidth;
		}

		if (options.minFirstPaneWidth) {
			this._minFirstPaneWidth = options.minFirstPaneWidth;
		}

		if (options.maxFirstPaneWidth) {
			this._maxFirstPaneWidth = options.maxFirstPaneWidth;
		}

		if (options.stateCookieName) {
			this._stateCookieName = options.stateCookieName;
		}

		if (options.toWindowResize) {
			this._toWindowResize = true;
		}
	}

	this._onSplitterResizedHandler = jQuery.proxy(this._onSplitterResized, this);
	this._onSplitterDragStartHandler = jQuery.proxy(this._onSplitterDragStart, this);
	this._onSplitterDropHandler = jQuery.proxy(this._onSplitterDrop, this);
};

Quantumart.QP8.BackendSplitter.prototype = {
	_splitterElementId: '',
	_splitterComponent: null,
	_firstPaneElement: null,
	_firstPaneWidth: 250,
	_minFirstPaneWidth: 50,
	_maxFirstPaneWidth: 400,
	_stateCookieName: 'leftMenuSize',
	_toWindowResize: false,

	get_splitterElementId: function () {
		return this._splitterElementId;
	},

	set_splitterElementId: function (value) {
		this._splitterElementId = value;
	},

	get_firstPaneWidth: function () {
		return this._firstPaneWidth;
	},

	set_firstPaneWidth: function (value) {
		this._firstPaneWidth = value;
	},

	get_minFirstPaneWidth: function () {
		return this._minFirstPaneWidth;
	},

	set_minFirstPaneWidth: function (value) {
		this._minFirstPaneWidth = value;
	},

	get_maxFirstPaneWidth: function () {
		return this._maxFirstPaneWidth;
	},

	set_maxFirstPaneWidth: function (value) {
		this._maxFirstPaneWidth = value;
	},

	get_stateCookieName: function () {
		return this._stateCookieName;
	},

	set_stateCookieName: function (value) {
		this._stateCookieName = value;
	},


	onSplitterResizedHandler: null,

	initialize: function () {
		this._splitterComponent = jQuery(`#${  this._splitterElementId}`);
		var splitter = this._splitterComponent.data('tSplitter');
		if (this._toWindowResize) {
			jQuery(window).trigger('resize');
		}

		var $firstPane = jQuery('.t-pane:first', this._splitterComponent);
		this._firstPaneElement = $firstPane.get(0);

		splitter.onResize = this._onSplitterResizedHandler;
		splitter.onDragStart = this._onSplitterDragStartHandler;
		splitter.onDrop = this._onSplitterDropHandler;

		var firstPaneWidth = $firstPane.width();
		var firstPaneHeight = $firstPane.height();

		var eventArgs = new Quantumart.QP8.BackendSplitterEventArgs();
		eventArgs.set_firstPane(this._firstPaneElement);
		eventArgs.set_firstPaneWidth(firstPaneWidth);
		eventArgs.set_firstPaneHeight(firstPaneHeight);


		this.notify(window.EVENT_TYPE_SPLITTER_INITIALIZED, eventArgs);


		eventArgs = null;
		$firstPane = null;
		splitter = null;
	},

	_onSplitterResized: function (event) {
		var $firstPane = jQuery(this._firstPaneElement);
		var firstPaneWidth = $firstPane.width();
		var firstPaneHeight = $firstPane.height();
		var eventArgs = new Quantumart.QP8.BackendSplitterEventArgs();

		eventArgs.set_firstPane(this._firstPaneElement);
		eventArgs.set_firstPaneWidth(firstPaneWidth);
		eventArgs.set_firstPaneHeight(firstPaneHeight);

		this.notify(window.EVENT_TYPE_SPLITTER_RESIZED, eventArgs);
	},

	_onSplitterDragStart: function (event) {
	    this.notify(window.EVENT_TYPE_SPLITTER_DRAG_START, {});
	},

	_onSplitterDrop: function (event) {
	    this.notify(window.EVENT_TYPE_SPLITTER_DROP, {});
	},


	resize: function () {
		if (this._splitterComponent) {
			var splitter = this._splitterComponent.data('tSplitter');
			splitter.resize();
			splitter = null;
		}
	},

	dispose: function () {
		Quantumart.QP8.BackendSplitter.callBaseMethod(this, 'dispose');

		if (this._splitterComponent != null) {
			var splitter = this._splitterComponent.data('tSplitter');
			splitter.onResize = null;
			splitter = null;
			this._splitterComponent = null;
		}

		this._firstPaneElement = null;

		this._onSplitterResizedHandler = null;
		this._onSplitterDragStartHandler = null;
		this._onSplitterDropHandler = null;

		$q.collectGarbageInIE();
	}
};

Quantumart.QP8.BackendSplitter.registerClass('Quantumart.QP8.BackendSplitter', Quantumart.QP8.Observable);
Quantumart.QP8.BackendSplitterEventArgs = function () {
	Quantumart.QP8.BackendSplitterEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendSplitterEventArgs.prototype = {
	_firstPaneElement: null,
	_firstPaneWidth: 0,
	_firstPaneHeight: 0,

	get_firstPane: function () {
		return this._firstPaneElement;
	},

	set_firstPane: function (value) {
		this._firstPaneElement = value;
	},

	get_firstPaneWidth: function () {
		return this._firstPaneWidth;
	},

	set_firstPaneWidth: function (value) {
		this._firstPaneWidth = value;
	},

	get_firstPaneHeight: function () {
		return this._firstPaneHeight;
	},

	set_firstPaneHeight: function (value) {
		this._firstPaneHeight = value;
	}
};

Quantumart.QP8.BackendSplitterEventArgs.registerClass('Quantumart.QP8.BackendSplitterEventArgs', Sys.EventArgs);
