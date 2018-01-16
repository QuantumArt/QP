/* eslint-disable no-plusplus */
import { BackendTabEventArgs } from '../Common/BackendTabEventArgs';
import { Observable } from '../Common/Observable';

window.EVENT_TYPE_HISTORY_POP_STATE = 'EVENT_TYPE_HISTORY_POP_STATE';

const HISTORY_PREVENT_NAVIGATION_STATE = 'HISTORY_PREVENT_NAVIGATION_STATE';
const HISTORY_DEFAULT_STATE = 'HISTORY_DEFAULT_STATE';
const HISTORY_TAB_EVENT_STATE = 'HISTORY_TAB_EVENT_STATE';

// TODO: HISTORY_MODAL_STATE (prevent navigation)
// TODO: check for deleted entities
// TODO: handle alert from modified document

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
  _stateId = 0;

  initialize() {
    const { state } = window.history;

    if (state === null) {
      window.history.replaceState(HISTORY_PREVENT_NAVIGATION_STATE, document.title);
      this.pushDefaultState();
    } else if (state.type === HISTORY_DEFAULT_STATE) {
      this._stateId = state.stateId;
    } else if (state.type === HISTORY_TAB_EVENT_STATE) {
      this._stateId = state.stateId;
      this.pushDefaultState();
    }
    window.addEventListener('popstate', this._onPopState.bind(this));
  }

  _onPopState({ state }) {
    console.log('PopStateEvent', state);

    if (state === HISTORY_PREVENT_NAVIGATION_STATE) {
      window.history.forward();
    } else if (this._shouldPreventNavigation) {
      if (state.stateId > this._stateId) {
        console.log('BACK');
        window.history.back();
      } else if (state.stateId < this._stateId) {
        console.log('FORWARD');
        window.history.forward();
      } else {
        console.log('SAME');
      }
    } else if (state.type === HISTORY_DEFAULT_STATE) {
      console.log('POP ', state);
      this._stateId = state.stateId;
    } else if (state.type === HISTORY_TAB_EVENT_STATE) {
      console.log('POP ', state);
      this._stateId = state.stateId;
      this._shouldPreventNavigation = true;

      const eventArgs = this._deserializeTabEvent(state);
      eventArgs.fromHistory = true;
      eventArgs.onExecutionFinished.attach(() => {
        this._shouldPreventNavigation = false;
      });

      this.notify(window.EVENT_TYPE_HISTORY_POP_STATE, eventArgs);
    }
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

    const eventState = this._serializeTabEvent(eventArgs);

    this._stateId++;
    eventState.stateId = this._stateId;

    window.history.pushState(eventState, document.title);
    console.log('PUSH', eventState);
  }

  pushDefaultState() {
    this._stateId++;
    const defaultState = {
      type: HISTORY_DEFAULT_STATE,
      stateId: this._stateId
    };

    window.history.pushState(defaultState, document.title);
    console.log('PUSH', defaultState);
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
