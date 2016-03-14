/*
Copyright (c) 2003-2011, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

/**
 * @file Special Character plugin
 */

CKEDITOR.plugins.add('qspecchar',
{
	// List of available localizations.
	//availableLangs : { en:1 },	

	init: function (editor) {
		var pluginName = 'qspecchar',
			plugin = this;

		// Register the dialog.
		CKEDITOR.dialog.add(pluginName, this.path + 'dialogs/qspecchar.js');		

		editor.addCommand(pluginName,
			{
				exec: function () {
					editor.openDialog(pluginName);
				},
				modes: { wysiwyg: 1 },
				canUndo: false
			});

		// Register the toolbar button.
		editor.ui.addButton('QSpecChar',
			{
				label: editor.lang.qspecchar.toolbar,
				command: pluginName,
				icon: CKEDITOR.plugins.getPath('qspecchar') + "/images/qspecchar.gif"
			});
	}
});

	CKEDITOR.config.qspecchar = (function () {
		var SpecialChar = function (entity, decimal) {
			this.entity = entity;
			this.decimal = decimal;
		}		

		return {
			chars: [
						new SpecialChar("&nbsp;", "&#160;"),
						new SpecialChar("&amp;", "&#38;"),
						new SpecialChar("&quot;", "&#34;"),
						new SpecialChar("&cent;", "&#162;"),
						new SpecialChar("&euro;", "&#8364;"),
						new SpecialChar("&pound;", "&#163;"),
						new SpecialChar("&yen;", "&#165;"),
						new SpecialChar("&copy;", "&#169;"),
						new SpecialChar("&reg;", "&#174;"),
						new SpecialChar("&trade;", "&#8482;"),
						new SpecialChar("&permil;", "&#8240;"),
						new SpecialChar("&micro;", "&#181;"),
						new SpecialChar("&middot;", "&#183;"),
						new SpecialChar("&bull;", "&#8226;"),
						new SpecialChar("&hellip;", "&#8230;"),
						new SpecialChar("&prime;", "&#8242;"),
						new SpecialChar("&Prime;", "&#8243;"),
						new SpecialChar("&sect;", "&#167;"),
						new SpecialChar("&para;", "&#182;"),
						new SpecialChar("&szlig;", "&#223;"),
						new SpecialChar("&lsaquo;", "&#8249;"),
						new SpecialChar("&rsaquo;", "&#8250;"),
						new SpecialChar("&laquo;", "&#171;"),
						new SpecialChar("&raquo;", "&#187;"),
						new SpecialChar("&lsquo;", "&#8216;"),
						new SpecialChar("&rsquo;", "&#8217;"),
						new SpecialChar("&ldquo;", "&#8220;"),
						new SpecialChar("&rdquo;", "&#8221;"),
						new SpecialChar("&sbquo;", "&#8218;"),
						new SpecialChar("&bdquo;", "&#8222;"),
						new SpecialChar("&lt;", "&#60;"),
						new SpecialChar("&gt;", "&#62;"),
						new SpecialChar("&le;", "&#8804;"),
						new SpecialChar("&ge;", "&#8805;"),
						new SpecialChar("&ndash;", "&#8211;"),
						new SpecialChar("&mdash;", "&#8212;"),
						new SpecialChar("&macr;", "&#175;"),
						new SpecialChar("&oline;", "&#8254;"),
						new SpecialChar("&curren;", "&#164;"),
						new SpecialChar("&brvbar;", "&#166;"),
						new SpecialChar("&uml;", "&#168;"),
						new SpecialChar("&iexcl;", "&#161;"),
						new SpecialChar("&iquest;", "&#191;"),
						new SpecialChar("&circ;", "&#710;"),
						new SpecialChar("&tilde;", "&#732;"),
						new SpecialChar("&deg;", "&#176;"),
						new SpecialChar("&minus;", "&#8722;"),
						new SpecialChar("&plusmn;", "&#177;"),
						new SpecialChar("&times;", "&#215;"),
						new SpecialChar("&divide;", "&#247;"),
						new SpecialChar("&frasl;", "&#8260;"),
						new SpecialChar("&sup1;", "&#185;"),
						new SpecialChar("&sup2;", "&#178;"),
						new SpecialChar("&sup3;", "&#179;"),
						new SpecialChar("&frac14;", "&#188;"),
						new SpecialChar("&frac12;", "&#189;"),
						new SpecialChar("&frac34;", "&#190;"),
						new SpecialChar("&fnof;", "&#402;"),
						new SpecialChar("&int;", "&#8747;"),
						new SpecialChar("&sum;", "&#8721;"),
						new SpecialChar("&infin;", "&#8734"),
						new SpecialChar("&radic;", "&#8730;"),
						new SpecialChar("&asymp;", "&#8776;"),
						new SpecialChar("&ne;", "&#8800;"),
						new SpecialChar("&equiv;", "&#8801;"),
						new SpecialChar("&prod;", "&#8719;"),
						new SpecialChar("&not;", "&#172;"),
						new SpecialChar("&cap;", "&#8745;"),
						new SpecialChar("&part;", "&#8706;"),
						new SpecialChar("&acute;", "&#180;"),
						new SpecialChar("&cedil;", "&#184;"),
						new SpecialChar("&ordf;", "&#170;"),
						new SpecialChar("&ordm;", "&#186;"),
						new SpecialChar("&dagger;", "&#8224;"),
						new SpecialChar("&Dagger;", "&#8225;"),
						new SpecialChar("&Agrave;", "&#192;"),
						new SpecialChar("&Aacute;", "&#193;"),
						new SpecialChar("&Acirc;", "&#194;"),
						new SpecialChar("&Atilde;", "&#195;"),
						new SpecialChar("&Auml;", "&#196;"),
						new SpecialChar("&Aring;", "&#197;"),
						new SpecialChar("&AElig;", "&#198;"),
						new SpecialChar("&Ccedil;", "&#199;"),
						new SpecialChar("&Egrave;", "&#200;"),
						new SpecialChar("&Eacute;", "&#201;"),
						new SpecialChar("&Ecirc;", "&#202;"),
						new SpecialChar("&Euml;", "&#203;"),
						new SpecialChar("&Igrave;", "&#204;"),
						new SpecialChar("&Iacute;", "&#205;"),
						new SpecialChar("&Icirc;", "&#206;"),
						new SpecialChar("&Iuml;", "&#207;"),
						new SpecialChar("&ETH;", "&#208;"),
						new SpecialChar("&Ntilde;", "&#209;"),
						new SpecialChar("&Ograve;", "&#210;"),
						new SpecialChar("&Oacute;", "&#211;"),
						new SpecialChar("&Ocirc;", "&#212;"),
						new SpecialChar("&Otilde;", "&#213;"),
						new SpecialChar("&Ouml;", "&#214;"),
						new SpecialChar("&Oslash;", "&#216;"),
						new SpecialChar("&OElig;", "&#338;"),
						new SpecialChar("&Scaron;", "&#352;"),
						new SpecialChar("&Ugrave;", "&#217;"),
						new SpecialChar("&Uacute;", "&#218;"),
						new SpecialChar("&Ucirc;", "&#219;"),
						new SpecialChar("&Uuml;", "&#220;"),
						new SpecialChar("&Yacute;", "&#221;"),
						new SpecialChar("&Yuml;", "&#376;"),
						new SpecialChar("&THORN;", "&#222;"),
						new SpecialChar("&agrave;", "&#224;"),
						new SpecialChar("&aacute;", "&#225;"),
						new SpecialChar("&acirc;", "&#226;"),
						new SpecialChar("&atilde;", "&#227;"),
						new SpecialChar("&auml;", "&#228;"),
						new SpecialChar("&aring;", "&#229;"),
						new SpecialChar("&aelig;", "&#230;"),
						new SpecialChar("&ccedil;", "&#231;"),
						new SpecialChar("&egrave;", "&#232;"),
						new SpecialChar("&eacute;", "&#233;"),
						new SpecialChar("&ecirc;", "&#234;"),
						new SpecialChar("&euml;", "&#235;"),
						new SpecialChar("&igrave", "&#236;"),
						new SpecialChar("&iacute;", "&#237;"),
						new SpecialChar("&icirc;", "&#238;"),
						new SpecialChar("&iuml;", "&#239;"),
						new SpecialChar("&eth;", "&#240;"),
						new SpecialChar("&ntilde;", "&#241;"),
						new SpecialChar("&ograve;", "&#242;"),
						new SpecialChar("&oacute;", "&#243;"),
						new SpecialChar("&ocirc;", "&#244;"),
						new SpecialChar("&otilde;", "&#245;"),
						new SpecialChar("&ouml;", "&#246;"),
						new SpecialChar("&oslash;", "&#248;"),
						new SpecialChar("&ugrave;", "&#249;"),
						new SpecialChar("&uacute;", "&#250;"),
						new SpecialChar("&ucirc;", "&#251;"),
						new SpecialChar("&uuml;", "&#252;"),
						new SpecialChar("&yacute;", "&#253;"),
						new SpecialChar("&thorn;", "&#254;"),
						new SpecialChar("&yuml;", "&#255;"),
						new SpecialChar("&Alpha;", "&#913;"),
						new SpecialChar("&Beta;", "&#914;"),
						new SpecialChar("&Gamma;", "&#915;"),
						new SpecialChar("&Delta;", "&#916;"),
						new SpecialChar("&Epsilon;", "&#917;"),
						new SpecialChar("&Zeta;", "&#918;"),
						new SpecialChar("&Eta;", "&#919;"),
						new SpecialChar("&Theta;", "&#920;"),
						new SpecialChar("&Iota;", "&#921;"),
						new SpecialChar("&Kappa;", "&#922;"),
						new SpecialChar("&Lambda;", "&#923;"),
						new SpecialChar("&Mu;", "&#924;"),
						new SpecialChar("&Nu;", "&#925;"),
						new SpecialChar("&Xi;", "&#926;"),
						new SpecialChar("&Omicron;", "&#927;"),
						new SpecialChar("&Pi;", "&#928;"),
						new SpecialChar("&Rho;", "&#929;"),
						new SpecialChar("&Sigma;", "&#931;"),
						new SpecialChar("&Tau;", "&#932;"),
						new SpecialChar("&Upsilon;", "&#933;"),
						new SpecialChar("&Phi;", "&#934;"),
						new SpecialChar("&Chi;", "&#935;"),
						new SpecialChar("&Psi;", "&#936;"),
						new SpecialChar("&Omega;", "&#937;"),
						new SpecialChar("&alpha;", "&#945;"),
						new SpecialChar("&beta;", "&#946;"),
						new SpecialChar("&gamma;", "&#947;"),
						new SpecialChar("&delta;", "&#948;"),
						new SpecialChar("&epsilon;", "&#949;"),
						new SpecialChar("&zeta;", "&#950;"),
						new SpecialChar("&eta;", "&#951;"),
						new SpecialChar("&theta;", "&#952;"),
						new SpecialChar("&iota;", "&#953;"),
						new SpecialChar("&kappa;", "&#954;"),
						new SpecialChar("&lambda;", "&#955;"),
						new SpecialChar("&mu;", "&#956;"),
						new SpecialChar("&nu;", "&#957;"),
						new SpecialChar("&xi;", "&#958;"),
						new SpecialChar("&omicron;", "&#959;"),
						new SpecialChar("&pi;", "&#960;"),
						new SpecialChar("&rho;", "&#961;"),
						new SpecialChar("&sigmaf;", "&#962;"),
						new SpecialChar("&sigma;", "&#963;"),
						new SpecialChar("&tau;", "&#964;"),
						new SpecialChar("&upsilon;", "&#965;"),
						new SpecialChar("&phi;", "&#966;"),
						new SpecialChar("&chi;", "&#967;"),
						new SpecialChar("&psi;", "&#968;"),
						new SpecialChar("&omega;", "&#969;"),
						new SpecialChar("&larr;", "&#8592;"),
						new SpecialChar("&uarr;", "&#8593;"),
						new SpecialChar("&rarr;", "&#8594;"),
						new SpecialChar("&darr;", "&#8595;"),
						new SpecialChar("&harr;", "&#8596;"),
						new SpecialChar("&loz;", "&#9674;"),
						new SpecialChar("&spades;", "&#9824;"),
						new SpecialChar("&clubs;", "&#9827;"),
						new SpecialChar("&hearts;", "&#9829;"),
						new SpecialChar("&diams;", "&#9830;"),
		                new SpecialChar("&rArr;", "&#8658;"),
		                new SpecialChar("&hArr;", "&#8660;")
					]
		}
	})();

