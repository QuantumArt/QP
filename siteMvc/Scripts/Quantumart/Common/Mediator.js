Quantumart.QP8.IMediator = function () {};
Quantumart.QP8.IMediator.prototype = { introduce() {} };
Quantumart.QP8.IMediator.registerInterface('Quantumart.QP8.IMediator');

Quantumart.QP8.Mediator = function () {
  this._firstComponent = null;
  this._secondComponent = null;
};

Quantumart.QP8.Mediator.prototype = {
  _firstComponent: null,
  _secondComponent: null,

  introduce(firstComponent, secondComponent) {
    if (!$q.isObject(firstComponent)) {
      throw new Error($l.Common.firstComponentInMediatorNotSpecified);
    }

    if (!$q.isObject(secondComponent)) {
      throw new Error($l.Common.secondComponentInMediatorNotSpecified);
    }

    this._firstComponent = firstComponent;
    this._secondComponent = secondComponent;
  },

  update() {},
  dispose() {}
};

Quantumart.QP8.Mediator.registerClass(
  'Quantumart.QP8.Mediator',
  null,
  Quantumart.QP8.IMediator,
  Quantumart.QP8.IObserver,
  Sys.IDisposable
);
