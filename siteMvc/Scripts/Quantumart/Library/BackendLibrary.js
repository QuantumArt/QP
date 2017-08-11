// ****************************************************************************
// *** Компонент "Библиотека"                         ***
// ****************************************************************************

// #region event types of entity library
// === Типы событий библиотеки ===
var EVENT_TYPE_LIBRARY_DATA_BOUND = 'OnLibraryDataBound';
var EVENT_TYPE_LIBRARY_ACTION_EXECUTING = 'OnLibraryActionExecuting';
var EVENT_TYPE_LIBRARY_ENTITY_SELECTED = 'OnLibraryEntitySelected';
var EVENT_TYPE_LIBRARY_ENTITY_REMOVED = 'OnLibraryEntityRemoved';
var EVENT_TYPE_LIBRARY_REQUEST_VIEW_TYPE_CODE = 'OnLibraryRequestViewTypeCode';
var EVENT_TYPE_LIBRARY_RESIZED = 'OnLibraryResized';

// #endregion

// #region class BackendLibrary
// === Класс "Библиотека" ===
Quantumart.QP8.BackendLibrary = function (libraryGroupCode, libraryElementId, parentEntityId, actionCode, options, hostOptions) {
  Quantumart.QP8.BackendLibrary.initializeBase(this);

  this._libraryGroupCode = libraryGroupCode;
  this._libraryElementId = libraryElementId;
  this._libraryElement = jQuery('#' + libraryElementId).get(0);
  this._fileContainers = {};
  this._fileContainers[VIEW_TYPE_CODE_DETAILS] = jQuery('.' + this.LIBRARY_GRID_CONTAINER_CLASS_NAME, this._libraryElement);
  this._fileContainers[VIEW_TYPE_CODE_LIST] = jQuery('.' + this.LIBRARY_LIST_CONTAINER_CLASS_NAME, this._libraryElement);
  this._fileContainers[VIEW_TYPE_CODE_THUMBNAILS] = jQuery('.' + this.LIBRARY_THUMBNAILS_CONTAINER_CLASS_NAME, this._libraryElement);
  this._parentEntityId = parentEntityId;
  this._actionCode = actionCode;
  this._folderEntityTypeCode = (actionCode == ACTION_CODE_SITE_LIBRARY) ? ENTITY_TYPE_CODE_SITE_FOLDER : ENTITY_TYPE_CODE_CONTENT_FOLDER;
  this._fileEntityTypeCode = (actionCode == ACTION_CODE_SITE_LIBRARY) ? ENTITY_TYPE_CODE_SITE_FILE : ENTITY_TYPE_CODE_CONTENT_FILE;
  this._folderId = options.folderId;
  this._splitterId = options.splitterId;
  this._folderTreeId = options.folderTreeId;
  this._fileGridId = options.fileGridId;
  this._allowMultipleSelection = options.allowMultipleSelection;
  this._uploaderType = options.uploaderType;
  if (options.filterFileTypeId) {
    this._filterFileTypeId = options.filterFileTypeId;
    this._isFilterFileTypeIdDefined = true;
  }

  if (hostOptions.viewTypeCode) {
    this._viewTypeCode = hostOptions.viewTypeCode;
  }

  if (hostOptions.zIndex) {
 this._zIndex = hostOptions.zIndex; 
}

  this._allowUpload = $q.toBoolean(options.allowUpload, false);

  this._fileTypeListElement = jQuery('.' + this.LIBRARY_FILE_TYPE_LIST_CLASS_NAME, this._libraryElement);
  this._fileNameSearchElement = jQuery('.' + this.LIBRARY_FILE_NAME_FILTER_CLASS_NAME, this._libraryElement);
  this._filterApplyButtonElement = jQuery('.' + this.LIBRARY_FILTER_BUTTON_CLASS_NAME, this._libraryElement);
  this._filterResetButtonElement = jQuery('.' + this.LIBRARY_RESET_FILTER_BUTTON_CLASS_NAME, this._libraryElement);
  this._filterFormElement = jQuery('.' + this.LIBRARY_FILTER_FORM_CLASS_NAME, this._libraryElement);

  // this._slUploaderContanerElement = jQuery("." + this.LIBRARY_SL_UPLOADER_CONTAINER_CLASS_NAME, this._libraryElement).get(0);
};

