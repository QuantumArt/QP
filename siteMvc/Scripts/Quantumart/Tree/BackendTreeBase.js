// ****************************************************************************
// *** Компонент "Дерево"                         ***
// ****************************************************************************

// #region class BackendTreeBase
// === Класс "Дерево" ===
Quantumart.QP8.BackendTreeBase = function (treeElementId, options) {
  Quantumart.QP8.BackendTreeBase.initializeBase(this);

  this._treeElementId = treeElementId;
  this._treeElementName = treeElementId;

  if ($q.isObject(options)) {
    if (options.treeContainerElementId) {
      this._treeContainerElementId = options.treeContainerElementId;
    }

    if (!$q.isNull(options.showIds)) {
      this._showIds = options.showIds;
    }

    if (!$q.isNull(options.autoGenerateLink)) {
      this._makeLinksFromIds = options.autoGenerateLink;
    }

    if (!$q.isNull(options.autoCheckChildren)) {
      this._autoCheckChildren = options.autoCheckChildren;
    }

    if (options.actionCodeForLink) {
      this._readActionCode = options.actionCodeForLink;
    }

    if (options.isWindow) {
      this._hostIsWindow = options.isWindow;
    }
  }

  $q.bindProxies.call(this, [
    '_onNodeClicking',
    '_onIdClicking'
  ]);
};

