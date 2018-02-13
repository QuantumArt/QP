/* eslint-disable no-plusplus */
import { BackendTabEventArgs } from '../Common/BackendTabEventArgs';
import { event } from '../Utils/Event';

const HISTORY_INITIAL_STATE = 'HISTORY_INITIAL_STATE';
const HISTORY_ALL_TABS_CLOSED_STATE = 'HISTORY_ALL_TABS_CLOSED_STATE';
const HISTORY_TAB_CHANGED_STATE = 'HISTORY_TAB_CHANGED_STATE';

export class BackendBrowserHistoryManager {
  /** @type {BackendBrowserHistoryManager} */
  static _instance;

  static getInstance() {
    if (!BackendBrowserHistoryManager._instance) {
      BackendBrowserHistoryManager._instance = new BackendBrowserHistoryManager();
    }
    return BackendBrowserHistoryManager._instance;
  }

  _currentStateIndex = 0;
  _modalWindowIsShown = false;
  _hasPendingTabChangedEvent = false;
  _pendingStateQueue = [];

  /** Переключение вкладки или навигация внутри вкладки */
  onPopStateTabChanged = event(this, BackendTabEventArgs);

  /** Переход к состоянию, когда ни одна вкладка не выбрана */
  onPopStateAllTabsClosed = event(this);

  constructor() {
    this._defaultTitle = document.title;
    this._handlePopState = this._handlePopState.bind(this);
    this.handleModalWindowOpen = this.handleModalWindowOpen.bind(this);
    this.handleModalWindowClose = this.handleModalWindowClose.bind(this);
  }

  initialize() {
    const { state } = window.history;

    if (state === null) {
      window.history.replaceState({
        type: HISTORY_INITIAL_STATE,
        stateIndex: this._currentStateIndex,
        title: this._defaultTitle,
      }, document.title);

      this.pushStateAllTabsClosed();
    } else if (state.type === HISTORY_ALL_TABS_CLOSED_STATE) {
      this._currentStateIndex = state.stateIndex;
    } else if (state.type === HISTORY_TAB_CHANGED_STATE) {
      this._currentStateIndex = state.stateIndex;
      this.pushStateAllTabsClosed();
    }

    window.addEventListener('popstate', this._handlePopState);
  }

  _handlePopState({ state }) {
    console.log('POP', state);

    if (this._modalWindowIsShown
      || this._hasPendingTabChangedEvent
      || state.type === HISTORY_INITIAL_STATE
    ) {
      this._rollbackBrowserState();
    } else if (state.type === HISTORY_ALL_TABS_CLOSED_STATE) {
      this._currentStateIndex = state.stateIndex;
      document.title = state.title;
      this.onPopStateAllTabsClosed();
    } else if (state.type === HISTORY_TAB_CHANGED_STATE) {
      console.log('START EXECUTION', state);
      const previousStateId = this._currentStateIndex;
      this._currentStateIndex = state.stateIndex;
      this._hasPendingTabChangedEvent = true;

      const eventArgs = this._deserializeTabEvent(state);
      eventArgs.fromHistory = true;

      eventArgs.onExecutionFinished.attach((_sender, isNavigationPerformed) => {
        if (!isNavigationPerformed) {
          this._currentStateIndex = previousStateId;
          this._rollbackBrowserState();
        }
        this._hasPendingTabChangedEvent = false;
        this._applyPendingStateQueue();
        console.log('FINISH EXECUTION', state);
      });

      document.title = state.title;
      this.onPopStateTabChanged(eventArgs);
    }
  }

  _rollbackBrowserState() {
    const { state } = window.history;

    if (state.stateIndex > this._currentStateIndex) {
      console.log('BACK');
      window.history.back();
    } else if (state.stateIndex < this._currentStateIndex) {
      console.log('FORWARD');
      window.history.forward();
    } else {
      console.log('SAME STATE');
    }
  }

  handleModalWindowOpen() {
    this._modalWindowIsShown = true;
  }

  handleModalWindowClose() {
    this._modalWindowIsShown = false;
  }

  /**
   * @param {BackendTabEventArgs} eventArgs
   */
  pushStateTabChanged(eventArgs) {
    const { state } = window.history;

    if (eventArgs.fromHistory) {
      return;
    }
    if (state.type === HISTORY_TAB_CHANGED_STATE) {
      const historyEventArgs = this._deserializeTabEvent(state);

      if (eventArgs.structurallyEquals(historyEventArgs)) {
        return;
      }
    }

    this._pushOrEnqueueState(this._serializeTabEvent(eventArgs));
  }

  pushStateAllTabsClosed() {
    const { state } = window.history;

    if (state.type === HISTORY_ALL_TABS_CLOSED_STATE) {
      return;
    }

    this._pushOrEnqueueState({
      type: HISTORY_ALL_TABS_CLOSED_STATE,
      title: this._defaultTitle
    });
  }

  /**
   * @param {object} state
   */
  _pushOrEnqueueState(state) {
    if (this._hasPendingTabChangedEvent) {
      this._pendingStateQueue.push(state);
      console.log('ENQUEUE', state);
    } else {
      this._pushState(state);
      console.log('PUSH', state);
    }
  }

  _applyPendingStateQueue() {
    this._pendingStateQueue.forEach(pendingState => {
      this._pushState(pendingState);
    });
    this._pendingStateQueue = [];
  }

  /**
   * @param {object} state
   */
  _pushState(state) {
    this._currentStateIndex++;
    // eslint-disable-next-line no-param-reassign
    state.stateIndex = this._currentStateIndex;
    window.history.pushState(state, document.title);
    document.title = state.title;
  }

  /**
   * @param {BackendTabEventArgs} eventArgs
   * @returns serialized BackendTabEventArgs
   */
  _serializeTabEvent(eventArgs) {
    return {
      type: HISTORY_TAB_CHANGED_STATE,
      entityTypeCode: eventArgs.get_entityTypeCode(),
      entityId: eventArgs.get_entityId(),
      entityName: eventArgs.get_entityName(),
      parentEntityId: eventArgs.get_parentEntityId(),
      actionCode: eventArgs.get_actionCode(),
      actionTypeCode: eventArgs.get_actionTypeCode(),
      entities: eventArgs.get_entities(),
      isMultipleEntities: eventArgs.get_isMultipleEntities(),
      tabId: eventArgs.get_tabId(),
      title: eventArgs.title,
    };
  }

  /**
   * @param {object} data Plain object
   * @returns deserialized BackendTabEventArgs
   */
  _deserializeTabEvent(data) {
    const eventArgs = new BackendTabEventArgs();
    eventArgs.set_entityTypeCode(data.entityTypeCode);
    eventArgs.set_entityId(data.entityId);
    eventArgs.set_entityName(data.entityName);
    eventArgs.set_parentEntityId(data.parentEntityId);
    eventArgs.set_actionCode(data.actionCode);
    eventArgs.set_actionTypeCode(data.actionTypeCode);
    eventArgs.set_entities(data.entities);
    eventArgs.set_isMultipleEntities(data.isMultipleEntities);
    eventArgs.set_tabId(data.tabId);
    return eventArgs;
  }
}

Quantumart.QP8.BackendBrowserHistoryManager = BackendBrowserHistoryManager;