Quantumart.QP8.BackendLibrary.prototype = {
  _libraryGroupCode: '', // код группы, в которую входит грид
  _libraryElementId: '', // клиентский идентификатор грида
  _libraryElement: null, // DOM-элемент, содержащий библиотеку
  _fileTypeListElement: null, // DOM-элемент со списком типов файлов
  _fileNameSearchElement: null, // DOM-элемент с именем файла в фильтре
  _filterApplyButtonElement: null, // DOM-элемент кнопки искать на фильтре
  _filterResetButtonElement: null, // DOM-элемент кнопки очистки фильтра
  _filterFormElement: null, // DOM-элемент формы фильтра
  _fileGridContainer: null, // DOM-элемент, содержащий grid
  // _slUploaderContanerElement: null,
  _entityTypeCode: '', // код типа сущности
  _parentEntityId: 0, // идентификатор родительской сущности
  _actionCode: '', // код действия
  _libraryManager: null, // менеджер списков сущностей
  _fileContainers: null, // ссылки на контейнеры списковых компонентов
  _viewTypeCode: VIEW_TYPE_CODE_LIST,
  _folderId: 0,
  _filterFileTypeId: '',
  _allowUpload: false,
  _isFilterFileTypeIdDefined: false,
  _filterFileName: '',
  _folderPath: '', // путь на диске к текущей папке
  _libraryPath: '', // путь к папке относительно корня библиотеки
  _folderUrl: '', // URL текущей папки
  _allowMultipleSelection: true,
  _uploaderType: Quantumart.QP8.Enums.UploaderType.Silverlight, // тип компонента для загрузки файлов

  LIBRARY_TREE_CONTAINER_CLASS_NAME: 'l-tree',
  LIBRARY_GRID_CONTAINER_CLASS_NAME: 'l-grid',
  LIBRARY_LIST_CONTAINER_CLASS_NAME: 'l-list',
  LIBRARY_THUMBNAILS_CONTAINER_CLASS_NAME: 'l-thumb',
  LIBRARY_FILE_TYPE_LIST_CLASS_NAME: 'l-fileTypeList',
  LIBRARY_FILE_NAME_FILTER_CLASS_NAME: 'l-fileNameSearch',
  LIBRARY_FILTER_BUTTON_CLASS_NAME: 'l-filterButton',
  LIBRARY_RESET_FILTER_BUTTON_CLASS_NAME: 'l-resetFilterButton',
  LIBRARY_FILTER_FORM_CLASS_NAME: 'l-filterForm',
  LIBRARY_PHYSICAL_PATH_CLASS_NAME: 'l-physical-path',
  LIBRARY_VIRTUAL_PATH_CLASS_NAME: 'l-virtual-path',

  // LIBRARY_SL_UPLOADER_CONTAINER_CLASS_NAME: "l-sl-uploader",

  set_libraryManager: function (value) {
    this._libraryManager = value;
  },
  get_libraryManager: function () {
    return this._libraryManager;
  },
  set_folderId: function (value) {
    this._folderId = value;
  },
  get_folderId: function () {
    return this._folderId;
  },
  set_viewTypeCode: function (value) {
    this._viewTypeCode = value;
  },
  get_viewTypeCode: function () {
    return this._viewTypeCode;
  },

  initialize: function () {
    jQuery(this._libraryElement).closest('.documentWrapper').addClass('libraryWrapper');
    this._folderTree = Quantumart.QP8.BackendEntityTreeManager.getInstance().createTree(
      this._folderTreeId, this._folderEntityTypeCode, this._parentEntityId, this._actionCode, {
        contextMenuCode: this._folderEntityTypeCode,
        rootEntityId: this._folderId,
        zIndex: this._zIndex
      }
    );
    this._folderTree.attachObserver(EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED, jQuery.proxy(this._onFolderSelectedHandler, this));
    this._folderTree.attachObserver(EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, jQuery.proxy(this._onActionExecutingHandler, this));
    this._folderTree.oneTimeObserver(EVENT_TYPE_ENTITY_TREE_DATA_BOUND, jQuery.proxy(this._onFolderTreeLoaded, this));
    this._folderTree.initialize();

    this._splitter = new Quantumart.QP8.BackendSplitter(this._splitterId,
    { firstPaneWidth: 250, minFirstPaneWidth: 50, maxFirstPaneWidth: 250, stateCookieName: 'FolderTreeSize' }
    );
    this._splitter.attachObserver(EVENT_TYPE_SPLITTER_RESIZED, jQuery.proxy(this._onSplitterResized, this));
    this._splitter.attachObserver(EVENT_TYPE_SPLITTER_INITIALIZED, jQuery.proxy(this._onSplitterResized, this));

    this._fileGrid = Quantumart.QP8.BackendEntityGridManager.getInstance().createGrid(
      this._fileGridId, this._fileEntityTypeCode, this._folderId, this._actionCode, {
        contextMenuCode: this._fileEntityTypeCode,
        autoLoad: false,
        keyColumnName: 'Name',
        allowMultipleRowSelection: this._allowMultipleSelection,
        zIndex: this._zIndex
      }
    );

    this._fileGrid.attachObserver(EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED, jQuery.proxy(this._onFileSelectedHandler, this));
    this._fileGrid.attachObserver(EVENT_TYPE_ENTITY_GRID_DATA_BOUND, jQuery.proxy(this._onFileListDataBoundHandler, this));
    this._fileGrid.attachObserver(EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, jQuery.proxy(this._onActionExecutingHandler, this));
    this._fileGrid.initialize();

    var options = { selectMode: (this._allowMultipleSelection ? FILE_LIST_SELECT_MODE_MULTIPLE : FILE_LIST_SELECT_MODE_SINGLE), zIndex: this._zIndex };

    this._fileList = new Quantumart.QP8.BackendFileList('#' + this._fileContainers[VIEW_TYPE_CODE_LIST].attr('id'), this._fileEntityTypeCode, this._actionCode, this._fileEntityTypeCode, FILE_LIST_MODE_NAME_LIST, options);
    this._fileList.attachObserver(EVENT_TYPE_FILE_LIST_SELECTED, jQuery.proxy(this._onFileSelectedHandler, this));
    this._fileList.attachObserver(EVENT_TYPE_FILE_LIST_DATA_BOUND, jQuery.proxy(this._onFileListDataBoundHandler, this));
    this._fileList.attachObserver(EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, jQuery.proxy(this._onActionExecutingHandler, this));
    this._fileList.initialize();

    this._filePreviewList = new Quantumart.QP8.BackendFileList('#' + this._fileContainers[VIEW_TYPE_CODE_THUMBNAILS].attr('id'), this._fileEntityTypeCode, this._actionCode, this._fileEntityTypeCode, FILE_LIST_MODE_PREVIEW_LIST, options);
    this._filePreviewList.attachObserver(EVENT_TYPE_FILE_LIST_SELECTED, jQuery.proxy(this._onFileSelectedHandler, this));
    this._filePreviewList.attachObserver(EVENT_TYPE_FILE_LIST_DATA_BOUND, jQuery.proxy(this._onFileListDataBoundHandler, this));
    this._filePreviewList.attachObserver(EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, jQuery.proxy(this._onActionExecutingHandler, this));
    this._filePreviewList.initialize();

    var fileExtensions = '';

    if (this._isFilterFileTypeIdDefined === true) {
      fileExtensions = LIBRARY_FILE_EXTENSIONS_DICTIONARY['' + this._filterFileTypeId];
    }

    if (this._allowUpload) {
      if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.Silverlight) {
        this._uploader = new Quantumart.QP8.BackendSilverlightUploader(this._libraryElement, { background: '#EBF5FB', extensions: fileExtensions });
      } else if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.Html) {
        this._uploader = new Quantumart.QP8.BackendHtmlUploader(this._libraryElement, { extensions: fileExtensions });
      } else if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.PlUpload) {
        this._uploader = new Quantumart.QP8.BackendPlUploader(this._libraryElement, { extensions: fileExtensions });
      }

      this._uploader.attachObserver(EVENT_TYPE_LIBRARY_FILE_UPLOADED, jQuery.proxy(this._onFileUploadedHandler, this));
      this._uploader.initialize();
    }

    this._loadFolderPath();
    this._updateFolderInfo();

    jQuery(this._filterApplyButtonElement).click(jQuery.proxy(this._onFilterChangedHandler, this));
    jQuery(this._filterResetButtonElement).click(jQuery.proxy(this._onFilterResetHandler, this));
    jQuery(this._filterFormElement).submit(jQuery.proxy(this._onFilterFormSubmittedHandler, this));
    jQuery(this._fileTypeListElement).change(jQuery.proxy(this._onFileTypeChangedHandler, this));

    // Если _filterFileTypeId был определен заранее, то выставить его в списке и задесейблить список
    if (this._isFilterFileTypeIdDefined) {
      jQuery("option[value='" + this._filterFileTypeId + "']", this._fileTypeListElement).prop('selected', true);
      jQuery(this._fileTypeListElement).prop('disabled', true);
    }

    this.resetCurrentFileList();
    this.showCurrentFileList();
  },

  onLoad: function () {
    this._splitter.initialize();
  },

  refreshLibrary: function () {
  },

  changeViewType: function (viewTypeCode) {
    this._viewTypeCode = viewTypeCode;
    this.resetCurrentFileList();
    this.showCurrentFileList();
  },

  showCurrentFileList: function () {
    for (var code in this._fileContainers) {
      var containerToHide = this._fileContainers[code];

      if (containerToHide) {
 containerToHide.hide(); 
}
    }

    var containerToShow = this._fileContainers[this._viewTypeCode];

    if (containerToShow) {
 containerToShow.show(); 
}
  },

  resetCurrentFileList: function () {
    if (this._viewTypeCode == VIEW_TYPE_CODE_DETAILS) {
      this._fileGrid.set_parentEntityId(this._folderId);
      this._fileGrid.resetGrid({
        searchQuery: JSON.stringify(
          {
            FileType: this._filterFileTypeId,
            FileNameFilter: this._filterFileName
          })
      });
    } else if (this._viewTypeCode == VIEW_TYPE_CODE_LIST) {
      this._fileList.rebind(
      {
        pageNumber: 0,
        folderId: this._folderId,
        fileTypeId: this._filterFileTypeId,
        fileNameFilter: this._filterFileName
      });
    } else if (this._viewTypeCode == VIEW_TYPE_CODE_THUMBNAILS) {
      this._filePreviewList.rebind(
      {
        pageNumber: 0,
        folderId: this._folderId,
        fileTypeId: this._filterFileTypeId,
        fileNameFilter: this._filterFileName
      });
    }
  },

  refreshCurrentFileList: function () {
    if (this._viewTypeCode == VIEW_TYPE_CODE_DETAILS) {
      this._fileGrid.refreshGrid();
    } else if (this._viewTypeCode == VIEW_TYPE_CODE_LIST) {
      this._fileList.rebind();
    } else if (this._viewTypeCode == VIEW_TYPE_CODE_THUMBNAILS) {
      this._filePreviewList.rebind();
    }
  },

  resize: function () {
    if (this._splitter) {
      this.notify(EVENT_TYPE_LIBRARY_RESIZED, {});
      this._splitter.resize();
    }
  },

  _onSplitterResized: function () {
    this.notify(EVENT_TYPE_LIBRARY_RESIZED, {});
    this._splitter.resize();
  },

  _onFolderSelectedHandler: function (eventType, sender, eventArgs) {
    var entities = eventArgs.get_entities();

    if (entities.length > 0) {
      var id = entities[0].Id;

      if (this._folderId != id) {
        this._folderId = id;
        this._loadFolderPath();
        this._updateFolderInfo();
        this.resetCurrentFileList();
      }
    }
  },

  _onFileSelectedHandler: function (eventType, sender, eventArgs) {
    eventArgs.set_context(this._libraryPath);
    this.notify(EVENT_TYPE_LIBRARY_ENTITY_SELECTED, eventArgs);
  },

  _onFileListDataBoundHandler: function (eventType, sender, eventArgs) {
    eventArgs.set_context(this._libraryPath);
    this.notify(EVENT_TYPE_LIBRARY_DATA_BOUND, eventArgs);
  },

  _onFilterChangedHandler: function () {
    this._filterFileTypeId = jQuery(this._fileTypeListElement).val();
    this._filterFileName = jQuery(this._fileNameSearchElement).val();
    this.resetCurrentFileList();
  },

  _onFilterResetHandler: function () {
    if (!this._isFilterFileTypeIdDefined) {
      jQuery("option[value='']", this._fileTypeListElement).prop('selected', true);
    }

    jQuery(this._fileNameSearchElement).val('');

    this._onFilterChangedHandler();
  },

  _onFilterFormSubmittedHandler: function (e) {
    e.preventDefault();
    jQuery(this._filterApplyButtonElement).trigger('click');
    return false;
  },

  _onFileTypeChangedHandler: function () {
    jQuery(this._filterApplyButtonElement).trigger('click');
  },

  _onFileUploadedHandler: function () {
    this.refreshCurrentFileList();
  },

  _onActionExecutingHandler: function (eventType, sender, eventArgs) {
    var action = $a.getBackendActionByCode(eventArgs.get_actionCode());

    if (action) {
      var params = new Quantumart.QP8.BackendActionParameters({ eventArgs: eventArgs });

      params.correct(action);
      var newEventArgs = $a.getEventArgsFromActionWithParams(action, params);

      newEventArgs.set_context(this._libraryPath);
      this.notify(EVENT_TYPE_LIBRARY_ACTION_EXECUTING, newEventArgs);
      params = null;
      newEventArgs = null;
    }
  },

  _onFolderTreeLoaded: function () {
    this._folderTree.selectRoot();
  },

  _loadFolderPath: function () {
    var url = '';

    // определить url в зависимости от fileEntityTypeCode
    if (this._fileEntityTypeCode == ENTITY_TYPE_CODE_SITE_FILE) {
 url = CONTROLLER_URL_SITE + '_FolderPath'; 
} else if (this._fileEntityTypeCode == ENTITY_TYPE_CODE_CONTENT_FILE) {
 url = CONTROLLER_URL_CONTENT + '_FolderPath'; 
} else {
 throw new Error('fileEntityTypeCode is unknown.'); 
}

    var self = this;

    $q.getJsonFromUrl(
    'GET',
    url,
      {
        folderId: this._folderId
      },
      false,
      false,
      function (data) {
        if (data.success) {
          self._folderPath = data.path;
          self._folderUrl = data.url;
          self._uploader.set_folderPath(data.path);
          self._libraryPath = data.libraryPath;
        } else {
          alert(data.message);
        }
      },
      function (jqXHR) {
        $q.processGenericAjaxError(jqXHR);
      }
    );
  },

  _updateFolderInfo: function () {
    jQuery('.' + this.LIBRARY_PHYSICAL_PATH_CLASS_NAME, this._libraryElement).html(this._folderPath);
    jQuery('.' + this.LIBRARY_VIRTUAL_PATH_CLASS_NAME, this._libraryElement).html(this._folderUrl);
  },

  dispose: function () {
    Quantumart.QP8.BackendLibraryManager.getInstance().removeLibrary(this._libraryElementId);

    if (this._splitter) {
      this._splitter.detachObserver(EVENT_TYPE_SPLITTER_RESIZED);
      this._splitter.detachObserver(EVENT_TYPE_SPLITTER_INITIALIZED);
      this._splitter.dispose();
      this._splitter = null;
    }

    if (this._folderTree) {
      this._folderTree.detachObserver(EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED);
      this._folderTree.detachObserver(EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING);
      this._folderTree.detachObserver(EVENT_TYPE_ENTITY_TREE_DATA_BOUND);
      this._folderTree.dispose();
      this._folderTree = null;
    }

    if (this._fileGrid) {
      this._fileGrid.detachObserver(EVENT_TYPE_ENTITY_GRID_DATA_BOUND);
      this._fileGrid.detachObserver(EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED);
      this._fileGrid.detachObserver(EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING);
      this._fileGrid.dispose();
      this._fileGrid = null;
    }

    if (this._fileList) {
      this._fileList.detachObserver(EVENT_TYPE_FILE_LIST_SELECTED);
      this._fileList.detachObserver(EVENT_TYPE_FILE_LIST_DATA_BOUND);
      this._fileList.detachObserver(EVENT_TYPE_FILE_LIST_ACTION_EXECUTING);
      this._fileList.dispose();
      this._fileList = null;
    }

    if (this._filePreviewList) {
      this._filePreviewList.detachObserver(EVENT_TYPE_FILE_LIST_SELECTED);
      this._filePreviewList.detachObserver(EVENT_TYPE_FILE_LIST_DATA_BOUND);
      this._filePreviewList.detachObserver(EVENT_TYPE_FILE_LIST_ACTION_EXECUTING);
      this._filePreviewList.dispose();
      this._filePreviewList = null;
    }

    if (this._uploader) {
      this._uploader.detachObserver(EVENT_TYPE_LIBRARY_FILE_UPLOADED);
      this._uploader.dispose();
      this._uploader = null;
    }

    this._fileContainers[VIEW_TYPE_CODE_DETAILS] = null;
    this._fileContainers[VIEW_TYPE_CODE_LIST] = null;
    this._fileContainers[VIEW_TYPE_CODE_THUMBNAILS] = null;
    this._fileContainers = null;

    // this._slUploaderContanerElement = null;

    this._libraryElement = null;

    jQuery(this._filterApplyButtonElement).unbind('click');
    jQuery(this._filterResetButtonElement).unbind('click');
    jQuery(this._filterFormElement).unbind('submit');
    jQuery(this._fileTypeListElement).unbind('change');

    this._fileTypeListElement = null;
    this._filterApplyButtonElement = null;
    this._filterResetButtonElement = null;
    this._fileNameSearchElement = null;
    this._filterFormElement = null;

    $q.collectGarbageInIE();

    Quantumart.QP8.BackendLibrary.callBaseMethod(this, 'dispose');
  }

};

Quantumart.QP8.BackendLibrary.generateActionUrl = function Quantumart$QP8$BackendLibrary$generateActionUrl(actionName, urlParams) {
  var result = '';

  if (urlParams) {
    var html = new $.telerik.stringBuilder();

    html.cat(APPLICATION_ROOT_URL);
    html.cat('/Library/');
    html.cat(actionName);
    html.cat('/' + (urlParams.id || 0));
    html.cat('?fileName=' + urlParams.fileName);
    html.cat('&isVersion=' + (urlParams.isVersion || false));
    html.catIf('&entityTypeCode=' + urlParams.entityTypeCode, urlParams.entityTypeCode);
    html.catIf('&entityId=' + urlParams.entityId, urlParams.entityId);
    html.catIf('&actionTypeCode=' + urlParams.actionTypeCode, urlParams.actionTypeCode);
    result = html.string();
  }

  return result;
};

Quantumart.QP8.BackendLibrary.registerClass('Quantumart.QP8.BackendLibrary', Quantumart.QP8.Observable);

// #endregion