Quantumart.QP8.BackendTreeBase.prototype = {
  _treeElementId: '', // клиентский идентификатор древовидного меню
  _treeElementName: '', // имя элемента дерева
  _treeElement: null, // DOM-элемент образующий древовидное меню
  _treeContainerElementId: '', // клиентский идентификатор контейнера, в котором располагается древовидное меню
  _stopDeferredOperations: false, // признак, отвечающий за остановку все отложенных операций
  _treeComponent: null, // компонент "Древовидное меню"
  _deferredNodeCodeToHighlight: '', // код узла для отложенной подсветки
  _showIds: false, // показывать или нет ID для узлов дерева
  _makeLinksFromIds: true, // делать ли из ID ссылки
  _autoCheckChildren: false, // автовыделение дочерних элементов при выделении родителей
  _readActionCode: '', // код действия, которое открывает форму редактирования сущности
  _hostIsWindow: false,

  ROOT_NODE_CODE: 'root',
  ROOT_NODE_CLASS_NAME: 't-treeview',
  NODE_HOVER_CLASS_NAME: 't-state-hover',
  NODE_SELECTED_CLASS_NAME: 't-state-selected',
  NODE_OLD_CLICKABLE_SELECTORS: '.t-in:not(.t-state-selected,.t-state-disabled)',
  NODE_NEW_CLICKABLE_SELECTORS: 'SPAN.t-in:not(.t-state-disabled)',
  NODE_ID_LINK_SELECTORS: 'SPAN.idLink A',
  NODE_WRAPPER_SELECTOR: '> DIV > SPAN.t-in',
  NODE_CHECKBOX_SELECTORS: '> DIV > SPAN.t-checkbox INPUT:checkbox:not(:disabled)',
  NODE_PLUS_SELECTOR: '> DIV .t-plus',
  NODE_MINUS_SELECTOR: '> DIV .t-minus',

  get_treeElementId: function () {
    return this._treeElementId;
  },
  set_treeElementId: function (value) {
    this._treeElementId = value;
  },
  get_treeElement: function () {
    return this._treeElement;
  },
  get_treeContainerElementId: function () {
    return this._treeContainerElementId;
  },
  set_treeContainerElementId: function (value) {
    this._treeContainerElementId = value;
  },

  _onDataBindingHandler: null,
  _onNodeClickingHandler: null,
  _onNodeCheckboxClickHandler: null,

  initialize: function () {
    var $tree = $('#' + this._treeElementId);
    var treeComponent = $tree.data('tTreeView'); // древовидное меню
    treeComponent.isAjax = this.isAjax;
    treeComponent.ajaxRequest = this._onDataBindingHandler;

    var self = this;
    this._initNodeCheck(treeComponent);
    this._initNewToggle(treeComponent);

    this._treeElement = $tree.get(0);
    this._treeComponent = treeComponent;

    $tree
      .unbind('select')
      .undelegate(this.NODE_OLD_CLICKABLE_SELECTORS, 'click')
      .delegate(this.NODE_NEW_CLICKABLE_SELECTORS, 'click', this._onNodeClickingHandler)
      .delegate(this.NODE_ID_LINK_SELECTORS, 'click', this._onIdClickingHandler);

    $tree = null;
  },

  _initNodeCheck: function (treeComponent) {
    treeComponent.nodeCheck = function (li, isChecked, suppressAutoCheck, autoCheckChildren) {
      this._legacyNodeCheck.call(treeComponent, li, isChecked);
      this.beforeCustomNodeCheck.call(this, li, isChecked, suppressAutoCheck, autoCheckChildren);
      this._proceedAutoCheckAllChildren.call(this, li, isChecked, suppressAutoCheck, autoCheckChildren);
      this.afterCustomNodeCheck.call(this, li, isChecked, suppressAutoCheck, autoCheckChildren);
    }.bind(this);

    treeComponent.nodeCheckExcludeSelf = this._proceedAutoCheckDirectChildren.bind(this);
  },

  _initNewToggle: function (treeComponent) {
    var oldToggle = treeComponent.nodeToggle;

    treeComponent.nodeToggle = function () {
      oldToggle.call(treeComponent, arguments[0], arguments[1], true);
    };
  },

  isAjax: function () {
    return true;
  },

  getChildNodesContainer: function (node) {
    var $node = this.getNode(node);
    var $childNodesContainer = $('> UL.t-group', $node);
    $node = null;
    return $childNodesContainer;
  },

  getAllNodes: function () {
    var $tree = $(this._treeElement);
    var $nodes = $tree.find('LI.t-item');
    $tree = null;
    return $nodes;
  },

  getSelectedNodes: function () {
    return this.getAllNodes().filter(':has(> DIV > SPAN.' + this.NODE_SELECTED_CLASS_NAME + ')');
  },

  getNode: function (node, parentNodeElem) {
    if ($q.isObject(node)) {
      return $q.toJQuery(node);
    } else if ($q.isString(node) || $.isNumeric(node)) {
      let $parentNode = $(parentNodeElem);
      if ($q.isNullOrEmpty($parentNode)) {
        $parentNode = $('#' + this._treeElementId);
      }

      var $node = null;
      if (node == this.ROOT_NODE_CODE) {
        $node = $('#' + this._treeElementId);
      } else {
        $node = $("LI DIV INPUT:hidden[value='" + node + "']", $parentNode).parent().parent().filter('.t-item');
        if ($node.length == 0) {
          $node = null;
        }
      }

      return $node;
    }
  },

  getNodeValue: function (node) {
    var $node = this.getNode(node);
    var nodeValue = '';

    if (!$q.isNullOrEmpty($node)) {
      if ($node.hasClass(this.ROOT_NODE_CLASS_NAME)) {
        nodeValue = this.ROOT_NODE_CODE;
      } else {
        nodeValue = this._treeComponent.getItemValue($node);
      }
    }

    return nodeValue;
  },

  getNodeText: function (node) {
    var $node = this.getNode(node);
    var nodeText = '';

    if (!$q.isNullOrEmpty($node)) {
      nodeText = this._treeComponent.getItemText($node);
    }

    return nodeText;
  },

  getParentNode: function (node) {
    var $node = this.getNode(node);
    let $parentNode = null;
    if (!$q.isNullOrEmpty($node)) {
      $parentNode = $node.parent().parent();
      if ($parentNode.length == 0 || !$parentNode.hasClass('t-item')) {
        return null;
      }
    }

    return $parentNode;
  },

  refreshNode: function (node, options) {
    var loadChildNodes = true;
    var callback = null;

    if ($q.isObject(options)) {
      if (!$q.isNull(options.loadChildNodes)) {
        loadChildNodes = options.loadChildNodes;
      }

      if (options.callback) {
        callback = options.callback;
      }
    }

    var $node = this.getNode(node);
    var self = this;

    if (!$q.isNullOrEmpty($node)) {
      var refreshCallback = function () { };

      if (this.getChildNodeCount($node) > 0) {
        this.collapseNode($node);
      }

      this._refreshNodeInner($node, loadChildNodes, callback);
    }
  },

  _refreshNodeInner: function () {
    alert($l.Common.methodNotImplemented);
    return;
  },

  refreshNodes: function (nodes, options) {
    var self = this;
    var $nodes = $q.toJQuery(nodes);

    if (!$q.isNullOrEmpty($nodes)) {
      $nodes.each(function (index, nodeElem) {
        var $node = $(nodeElem);
        self.refreshNode($node, options);
      });
    }
  },

  _renderNode: function (node, dataItem, isRootNode) {
    var $node = this.getNode(node);
    var nodeIndex = this.getCheckedNodeIndex(node);
    var nodeHtml = new $.telerik.stringBuilder();

    $.telerik.treeview.getItemHtml({
      elementId: this._treeElementName,
      item: dataItem,
      html: nodeHtml,
      isAjax: this.isAjax(),
      isFirstLevel: isRootNode,
      showCheckBoxes: this._treeComponent.showCheckBox,
      itemIndex: nodeIndex,
      showIds: this._showIds,
      makeLinksFromIds: this._makeLinksFromIds
    });

    var $newNode = $(nodeHtml.string());
    var cssClassNames = $newNode.attr('class');
    var htmlContent = $newNode.html();

    nodeHtml = null;
    $newNode = null;

    $node.attr('class', cssClassNames).html(htmlContent);
  },

  _renderChildNodes: function (parentNode, dataItems, isRootNode) {
    let $parentNode = this.getNode(parentNode);
    var $group = this._getChildNodesContainer($parentNode);

    var isGroup = $q.isNullOrEmpty($group);
    var groupHtml = new $.telerik.stringBuilder();

    var parentNodeIndex = this.getCheckedNodeIndex(parentNode);

    $.telerik.treeview.getGroupHtml({
      elementId: this._treeElementName,
      data: dataItems,
      html: groupHtml,
      isAjax: true,
      isFirstLevel: isRootNode,
      showCheckBoxes: this._treeComponent.showCheckBox,
      renderGroup: isGroup,
      groupLevel: parentNodeIndex,
      showIds: this._showIds,
      makeLinksFromIds: this._makeLinksFromIds
    });

    if (!$q.isNullOrEmpty($group) && $parentNode.data('loaded') === false) {
      $(groupHtml.string()).prependTo($group);
    } else if (!$q.isNullOrEmpty($group) && $parentNode.data('loaded') !== false) {
      $group.empty().html(groupHtml.string());
    } else if ($q.isNullOrEmpty($group)) {
      $group = $(groupHtml.string()).appendTo($parentNode);
    }

    $group.show();
    $group = null;
  },

  _isSearchQueryEmpty: function () {
    return true;
  },

  refreshTree: function () {
    var maxExpandLevel = this._isSearchQueryEmpty() ? 1 : 0;
    $('ul.t-group', this._treeElement).remove();
    this.addNodesToParentNode(this._treeElement, maxExpandLevel);
  },

  _onNodeClicking: function (e) {
    var $element = $(e.currentTarget);
    var $node = $($element.closest('.t-item')[0]);
    if (!this._treeComponent.shouldNavigate($element)) {
      $node = null;
      $element = null;
      e.preventDefault();
      return false;
    } else {
      $node = null;
      $element = null;
    }
  },

  _onIdClicking: function (e) {
    e.preventDefault();
    e.stopPropagation();
    if (this._readActionCode) {
      this.executeAction($(e.currentTarget).closest('.t-item').first(), this._readActionCode, { ctrlKey: e.ctrlKey, shiftKey: e.shiftKey });
    }
  },

  expandNode: function (node) {
    var $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      this._treeComponent.expand($node);
    }
  },

  collapseNode: function (node) {
    var $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      this._treeComponent.collapse($node);
    }
  },

  removeNodeOrRefreshParent: function (node, parentNode, options) {
    var $node = this.getNode(node);
    if ($node) {
      if ($node.siblings().length > 0) {
        this.removeNode($node);
      } else {
        this.refreshNode(parentNode, options);
      }
    }
  },

  removeNode: function (node) {
    var $node = this.getNode(node);

    if (!$q.isNullOrEmpty($node)) {
      $node.hide(100, function () {
        $node.removeData().empty().remove();
      });
    }
  },

  removeChildNodes: function (node) {
    var $node = this.getNode(node);
    var $group = this._getChildNodesContainer($node);

    if (!$q.isNullOrEmpty($group)) {
      $group.empty();
    }
  },

  isRootNode: function (node) {
    var $node = this.getNode(node);
    var isRootNode = false;

    if (!$q.isNullOrEmpty($node)) {
      isRootNode = $node.hasClass(this.ROOT_NODE_CLASS_NAME);
    }

    return isRootNode;
  },

  getNodeLevel: function (node) {
    var $node = this.getNode(node);
    var level = -1;

    if (!$q.isNullOrEmpty($node)) {
      if (!this.isRootNode($node)) {
        let $parentNode = $node.parent();

        level = 1;
        while (!$q.isNullOrEmpty($parentNode) && !this.isRootNode($node)) {
          var parentNodeElem = $parentNode.get(0);

          if (parentNodeElem && parentNodeElem.tagName == 'LI') {
            level++;
          }

          $parentNode = $parentNode.parent();
        }
      } else {
        level = 0;
      }
    }

    return level;
  },

  getCheckedNodeIndex: function (node) {
    var $node = this.getNode(node);
    var nodeIndex = undefined;

    if (this._treeComponent.showCheckBox === true) {
 nodeIndex = $(".t-checkbox input[type='hidden'][name='" + this._treeElementName + ".Index']", $node).val(); 
}
    return nodeIndex;
  },

  getChildNodeCount: function (node) {
    var $node = this.getNode(node);
    var childNodeCount = 0;

    if (!$q.isNullOrEmpty($node)) {
      childNodeCount = $node.find('> UL.t-group > LI').length;
    }

    return childNodeCount;
  },

  _getChildNodesContainer: function (node) {
    return this.getNode(node).find('> UL.t-group');
  },

  _showAjaxLoadingIndicatorForNode: function (node) {
    var self = this;
    var $node = this.getNode(node);

    $node.data('loading_icon_timeout', setTimeout(function () {
      if (self._stopDeferredOperations) {
        clearTimeout($node.data('loading_icon_timeout'));
        return;
      }

      $node.find('> DIV > SPAN.t-icon').addClass('t-loading');
    }, 100));
  },

  _hideAjaxLoadingIndicatorForNode: function (node) {
    var $node = this.getNode(node);

    clearTimeout($node.data('loading_icon_timeout'));

    if ($node.hasClass('t-item')) {
      $node.data('loaded', true)
        .find('SPAN.t-icon:first')
        .removeClass('t-loading')
        .removeClass('t-plus')
        .addClass('t-minus');
    }
  },

  executeAction: function () {
    alert($l.Common.methodNotImplemented);
    return;
  },

  _legacyNodeCheck: function (li, isChecked) {
    $(li, this.element).each($.proxy(function (index, item) {
      var $item = $(item).closest('.t-item'),
      $checkboxHolder = $('> div > .t-checkbox', $item),
      arrayName = $checkboxHolder.data('array_name'); // this.element.id + '_checkedNodes',
      index = $checkboxHolder.find(':input[name="' + arrayName + '.Index"]').val();

      $checkboxHolder.find(':input[name="' + arrayName + '[' + index + '].Text"]').remove();
      $checkboxHolder.find(':input[name="' + arrayName + '[' + index + '].Value"]').remove();

      $checkboxHolder.find(':checkbox').attr({
        checked: isChecked ? true : false,
        value: isChecked
      });

      if (isChecked) {
        $($.telerik.treeview.getNodeInputsHtml(this.getItemValue($item), this.getItemText($item), arrayName, index)).appendTo($checkboxHolder);
      }
    }, this));
  },

  _proceedAutoCheckChildren: function (nodeSelectorFn, li, isChecked, suppressAutoCheck, autoCheckChildren) {
    if (!suppressAutoCheck && (autoCheckChildren || this._autoCheckChildren)) {
      var $node = $(li).closest('.t-item').first();

      if (this._isNodeCollapsed($node)) {
        this._treeComponent.nodeToggle(null, $node, true);
      }

      nodeSelectorFn.call(this, $node);
    }
  },

  _proceedAutoCheckAllChildren: function (li, isChecked, suppressAutoCheck, autoCheckChildren) {
    return this._proceedAutoCheckChildren(function ($node) {
      var self = this;
      $node.find('ul.t-group .t-checkbox [type=checkbox]').each(function (index, item) {
        var $checkbox = $(item);
        $checkbox.prop('checked', isChecked);
        self._treeComponent.nodeCheck($checkbox, isChecked, true);
        $checkbox.removeClass(window.CHANGED_FIELD_CLASS_NAME);
        $checkbox = null;
      });
    }, li, isChecked, suppressAutoCheck, autoCheckChildren);
  },

  _proceedAutoCheckDirectChildren: function (li, isChecked, suppressAutoCheck, autoCheckChildren) {
    return this._proceedAutoCheckChildren(function ($node) {
      var self = this;
      $node.children('ul.t-group').children('li.t-item').children('div').find('.t-checkbox [type=checkbox]').each(function (index, item) {
        var $checkbox = $(item);
        $checkbox.prop('checked', isChecked);
        self._treeComponent.nodeCheck($checkbox, isChecked, true);
        $checkbox.removeClass(window.CHANGED_FIELD_CLASS_NAME);
        $checkbox = null;
      });
    }, li, isChecked, suppressAutoCheck, autoCheckChildren);
  },

  beforeCustomNodeCheck: function () { },
  afterCustomNodeCheck: function () { },

  _isNodeCollapsed: function ($node) {
    return $node.find(this.NODE_PLUS_SELECTOR).length;
  },

  dispose: function () {
    this._stopDeferredOperations = true;

    Quantumart.QP8.BackendTreeBase.callBaseMethod(this, 'dispose');

    if (this._treeComponent) {
      this._treeComponent.nodeToggle = null;
      this._treeComponent.nodeCheck = null;
      this._treeComponent.element = null;
      this._treeComponent = null;
    }

    var $tree = $(this._treeElement);

    $tree
      .undelegate(this.NODE_NEW_CLICKABLE_SELECTORS, 'click')
      .undelegate(this.NODE_ID_LINK_SELECTORS, 'click')
      .removeData('tTreeView')
      .empty();

    $tree = null;
    this._treeElement = null;
    this._onNodeClickingHandler = null;
    this._onIdClickingHandler = null;

    $q.collectGarbageInIE();
  }
};

