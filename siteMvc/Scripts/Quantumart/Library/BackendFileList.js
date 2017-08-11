// ****************************************************************************
// *** Компонент "Краткий список файлов"									***
// ****************************************************************************

// #region constants of file name list
var FILE_LIST_MODE_NAME_LIST = "FILE_LIST_MODE_NAME_LIST";
var FILE_LIST_MODE_PREVIEW_LIST = "FILE_LIST_MODE_PREVIEW_LIST";
var FILE_LIST_SELECT_MODE_MULTIPLE = "FILE_LIST_SELECT_MODE_MULTIPLE";
var FILE_LIST_SELECT_MODE_SINGLE = "FILE_LIST_SELECT_MODE_SINGLE";
var FILE_LIST_NAME_PAGE_SIZE = 60;
var FILE_LIST_PREVIEW_PAGE_SIZE = 24;
var FILE_LIST_ITEMS_PER_COLUMN = 20;
// #endregion

// #region event types of file name list
// === Типы событий списка файлов ===
// var EVENT_TYPE_FILE_LIST_DATA_BINDING = "OnFileListDataBinding"
var EVENT_TYPE_FILE_LIST_DATA_BOUND = "OnFileListDataBound";
var EVENT_TYPE_FILE_LIST_ACTION_EXECUTING = "OnFileListActionExecuting";
var EVENT_TYPE_FILE_LIST_SELECTED = "OnFileNameSelected";
// #endregion

// #region class BackendFileList
// === Класс "Cписок файлов" ===
Quantumart.QP8.BackendFileList = function (listElementId, fileEntityTypeCode, actionCode, contextMenuCode, viewMode, options) {
	Quantumart.QP8.BackendFileList.initializeBase(this);

	this._listElementId = listElementId;
	this._fileEntityTypeCode = fileEntityTypeCode;
	this._contextMenuCode = contextMenuCode;
	this._viewMode = viewMode;
	this._actionCode = actionCode;

	if (!$q.isNull(options)) {
		if (!$q.isNull(options.selectMode))
		    this._selectMode = options.selectMode;
		if (options.zIndex)
		    this._zIndex = options.zIndex;
	}

	this._currentDataQueryOptions = {
		pageNumber: 0,
		pageSize: 0,
		folderId: 0,
		fileTypeId: "",
		fileNameFilter: ""
	};
	if (this._viewMode == FILE_LIST_MODE_NAME_LIST)
		this._currentDataQueryOptions.pageSize = FILE_LIST_NAME_PAGE_SIZE;
	else if (this._viewMode == FILE_LIST_MODE_PREVIEW_LIST)
		this._currentDataQueryOptions.pageSize = FILE_LIST_PREVIEW_PAGE_SIZE;
};

