(function () {
	var pluginName = 'removeEmptyP';

	var clearEmpty = function (element) {
	    var child = element.children[0];
	    var firstOrLastInDocument = (!element.previous || !element.next) && element.parent.parent == null;
	    var hasOnlyChildWithEmptyOrDefaultValue = child && (!child.children || child.children.length == 0) && !child.previous && !child.next
        && (child.type == 1 && child.name == 'br' || child.type == 3 && CKEDITOR.tools.trim(child.value).match(/^(?:&nbsp;|\xa0)$/));
	    var hasUsefulAttributes = element.attributes["id"] || element.attributes["class"] || element.attributes["style"];
	    if (firstOrLastInDocument && hasOnlyChildWithEmptyOrDefaultValue && !hasUsefulAttributes)
	        return false;
	    else
	        return element;
	};

	var removeEmptyPFilter = {
		elements: {
			p: clearEmpty,
			div: clearEmpty
		}
	};

	CKEDITOR.plugins.add(pluginName,
	{
		init: function (editor) {
			// Give the filters lower priority makes it get applied after the default ones.
			editor.dataProcessor.htmlFilter.addRules(removeEmptyPFilter, 100);
			editor.dataProcessor.dataFilter.addRules(removeEmptyPFilter, 100);
		}
	});
})();