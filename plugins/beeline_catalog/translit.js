Quantumart.QP8.BackendDocumentContext.prototype.notifyCustomButtonExistence = false;

Quantumart.QP8.BackendDocumentContext.setGlobal('Transliterator',
      {
      symbols: {
      'а': 'a',
      'б': 'b',
      'в': 'v',
      'г': 'g',
      'д': 'd',
      'е': 'e',
      'ё': 'e',
      'ж': 'zh',
      'з': 'z',
      'и': 'i',
      'й': 'y',
      'к': 'k',
      'л': 'l',
      'м': 'm',
      'н': 'n',
      'о': 'o',
      'п': 'p',
      'р': 'r',
      'с': 's',
      'т': 't',
      'у': 'u',
      'ф': 'f',
      'х': 'h',
      'ц': 'c',
      'ч': 'ch',
      'ш': 'sh',
      'щ': 'shh',
      'ы': 'i',
      'э': 'e',
      'ю': 'yu',
      'я': 'ya',
      ' ': '-'
      },

      dashCode: "-".charCodeAt(0),
      enLettersStartCode: "a".charCodeAt(0),
      enLettersEndCode: "z".charCodeAt(0),
      digitsStartCode: "0".charCodeAt(0),
      digitsEndCode: "9".charCodeAt(0),

      transliterate: function (input) {

      input = input.toLowerCase();

      var result = "";

      for (var i = 0; i < input.length; i++) {

      var currentSymbol = input[i];

      var symbolToReplace = this.symbols[currentSymbol];

      if (symbolToReplace != undefined) {
      result += symbolToReplace;
      }
      else {
      var currentSymbolCode = currentSymbol.charCodeAt(0);

      if (currentSymbolCode == this.dashCode
      || currentSymbolCode >= this.enLettersStartCode && currentSymbolCode <= this.enLettersEndCode
      || currentSymbolCode >= this.digitsStartCode && currentSymbolCode <= this.digitsEndCode)
      result += currentSymbol;
      }
      }

      result = result.replace(/-+/g, "-");

      if (result.length > 0 && result[0] == "-")
      result = result.substr(1);

      if (result.length > 0 && result[result.length - 1] == "-")
      result = result.substr(0, result.length - 1);

      return result;
      }
      });

Quantumart.QP8.BackendDocumentContext.prototype.addTransliterateButton = function (srcInputName, dstInputName)
{
    var self = this;
    this.addCustomLinkButton({
      name: dstInputName,
      title: "Transliterate",
      suffix: "translit",
      "class": "customLinkButton",
      url: "/Backend/Content/QP8/icons/16x16/insert_call.gif",
      onClick: function (evt) {
		var resultInput = evt.data.$input;
		var textToProcess = self.getValue(evt.data.$form, srcInputName);
		resultInput.val($ctx.getGlobal('Transliterator').transliterate(textToProcess)).trigger("change");
      }
    });
};