$.telerik.treeview.getItemHtml = function (options) {
  var item = options.item,
  html = options.html,
  isFirstLevel = options.isFirstLevel,
  groupLevel = options.groupLevel,
  itemIndex = options.itemIndex,
  itemsCount = options.itemsCount,
  absoluteIndex = new $.telerik.stringBuilder().cat(groupLevel).catIf(':', groupLevel).cat(itemIndex).string();
  html
    .cat('<li class="t-item')
    .catIf(' t-first', isFirstLevel && itemIndex == 0)
    .catIf(' t-last', itemIndex == itemsCount - 1)
    .cat('">')
    .cat('<div class="')
    .catIf('t-top ', isFirstLevel && itemIndex == 0)
    .catIf('t-top', itemIndex != itemsCount - 1 && itemIndex == 0)
    .catIf('t-mid', itemIndex != itemsCount - 1 && itemIndex != 0)
    .catIf('t-bot', itemIndex == itemsCount - 1)
    .cat('">');

  if ((options.isAjax && item.LoadOnDemand) || (item.Items && item.Items.length > 0)) {
 html
    .cat('<span class="t-icon')
    .catIf(' t-plus', item.Expanded !== true)
    .catIf(' t-minus', item.Expanded === true)
    .cat('"></span>'); 
}

  if (options.showCheckBoxes && item.Checkable !== false) {
    var arrayName = options.elementId; // + '_checkedNodes';

    html
      .cat('<span class="t-checkbox" data-array_name="').cat(arrayName).cat('">')
      .cat('<input type="hidden" value="').cat(absoluteIndex)
      .cat('" name="').cat(arrayName).cat('.Index')
      .cat('" class="t-input"/>')
      .cat('<input type="checkbox" value="').cat(item.Checked === true ? 'True' : 'False')
      .cat('" class="t-input')
      .cat('" name="').cat(arrayName).cat('[').cat(absoluteIndex).cat('].Checked"')
      .catIf(' disabled="disabled"', item.Enabled === false)
      .catIf(' checked="checked"', item.Checked)
      .cat('/>');

    if (item.Checked) {
 html.cat($.telerik.treeview.getNodeInputsHtml(item.Value, item.Text, arrayName, absoluteIndex)); 
}
    html.cat('</span>');
  }

  var startLinkFunction = function (html, item) {
    var navigateUrl = item.NavigateUrl || item.Url;

    html
      .cat(navigateUrl ? '<a href="' + navigateUrl + '" class="t-link ' : '<span class="')
      .cat('t-in')
      .catIf(' t-state-selected', item.Selected === true)
      .cat('">');
  };

  var endLinkFunction = function (html, item) {
    var navigateUrl = item.NavigateUrl || item.Url;
    html.cat(navigateUrl ? '</a>' : '</span>');
  };

  startLinkFunction(html, item);

  if (item.ImageUrl != null) {
    html.cat('<img class="t-image" alt="" src="').cat(item.ImageUrl).cat('" />');
  }

  if (item.SpriteCssClasses != null) {
    html.cat('<span class="t-sprite ').cat(item.SpriteCssClasses).cat('"></span>');
  }

  if (options.showIds) {
    if (options.makeLinksFromIds) {
      html.cat('<span class="idLink">(<a class="js" href="javascript:void(0)">' + $q.htmlEncode(item.Value) + '</a>)</span>');
    } else {
      html.cat('<span class="idLink">(' + $q.htmlEncode(item.Value) + ')</span>');
    }

    html.cat(' ');
  }

  html.catIf(item.Text, item.Encoded === false).catIf(item.Text.replace(/</g, '&lt;').replace(/>/g, '&gt;'), item.Encoded !== false);
  endLinkFunction(html, item);

  if (item.Value) {
    html.cat('<input type="hidden" class="t-input" name="itemValue" value="').cat(item.Value).cat('" />');
  }

  html.cat('</div>');

  if (item.Items && item.Items.length > 0) {
    $.telerik.treeview.getGroupHtml({
      data: item.Items,
      html: html,
      isAjax: options.isAjax,
      isFirstLevel: false,
      showCheckBoxes: options.showCheckBoxes,
      groupLevel: absoluteIndex,
      elementId: options.elementId,
      showIds: options.showIds,
      makeLinksFromIds: options.makeLinksFromIds
    });
  }

  html.cat('</li>');
};

