import { BackendFileNameListView } from './BackendFileNameListView';
import { BackendFilePreviewListView } from './BackendFilePreviewListView';
import { BackendPager } from './BackendPager';
import { Observable } from '../Common/Observable';
import { $a } from '../BackendActionExecutor';
import { $q } from '../Utils';

window.FILE_LIST_MODE_NAME_LIST = 'FILE_LIST_MODE_NAME_LIST';
window.FILE_LIST_MODE_PREVIEW_LIST = 'FILE_LIST_MODE_PREVIEW_LIST';
window.FILE_LIST_SELECT_MODE_MULTIPLE = 'FILE_LIST_SELECT_MODE_MULTIPLE';
window.FILE_LIST_SELECT_MODE_SINGLE = 'FILE_LIST_SELECT_MODE_SINGLE';
window.FILE_LIST_NAME_PAGE_SIZE = 60;
window.FILE_LIST_PREVIEW_PAGE_SIZE = 24;
window.FILE_LIST_ITEMS_PER_COLUMN = 20;

window.EVENT_TYPE_FILE_LIST_DATA_BOUND = 'OnFileListDataBound';
window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING = 'OnFileListActionExecuting';
window.EVENT_TYPE_FILE_LIST_SELECTED = 'OnFileNameSelected';

export class BackendFileList extends Observable {
  // eslint-disable-next-line max-params
  constructor(
    listElementId,
    fileEntityTypeCode,
    actionCode,
    contextMenuCode,
    viewMode,
    options
  ) {
    super();

    this._listElementId = listElementId;
    this._fileEntityTypeCode = fileEntityTypeCode;
    this._contextMenuCode = contextMenuCode;
    this._viewMode = viewMode;
    this._actionCode = actionCode;

    if (!$q.isNull(options)) {
      if (!$q.isNull(options.selectMode)) {
        this._selectMode = options.selectMode;
      }
      if (options.zIndex) {
        this._zIndex = options.zIndex;
      }
    }

    this._currentDataQueryOptions = {
      pageNumber: 0,
      pageSize: 0,
      folderId: 0,
      fileTypeId: '',
      fileNameFilter: ''
    };
    if (this._viewMode === window.FILE_LIST_MODE_NAME_LIST) {
      this._currentDataQueryOptions.pageSize = window.FILE_LIST_NAME_PAGE_SIZE;
    } else if (this._viewMode === window.FILE_LIST_MODE_PREVIEW_LIST) {
      this._currentDataQueryOptions.pageSize = window.FILE_LIST_PREVIEW_PAGE_SIZE;
      this._loadDimensions = true;
    }
  }

  _fileEntityTypeCode = 0;
  _contextMenuCode = 0;
  _viewMode = window.FILE_LIST_MODE_NAME_LIST;
  _selectMode = window.FILE_LIST_SELECT_MODE_MULTIPLE;
  _actionCode = '';

  _currentDataQueryOptions = null;

  _listElementId = '';
  _listElement = null;
  _allSelectorElement = null;
  _fileListContentElement = null;

  _contextMenuComponent = null;
  _pagerComponent = null;
  _listViewComponent = null;
  _zIndex = 0;
  _loadDimensions = false;

  _onAllSelectorClicked() {
    this._listViewComponent.selectAll(jQuery(this._allSelectorElement).is(':checked'));
  }

  _onPageNumberChanged(eventType, sender, args) {
    this.rebind({ pageNumber: args.getPageNumber() });
  }

  _onListViewSelected(eventType, sender, args) {
    $(this._allSelectorElement).prop('checked', this._listViewComponent.isAllSelected());
    this._raiseMultipleEventArgsEvent(window.EVENT_TYPE_FILE_LIST_SELECTED, args);
  }

  _onContextMenuItemClicked(eventType, sender, args) {
    args.set_parentEntityId(this._currentDataQueryOptions.folderId);
    this.notify(eventType, args);
  }

