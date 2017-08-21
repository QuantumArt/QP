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
  _treeElementId: '',
  _treeElementName: '',
  _treeElement: null,
  _treeContainerElementId: '',
  _stopDeferredOperations: false,
  _treeComponent: null,
  _deferredNodeCodeToHighlight: '',
  _showIds: false,
  _makeLinksFromIds: true,
  _autoCheckChildren: false,
  _readActionCode: '',
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

  // eslint-disable-next-line camelcase
  get_treeElementId() {
    return this._treeElementId;
  },

  // eslint-disable-next-line camelcase
  set_treeElementId(value) {
    this._treeElementId = value;
  },

  // eslint-disable-next-line camelcase
  get_treeElement() {
    return this._treeElement;
  },

  // eslint-disable-next-line camelcase
  get_treeContainerElementId() {
    return this._treeContainerElementId;
  },

  // eslint-disable-next-line camelcase
  set_treeContainerElementId(value) {
    this._treeContainerElementId = value;
  },

  _onDataBindingHandler: null,
  _onNodeClickingHandler: null,
  _onNodeCheckboxClickHandler: null,

  initialize() {
    const $tree = $(`#${this._treeElementId}`);
    const treeComponent = $tree.data('tTreeView');
    treeComponent.isAjax = this.isAjax;
    treeComponent.ajaxRequest = this._onDataBindingHandler;

    this._initNodeCheck(treeComponent);
    this._initNewToggle(treeComponent);

    this._treeElement = $tree.get(0);
    this._treeComponent = treeComponent;

    $tree
      .unbind('select')
      .undelegate(this.NODE_OLD_CLICKABLE_SELECTORS, 'click')
      .delegate(this.NODE_NEW_CLICKABLE_SELECTORS, 'click', this._onNodeClickingHandler)
      .delegate(this.NODE_ID_LINK_SELECTORS, 'click', this._onIdClickingHandler);
  },

  _initNodeCheck(treeComponent) {
    Object.assign(treeComponent, {
      nodeCheckExcludeSelf: this._proceedAutoCheckDirectChildren.bind(this),
      nodeCheck: (li, isChecked, suppressAutoCheck, autoCheckChildren) => {
        this._legacyNodeCheck.call(treeComponent, li, isChecked);
        this.beforeCustomNodeCheck(li, isChecked, suppressAutoCheck, autoCheckChildren);
        this._proceedAutoCheckAllChildren(li, isChecked, suppressAutoCheck, autoCheckChildren);
        this.afterCustomNodeCheck(li, isChecked, suppressAutoCheck, autoCheckChildren);
      }
    });
  },

  _initNewToggle(treeComponent) {
    const { nodeToggle: oldToggle } = treeComponent;
    Object.assign(treeComponent, {
      nodeToggle: (...args) => {
        oldToggle.call(treeComponent, args[0], args[1], true);
      }
    });
  },

  isAjax() {
    return true;
  },

  getChildNodesContainer(node) {
    return $('> UL.t-group', this.getNode(node));
  },

  getAllNodes() {
    return $(this._treeElement).find('LI.t-item');
  },

  getSelectedNodes() {
    return this.getAllNodes().filter(`:has(> DIV > SPAN.${this.NODE_SELECTED_CLASS_NAME})`);
  },

  getNode(node, parentNodeElem) {
    if ($q.isObject(node)) {
      return $q.toJQuery(node);
    } else if ($q.isString(node) || $.isNumeric(node)) {
      let $parentNode = $(parentNodeElem);
      if ($q.isNullOrEmpty($parentNode)) {
        $parentNode = $(`#${this._treeElementId}`);
      }

      let $node = null;
      if (node === this.ROOT_NODE_CODE) {
        $node = $(`#${this._treeElementId}`);
      } else {
        $node = $(`LI DIV INPUT:hidden[value='${node}']`, $parentNode).parent().parent().filter('.t-item');
        if ($node.length === 0) {
          $node = null;
        }
      }

      return $node;
    }

    return undefined;
  },

  getNodeValue(node) {
    const $node = this.getNode(node);
    let nodeValue = '';

    if ($node) {
      if ($node.hasClass(this.ROOT_NODE_CLASS_NAME)) {
        nodeValue = this.ROOT_NODE_CODE;
      } else {
        nodeValue = this._treeComponent.getItemValue($node);
      }
    }

    return nodeValue;
  },

  getNodeText(node) {
    const $node = this.getNode(node);
    let nodeText = '';

    if ($node) {
      nodeText = this._treeComponent.getItemText($node);
    }

    return nodeText;
  },

  getParentNode(node) {
    const $node = this.getNode(node);
    let $parentNode = null;
    if ($node) {
      $parentNode = $node.parent().parent();
      if ($parentNode.length === 0 || !$parentNode.hasClass('t-item')) {
        return null;
      }
    }

    return $parentNode;
  },

  refreshNode(node, options) {
    const loadChildNodes = options && !$q.isNull(options.loadChildNodes) ? options.loadChildNodes : true;
    const callback = options && options.callback ? options.callback : null;
    const $node = this.getNode(node);
    if ($node) {
      if (this.getChildNodeCount($node) > 0) {
        this.collapseNode($node);
      }

      this._refreshNodeInner($node, loadChildNodes, callback);
    }
  },

  _refreshNodeInner() {
    $q.alertFail($l.Common.methodNotImplemented);
  },

  refreshNodes(nodes, options) {
    const that = this;
    const $nodes = $q.toJQuery(nodes);

    if (!$q.isNullOrEmpty($nodes)) {
      $nodes.each((index, nodeElem) => {
        const $node = $(nodeElem);
        that.refreshNode($node, options);
      });
    }
  },

  _renderNode(node, dataItem, isRootNode) {
    const $node = this.getNode(node);
    const nodeIndex = this.getCheckedNodeIndex(node);
    let nodeHtml = new $.telerik.stringBuilder();

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

    let $newNode = $(nodeHtml.string());
    const cssClassNames = $newNode.attr('class');
    const htmlContent = $newNode.html();

    nodeHtml = null;
    $newNode = null;

    $node.attr('class', cssClassNames).html(htmlContent);
  },

  _renderChildNodes(parentNode, dataItems, isRootNode) {
    const $parentNode = this.getNode(parentNode);
    let $group = this._getChildNodesContainer($parentNode);

    const isGroup = $q.isNullOrEmpty($group);
    const groupHtml = new $.telerik.stringBuilder();

    const parentNodeIndex = this.getCheckedNodeIndex(parentNode);

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

    if (!$q.isNullOrEmpty($group) && !$parentNode.data('loaded')) {
      $(groupHtml.string()).prependTo($group);
    } else if (!$q.isNullOrEmpty($group) && $parentNode.data('loaded')) {
      $group.empty().html(groupHtml.string());
    } else if ($q.isNullOrEmpty($group)) {
      $group = $(groupHtml.string()).appendTo($parentNode);
    }

    $group.show();
    $group = null;
  },

  _isSearchQueryEmpty() {
    return true;
  },

  refreshTree() {
    const maxExpandLevel = this._isSearchQueryEmpty() ? 1 : 0;
    $('ul.t-group', this._treeElement).remove();
    this.addNodesToParentNode(this._treeElement, maxExpandLevel);
  },

  _onNodeClicking(e) {
    if (!this._treeComponent.shouldNavigate($(e.currentTarget))) {
      e.preventDefault();
      return false;
    }

    return undefined;
  },

  _onIdClicking(e) {
    e.preventDefault();
    e.stopPropagation();
    if (this._readActionCode) {
      this.executeAction(
        $(e.currentTarget).closest('.t-item').first(),
        this._readActionCode,
        {
          ctrlKey: e.ctrlKey,
          shiftKey: e.shiftKey
        }
      );
    }
  },

  expandNode(node) {
    const $node = this.getNode(node);
    if ($node) {
      this._treeComponent.expand($node);
    }
  },

  collapseNode(node) {
    const $node = this.getNode(node);
    if ($node) {
      this._treeComponent.collapse($node);
    }
  },

  removeNodeOrRefreshParent(node, parentNode, options) {
    const $node = this.getNode(node);
    if ($node) {
      if ($node.siblings().length > 0) {
        this.removeNode($node);
      } else {
        this.refreshNode(parentNode, options);
      }
    }
  },

  removeNode(node) {
    const $node = this.getNode(node);

    if ($node) {
      $node.hide(100, () => {
        $node.removeData().empty().remove();
      });
    }
  },

  removeChildNodes(node) {
    const $node = this.getNode(node);
    const $group = this._getChildNodesContainer($node);

    if (!$q.isNullOrEmpty($group)) {
      $group.empty();
    }
  },

  isRootNode(node) {
    const $node = this.getNode(node);
    let isRootNode = false;

    if ($node) {
      isRootNode = $node.hasClass(this.ROOT_NODE_CLASS_NAME);
    }

    return isRootNode;
  },

  getNodeLevel(node) {
    const $node = this.getNode(node);
    let level = -1;

    if ($node) {
      if (this.isRootNode($node)) {
        level = 0;
      } else {
        let $parentNode = $node.parent();
        level = 1;

        while (!$q.isNullOrEmpty($parentNode) && !this.isRootNode($node)) {
          const parentNodeElem = $parentNode.get(0);
          if (parentNodeElem && parentNodeElem.tagName === 'LI') {
            level += 1;
          }

          $parentNode = $parentNode.parent();
        }
      }
    }

    return level;
  },

  getCheckedNodeIndex(node) {
    const $node = this.getNode(node);
    let nodeIndex;

    if (this._treeComponent.showCheckBox) {
      nodeIndex = $(`.t-checkbox input[type='hidden'][name='${this._treeElementName}.Index']`, $node).val();
    }
    return nodeIndex;
  },

  getChildNodeCount(node) {
    const $node = this.getNode(node);
    let childNodeCount = 0;

    if ($node) {
      childNodeCount = $node.find('> UL.t-group > LI').length;
    }

    return childNodeCount;
  },

  _getChildNodesContainer(node) {
    return this.getNode(node).find('> UL.t-group');
  },

  _showAjaxLoadingIndicatorForNode(node) {
    const that = this;
    const $node = this.getNode(node);
    $node.data('loading_icon_timeout', setTimeout(() => {
      if (that._stopDeferredOperations) {
        clearTimeout($node.data('loading_icon_timeout'));
        return;
      }

      $node.find('> DIV > SPAN.t-icon').addClass('t-loading');
    }, 100));
  },

  _hideAjaxLoadingIndicatorForNode(node) {
    const $node = this.getNode(node);
    clearTimeout($node.data('loading_icon_timeout'));
    if ($node.hasClass('t-item')) {
      $node.data('loaded', true)
        .find('SPAN.t-icon:first')
        .removeClass('t-loading')
        .removeClass('t-plus')
        .addClass('t-minus');
    }
  },

  executeAction() {
    $q.alertFail($l.Common.methodNotImplemented);
  },

  _legacyNodeCheck(li, isChecked) {
    $(li, this.element).each($.proxy(function (index, item) {
      const $item = $(item).closest('.t-item');
      const $checkboxHolder = $('> div > .t-checkbox', $item);
      const arrayName = $checkboxHolder.data('array_name');
      const newIndex = $checkboxHolder.find(`:input[name="${arrayName}.Index"]`).val();

      $checkboxHolder.find(`:input[name="${arrayName}[${newIndex}].Text"]`).remove();
      $checkboxHolder.find(`:input[name="${arrayName}[${newIndex}].Value"]`).remove();
      $checkboxHolder.find(':checkbox').attr({
        checked: !!isChecked,
        value: isChecked
      });

      if (isChecked) {
        $($.telerik.treeview.getNodeInputsHtml(
          this.getItemValue($item),
          this.getItemText($item), arrayName, newIndex)
        ).appendTo($checkboxHolder);
      }
    }, this));
  },

  _proceedAutoCheckChildren(nodeSelectorFn, li, isChecked, suppressAutoCheck, autoCheckChildren) {
    if (!suppressAutoCheck && (autoCheckChildren || this._autoCheckChildren)) {
      const $node = $(li).closest('.t-item').first();

      if (this._isNodeCollapsed($node)) {
        this._treeComponent.nodeToggle(null, $node, true);
      }

      nodeSelectorFn.call(this, $node);
    }
  },

  _proceedAutoCheckAllChildren(li, isChecked, suppressAutoCheck, autoCheckChildren) {
    return this._proceedAutoCheckChildren(function ($node) {
      const that = this;
      $node.find('ul.t-group .t-checkbox [type=checkbox]').each((index, item) => {
        const $checkbox = $(item);
        $checkbox.prop('checked', isChecked);
        that._treeComponent.nodeCheck($checkbox, isChecked, true);
        $checkbox.removeClass(window.CHANGED_FIELD_CLASS_NAME);
      });
    }, li, isChecked, suppressAutoCheck, autoCheckChildren);
  },

  _proceedAutoCheckDirectChildren(li, isChecked, suppressAutoCheck, autoCheckChildren) {
    return this._proceedAutoCheckChildren(function ($node) {
      const that = this;
      $node
        .children('ul.t-group')
        .children('li.t-item')
        .children('div')
        .find('.t-checkbox [type=checkbox]')
        .each((index, item) => {
          let $checkbox = $(item);
          $checkbox.prop('checked', isChecked);
          that._treeComponent.nodeCheck($checkbox, isChecked, true);
          $checkbox.removeClass(window.CHANGED_FIELD_CLASS_NAME);
          $checkbox = null;
        });
    }, li, isChecked, suppressAutoCheck, autoCheckChildren);
  },

  beforeCustomNodeCheck() {
    // TODO: empty fn
  },

  afterCustomNodeCheck() {
    // TODO: empty fn
  },

  _isNodeCollapsed($node) {
    return $node.find(this.NODE_PLUS_SELECTOR).length;
  },

  dispose() {
    this._stopDeferredOperations = true;
    Quantumart.QP8.BackendTreeBase.callBaseMethod(this, 'dispose');

    if (this._treeComponent) {
      this._treeComponent.nodeToggle = null;
      this._treeComponent.nodeCheck = null;
      this._treeComponent.element = null;
      this._treeComponent = null;
    }

    $(this._treeElement)
      .undelegate(this.NODE_NEW_CLICKABLE_SELECTORS, 'click')
      .undelegate(this.NODE_ID_LINK_SELECTORS, 'click')
      .removeData('tTreeView')
      .empty();

    this._treeElement = null;
    this._onNodeClickingHandler = null;
    this._onIdClickingHandler = null;
    $q.collectGarbageInIE();
  }
};

