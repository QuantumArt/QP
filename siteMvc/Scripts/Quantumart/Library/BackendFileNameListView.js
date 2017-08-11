// #region class BackendFileNameListView
// === Класс "Краткий список файлов" ===
Quantumart.QP8.BackendFileNameListView = function (fileListContentElement, contextMenuCode, selectMode, zIndex) {
	Quantumart.QP8.BackendFileNameListView.initializeBase(this);

	this._fileListContentElement = fileListContentElement;
	this._contextMenuCode = contextMenuCode;
	this._selectMode = selectMode;
	this._zIndex = zIndex;
};

Quantumart.QP8.BackendFileNameListView.prototype = {
	_fileListContentElement: null, // контейнер списка файлов
	_contextMenuComponent: null, // контекстоное меняю
	_contextMenuCode: 0, // код контекстного меню
	_contextMenuActionCode: "",
	_currentContextMenuSelectedElement: null,
	SELECTED_CLASS: "fileItem-selected",
    _zIndex: 0,

	_selectMode: "",

	shortNameLength: 20,

	initialize: function () {
		var $fileListContentElement = jQuery(this._fileListContentElement);
		$fileListContentElement.html('<div class="fileListNameContainer"></div>');

		// привязать события
		$fileListContentElement.delegate('.fileItem input:checkbox', "click", jQuery.proxy(this._onFileCheckBoxClickedHandler, this));
		$fileListContentElement.delegate('.fileItem', "click", jQuery.proxy(this._onFileContainerClickedHandler, this));

		if (!$q.isNullOrWhiteSpace(this._contextMenuCode)) {
			var contextMenuComponent = new Quantumart.QP8.BackendContextMenu(this._contextMenuCode, String.format("{0}_ContextMenu", $fileListContentElement.attr("id")),
				{ "targetElements": this._fileListContentElement, "allowManualShowing": true, "zIndex": this._zIndex});
			contextMenuComponent.initialize();

			contextMenuComponent.addMenuItemsToMenu(false);

			var contextMenuEventType = contextMenuComponent.getContextMenuEventType();
			$fileListContentElement.delegate('.fileItem', contextMenuEventType, jQuery.proxy(this._onContextMenuHandler, this));
			contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, jQuery.proxy(this._onNodeContextMenuItemClickingHandler, this));
			contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_HIDDEN, jQuery.proxy(this._onNodeContextMenuHiddenHandler, this));

			this._contextMenuComponent = contextMenuComponent;
			contextMenuComponent = null;
		}

		$fileListContentElement = null;
	},
	// инициализация
	redraw: function (data, options) {
		var $fileListContentElement = jQuery(this._fileListContentElement);
		var $fileListNameContainer = $fileListContentElement.find('.fileListNameContainer');


		var html = new $.telerik.stringBuilder();

		var self = this;
		if (data.TotalRecords > 0) {
			// сформировать содержимое по колонкам
			var columnCounter = 0;
			jQuery.each(data.Data, function (index, item) {
				html
					.catIf('<div class="column"><ul>', columnCounter == 0)
					.cat(String.format('<li title="{0}"><div class="fileItem" data-file_name="{0}">', item.FullName))
					.catIf('<input type="checkbox" />', self._selectMode == FILE_LIST_SELECT_MODE_MULTIPLE)
					.cat(String.format('<img src="{1}{2}" /><label>{0}</label></div></li>', item.Name, THEME_IMAGE_FOLDER_URL_SMALL_FILE_TYPE_ICONS, item.SmallIconLink));

				if (columnCounter == FILE_LIST_ITEMS_PER_COLUMN - 1) {
					html.cat('</ul></div>');
					columnCounter = 0;
				}
				else
					columnCounter++;
			});
			html.catIf('</ul></div>', columnCounter > 0);
		}
		else
			html.cat($l.FileList.noRecords);

		$fileListNameContainer.html(html.string());

		$fileListNameContainer = null;
		$fileListContentElement = null;
		html = null;

		// поднимаем deselect так как на новой странице нет выбранных элементов
		this._raiseSelectEvent();
	},
	// перерисовать
	selectAll: function (value) {
		var $fileListContentElement = jQuery(this._fileListContentElement);
		$fileListContentElement.find('.fileItem input:checkbox').prop('checked', value);
		$fileListContentElement.find('.fileItem').addClass(this.SELECTED_CLASS);
		$fileListContentElement = null;

		this._raiseSelectEvent();
	},
	// выбрать все

	isAllSelected: function () {
		var $fileListContentElement = jQuery(this._fileListContentElement);
		var b = $fileListContentElement.find('.fileItem input::checkbox:not(:checked)').size() == 0 && $fileListContentElement.find('.fileItem').size() > 0;
		$fileListContentElement = null;
		return b;
	},
	// позволяет определить, все ли выбраны
	dispose: function () {
		var $fileListContentElement = jQuery(this._fileListContentElement);
		$fileListContentElement.undelegate('.fileItem input:checkbox', "click");
		$fileListContentElement.undelegate('.fileItem', "click");

		if (this._contextMenuComponent) {
			this._contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING);
			this._contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_HIDDEN);
			var contextMenuEventType = this._contextMenuComponent.getContextMenuEventType();
			$fileListContentElement.undelegate('.fileItem', contextMenuEventType);
			this._contextMenuComponent.dispose();
		}

		$fileListContentElement = null;
		this._fileListContentElement = null;
		this._contextMenuComponent = null;
	},
	// dispose

	_raiseSelectEvent: function () {
		var eventArgs = new Quantumart.QP8.BackendEventArgs();
		eventArgs.set_entities(this._getSelectedEntities());
		eventArgs.set_isMultipleEntities(true);

		this.notify(EVENT_TYPE_FILE_LIST_SELECTED, eventArgs);

		eventArgs = null;
	},
	// поднять событие Selected

	_getSelectedEntities: function () {
		var selectedEntities = [];
		var $fileListContentElement = jQuery(this._fileListContentElement);

		$fileListContentElement
			.find('.fileItem.' + this.SELECTED_CLASS)
			.each(function (index, item) {
				var name = jQuery(item).data('file_name');
				Array.add(selectedEntities, { "Id": name, "Name": name });
			});

		$fileListContentElement = null;
		return selectedEntities;
	},
	// получить список выбранных файлов

	_onFileCheckBoxClickedHandler: function (event) {
		event.stopPropagation();

		// пометить файл как выбранный
		var $chb = jQuery(event.currentTarget);
		if ($chb.is(':checked')) {
		    $chb.parent('.fileItem').addClass(this.SELECTED_CLASS);
		}
		else {
		    $chb.parent('.fileItem').removeClass(this.SELECTED_CLASS);
		}
		this._raiseSelectEvent();
		$chb = null;
	},

	_onFileContainerClickedHandler: function (event) {
		var $item = jQuery(event.currentTarget);
		var $fileListContentElement = jQuery(this._fileListContentElement);

		if (!event.ctrlKey || this._selectMode == FILE_LIST_SELECT_MODE_SINGLE) {
			// снять выбор со всех выбранных файлов
		    $fileListContentElement.find('.' + this.SELECTED_CLASS).removeClass(this.SELECTED_CLASS);
			$fileListContentElement.find('.fileItem input:checkbox:checked').prop('checked', false);
		}

		$item.toggleClass(this.SELECTED_CLASS);
		$item.find('input:checkbox').prop('checked', $item.hasClass(this.SELECTED_CLASS));
		this._raiseSelectEvent();

		$item = null;
		$fileListContentElement = null;
	},

	_onContextMenuHandler: function (e) {
		var $element = jQuery(e.currentTarget);
		if (this._contextMenuComponent) {
			this._contextMenuComponent.showMenu(e, $element.get(0));
		}
		$element = null;
		this._currentContextMenuSelectedElement = e.currentTarget;
		e.preventDefault();
	},

	_onNodeContextMenuItemClickingHandler: function (eventType, sender, args) {
		var $menuItem = jQuery(args.get_menuItem());
		if (!$q.isNullOrEmpty($menuItem)) {
			this._contextMenuActionCode = $menuItem.data("action_code");
		}
		$menuItem = null;
	},

	_onNodeContextMenuHiddenHandler: function (eventType, sender, args) {
		if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
			var $element = jQuery(this._currentContextMenuSelectedElement);
			var eventArgs = new Quantumart.QP8.BackendEventArgs();
			eventArgs.set_actionCode(this._contextMenuActionCode);
			eventArgs.set_entityId($element.data('file_name'));
			eventArgs.set_entityName($element.data('file_name'));
			this._contextMenuActionCode = null;
			this.notify(EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, eventArgs);
			$element = null;
		}
	}
};

Quantumart.QP8.BackendFileNameListView.registerClass("Quantumart.QP8.BackendFileNameListView", Quantumart.QP8.Observable, Quantumart.QP8.IBackendFileListView);
// #endregion
