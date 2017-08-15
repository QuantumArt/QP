Quantumart.QP8.BackendExpandedContainer = function () { };
Quantumart.QP8.BackendExpandedContainer.prototype = {
  $content: null,

  initialize: function (element) {
    this.$content = $q.toJQuery(element);
    if (this.$content.data(Quantumart.QP8.BackendExpandedContainer.DATA_ATTRIBUTE_KEY)) {
      return;
    }

    this.$content.data(Quantumart.QP8.BackendExpandedContainer.DATA_ATTRIBUTE_KEY, this);

    const html = new $.telerik.stringBuilder();
    html
      .cat('<div class="qp-expandedContainer-component">')
      .cat('<div class="qp-expandedContainer-toolBar">')
      .cat('<ul class="linkButtons group">')
      .cat('<li style="display: none;" class="expand">')
      .cat('<span class="linkButton actionLink">')
      .cat('<a href="javascript:void(0);">')
      .cat('<span class="icon expand"><img src="/Backend/Content/Common/0.gif"></span>')
      .cat('<span class="text">')
      .cat($q.toString(this.$content.data('expand_text'), $l.ExpandedContainer.expandText))
      .cat('</span>')
      .cat('</a>')
      .cat('</span>')
      .cat('</li>')
      .cat('<li style="" class="collapse">')
      .cat('<span class="linkButton actionLink">')
      .cat('<a href="javascript:void(0);">')
      .cat('<span class="icon collapse"><img src="/Backend/Content/Common/0.gif"></span>')
      .cat('<span class="text">')
      .cat($q.toString(this.$content.data('collapse_text'), $l.ExpandedContainer.collapseText))
      .cat('</span>')
      .cat('</a>')
      .cat('</span>')
      .cat('</li>')
      .cat('</ul>')
      .cat('</div>')
      .cat('</div>');

    let $component = jQuery(html.string());
    this.$content.before($component);
    $component.append(this.$content);

    const that = this;
    this._$expandLink = $component
      .find('LI.expand')
      .click(function (e) {
        jQuery(this).hide();
        that._$collapseLink.show();
        that.$content.show();
        e.preventDefault();
      }).hide();

    this._$collapseLink = $component.find('LI.collapse').click(function (e) {
      jQuery(this).hide();
      that._$expandLink.show();
      that.$content.hide();
      e.preventDefault();
    }).show();

    if ($q.toBoolean(this.$content.data('is_expanded', false))) {
      this._$expandLink.trigger('click');
    } else {
      this._$collapseLink.trigger('click');
    }

    $component = null;
  },

  dispose: function () {
    if (this.$content) {
      this.$content.removeData(Quantumart.QP8.BackendExpandedContainer.DATA_ATTRIBUTE_KEY);
      this.$content = null;
    }

    if (this._$expandLink) {
      this._$expandLink.off();
      this._$expandLink = null;
    }

    if (this._$collapseLink) {
      this._$collapseLink.off();
      this._$collapseLink = null;
    }
  }
};

Quantumart.QP8.BackendExpandedContainer.initAll = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  Quantumart.QP8.BackendExpandedContainer.getAllElements(parentElement).each(function () {
    new Quantumart.QP8.BackendExpandedContainer().initialize(this);
  });
};

Quantumart.QP8.BackendExpandedContainer.getAllElements = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  return $q.toJQuery(parentElement).find('.qp-expandedContent');
};

Quantumart.QP8.BackendExpandedContainer.destroyAll = function (parentElement) {
  if (!parentElement) {
    throw new Error($l.Common.parentDomElementNotSpecified);
  }

  Quantumart.QP8.BackendExpandedContainer.getAllElements(parentElement).each(function () {
    let component = $q.toJQuery(this).data(Quantumart.QP8.BackendExpandedContainer.DATA_ATTRIBUTE_KEY);
    if (component) {
      component.dispose();
    }

    component = null;
  });
};

Quantumart.QP8.BackendExpandedContainer.DATA_ATTRIBUTE_KEY = 'QP8_ExpandedContainer';
Quantumart.QP8.BackendExpandedContainer.registerClass('Quantumart.QP8.BackendExpandedContainer');