Quantumart.QP8.BackendFileList.prototype = {
	_fileEntityTypeCode: 0, // EntityTypeCode файлов
	_contextMenuCode: 0, // код контекстного меня
	_viewMode: FILE_LIST_MODE_NAME_LIST, // режим отображения списка файлов
	_selectMode: FILE_LIST_SELECT_MODE_MULTIPLE, // режим выбора элементов списка
	_actionCode: "", // action Code

	_currentDataQueryOptions: null, // текущие параметры запроса списка файлов

	_listElementId: "", // id корневого элемента списка
	_listElement: null, // корневой dom-элемент компонента
	_allSelectorElement: null, // dom-элемент чекбокс выбора всех файлов
	_fileListContentElement: null, // dom-элемент контейнера списка файлов

	_contextMenuComponent: null, // компонент "Контекстное меню"
	_pagerComponent: null, // компонент пайджер
	_listViewComponent: null, // компонент показа списка файлов
    _zIndex: 0,

	_onAllSelectorClicked: function () {
		this._listViewComponent.selectAll(jQuery(this._allSelectorElement).is(":checked"));
	},
 // обработчик клика на чекбоксе выбора всех файлов
	_onPageNumberChanged: function (eventType, sender, args) {
		this.rebind({ pageNumber: args.get_PageNumber() });
	},
 // обработчик смены номера страницы
	_onListViewSelected: function (eventType, sender, args) {
		// выставить или снять чекбокс "Выбрать все" в зависимости о того, все ли выбраны
		jQuery(this._allSelectorElement).prop('checked', this._listViewComponent.isAllSelected());

		this._raiseMultipleEventArgsEvent(EVENT_TYPE_FILE_LIST_SELECTED, args);

		action = null;
		eventArgs = null;
	},
 // выбор элемента на списке
	_onContextMenuItemClicked: function (eventType, sender, args) {
		args.set_parentEntityId(this._currentDataQueryOptions.folderId);
		this.notify(eventType, args);
	},
 // клик на контекстном меню

	_loadData: function () {
		var url = "";
		// определить url в зависимости от fileEntityTypeCode
		if (this._fileEntityTypeCode == ENTITY_TYPE_CODE_SITE_FILE)
			url = CONTROLLER_URL_SITE + '_FileList';
		else if (this._fileEntityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE)
			url = CONTROLLER_URL_CONTENT + '_FileList';
		else
			throw new Error('fileEntityTypeCode is unknown.');

		var result;
		$q.getJsonFromUrl(
			"GET",
			url,
			{
				"folderId": this._currentDataQueryOptions.folderId,
				"pageSize": this._currentDataQueryOptions.pageSize,
				"pageNumber": this._currentDataQueryOptions.pageNumber,
				"fileTypeId": this._currentDataQueryOptions.fileTypeId,
				"fileNameFilter": this._currentDataQueryOptions.fileNameFilter,
				"fileShortNameLength": this._listViewComponent.shortNameLength
			},
			false,
			false,
			function (data, textStatus, jqXHR) {
				if (data.success)
					result = data.data;
				else {
					alert(data.message);
				}
			},
			function (jqXHR, textStatus, errorThrown) {
				result = null;
				$q.processGenericAjaxError(jqXHR);
			}
		);

		return result;
	},
 // получение данных с сервера
	_raiseMultipleEventArgsEvent: function (eventType, args) {
		var action = $a.getBackendActionByCode(this._actionCode);
		if (action != null) {
			var eventArgs = $a.getEventArgsFromAction(action);
			eventArgs.set_isMultipleEntities(true);
			if (args)
				eventArgs.set_entities(args.get_entities());
			eventArgs.set_entityTypeCode(this._fileEntityTypeCode);
			eventArgs.set_parentEntityId(this._currentDataQueryOptions.folderId);

			this.notify(eventType, eventArgs);
		}

		action = null;
		eventArgs = null;
	},
 // поднимает событие

	initialize: function () {
		var $listElement = jQuery(this._listElementId);
		this._listElement = $listElement.get(0);

		var html = new $.telerik.stringBuilder();

		html.cat('<div class="fileList">')
						.cat('<div class="fileListArea">')
							.cat('<div class="fileListHeader">')
								.catIf('<input type="checkbox" class="fileListAllSelector" />', this._selectMode == FILE_LIST_SELECT_MODE_MULTIPLE)
								.catIf('<br />', this._selectMode == FILE_LIST_SELECT_MODE_SINGLE)
							.cat('</div>')
							.cat(String.format('<div class="fileListContent" id="{0}_fileListContent">{1}</div>', $listElement.attr('id'), $l.FileList.noRecords))
						.cat('</div>')
						.cat('<div class="fileListPager"></div>')
					 .cat('</div>');
		$listElement.html(html.string());

		// пейджер
		var pagerComponent = new Quantumart.QP8.BackendPager($listElement.find('.fileListPager').get(0));
		pagerComponent.initialize();
		pagerComponent.attachObserver(EVENT_TYPE_PAGE_NUMBER_CHANGED, jQuery.proxy(this._onPageNumberChanged, this));
		this._pagerComponent = pagerComponent;

		// checkbox выбора всех файлов
		var $allSelectorElement = $listElement.find('.fileListAllSelector');
		$allSelectorElement.click(jQuery.proxy(this._onAllSelectorClicked, this));
		this._allSelectorElement = $allSelectorElement.get(0);

		// область view списка файлов
		this._fileListContentElement = $listElement.find('.fileListContent').get(0);
		var listViewComponent = null;
		if (this._viewMode == FILE_LIST_MODE_NAME_LIST)
			listViewComponent = new Quantumart.QP8.BackendFileNameListView(this._fileListContentElement, this._contextMenuCode, this._selectMode, this._zIndex);
		else if (this._viewMode == FILE_LIST_MODE_PREVIEW_LIST)
		    listViewComponent = new Quantumart.QP8.BackendFilePreviewListView(this._fileListContentElement, this._contextMenuCode, this._selectMode, this._zIndex);
		else
			throw new Error('View Mode is unknown.');

		// -----
		listViewComponent.initialize();
		listViewComponent.attachObserver(EVENT_TYPE_FILE_LIST_SELECTED, jQuery.proxy(this._onListViewSelected, this));
		listViewComponent.attachObserver(EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, jQuery.proxy(this._onContextMenuItemClicked, this));
		this._listViewComponent = listViewComponent;

		pagerComponent = null;
		listViewComponent = null;
		$allSelectorElement = null;
		$listElement = null;
	},
 // нициализация
	rebind: function (options) {
		// this.notify(EVENT_TYPE_FILE_LIST_DATA_BINDING, {});

		// установить новые значения параметров поиска
		if ($q.isObject(options)) {
			if (!$q.isNull(options.pageSize))
				this._currentDataQueryOptions.pageSize = $q.toInt(options.pageSize);
			if (!$q.isNull(options.pageNumber))
				this._currentDataQueryOptions.pageNumber = $q.toInt(options.pageNumber);
			if (!$q.isNull(options.folderId))
				this._currentDataQueryOptions.folderId = $q.toInt(options.folderId);
			if (!$q.isNull(options.fileTypeId))
				this._currentDataQueryOptions.fileTypeId = options.fileTypeId;
			if (!$q.isNull(options.fileNameFilter))
				this._currentDataQueryOptions.fileNameFilter = options.fileNameFilter;
		}

		// загрузить данные с сервера
		var data = this._loadData();
		if (data) {
			// Перерисовать список файлов
			this._listViewComponent.redraw(data,
			{
				folderId: this._currentDataQueryOptions.folderId,
				fileEntityTypeCode: this._fileEntityTypeCode
			});

			// перерисовать pager
			this._pagerComponent.set(
			{
				totalCount: data.TotalRecords,
				pageSize: this._currentDataQueryOptions.pageSize,
				currentPageNumber: this._currentDataQueryOptions.pageNumber
			});
			// получить реальный текущий номер страницы
			this._currentDataQueryOptions.pageNumber = this._pagerComponent.get_pageNumber();
			// перерисовать пейджер
			this._pagerComponent.redraw();
			// ------------------
		}

		this._raiseMultipleEventArgsEvent(EVENT_TYPE_FILE_LIST_DATA_BOUND);
	},
 // получение данных с сервера и отрисовка
	dispose: function () {
		if (this._pagerComponent) {
			this._pagerComponent.detachObserver(EVENT_TYPE_PAGE_NUMBER_CHANGED);
			this._pagerComponent.dispose();
		}

		if (this._listViewComponent) {
			this._listViewComponent.detachObserver(EVENT_TYPE_FILE_LIST_SELECTED);
			this._listViewComponent.detachObserver(EVENT_TYPE_FILE_LIST_ACTION_EXECUTING);
			this._listViewComponent.dispose();
		}

		jQuery(this._allSelectorElement).unbind();

		jQuery(this._listElement).children().remove();

		this._listElement = null;
		this._allSelectorElement = null;
		this._fileListContentElement = null;

		this._contextMenuComponent = null;
		this._pagerComponent = null;
	} // dispose
};















// получение данных с сервера и отрисовка






Quantumart.QP8.BackendFileList.registerClass("Quantumart.QP8.BackendFileList", Quantumart.QP8.Observable);
// #endregion

// #region interface IBackendFileListView
Quantumart.QP8.IBackendFileListView = function () { };
Quantumart.QP8.IBackendFileListView.prototype = {
	initialize: function () { }, // инициализация
	redraw: function (data, options) { }, // перерисовать
	selectAll: function (value) { }, // выбрать все
	isAllSelected: function () { }, // позволяет определить, все ли выбраны
	dispose: function () { } // dispose
};
Quantumart.QP8.IBackendFileListView.registerInterface("Quantumart.QP8.IBackendFileListView");
// #endregion

