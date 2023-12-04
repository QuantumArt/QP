import { BackendContextMenu } from '../BackendContextMenu';
import { BackendFileNameListView } from './BackendFileNameListView';
import { $q } from '../Utils';

export class BackendFilePreviewListView extends BackendFileNameListView {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor(fileListContentElement, contextMenuCode, selectMode, zIndex) {
    super(fileListContentElement, contextMenuCode, selectMode, zIndex);
  }

  initialize() {
    const $fileListContentElement = jQuery(this._fileListContentElement);
    $fileListContentElement.html('<div class="fileListPreviewContainer"></div>');

    $fileListContentElement.delegate(
      '.fileItem input:checkbox', 'click', $.proxy(this._onFileCheckBoxClickedHandler, this)
    );
    $fileListContentElement.delegate(
      '.fileItem', 'click', $.proxy(this._onFileContainerClickedHandler, this)
    );

    if (!$q.isNullOrWhiteSpace(this._contextMenuCode)) {
      const contextMenuComponent = new BackendContextMenu(
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
  }

  redraw(data, options) {
    let $fileListContentElement = jQuery(this._fileListContentElement);
    let $fileListPreviewContainer = $fileListContentElement.find('.fileListPreviewContainer');
    let html = new $.telerik.stringBuilder();

    const that = this;
    if (data.TotalRecords > 0) {
      jQuery.each(data.Data, (index, item) => {
        const link = that._getThumbnailLink(item, options);
        html.cat(String.format('<div class="fileItem" data-file_name="{0}">', encodeURI(item.FullName)))
          .catIf('<input type="checkbox" />', that._selectMode === window.FILE_LIST_SELECT_MODE_MULTIPLE)
          .cat(
            String.format('<div class="preview" style="background-image:url({3});"></div>'
            + '<h5><ul><li title="{4}">{0}</li></ul></h5>'
            + '<span>{1}</span>'
            + '<span>{2}</span>'
            + '</div>',
              $('<div>').text(item.Name).html(),
              item.Modified, item.Size,
              link, $('<div>').text(item.FullName).html()));
      });
    } else {
      html.cat($l.FileList.noRecords);
    }

    $fileListPreviewContainer.html(html.string());

    $fileListPreviewContainer = null;
    $fileListContentElement = null;
    html = null;
    this._raiseSelectEvent();
  }

  dispose() {
    super.dispose();
  }

  shortNameLength = 15;
  _getThumbnailLink(item, options) {
    if (item.FileType === Quantumart.QP8.Enums.LibraryFileType.Image) {
      let url = '';

      if (options.fileEntityTypeCode === window.ENTITY_TYPE_CODE_SITE_FILE) {
        url = `${window.CONTROLLER_URL_THUMBNAIL}_SiteFileThumbnail`;
      } else if (options.fileEntityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_FILE) {
        url = `${window.CONTROLLER_URL_THUMBNAIL}_ContentFileThumbnail`;
      }

      return $q.htmlEncode(String.format(
        "'{0}?folderId={1}&fileName={2}&{3}'", url, options.folderId, item.FullName, new Date().getTime()
      ));
    }
    return $q.htmlEncode(String.format(
      "'{0}{1}'", window.THEME_IMAGE_FOLDER_URL_BIG_FILE_TYPE_ICONS, item.BigIconLink
    ));
  }
}


Quantumart.QP8.BackendFilePreviewListView = BackendFilePreviewListView;