/**
  * The list of special characters visible in Special Character dialog.
  * @type Array
  * @example
  * config.specialChars = [ '&quot;', '&rsquo;', [ '&custom;', 'Custom label' ] ];
  * config.specialChars = config.specialChars.concat( [ '&quot;', [ '&rsquo;', 'Custom label' ] ] );
  */
//	CKEDITOR.config.qspecchar =
//	[
//		'!','&quot;','#','$','%','&amp;',"'",'(',')','*','+','-','.','/',
//		'0','1','2','3','4','5','6','7','8','9',':',';',
//		'&lt;','=','&gt;','?','@',
//		'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O',
//		'P','Q','R','S','T','U','V','W','X','Y','Z',
//		'[',']','^','_','`',
//		'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p',
//		'q','r','s','t','u','v','w','x','y','z',
//		'{','|','}','~',
//		"&euro;", "&lsquo;", "&rsquo;", "&ldquo;", "&rdquo;", "&ndash;", "&mdash;", "&iexcl;", "&cent;", "&pound;", "&curren;", "&yen;", "&brvbar;", "&sect;", "&uml;", "&copy;", "&ordf;", "&laquo;", "&not;", "&reg;", "&macr;", "&deg;", "&", "&sup2;", "&sup3;", "&acute;", "&micro;", "&para;", "&middot;", "&cedil;", "&sup1;", "&ordm;", "&", "&frac14;", "&frac12;", "&frac34;", "&iquest;", "&Agrave;", "&Aacute;", "&Acirc;", "&Atilde;", "&Auml;", "&Aring;", "&AElig;", "&Ccedil;", "&Egrave;", "&Eacute;", "&Ecirc;", "&Euml;", "&Igrave;", "&Iacute;", "&Icirc;", "&Iuml;", "&ETH;", "&Ntilde;", "&Ograve;", "&Oacute;", "&Ocirc;", "&Otilde;", "&Ouml;", "&times;", "&Oslash;", "&Ugrave;", "&Uacute;", "&Ucirc;", "&Uuml;", "&Yacute;", "&THORN;", "&szlig;", "&agrave;", "&aacute;", "&acirc;", "&atilde;", "&auml;", "&aring;", "&aelig;", "&ccedil;", "&egrave;", "&eacute;", "&ecirc;", "&euml;", "&igrave;", "&iacute;", "&icirc;", "&iuml;", "&eth;", "&ntilde;", "&ograve;", "&oacute;", "&ocirc;", "&otilde;", "&ouml;", "&divide;", "&oslash;", "&ugrave;", "&uacute;", "&ucirc;", "&uuml;", "&uuml;", "&yacute;", "&thorn;", "&yuml;", "&OElig;", "&oelig;", "&#372;", "&#374", "&#373", "&#375;", "&sbquo;", "&#8219;", "&bdquo;", "&hellip;", "&trade;", "&#9658;", "&bull;", "&rarr;", "&rArr;", "&hArr;", "&diams;", "&asymp;"
//	];
