import { $q } from '../Utils';

export class ActionLogFilterBase {
  constructor(filterContainer) {
    this.$container = $(filterContainer);
  }

  $container = null;

  /** @abstract */
  initialize() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @virtual */
  onOpen() {
    // default implementation
  }

  /**
   * @abstract
   * @returns {any}
   */
  getValue() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @virtual
   * @returns {string}
   */
  getFilterDetails() {
    return '?';
  }

  dispose() {
    $q.dispose.call(this, [
      '$container'
    ]);
  }
}


Quantumart.QP8.ActionLogFilterBase = ActionLogFilterBase;
