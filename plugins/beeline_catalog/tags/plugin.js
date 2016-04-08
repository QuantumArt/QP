'use strict';

var QA = QA || { BackendAPI: {} };

QA.BackendAPI.EntitiesPicker = QA.BackendAPI.EntitiesPicker || (function() {
  Options = function(contentId) {
    this.contentId = contentId;
    this.onSelected = function(name, args) {
      if (console && console.log) {
        console.log(name);
        console.log(args);
      }
    };

    this.onClosed = function(name, args) {
      if (console && console.log) {
        console.log(name);
        console.log(args);
      }
    };

    this.selectedItems = [];
  };

  Options.prototype = {
    contentId: null,
    isMultipleChoice: true,
    selectedItems: null,
    onSelected: null,
    onClosed: null
  };

  OnSelectedEventArgs = function() { };

  OnSelectedEventArgs.prototype = {
    isSelected: false,
    selectedItems: null
  };

  var QPEntitiesPicker = function(options) {
    this.options = options;
    var eventArgs = new Quantumart.QP8.BackendEventArgs();

    eventArgs.set_isMultipleEntities(options.isMultipleChoice);
    eventArgs.set_parentEntityId(options.contentId);
    eventArgs.set_entityTypeCode('article');

    if (options.isMultipleChoice == true) {
      eventArgs.set_actionCode('multiple_select_article');
    } else {
      eventArgs.set_actionCode('select_article');
    }

    var entities = options.selectedItems;

    if (entities.length > 0) {
      if (options.isMultipleChoice) {
        eventArgs.set_entities(entities);
      } else {
        eventArgs.set_entityId(entities[0].Id);
        eventArgs.set_entityName(entities[0].Name);
      }
    }

    this.component = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, {
      filter: 'c.archive = 0'
    });

    eventArgs = null;
  };

  QPEntitiesPicker.prototype = {
    options: null,
    component: null,
    _onSelectedInternal: function(args0, args1, args2) {
      this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      this.component.closeWindow();
      this.component.dispose();
      if (this.options.onSelected) {
        var args = new OnSelectedEventArgs();

        args.isSelected = true;
        args.selectedItems = args2.entities;

        try {
          this.options.onSelected(args0, args);
        } finally {
          this.options = null;
        }
      }
    },
    _onClosedInternal: function(args0) {
      this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      this.component.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      this.component.dispose();
      if (this.options.onClosed) {
        var args = new OnSelectedEventArgs();

        args.isSelected = false;

        try {
          this.options.onClosed(args0, args);
        } finally {
          this.options = null;
        }
      }
    },
    openDialog: function() {
      this.component.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED,
        jQuery.proxy(this._onSelectedInternal, this));
      this.component.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED,
        jQuery.proxy(this._onClosedInternal, this));
      this.component.openWindow();
    }
  };

  return {
    Options: Options,
    QPEntitiesPicker: QPEntitiesPicker
  };
})();

CKEDITOR.plugins.add('tags', {
  requires: ['styles', 'button'],
  init: function(editor) {
    var addButtonCommand = function(buttonName, buttonLabel, commandName, styleDefiniton) {
      var tagStyle = new CKEDITOR.style(styleDefiniton);

      editor.attachStyleStateChange(tagStyle, function(state) {
        !editor.readOnly && editor.getCommand(commandName).setState(state);
      });

      editor.addCommand(commandName, {
        style: tagStyle,
        exec: function(editor) {
          var options = new QA.BackendAPI.EntitiesPicker.Options(321);

          options.isMultipleChoice = false;
          options.onSelected = function(name, args) {
            var id = args.selectedItems[0].Id;
            var name = $o.getEntityName('article', id);
            var node = editor.getSelection().getStartElement().$;

            if (node.nodeName.toLowerCase() == 'replacement') {
              node.outerHTML = '<replacement>tag=' + name + '</replacement> ';
            } else {
              editor.insertElement(CKEDITOR.dom.element.createFromHtml('<replacement>tag=' + name + '</replacement>'));
            }
          };

          options.onClosed = function(name, args) {
            var tt = args;
          };

          var component = new QA.BackendAPI.EntitiesPicker.QPEntitiesPicker(options);

          component.openDialog();
        }
      });

      editor.ui.addButton(buttonName, {
        label: buttonLabel,
        command: commandName,
        icon: CKEDITOR.plugins.getPath('tags') + 'images/tag_plus.png'
      });
    };

    var config = editor.config;

    config.coreStyles_tag = { element: 'replacement' };
    addButtonCommand('Tag', 'Insert Tag', 'tag', config.coreStyles_tag);
  }
});
