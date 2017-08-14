window.EVENT_TYPE_LIBRARY_DATA_BOUND = 'OnLibraryDataBound';
window.EVENT_TYPE_LIBRARY_ACTION_EXECUTING = 'OnLibraryActionExecuting';
window.EVENT_TYPE_LIBRARY_ENTITY_SELECTED = 'OnLibraryEntitySelected';
window.EVENT_TYPE_LIBRARY_ENTITY_REMOVED = 'OnLibraryEntityRemoved';
window.EVENT_TYPE_LIBRARY_REQUEST_VIEW_TYPE_CODE = 'OnLibraryRequestViewTypeCode';
window.EVENT_TYPE_LIBRARY_RESIZED = 'OnLibraryResized';

Quantumart.QP8.BackendLibrary = function (libraryGroupCode, libraryElementId, parentEntityId, actionCode, options, hostOptions) {
  Quantumart.QP8.BackendLibrary.initializeBase(this);

  this._libraryGroupCode = libraryGroupCode;
  this._libraryElementId = libraryElementId;
  this._libraryElement = $(`#${  libraryElementId}`).get(0);
  this._fileContainers = {};
  this._fileContainers[window.VIEW_TYPE_CODE_DETAILS] = $(`.${  this.LIBRARY_GRID_CONTAINER_CLASS_NAME}`, this._libraryElement);
  this._fileContainers[window.VIEW_TYPE_CODE_LIST] = $(`.${  this.LIBRARY_LIST_CONTAINER_CLASS_NAME}`, this._libraryElement);
  this._fileContainers[window.VIEW_TYPE_CODE_THUMBNAILS] = $(`.${  this.LIBRARY_THUMBNAILS_CONTAINER_CLASS_NAME}`, this._libraryElement);
  this._parentEntityId = parentEntityId;
  this._actionCode = actionCode;
  this._folderEntityTypeCode = actionCode == window.ACTION_CODE_SITE_LIBRARY ? window.ENTITY_TYPE_CODE_SITE_FOLDER : window.ENTITY_TYPE_CODE_CONTENT_FOLDER;
  this._fileEntityTypeCode = actionCode == window.ACTION_CODE_SITE_LIBRARY ? window.ENTITY_TYPE_CODE_SITE_FILE : window.ENTITY_TYPE_CODE_CONTENT_FILE;
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

  this._fileTypeListElement = $(`.${  this.LIBRARY_FILE_TYPE_LIST_CLASS_NAME}`, this._libraryElement);
  this._fileNameSearchElement = $(`.${  this.LIBRARY_FILE_NAME_FILTER_CLASS_NAME}`, this._libraryElement);
  this._filterApplyButtonElement = $(`.${  this.LIBRARY_FILTER_BUTTON_CLASS_NAME}`, this._libraryElement);
  this._filterResetButtonElement = $(`.${  this.LIBRARY_RESET_FILTER_BUTTON_CLASS_NAME}`, this._libraryElement);
  this._filterFormElement = $(`.${  this.LIBRARY_FILTER_FORM_CLASS_NAME}`, this._libraryElement);
};

