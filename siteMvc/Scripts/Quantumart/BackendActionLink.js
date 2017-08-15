window.EVENT_TYPE_ACTION_LINK_CLICK = 'OnActionLinkClick';
window.EVENT_TYPE_ACTION_LINK_SELF_CLICK = 'OnActionLinkSelfClick';

Quantumart.QP8.BackendActionLink = function (actionLinkElementId, options) {
  Quantumart.QP8.BackendActionLink.initializeBase(this);

  this._actionLinkElementId = actionLinkElementId;
  if ($q.isObject(options)) {
    if (options.entityId) {
      this._entityId = options.entityId;
    }

    if (options.entityName) {
      this._entityName = options.entityName;
    }

    if (options.parentEntityId) {
      this._parentEntityId = options.parentEntityId;
    }

    if (options.actionTypeCode) {
      this._actionTypeCode = options.actionTypeCode;
    }

    if (options.actionCode) {
      this._actionCode = options.actionCode;
    }

    if (options.context) {
      this._context = options.context;
    }

    if (!$q.isNull(options.actionTargetType)) {
      this._actionTargetType = options.actionTargetType;
    } else {
      this._actionTargetType = Quantumart.QP8.Enums.ActionTargetType.NewTab;
    }
  }

  this._onActionExecutingHandler = jQuery.proxy(this.onActionExecuting, this);
};

Quantumart.QP8.BackendActionLink.prototype = {
  _actionLinkElementId: '',
  _actionLinkElement: null,
  _iconWrapperElement: null,
  _captionElement: null,
  _entityId: 0,
  _entityName: '',
  _parentEntityId: 0,
  _actionTypeCode: '',
  _actionCode: '',
  _actionTargetType: null,
  _context: null,

  ACTION_LINK_DISABLED_CLASS_NAME: 'disabled',
  ACTION_LINK_BUSY_CLASS_NAME: 'busy',

  get_entityId () {
    return this._entityId;
  },

  set_entityId (value) {
    this._entityId = value;
  },

  get_entityName () {
    return this._entityName;
  },

  set_entityName (value) {
    this._entityName = value;
  },

  get_parentEntityId () {
    return this._parentEntityId;
  },

  set_parentEntityId (value) {
    this._parentEntityId = value;
  },

  get_actionTypeCode () {
    return this._actionTypeCode;
  },

  set_actionTypeCode (value) {
    this._actionTypeCode = value;
  },

  get_actionCode () {
    return this._actionCode;
  },

  set_actionCode (value) {
    this._actionCode = value;
  },

  get_actionTargetType () {
    return this._actionTargetType;
  },

  set_actionTargetType (value) {
    this._actionTargetType = value;
  },


  _onActionExecutingHandler: null,

  initialize () {
    const $actionLink = jQuery(`#${this._actionLinkElementId}`);
    const $iconWrapper = $actionLink.find('SPAN.icon:first');
    const $caption = $actionLink.find('SPAN.text:first');

    this._actionLinkElement = $actionLink.get(0);
    this._iconWrapperElement = $iconWrapper.get(0);
    this._captionElement = $caption.get(0);

    this._attachActionLinkEventHandlers();
  },

  _attachActionLinkEventHandlers () {
    let $link = jQuery(this._actionLinkElement);
    $link.bind('click', this._onActionExecutingHandler);
    $link.bind('mouseup', this._onActionExecutingHandler);

    $link = null;
  },

  _detachActionLinkEventHandlers () {
    let $link = jQuery(this._actionLinkElement);
    $link.unbind('click', this._onActionExecutingHandler);
    $link.unbind('mouseup', this._onActionExecutingHandler);

    $link = null;
  },

  markActionLinkAsBusy () {
    let $link = jQuery(this._actionLinkElement);
    $link.find('A:first').addClass(this.ACTION_LINK_BUSY_CLASS_NAME);

    $link = null;
  },

  unmarkActionLinkAsBusy () {
    let $link = jQuery(this._actionLinkElement);
    $link.find('A:first').removeClass(this.ACTION_LINK_BUSY_CLASS_NAME);

    $link = null;
  },

  isActionLinkBusy () {
    let $link = jQuery(this._actionLinkElement);
    const isBusy = $link.find('A:first').hasClass(this.ACTION_LINK_BUSY_CLASS_NAME);

    $link = null;

    return isBusy;
  },

  enableActionLink () {
    let $link = jQuery(this._actionLinkElement);
    $link.find('A:first').removeClass(this.ACTION_LINK_DISABLED_CLASS_NAME);

    $link = null;
  },

  disableActionLink () {
    let $link = jQuery(this._actionLinkElement);
    $link.find('A:first').addClass(this.ACTION_LINK_DISABLED_CLASS_NAME);

    $link = null;
  },

  isActionLinkDisabled () {
    let $link = jQuery(this._actionLinkElement);
    const isDisabled = $link.find('A:first').hasClass(this.ACTION_LINK_DISABLED_CLASS_NAME);

    $link = null;

    return isDisabled;
  },

  onActionExecuting (e) {
    e.preventDefault();
    const isLeftClick = e.type == 'click' && (e.which == 1 || e.which == 0);
    const isMiddleClick = e.type == 'mouseup' && e.which == 2;
    if (!this.isActionLinkDisabled() && !this.isActionLinkBusy() && (isLeftClick || isMiddleClick)) {
      const actionTargetType = this._actionTargetType;
      if (!$q.isNull(actionTargetType)) {
        const actionCode = this._actionCode;
        const action = $a.getBackendActionByCode(actionCode);
        if (action) {
          let params = new Quantumart.QP8.BackendActionParameters({
            entityId: this._entityId,
            entityName: this._entityName,
            parentEntityId: this._parentEntityId,
            context: this._context,
            entityTypeCode: action.EntityType.Code,
            forceOpenWindow: actionTargetType == Quantumart.QP8.Enums.ActionTargetType.NewWindow
          });

          params.correct(action);
          let eventArgs = $a.getEventArgsFromActionWithParams(action, params);

          if (action.ActionType.Code === window.ACTION_TYPE_CODE_ADD_NEW) {
            eventArgs.set_context(Object.assign({ ctrlKey: e.ctrlKey || isMiddleClick }, eventArgs.get_context()));
          }

          if (action.IsInterface && actionTargetType == Quantumart.QP8.Enums.ActionTargetType.Self) {
            this.notify(window.EVENT_TYPE_ACTION_LINK_SELF_CLICK, eventArgs);
          } else {
            this.notify(window.EVENT_TYPE_ACTION_LINK_CLICK, eventArgs);
          }

          eventArgs = null;
          params = null;
        }
      } else {
        $q.alertFail($l.ActionLink.actionTargetTypeNotSpecified);
      }
    }
  },

  dispose () {
    Quantumart.QP8.BackendActionLink.callBaseMethod(this, 'dispose');

    this._detachActionLinkEventHandlers();

    if (this._iconWrapperElement) {
      this._iconWrapperElement = null;
    }

    if (this._captionElement) {
      this._captionElement = null;
    }

    this._actionLinkElement = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendActionLink.registerClass('Quantumart.QP8.BackendActionLink', Quantumart.QP8.Observable);
