Quantumart.QP8.IObservable = function () {
  // empty constructor
};
Quantumart.QP8.IObservable.prototype = {
  attachObserver: $c.notImplemented,
  detachObserver: $c.notImplemented,
  notify: $c.notImplemented
};

Quantumart.QP8.IObservable.registerInterface('Quantumart.QP8.IObservable');

Quantumart.QP8.Observable = function () {
  this._observerInfos = [];
};

Quantumart.QP8.Observable.prototype = {
  _observerInfos: null,

  _getObserverInfo(eventType, observer) {
    let observerInfo = null;

    const observerInfos = $.grep(this._observerInfos[eventType], info => {
      if (info.observer) {
        return info.observer === observer;
      }
      return false;
    });

    if (observerInfos && observerInfos.length > 0) {
      observerInfo = observerInfos[0];
    }

    return observerInfo;
  },

  _checkObserver(observer) {
    let result = false;
    if ($q.isObject(observer) || $q.isFunction(observer)) {
      result = true;
    } else {
      throw new Error($l.Common.observerIsNotFunctionOrObject);
    }

    if (result) {
      if (!$q.isFunction(observer)) {
        let isObserver = true;

        try {
          isObserver = Object.getType(observer).implementsInterface(Quantumart.QP8.IObserver);
        } catch (e) {
          $q.trace('Exception was catched', e);
        }

        if (isObserver) {
          result = true;
        } else {
          throw new Error($l.Common.observerIsNotImplementedInterface);
        }
      }
    }

    return result;
  },

  attachObserver(eventType, observer, times) {
    if (!this._checkObserver(observer)) {
      return;
    }

    const newTimes = $q.isNull(times) ? -1 : times;

    if (!this._observerInfos[eventType]) {
      this._observerInfos[eventType] = [];
    }

    const observerInfo = this._getObserverInfo(eventType, observer);

    if ($q.isNull(observerInfo)) {
      Array.add(this._observerInfos[eventType], { observer, newTimes });
    } else {
      observerInfo.times = newTimes;
    }
  },

  detachObserver(eventType, observer) {
    if (!$q.isNull(this._observerInfos)
    && this._observerInfos[eventType]) {
      if ($q.isNull(observer)) {
        $q.removeProperty(this._observerInfos, eventType);
      } else {
        const observerInfo = this._getObserverInfo(eventType, observer);
        if (!$q.isNull(observerInfo)) {
          Array.remove(this._observerInfos[eventType], observerInfo);
        }
      }
    }
  },


  oneTimeObserver(eventType, observer) {
    this.attachObserver(eventType, observer, 1);
  },

  notify(eventType, eventArgs) {
    if ($q.isNullOrWhiteSpace(eventType)) {
      throw new Error($l.Common.eventTypeNotSpecified);
    }

    if (!$q.isObject(eventArgs)) {
      throw new Error(String.format($l.Common.eventArgsNotSpecified, eventType));
    }

    if ($q.isNull(this._observerInfos)) {
      return;
    }

    const observerInfos = this._observerInfos[eventType];
    let observerInfoCount = 0;

    if (!$q.isNull(observerInfos)) {
      observerInfoCount = observerInfos.length;
    }

    if (observerInfoCount > 0) {
      for (let observerInfoIndex = observerInfoCount - 1; observerInfoIndex >= 0; observerInfoIndex--) {
        const observerInfo = observerInfos[observerInfoIndex];

        if (observerInfo) {
          const observer = observerInfo.observer;

          if (observerInfo.times === -1) {
            this._updateObserver(eventType, eventArgs, observer);
          } else if (observerInfo.times > 0) {
            observerInfo.times -= 1;
            if (observerInfo.times === 0) {
              this.detachObserver(eventType, observer);
            }

            this._updateObserver(eventType, eventArgs, observer);
          }
        }
      }
    }
  },

  _updateObserver(eventType, eventArgs, observer) {
    if ($q.isObject(observer)) {
      let isObserver = false;

      try {
        isObserver = Object.getType(observer).implementsInterface(Quantumart.QP8.IObserver);
      } catch (e) {
        isObserver = true;
      }

      if (isObserver) {
        observer.update(eventType, this, eventArgs);
      }
    } else if ($q.isFunction(observer)) {
      observer(eventType, this, eventArgs);
    }
  },

  dispose() {
    $q.clearArray(this._observerInfos);
  }
};

Quantumart.QP8.Observable.registerClass('Quantumart.QP8.Observable', null, Quantumart.QP8.IObservable, Sys.IDisposable);