Quantumart.QP8.BackendLibrary.prototype = {
  _libraryGroupCode: '',
  _libraryElementId: '',
  _libraryElement: null,
  _fileTypeListElement: null,
  _fileNameSearchElement: null,
  _filterApplyButtonElement: null,
  _filterResetButtonElement: null,
  _filterFormElement: null,
  _fileGridContainer: null,
  _entityTypeCode: '',
  _parentEntityId: 0,
  _actionCode: '',
  _libraryManager: null,
  _fileContainers: null,
  _viewTypeCode: window.VIEW_TYPE_CODE_LIST,
  _folderId: 0,
  _filterFileTypeId: '',
  _allowUpload: false,
  _isFilterFileTypeIdDefined: false,
  _filterFileName: '',
  _folderPath: '',
  _libraryPath: '',
  _folderUrl: '',
  _allowMultipleSelection: true,
  _uploaderType: Quantumart.QP8.Enums.UploaderType.Silverlight,

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
    $(this._libraryElement).closest('.documentWrapper').addClass('libraryWrapper');
    this._folderTree = Quantumart.QP8.BackendEntityTreeManager.getInstance().createTree(
      this._folderTreeId, this._folderEntityTypeCode, this._parentEntityId, this._actionCode, {
        contextMenuCode: this._folderEntityTypeCode,
        rootEntityId: this._folderId,
        zIndex: this._zIndex
      }
    );
    this._folderTree.attachObserver(window.EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED, $.proxy(this._onFolderSelectedHandler, this));
    this._folderTree.attachObserver(window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, $.proxy(this._onActionExecutingHandler, this));
    this._folderTree.oneTimeObserver(window.EVENT_TYPE_ENTITY_TREE_DATA_BOUND, $.proxy(this._onFolderTreeLoaded, this));
    this._folderTree.initialize();

    this._splitter = new Quantumart.QP8.BackendSplitter(this._splitterId,
      { firstPaneWidth: 250, minFirstPaneWidth: 50, maxFirstPaneWidth: 250, stateCookieName: 'FolderTreeSize' }
    );
    this._splitter.attachObserver(window.EVENT_TYPE_SPLITTER_RESIZED, $.proxy(this._onSplitterResized, this));
    this._splitter.attachObserver(window.EVENT_TYPE_SPLITTER_INITIALIZED, $.proxy(this._onSplitterResized, this));

    this._fileGrid = Quantumart.QP8.BackendEntityGridManager.getInstance().createGrid(
      this._fileGridId, this._fileEntityTypeCode, this._folderId, this._actionCode, {
        contextMenuCode: this._fileEntityTypeCode,
        autoLoad: false,
        keyColumnName: 'Name',
        allowMultipleRowSelection: this._allowMultipleSelection,
        zIndex: this._zIndex
      }
    );

    this._fileGrid.attachObserver(window.EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED, $.proxy(this._onFileSelectedHandler, this));
    this._fileGrid.attachObserver(window.EVENT_TYPE_ENTITY_GRID_DATA_BOUND, $.proxy(this._onFileListDataBoundHandler, this));
    this._fileGrid.attachObserver(window.EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, $.proxy(this._onActionExecutingHandler, this));
    this._fileGrid.initialize();

    var options = { selectMode: this._allowMultipleSelection ? window.FILE_LIST_SELECT_MODE_MULTIPLE : window.FILE_LIST_SELECT_MODE_SINGLE, zIndex: this._zIndex };

    this._fileList = new Quantumart.QP8.BackendFileList(`#${  this._fileContainers[window.VIEW_TYPE_CODE_LIST].attr('id')}`, this._fileEntityTypeCode, this._actionCode, this._fileEntityTypeCode, window.FILE_LIST_MODE_NAME_LIST, options);
    this._fileList.attachObserver(window.EVENT_TYPE_FILE_LIST_SELECTED, $.proxy(this._onFileSelectedHandler, this));
    this._fileList.attachObserver(window.EVENT_TYPE_FILE_LIST_DATA_BOUND, $.proxy(this._onFileListDataBoundHandler, this));
    this._fileList.attachObserver(window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, $.proxy(this._onActionExecutingHandler, this));
    this._fileList.initialize();

    this._filePreviewList = new Quantumart.QP8.BackendFileList(`#${  this._fileContainers[window.VIEW_TYPE_CODE_THUMBNAILS].attr('id')}`, this._fileEntityTypeCode, this._actionCode, this._fileEntityTypeCode, window.FILE_LIST_MODE_PREVIEW_LIST, options);
    this._filePreviewList.attachObserver(window.EVENT_TYPE_FILE_LIST_SELECTED, $.proxy(this._onFileSelectedHandler, this));
    this._filePreviewList.attachObserver(window.EVENT_TYPE_FILE_LIST_DATA_BOUND, $.proxy(this._onFileListDataBoundHandler, this));
    this._filePreviewList.attachObserver(window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, $.proxy(this._onActionExecutingHandler, this));
    this._filePreviewList.initialize();

    var fileExtensions = '';

    if (this._isFilterFileTypeIdDefined === true) {
      fileExtensions = window.LIBRARY_FILE_EXTENSIONS_DICTIONARY[`${  this._filterFileTypeId}`];
    }

    if (this._allowUpload) {
      if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.Silverlight) {
        this._uploader = new Quantumart.QP8.BackendSilverlightUploader(this._libraryElement, { background: '#EBF5FB', extensions: fileExtensions });
      } else if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.Html) {
        this._uploader = new Quantumart.QP8.BackendHtmlUploader(this._libraryElement, { extensions: fileExtensions });
      } else if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.PlUpload) {
        this._uploader = new Quantumart.QP8.BackendPlUploader(this._libraryElement, { extensions: fileExtensions });
      }

      this._uploader.attachObserver(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, $.proxy(this._onFileUploadedHandler, this));
      this._uploader.initialize();
    }

    this._loadFolderPath();
    this._updateFolderInfo();

    $(this._filterApplyButtonElement).click($.proxy(this._onFilterChangedHandler, this));
    $(this._filterResetButtonElement).click($.proxy(this._onFilterResetHandler, this));
    $(this._filterFormElement).submit($.proxy(this._onFilterFormSubmittedHandler, this));
    $(this._fileTypeListElement).change($.proxy(this._onFileTypeChangedHandler, this));

    if (this._isFilterFileTypeIdDefined) {
      $(`option[value='${  this._filterFileTypeId  }']`, this._fileTypeListElement).prop('selected', true);
      $(this._fileTypeListElement).prop('disabled', true);
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
    if (this._viewTypeCode == window.VIEW_TYPE_CODE_DETAILS) {
      this._fileGrid.set_parentEntityId(this._folderId);
      this._fileGrid.resetGrid({
        searchQuery: JSON.stringify(
          {
            FileType: this._filterFileTypeId,
            FileNameFilter: this._filterFileName
          })
      });
    } else if (this._viewTypeCode == window.VIEW_TYPE_CODE_LIST) {
      this._fileList.rebind(
        {
          pageNumber: 0,
          folderId: this._folderId,
          fileTypeId: this._filterFileTypeId,
          fileNameFilter: this._filterFileName
        });
    } else if (this._viewTypeCode == window.VIEW_TYPE_CODE_THUMBNAILS) {
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
    if (this._viewTypeCode == window.VIEW_TYPE_CODE_DETAILS) {
      this._fileGrid.refreshGrid();
    } else if (this._viewTypeCode == window.VIEW_TYPE_CODE_LIST) {
      this._fileList.rebind();
    } else if (this._viewTypeCode == window.VIEW_TYPE_CODE_THUMBNAILS) {
      this._filePreviewList.rebind();
    }
  },

  resize: function () {
    if (this._splitter) {
      this.notify(window.EVENT_TYPE_LIBRARY_RESIZED, {});
      this._splitter.resize();
    }
  },

  _onSplitterResized: function () {
    this.notify(window.EVENT_TYPE_LIBRARY_RESIZED, {});
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
    this.notify(window.EVENT_TYPE_LIBRARY_ENTITY_SELECTED, eventArgs);
  },

  _onFileListDataBoundHandler: function (eventType, sender, eventArgs) {
    eventArgs.set_context(this._libraryPath);
    this.notify(window.EVENT_TYPE_LIBRARY_DATA_BOUND, eventArgs);
  },

  _onFilterChangedHandler: function () {
    this._filterFileTypeId = $(this._fileTypeListElement).val();
    this._filterFileName = $(this._fileNameSearchElement).val();
    this.resetCurrentFileList();
  },

  _onFilterResetHandler: function () {
    if (!this._isFilterFileTypeIdDefined) {
      $("option[value='']", this._fileTypeListElement).prop('selected', true);
    }

    $(this._fileNameSearchElement).val('');

    this._onFilterChangedHandler();
  },

  _onFilterFormSubmittedHandler: function (e) {
    e.preventDefault();
    $(this._filterApplyButtonElement).trigger('click');
    return false;
  },

  _onFileTypeChangedHandler: function () {
    $(this._filterApplyButtonElement).trigger('click');
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
      this.notify(window.EVENT_TYPE_LIBRARY_ACTION_EXECUTING, newEventArgs);
      params = null;
      newEventArgs = null;
    }
  },

  _onFolderTreeLoaded: function () {
    this._folderTree.selectRoot();
  },

  _loadFolderPath: function () {
    var url = '';
    if (this._fileEntityTypeCode == window.ENTITY_TYPE_CODE_SITE_FILE) {
      url = `${window.CONTROLLER_URL_SITE  }_FolderPath`;
    } else if (this._fileEntityTypeCode == window.ENTITY_TYPE_CODE_CONTENT_FILE) {
      url = `${window.CONTROLLER_URL_CONTENT  }_FolderPath`;
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
      (data) => {
        if (data.success) {
          self._folderPath = data.path;
          self._folderUrl = data.url;
          self._uploader.set_folderPath(data.path);
          self._libraryPath = data.libraryPath;
        } else {
          $q.alertFail(data.message);
        }
      },
      (jqXHR) => {
        $q.processGenericAjaxError(jqXHR);
      }
    );
  },

  _updateFolderInfo: function () {
    $(`.${  this.LIBRARY_PHYSICAL_PATH_CLASS_NAME}`, this._libraryElement).html(this._folderPath);
    $(`.${  this.LIBRARY_VIRTUAL_PATH_CLASS_NAME}`, this._libraryElement).html(this._folderUrl);
  },

  dispose: function () {
    Quantumart.QP8.BackendLibraryManager.getInstance().removeLibrary(this._libraryElementId);

    if (this._splitter) {
      this._splitter.detachObserver(window.EVENT_TYPE_SPLITTER_RESIZED);
      this._splitter.detachObserver(window.EVENT_TYPE_SPLITTER_INITIALIZED);
      this._splitter.dispose();
      this._splitter = null;
    }

    if (this._folderTree) {
      this._folderTree.detachObserver(window.EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED);
      this._folderTree.detachObserver(window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING);
      this._folderTree.detachObserver(window.EVENT_TYPE_ENTITY_TREE_DATA_BOUND);
      this._folderTree.dispose();
      this._folderTree = null;
    }

    if (this._fileGrid) {
      this._fileGrid.detachObserver(window.EVENT_TYPE_ENTITY_GRID_DATA_BOUND);
      this._fileGrid.detachObserver(window.EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED);
      this._fileGrid.detachObserver(window.EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING);
      this._fileGrid.dispose();
      this._fileGrid = null;
    }

    if (this._fileList) {
      this._fileList.detachObserver(window.EVENT_TYPE_FILE_LIST_SELECTED);
      this._fileList.detachObserver(window.EVENT_TYPE_FILE_LIST_DATA_BOUND);
      this._fileList.detachObserver(window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING);
      this._fileList.dispose();
      this._fileList = null;
    }

    if (this._filePreviewList) {
      this._filePreviewList.detachObserver(window.EVENT_TYPE_FILE_LIST_SELECTED);
      this._filePreviewList.detachObserver(window.EVENT_TYPE_FILE_LIST_DATA_BOUND);
      this._filePreviewList.detachObserver(window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING);
      this._filePreviewList.dispose();
      this._filePreviewList = null;
    }

    if (this._uploader) {
      this._uploader.detachObserver(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED);
      this._uploader.dispose();
      this._uploader = null;
    }

    this._fileContainers[window.VIEW_TYPE_CODE_DETAILS] = null;
    this._fileContainers[window.VIEW_TYPE_CODE_LIST] = null;
    this._fileContainers[window.VIEW_TYPE_CODE_THUMBNAILS] = null;
    this._fileContainers = null;
    this._libraryElement = null;

    $(this._filterApplyButtonElement).unbind('click');
    $(this._filterResetButtonElement).unbind('click');
    $(this._filterFormElement).unbind('submit');
    $(this._fileTypeListElement).unbind('change');

    this._fileTypeListElement = null;
    this._filterApplyButtonElement = null;
    this._filterResetButtonElement = null;
    this._fileNameSearchElement = null;
    this._filterFormElement = null;

    $q.collectGarbageInIE();

    Quantumart.QP8.BackendLibrary.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendLibrary.generateActionUrl = function (actionName, urlParams) {
  var result = '';

  if (urlParams) {
    var html = new $.telerik.stringBuilder();

    html.cat(window.APPLICATION_ROOT_URL);
    html.cat('/Library/');
    html.cat(actionName);
    html.cat(`/${  urlParams.id || 0}`);
    html.cat(`?fileName=${  urlParams.fileName}`);
    html.cat(`&isVersion=${  urlParams.isVersion || false}`);
    html.catIf(`&entityTypeCode=${  urlParams.entityTypeCode}`, urlParams.entityTypeCode);
    html.catIf(`&entityId=${  urlParams.entityId}`, urlParams.entityId);
    html.catIf(`&actionTypeCode=${  urlParams.actionTypeCode}`, urlParams.actionTypeCode);
    result = html.string();
  }

  return result;
};

Quantumart.QP8.BackendLibrary.registerClass('Quantumart.QP8.BackendLibrary', Quantumart.QP8.Observable);
