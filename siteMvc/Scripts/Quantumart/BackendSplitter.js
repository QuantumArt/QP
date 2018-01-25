import { Observable } from './Common/Observable';
import { $q } from './Utils';

window.EVENT_TYPE_SPLITTER_INITIALIZED = 'OnSplitterInitialized';
window.EVENT_TYPE_SPLITTER_RESIZED = 'OnSplitterResized';
window.EVENT_TYPE_SPLITTER_DRAG_START = 'OnSplitterDragStart';
window.EVENT_TYPE_SPLITTER_DROP = 'OnSplitterDrop';

export class BackendSplitter extends Observable {
  constructor(splitterElementId, options) {
    super();

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

    this._onSplitterResizedHandler = $.proxy(this._onSplitterResized, this);
    this._onSplitterDragStartHandler = $.proxy(this._onSplitterDragStart, this);
    this._onSplitterDropHandler = $.proxy(this._onSplitterDrop, this);
  }

  _splitterElementId = '';
  _splitterComponent = null;
  _firstPaneElement = null;
  _firstPaneWidth = 250;
  _minFirstPaneWidth = 50;
  _maxFirstPaneWidth = 400;
  _stateCookieName = 'leftMenuSize';
  _toWindowResize = false;

  getSplitterElementId() {
    return this._splitterElementId;
  }

  setSplitterElementId(value) {
    this._splitterElementId = value;
  }

  getFirstPaneWidth() {
    return this._firstPaneWidth;
  }

  setFirstPaneWidth(value) {
    this._firstPaneWidth = value;
  }

  getMinFirstPaneWidth() {
    return this._minFirstPaneWidth;
  }

  setMinFirstPaneWidth(value) {
    this._minFirstPaneWidth = value;
  }

  getMaxFirstPaneWidth() {
    return this._maxFirstPaneWidth;
  }

  setMaxFirstPaneWidth(value) {
    this._maxFirstPaneWidth = value;
  }

  getStateCookieName() {
    return this._stateCookieName;
  }

  setStateCookieName(value) {
    this._stateCookieName = value;
  }


  onSplitterResizedHandler = null;

  initialize() {
    this._splitterComponent = $(`#${this._splitterElementId}`);
    let splitter = this._splitterComponent.data('tSplitter');
    if (this._toWindowResize) {
      $(window).trigger('resize');
    }

    let $firstPane = $('.t-pane:first', this._splitterComponent);
    this._firstPaneElement = $firstPane.get(0);

    splitter.onResize = this._onSplitterResizedHandler;
    splitter.onDragStart = this._onSplitterDragStartHandler;
    splitter.onDrop = this._onSplitterDropHandler;

    const firstPaneWidth = $firstPane.width();
    const firstPaneHeight = $firstPane.height();

    // eslint-disable-next-line no-use-before-define
    let eventArgs = new BackendSplitterEventArgs();
    eventArgs.setFirstPane(this._firstPaneElement);
    eventArgs.setFirstPaneWidth(firstPaneWidth);
    eventArgs.setFirstPaneHeight(firstPaneHeight);


    this.notify(window.EVENT_TYPE_SPLITTER_INITIALIZED, eventArgs);


    eventArgs = null;
    $firstPane = null;
    splitter = null;
  }

  _onSplitterResized() {
    const $firstPane = $(this._firstPaneElement);
    const firstPaneWidth = $firstPane.width();
    const firstPaneHeight = $firstPane.height();
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendSplitterEventArgs();

    eventArgs.setFirstPane(this._firstPaneElement);
    eventArgs.setFirstPaneWidth(firstPaneWidth);
    eventArgs.setFirstPaneHeight(firstPaneHeight);

    this.notify(window.EVENT_TYPE_SPLITTER_RESIZED, eventArgs);
  }

  _onSplitterDragStart() {
    this.notify(window.EVENT_TYPE_SPLITTER_DRAG_START, {});
  }

  _onSplitterDrop() {
    this.notify(window.EVENT_TYPE_SPLITTER_DROP, {});
  }


  resize() {
    if (this._splitterComponent) {
      let splitter = this._splitterComponent.data('tSplitter');
      splitter.resize();
      splitter = null;
    }
  }

  dispose() {
    super.dispose();

    if (!$q.isNull(this._splitterComponent)) {
      let splitter = this._splitterComponent.data('tSplitter');
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
}


export class BackendSplitterEventArgs extends Sys.EventArgs {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    super();
  }

  _firstPaneElement = null;
  _firstPaneWidth = 0;
  _firstPaneHeight = 0;

  getFirstPane() {
    return this._firstPaneElement;
  }

  setFirstPane(value) {
    this._firstPaneElement = value;
  }

  getFirstPaneWidth() {
    return this._firstPaneWidth;
  }

  setFirstPaneWidth(value) {
    this._firstPaneWidth = value;
  }

  getFirstPaneHeight() {
    return this._firstPaneHeight;
  }

  setFirstPaneHeight(value) {
    this._firstPaneHeight = value;
  }
}


Quantumart.QP8.BackendSplitter = BackendSplitter;
Quantumart.QP8.BackendSplitterEventArgs = BackendSplitterEventArgs;
