/* eslint-disable no-plusplus */
import { BackendTabEventArgs } from '../Common/BackendTabEventArgs';
import { Observable } from '../Common/Observable';

window.EVENT_TYPE_HISTORY_POP_STATE = 'EVENT_TYPE_HISTORY_POP_STATE';

const HISTORY_INITIAL_STATE = 'HISTORY_INITIAL_STATE';
const HISTORY_DEFAULT_STATE = 'HISTORY_DEFAULT_STATE';
const HISTORY_TAB_EVENT_STATE = 'HISTORY_TAB_EVENT_STATE';

// TODO: what if user opens modal window or tab during history event execution ?
// TODO: what if error was thrown (or warning is alerted) ?

export class BackendBrowserHistoryManager extends Observable {
  /** @type {BackendBrowserHistoryManager} */
  static _instance;

  static getInstance() {
    if (!BackendBrowserHistoryManager._instance) {
      BackendBrowserHistoryManager._instance = new BackendBrowserHistoryManager();
    }
    return BackendBrowserHistoryManager._instance;
  }

  _shouldPreventNavigation = false;
  _modalWindowIsShown = false;
  _currentStateId = 0;

  constructor() {
    super();

    this._handlePopState = this._handlePopState.bind(this);
    this._handleExecutionFinished = this._handleExecutionFinished.bind(this);
    this.handleModalWindowOpen = this.handleModalWindowOpen.bind(this);
    this.handleModalWindowClose = this.handleModalWindowClose.bind(this);
  }

  initialize() {
    const { state } = window.history;

    if (state === null) {
      window.history.replaceState({
        type: HISTORY_INITIAL_STATE,
        stateId: this._currentStateId
      }, document.title);

      this.pushDefaultState();
    } else if (state.type === HISTORY_DEFAULT_STATE) {
      this._currentStateId = state.stateId;
    } else if (state.type === HISTORY_TAB_EVENT_STATE) {
      this._currentStateId = state.stateId;
      this.pushDefaultState();
    }

    window.addEventListener('popstate', this._handlePopState);
  }

  _handlePopState({ state }) {
    console.log('POP', state);

    if (state.type === HISTORY_INITIAL_STATE) {
      window.history.forward();
    } else if (this._shouldPreventNavigation || this._modalWindowIsShown) {
      this._restorePreviousState(state);
    } else if (state.type === HISTORY_DEFAULT_STATE) {
      this._currentStateId = state.stateId;
    } else if (state.type === HISTORY_TAB_EVENT_STATE) {
      console.log('DENY NAVIGATION');
      console.log('START EXECUTION', state);
      this._shouldPreventNavigation = true;

      const eventArgs = this._deserializeTabEvent(state);
      eventArgs.fromHistory = true;
      eventArgs.onExecutionFinished.attach((_sender, isNavigationPerformed) => {
        this._handleExecutionFinished(state, isNavigationPerformed);
      });

      this.notify(window.EVENT_TYPE_HISTORY_POP_STATE, eventArgs);
    }
  }

  /**
   * @param {object} state
   * @param {boolean} isNavigationPerformed
   */
  _handleExecutionFinished(state, isNavigationPerformed) {
    console.log('FINISH EXECUTION', state);
    if (window.history.state.stateId === state.stateId) {
      if (isNavigationPerformed) {
        this._currentStateId = state.stateId;
      } else {
        this._restorePreviousState(state);
      }
    }
    setTimeout(() => {
      console.log('ALLOW NAVIGATION');
      this._shouldPreventNavigation = false;
    }, 0);
  }

  /**
   * @param {object} state
   */
  _restorePreviousState(state) {
    if (state.stateId > this._currentStateId) {
      console.log('BACK');
      window.history.back();
    } else if (state.stateId < this._currentStateId) {
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
  pushTabEvent(eventArgs) {
    if (eventArgs.fromHistory) {
      return;
    }
    const { state } = window.history;
    if (state.type === HISTORY_TAB_EVENT_STATE) {
      const historyEventArgs = this._deserializeTabEvent(state);

      if (eventArgs.structurallyEquals(historyEventArgs)) {
        return;
      }
    }

    this._currentStateId++;

    const eventState = this._serializeTabEvent(eventArgs);
    eventState.stateId = this._currentStateId;

    window.history.pushState(eventState, document.title);
    console.log('PUSH', eventState);
  }

  pushDefaultState() {
    this._currentStateId++;

    window.history.pushState({
      type: HISTORY_DEFAULT_STATE,
      stateId: this._currentStateId
    }, document.title);
    console.log('PUSH', window.history.state);
  }

  /**
   * @param {BackendTabEventArgs} eventArgs
   * @returns serialized BackendTabEventArgs
   */
  _serializeTabEvent(eventArgs) {
    return {
      type: HISTORY_TAB_EVENT_STATE,
      entityTypeCode: eventArgs.get_entityTypeCode(),
      entityId: eventArgs.get_entityId(),
      entityName: eventArgs.get_entityName(),
      parentEntityId: eventArgs.get_parentEntityId(),
      actionCode: eventArgs.get_actionCode(),
      actionTypeCode: eventArgs.get_actionTypeCode(),
      entities: eventArgs.get_entities(),
      isMultipleEntities: eventArgs.get_isMultipleEntities(),
      tabId: eventArgs.get_tabId()
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
