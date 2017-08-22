// //////////////////////////////////////////////////
// controlWindow object
// //////////////////////////////////////////////////
window.controlWindow = function (controlForm) {
    // private properties
  this._form = controlForm;

  // public properties
  this.windowType = 'controlWindow';
  this.noSuggestionSelection = '- No suggestions -';

  // set up the properties for elements of the given control form
  this.suggestionList = this._form.sugg;
  this.evaluatedText = this._form.misword;
  this.replacementText = this._form.txtsugg;
  this.undoButton = this._form.btnUndo;
};

window.controlWindow.prototype = {
  resetForm: function () {
    if (this._form) {
      this._form.reset();
    }
  },

  setSuggestedText: function () {
    var slct = this.suggestionList;
    var txt = this.replacementText;
    var str = '';

    if ((slct.options[0].text) && slct.options[0].text != this.noSuggestionSelection) {
      str = slct.options[slct.selectedIndex].text;
    }

    txt.value = str;
  },

  selectDefaultSuggestion: function () {
    var slct = this.suggestionList;
    var txt = this.replacementText;

    if (slct.options.length == 0) {
      this.addSuggestion(this.noSuggestionSelection);
    } else {
      slct.options[0].selected = true;
    }

    this.setSuggestedText();
  },

  addSuggestion: function (sugg_text) {
    var slct = this.suggestionList;

    if (sugg_text) {
      var i = slct.options.length;
      var newOption = new Option(sugg_text, 'sugg_text' + i);

      slct.options[i] = newOption;
    }
  },

  clearSuggestions: function () {
    var slct = this.suggestionList;

    for (var j = slct.length - 1; j > -1; j--) {
      if (slct.options[j]) {
        slct.options[j] = null;
      }
    }
  },

  enableUndo: function () {
    if (this.undoButton) {
      if (this.undoButton.disabled == true) {
        this.undoButton.disabled = false;
      }
    }
  },

  disableUndo: function () {
    if (this.undoButton) {
      if (this.undoButton.disabled == false) {
        this.undoButton.disabled = true;
      }
    }
  }
};
