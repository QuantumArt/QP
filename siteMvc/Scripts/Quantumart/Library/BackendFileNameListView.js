Quantumart.QP8.BackendFileNameListView = function (fileListContentElement, contextMenuCode, selectMode, zIndex) {
  Quantumart.QP8.BackendFileNameListView.initializeBase(this);

  this._fileListContentElement = fileListContentElement;
  this._contextMenuCode = contextMenuCode;
  this._selectMode = selectMode;
  this._zIndex = zIndex;
};

Quantumart.QP8.BackendFileNameListView.prototype = {
  _fileListContentElement: null,
  _contextMenuComponent: null,
  _contextMenuCode: 0,
  _contextMenuActionCode: '',
  _currentContextMenuSelectedElement: null,
  SELECTED_CLASS: 'fileItem-selected',
  _zIndex: 0,

  _selectMode: '',

  shortNameLength: 20,

  initialize() {
    const $fileListContentElement = $(this._fileListContentElement);
    $fileListContentElement.html('<div class="fileListNameContainer"></div>');

    $fileListContentElement.delegate(
      '.fileItem input:checkbox', 'click', $.proxy(this._onFileCheckBoxClickedHandler, this)
    );
    $fileListContentElement.delegate('.fileItem', 'click', $.proxy(this._onFileContainerClickedHandler, this));

    if (!$q.isNullOrWhiteSpace(this._contextMenuCode)) {
      const contextMenuComponent = new Quantumart.QP8.BackendContextMenu(
        this._contextMenuCode, String.format('{0}_ContextMenu', $fileListContentElement.attr('id')),
        { targetElements: this._fileListContentElement, allowManualShowing: true, zIndex: this._zIndex }
      );
      contextMenuComponent.initialize();

      contextMenuComponent.addMenuItemsToMenu(false);

      const contextMenuEventType = $.fn.jeegoocontext.getContextMenuEventType();
      $fileListContentElement.delegate('.fileItem', contextMenuEventType, $.proxy(this._onContextMenuHandler, this));
      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, $.proxy(this._onNodeContextMenuItemClickingHandler, this)
      );
      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, $.proxy(this._onNodeContextMenuHiddenHandler, this)
      );

      this._contextMenuComponent = contextMenuComponent;
    }
  },

  redraw(data) {
    const $fileListContentElement = $(this._fileListContentElement);
    const $fileListNameContainer = $fileListContentElement.find('.fileListNameContainer');
    const html = new $.telerik.stringBuilder();
    const that = this;
    if (data.TotalRecords > 0) {
      let columnCounter = 0;
      $.each(data.Data, (index, item) => {
        html
          .catIf('<div class="column"><ul>', columnCounter === 0)
          .cat(String.format('<li title="{0}"><div class="fileItem" data-file_name="{0}">', item.FullName))
          .catIf('<input type="checkbox" />', that._selectMode === window.FILE_LIST_SELECT_MODE_MULTIPLE)
          .cat(String.format('<img src="{1}{2}" /><label>{0}</label></div></li>',
            item.Name, window.THEME_IMAGE_FOLDER_URL_SMALL_FILE_TYPE_ICONS, item.SmallIconLink));

        if (columnCounter === window.FILE_LIST_ITEMS_PER_COLUMN - 1) {
          html.cat('</ul></div>');
          columnCounter = 0;
        } else {
          columnCounter += 1;
        }
      });
      html.catIf('</ul></div>', columnCounter > 0);
    } else {
      html.cat($l.FileList.noRecords);
    }

    $fileListNameContainer.html(html.string());
    this._raiseSelectEvent();
  },

  selectAll(value) {
    const $fileListContentElement = $(this._fileListContentElement);
    $fileListContentElement.find('.fileItem input:checkbox').prop('checked', value);
    $fileListContentElement.find('.fileItem').addClass(this.SELECTED_CLASS);

    this._raiseSelectEvent();
  },

  isAllSelected() {
    const $fileListContentElement = $(this._fileListContentElement);
    return $fileListContentElement.find('.fileItem input::checkbox:not(:checked)').size() === 0
    && $fileListContentElement.find('.fileItem').size() > 0;
  },

  dispose() {
    const $fileListContentElement = $(this._fileListContentElement);
    $fileListContentElement.undelegate('.fileItem input:checkbox', 'click');
    $fileListContentElement.undelegate('.fileItem', 'click');

    if (this._contextMenuComponent) {
      this._contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING);
      this._contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN);
      const contextMenuEventType = $.fn.jeegoocontext.getContextMenuEventType();
      $fileListContentElement.undelegate('.fileItem', contextMenuEventType);
      this._contextMenuComponent.dispose();
    }

    this._fileListContentElement = null;
    this._contextMenuComponent = null;
  },

  _raiseSelectEvent() {
    const eventArgs = new Quantumart.QP8.BackendEventArgs();
    eventArgs.set_entities(this._getSelectedEntities());
    eventArgs.set_isMultipleEntities(true);
    this.notify(window.EVENT_TYPE_FILE_LIST_SELECTED, eventArgs);
  },

  _getSelectedEntities() {
    const selectedEntities = [];
    let $fileListContentElement = $(this._fileListContentElement);

    $fileListContentElement
      .find(`.fileItem.${this.SELECTED_CLASS}`)
      .each((index, item) => {
        const fn = $(item).data('file_name');
        Array.add(selectedEntities, { Id: fn, Name: fn });
      });

    $fileListContentElement = null;
    return selectedEntities;
  },

  _onFileCheckBoxClickedHandler(evt) {
    evt.stopPropagation();
    let $chb = $(evt.currentTarget);
    if ($chb.is(':checked')) {
      $chb.parent('.fileItem').addClass(this.SELECTED_CLASS);
    } else {
      $chb.parent('.fileItem').removeClass(this.SELECTED_CLASS);
    }
    this._raiseSelectEvent();
    $chb = null;
  },

  _onFileContainerClickedHandler(evt) {
    const $item = $(evt.currentTarget);
    const $fileListContentElement = $(this._fileListContentElement);

    if (!evt.ctrlKey || this._selectMode === window.FILE_LIST_SELECT_MODE_SINGLE) {
      $fileListContentElement.find(`.${this.SELECTED_CLASS}`).removeClass(this.SELECTED_CLASS);
      $fileListContentElement.find('.fileItem input:checkbox:checked').prop('checked', false);
    }

    $item.toggleClass(this.SELECTED_CLASS);
    $item.find('input:checkbox').prop('checked', $item.hasClass(this.SELECTED_CLASS));
    this._raiseSelectEvent();
  },

  _onContextMenuHandler(e) {
    const $element = $(e.currentTarget);
    if (this._contextMenuComponent) {
      this._contextMenuComponent.showMenu(e, $element.get(0));
    }

    this._currentContextMenuSelectedElement = e.currentTarget;
    e.preventDefault();
  },

  _onNodeContextMenuItemClickingHandler(eventType, sender, args) {
    const $menuItem = $(args.get_menuItem());
    if (!$q.isNullOrEmpty($menuItem)) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onNodeContextMenuHiddenHandler() {
    if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
      const $element = $(this._currentContextMenuSelectedElement);
      const eventArgs = new Quantumart.QP8.BackendEventArgs();
      eventArgs.set_actionCode(this._contextMenuActionCode);
      eventArgs.set_entityId($element.data('file_name'));
      eventArgs.set_entityName($element.data('file_name'));
      this.notify(window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, eventArgs);
    }
  }
};

Quantumart.QP8.BackendFileNameListView.registerClass(
  'Quantumart.QP8.BackendFileNameListView', Quantumart.QP8.Observable
);