$.telerik.treeview.getItemHtml = ({
  item,
  html,
  isFirstLevel,
  groupLevel,
  itemIndex,
  itemsCount,
  isAjax,
  showCheckBoxes,
  elementId,
  makeLinksFromIds,
  showIds
}) => {
  const absoluteIndex = new $.telerik.stringBuilder()
    .cat(groupLevel)
    .catIf(':', groupLevel)
    .cat(itemIndex)
    .string();

  html
    .cat('<li class="t-item')
    .catIf(' t-first', isFirstLevel && itemIndex === 0)
    .catIf(' t-last', itemIndex === itemsCount - 1)
    .cat('">')
    .cat('<div class="')
    .catIf('t-top ', isFirstLevel && itemIndex === 0)
    .catIf('t-top', itemIndex !== itemsCount - 1 && itemIndex === 0)
    .catIf('t-mid', itemIndex !== itemsCount - 1 && itemIndex !== 0)
    .catIf('t-bot', itemIndex === itemsCount - 1)
    .cat('">');

  if ((isAjax && item.LoadOnDemand) || (item.Items && item.Items.length > 0)) {
    html
      .cat('<span class="t-icon')
      .catIf(' t-plus', !item.Expanded)
      .catIf(' t-minus', item.Expanded)
      .cat('"></span>');
  }

  if (showCheckBoxes && item.Checkable !== false) {
    const arrayName = elementId;

    html
      .cat('<span class="t-checkbox" data-array_name="')
      .cat(arrayName)
      .cat('">')
      .cat('<input type="hidden" value="')
      .cat(absoluteIndex)
      .cat('" name="')
      .cat(arrayName)
      .cat('.Index')
      .cat('" class="t-input"/>')
      .cat('<input type="checkbox" value="')
      .cat(item.Checked ? 'True' : 'False')
      .cat('" class="t-input')
      .cat('" name="')
      .cat(arrayName)
      .cat('[')
      .cat(absoluteIndex)
      .cat('].Checked"')
      .catIf(' disabled="disabled"', !item.Enabled)
      .catIf(' checked="checked"', item.Checked)
      .cat('/>');

    if (item.Checked) {
      html.cat($.telerik.treeview.getNodeInputsHtml(item.Value, item.Text, arrayName, absoluteIndex));
    }

    html.cat('</span>');
  }

  const startLinkFunction = function (fnHtml, fnItem) {
    const navigateUrl = fnItem.NavigateUrl || fnItem.Url;
    fnHtml
      .cat(navigateUrl ? `<a href="${navigateUrl}" class="t-link ` : '<span class="')
      .cat('t-in')
      .catIf(' t-state-selected', fnItem.Selected)
      .cat('">');
  };

  const endLinkFunction = function (fnHtml, fnItem) {
    const navigateUrl = fnItem.NavigateUrl || fnItem.Url;
    fnHtml.cat(navigateUrl ? '</a>' : '</span>');
  };

  startLinkFunction(html, item);

  if (item.ImageUrl) {
    html.cat('<img class="t-image" alt="" src="').cat(item.ImageUrl).cat('" />');
  }

  if (item.SpriteCssClasses) {
    html.cat('<span class="t-sprite ').cat(item.SpriteCssClasses).cat('"></span>');
  }

  if (showIds) {
    if (makeLinksFromIds) {
      html.cat(
        `<span class="idLink">(<a class="js" href="javascript:void(0)">${$q.htmlEncode(item.Value)}</a>)</span>`
      );
    } else {
      html.cat(`<span class="idLink">(${$q.htmlEncode(item.Value)})</span>`);
    }

    html.cat(' ');
  }

  html.catIf(item.Text, !item.Encoded).catIf(item.Text.replace(/</g, '&lt;').replace(/>/g, '&gt;'), item.Encoded);
  endLinkFunction(html, item);

  if (item.Value) {
    html.cat('<input type="hidden" class="t-input" name="itemValue" value="').cat(item.Value).cat('" />');
  }

  html.cat('</div>');

  if (item.Items && item.Items.length > 0) {
    $.telerik.treeview.getGroupHtml({
      data: item.Items,
      html,
      isAjax,
      isFirstLevel: false,
      showCheckBoxes,
      groupLevel: absoluteIndex,
      elementId,
      showIds,
      makeLinksFromIds
    });
  }

  html.cat('</li>');
};

$.telerik.treeview.getGroupHtml = ({
  showLines,
  html,
  isFirstLevel,
  groupLevel,
  renderGroup,
  isExpanded,
  isAjax,
  showCheckBoxes,
  elementId,
  makeLinksFromIds,
  showIds,
  data
}) => {
  if (renderGroup) {
    html.cat('<ul class="t-group')
      .catIf(' t-treeview-lines', isFirstLevel && (typeof showLines === typeof undefined || showLines))
      .cat('"')
      .catIf(' style="display:none"', !isExpanded)
      .cat('>');
  }

  if (data && data.length > 0) {
    for (let i = 0; i < data.length; i++) {
      $.telerik.treeview.getItemHtml({
        item: data[i],
        html,
        isAjax,
        isFirstLevel,
        showCheckBoxes,
        groupLevel,
        itemIndex: i,
        itemsCount: data.length,
        elementId,
        showIds,
        makeLinksFromIds
      });
    }
  }

  if (renderGroup) {
    html.cat('</ul>');
  }
};

Quantumart.QP8.BackendTreeBase.registerClass('Quantumart.QP8.BackendTreeBase', Quantumart.QP8.Observable);
