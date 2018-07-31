/* global CKEDITOR */

// eslint-disable-next-line no-extra-semi
; (function init() {
  const onBrowseSiteLibrary = function onBrowseSiteLibrary(cb) {
    let evArgs, options;
    const dialog = CKEDITOR.dialog.getCurrent();
    const $field = $(dialog.getParentEditor().element.$);
    let popup = $field.data('linkWindow-site');

    if (!popup) {
      evArgs = new Quantumart.QP8.BackendEventArgs();
      evArgs.set_entityId(+$field.data('library_entity_id'));
      evArgs.set_parentEntityId(+$field.data('library_parent_entity_id'));
      evArgs.set_entityTypeCode(window.ENTITY_TYPE_CODE_SITE);
      evArgs.set_actionCode(window.ACTION_CODE_POPUP_SITE_LIBRARY);

      options = {
        additionalUrlParameters: { subFolder: '' },
        zIndex: +$(dialog.getElement().$).find('.cke_dialog').css('zIndex') + 10,
        libraryUrl: $field.data('library_url')
      };

      popup = new Quantumart.QP8.LibraryPopupWindow(evArgs, options);
      $field.data('linkWindow-site', popup);
    }

    popup.setContentId(0);
    popup.setSelectCallback(cb);
    popup.openWindow();
    evArgs = null;
  };

  const onBrowseContentLibrary = function onBrowseContentLibrary(cb) {
    let evArgs, options;
    const dialog = CKEDITOR.dialog.getCurrent();
    const $field = $(dialog.getParentEditor().element.$);
    let popup = $field.data('linkWindow-content');

    if (!popup) {
      evArgs = new Quantumart.QP8.BackendEventArgs();
      evArgs.set_entityId(+$field.data('content_id'));
      evArgs.set_parentEntityId(+$field.data('library_parent_entity_id'));
      evArgs.set_entityTypeCode(window.ENTITY_TYPE_CODE_CONTENT);
      evArgs.set_actionCode(window.ACTION_CODE_POPUP_CONTENT_LIBRARY);

      options = {
        additionalUrlParameters: { subFolder: '' },
        zIndex: +$(dialog.getElement().$).find('.cke_dialog').css('zIndex') + 10,
        libraryUrl: $field.data('library_url')
      };

      popup = new Quantumart.QP8.LibraryPopupWindow(evArgs, options);
      $field.data('linkWindow-content', popup);
    }

    popup.setContentId(+$field.data('content_id'));
    popup.setSelectCallback(cb);
    popup.openWindow();
    evArgs = null;
  };

  const bindEvents = function bindEvents() {
    CKEDITOR.on('dialogDefinition', function onDialogDefinition(ev) {
      let fieldlName, container;
      const onSelectCb = function onSelectCb(url) {
        CKEDITOR.dialog.getCurrent().setValueOf('info', fieldlName, url);
      };

      if (ev.data.name === 'link') {
        fieldlName = 'url';
        container = ev.data.definition.getContents('info').get('urlOptions').children;
      } else if (ev.data.name === 'image') {
        fieldlName = 'txtUrl';
        container = ev.data.definition.getContents('info').elements[0].children;
      } else if (ev.data.name === 'flash') {
        fieldlName = 'src';
        container = ev.data.definition.getContents('info').elements[0].children;
      }

      if (['link', 'image', 'flash'].indexOf(ev.data.name) !== -1) {
        container.push({
          type: 'hbox',
          children: [{
            type: 'button',
            id: 'browseQpSite',
            label: 'Просмотр библиотеки сайта',
            onClick: onBrowseSiteLibrary.bind(this, onSelectCb)
          }, {
            type: 'button',
            id: 'browseQpContent',
            label: 'Просмотр библиотеки контента',
            onClick: onBrowseContentLibrary.bind(this, onSelectCb)
          }]
        });
      }
    });
  };

  // https://github.com/ckeditor/ckeditor-dev/issues/514
  Object.keys(CKEDITOR.dtd.$block).forEach(tag => {
    CKEDITOR.dtd.a[tag] = 1;
  });

  ['a', 'i', 'b', 'span'].forEach(tag => {
    delete CKEDITOR.dtd.$removeEmpty[tag];
  });

  bindEvents();
}());