  _loadData() {
    let url = '';
    if (this._fileEntityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE) {
      url = `${window.CONTROLLER_URL_SITE}_FileList`;
    } else if (this._fileEntityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE) {
      url = `${window.CONTROLLER_URL_CONTENT}_FileList`;
    } else {
      throw new Error('fileEntityTypeCode is unknown.');
    }

    let result;
    $q.getJsonFromUrl(
      'GET',
      url,
      {
        folderId: this._currentDataQueryOptions.folderId,
        pageSize: this._currentDataQueryOptions.pageSize,
        pageNumber: this._currentDataQueryOptions.pageNumber,
        fileTypeId: this._currentDataQueryOptions.fileTypeId,
        fileNameFilter: this._currentDataQueryOptions.fileNameFilter,
        fileShortNameLength: this._listViewComponent.shortNameLength,
        loadDimensions: this._loadDimensions
      },
      false,
      false,
      data => {
        if (data.success) {
          result = data.data;
        } else {
          $q.alertError(data.message);
        }
      },
      jqXHR => {
        result = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );

    return result;
  }

  _raiseMultipleEventArgsEvent(eventType, args) {
    const action = $a.getBackendActionByCode(this._actionCode);
    if (!$q.isNull(action)) {
      const eventArgs = $a.getEventArgsFromAction(action);
      eventArgs.set_isMultipleEntities(true);
      if (args) {
        eventArgs.set_entities(args.get_entities());
      }
      eventArgs.set_entityTypeCode(this._fileEntityTypeCode);
      eventArgs.set_parentEntityId(this._currentDataQueryOptions.folderId);

      this.notify(eventType, eventArgs);
    }
  }

  initialize() {
    let $listElement = jQuery(this._listElementId);
    this._listElement = $listElement.get(0);

    const html = new $.telerik.stringBuilder();
    const isMultiple = this._selectMode === window.FILE_LIST_SELECT_MODE_MULTIPLE;
    const isSingle = this._selectMode === window.FILE_LIST_SELECT_MODE_SINGLE;

    html.cat('<div class="fileList">')
      .cat('<div class="fileListArea">')
      .cat('<div class="fileListHeader">')
      .catIf('<input type="checkbox" class="fileListAllSelector" />', isMultiple)
      .catIf('<br />', isSingle)
      .cat('</div>')
      .cat(String.format(
        '<div class="fileListContent" id="{0}_fileListContent">{1}</div>',
        $listElement.attr('id'), $l.FileList.noRecords)
      )
      .cat('</div>')
      .cat('<div class="fileListPager"></div>')
      .cat('</div>');
    $listElement.html(html.string());

    let pagerComponent = new BackendPager($listElement.find('.fileListPager').get(0));
    pagerComponent.initialize();
    pagerComponent.attachObserver(window.EVENT_TYPE_PAGE_NUMBER_CHANGED, $.proxy(this._onPageNumberChanged, this));
    this._pagerComponent = pagerComponent;

    let $allSelectorElement = $listElement.find('.fileListAllSelector');
    $allSelectorElement.click($.proxy(this._onAllSelectorClicked, this));
    this._allSelectorElement = $allSelectorElement.get(0);

    this._fileListContentElement = $listElement.find('.fileListContent').get(0);
    let listViewComponent = null;
    if (this._viewMode === window.FILE_LIST_MODE_NAME_LIST) {
      listViewComponent = new BackendFileNameListView(
        this._fileListContentElement, this._contextMenuCode, this._selectMode, this._zIndex
      );
    } else if (this._viewMode === window.FILE_LIST_MODE_PREVIEW_LIST) {
      listViewComponent = new BackendFilePreviewListView(
        this._fileListContentElement, this._contextMenuCode, this._selectMode, this._zIndex
      );
    } else {
      throw new Error('View Mode is unknown.');
    }

    listViewComponent.initialize();
    listViewComponent.attachObserver(
      window.EVENT_TYPE_FILE_LIST_SELECTED, $.proxy(this._onListViewSelected, this)
    );
    listViewComponent.attachObserver(
      window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING, $.proxy(this._onContextMenuItemClicked, this)
    );
    this._listViewComponent = listViewComponent;

    pagerComponent = null;
    listViewComponent = null;
    $allSelectorElement = null;
    $listElement = null;
  }

  rebind(options) {
    if ($q.isObject(options)) {
      if (!$q.isNull(options.pageSize)) {
        this._currentDataQueryOptions.pageSize = $q.toInt(options.pageSize);
      }
      if (!$q.isNull(options.pageNumber)) {
        this._currentDataQueryOptions.pageNumber = $q.toInt(options.pageNumber);
      }
      if (!$q.isNull(options.folderId)) {
        this._currentDataQueryOptions.folderId = $q.toInt(options.folderId);
      }
      if (!$q.isNull(options.fileTypeId)) {
        this._currentDataQueryOptions.fileTypeId = options.fileTypeId;
      }
      if (!$q.isNull(options.fileNameFilter)) {
        this._currentDataQueryOptions.fileNameFilter = options.fileNameFilter;
      }
    }

    const data = this._loadData();
    if (data) {
      this._listViewComponent.redraw(data,
        {
          folderId: this._currentDataQueryOptions.folderId,
          fileEntityTypeCode: this._fileEntityTypeCode
        });

      this._pagerComponent.set(
        {
          // @ts-ignore data is checked above
          totalCount: data.TotalRecords,
          pageSize: this._currentDataQueryOptions.pageSize,
          currentPageNumber: this._currentDataQueryOptions.pageNumber
        });

      this._currentDataQueryOptions.pageNumber = this._pagerComponent.getPageNumber();
      this._pagerComponent.redraw();
    }

    this._raiseMultipleEventArgsEvent(window.EVENT_TYPE_FILE_LIST_DATA_BOUND);
  }

  dispose() {
    if (this._pagerComponent) {
      this._pagerComponent.detachObserver(window.EVENT_TYPE_PAGE_NUMBER_CHANGED);
      this._pagerComponent.dispose();
    }

    if (this._listViewComponent) {
      this._listViewComponent.detachObserver(window.EVENT_TYPE_FILE_LIST_SELECTED);
      this._listViewComponent.detachObserver(window.EVENT_TYPE_FILE_LIST_ACTION_EXECUTING);
      this._listViewComponent.dispose();
    }

    $(this._allSelectorElement).unbind();

    $(this._listElement).children().remove();

    this._listElement = null;
    this._allSelectorElement = null;
    this._fileListContentElement = null;

    this._contextMenuComponent = null;
    this._pagerComponent = null;
  }
}


Quantumart.QP8.BackendFileList = BackendFileList;
