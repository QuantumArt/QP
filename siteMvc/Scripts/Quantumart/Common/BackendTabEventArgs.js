/* eslint camelcase: 0 */
import { BackendEventArgs } from './BackendEventArgs';
import { event } from '../Utils/Event';
import { setEquals } from '../Utils/Set';

export class BackendTabEventArgs extends BackendEventArgs {
  _tabId = '';
  title = '';
  isExpandRequested = false;
  fromHistory = false;
  onExecutionFinished = event(this, Boolean);

  // eslint-disable-next-line camelcase
  get_tabId() {
    return this._tabId;
  }

  // eslint-disable-next-line camelcase
  set_tabId(value) {
    this._tabId = value;
  }

  finishExecution(isNavigationPerformed = true) {
    this.onExecutionFinished(isNavigationPerformed);
  }

  hasSameDocument(eventArgs) {
    let equals = (eventArgs instanceof BackendTabEventArgs)
      && this.get_entityTypeCode() === eventArgs.get_entityTypeCode()
      && this.get_entityId() === eventArgs.get_entityId()
      && this.get_parentEntityId() === eventArgs.get_parentEntityId()
      && this.get_actionCode() === eventArgs.get_actionCode()
      && this.get_actionTypeCode() === eventArgs.get_actionTypeCode()
      && this.get_isMultipleEntities() === eventArgs.get_isMultipleEntities();

    if (equals
      && Array.isArray(this.get_entities())
      && Array.isArray(eventArgs.get_entities())
    ) {
      // contains same set of entities
      equals = setEquals(this.get_entities(), eventArgs.get_entities());
    }

    return equals;
  }

  structurallyEquals(eventArgs) {
    return (eventArgs instanceof BackendTabEventArgs)
      && this.get_tabId() === eventArgs.get_tabId()
      && this.hasSameDocument(eventArgs);
  }
}

Quantumart.QP8.BackendTabEventArgs = BackendTabEventArgs;