$.telerik.treeview.getGroupHtml = function (options) {
  var data = options.data,
      html = options.html,
      showLines = options.showLines,
      isFirstLevel = options.isFirstLevel,
      renderGroup = options.renderGroup;

  if (renderGroup !== false) {
    html.cat('<ul class="t-group')
        .catIf(' t-treeview-lines', isFirstLevel && (typeof showLines == typeof undefined || showLines))
        .cat('"')
        .catIf(' style="display:none"', options.isExpanded !== true)
        .cat('>');
  }

  if (data && data.length > 0) {
    for (var i = 0, len = data.length; i < len; i++) {
      $.telerik.treeview.getItemHtml({
        item: data[i],
        html: html,
        isAjax: options.isAjax,
        isFirstLevel: isFirstLevel,
        showCheckBoxes: options.showCheckBoxes,
        groupLevel: options.groupLevel,
        itemIndex: i,
        itemsCount: len,
        elementId: options.elementId,
        showIds: options.showIds,
        makeLinksFromIds: options.makeLinksFromIds
      });
    }
  }

  if (renderGroup !== false) {
    html.cat('</ul>');
  }
};

Quantumart.QP8.BackendTreeBase.registerClass('Quantumart.QP8.BackendTreeBase', Quantumart.QP8.Observable);

// #endregion
