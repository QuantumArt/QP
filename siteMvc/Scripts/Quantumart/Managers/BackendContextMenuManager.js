import { Observable } from '../Common/Observable';
import { $q } from '../Utils';

window.EVENT_TYPE_CUSTOM_ACTION_CHANGED = 'OnCustomActionChanged';
export class BackendContextMenuManager extends Observable {
  /** @type {BackendContextMenuManager} */
  static _instance;

  static getInstance() {
    if (!BackendContextMenuManager._instance) {
      BackendContextMenuManager._instance = new BackendContextMenuManager();
    }

    return BackendContextMenuManager._instance;
  }

  static destroyInstance() {
    if (BackendContextMenuManager._instance) {
      BackendContextMenuManager._instance.dispose();
      BackendContextMenuManager._instance = null;
    }
  }

  onActionExecuted(eventArgs) {
    if (eventArgs
      && eventArgs.get_entityTypeCode() === window.ENTITY_TYPE_CODE_CUSTOM_ACTION
      && (eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving())
    ) {
      this.notify(window.EVENT_TYPE_CUSTOM_ACTION_CHANGED, {});
    }
  }

  dispose() {
    super.dispose();
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendContextMenuManager = BackendContextMenuManager;
