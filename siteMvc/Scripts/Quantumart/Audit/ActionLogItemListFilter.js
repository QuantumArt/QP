// #region class ActionLogItemListFilter
Quantumart.QP8.ActionLogItemListFilter = function (filterContainer, items) {
	Quantumart.QP8.ActionLogItemListFilter.initializeBase(this, [filterContainer]);
	this._items = items;
};

Quantumart.QP8.ActionLogItemListFilter.prototype = {

	_items: null,

	initialize: function () {
		var html = new $.telerik.stringBuilder();

		html
		.cat('<div class="row">')
			.cat('<select class="dropDownList">');

		for (var i = 0; i < this._items.length; i++) {
			var item = this._items[i];
			html.cat('<option value="').cat(item.value).cat('">')
				.cat(item.text)
			.cat('</option>');
		}

		html
			.cat('</select>')
		.cat('</div>');

		this.$container.append(html.string());

		this.$container.find("select.dropDownList").focus();
	},

	onOpen: function () {
		this.$container.find("select.dropDownList").focus();
	},

	get_value: function () {
		return this.$container.find("select.dropDownList option:selected").val();
	},

	get_filterDetails: function () {
		return this.$container.find("select.dropDownList option:selected").text();
	}
};

Quantumart.QP8.ActionLogItemListFilter.registerClass("Quantumart.QP8.ActionLogItemListFilter", Quantumart.QP8.ActionLogFilterBase);
// #endregion
