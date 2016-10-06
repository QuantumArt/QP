//#region class BackendSilverlightUploader
// === Класс "Silverlight загрузчик" ===
Quantumart.QP8.BackendSilverlightUploader = function (parentElement, options) {
	Quantumart.QP8.BackendSilverlightUploader.initializeBase(this);

	var $parentElement = jQuery(".l-sl-uploader", parentElement);

	this._parentElement = $parentElement.get(0);
	this._parentElementId = $parentElement.attr('id');

	$parentElement = null;

	if (!$q.isNull(options)) {
		if (!$q.isNull(options.background))
			this._background = options.background;
		if (!$q.isNull(options.extensions))
			this._extensions = options.extensions;
		if (!$q.isNull(options.resolveName))
			this._resolveName = options.resolveName;
	}
};

Quantumart.QP8.BackendSilverlightUploader.prototype = {
	_parentElementId: "", // ID элемента контейнера
	_parentElement: null, // DOM-элемент контейнер

	_folderPath: "", // текущий путь к папке
	_background: "#EBF5FB",
	_extensions: "",
	_resolveName: false,

	initialize: function () {
		var $parentElement = jQuery(this._parentElement);
		$parentElement.data('qp_sl_uploader', this);

		this._createFileUploader();

		$parentElement = null;
	},

	dispose: function () {
		var $parentElement = jQuery(this._parentElement);
		$parentElement.removeData();

		$parentElement = null;
		this._parentElement = null;
	},


	_createFileUploader: function () {
		var functionPrefix = "Quantumart$QP8$BackendSilverlightUploader$";
		var uploadUrl = APPLICATION_ROOT_URL + "Upload/";
		var resolveNameFuction = this._resolveName ? functionPrefix + "resolveFileName" : "";
		var params = {
			path: "",
			f_callback: functionPrefix + "uploadCallback",
			f_check: functionPrefix + "checkFileExistence",
			f_resolve: resolveNameFuction,
			f_localize: functionPrefix + "localizeUpload",
			f_path: functionPrefix + "returnFolderPath",
			f_security: functionPrefix + "checkSecurity",
			return_id: this._parentElementId,
			extensions: this._extensions,
			url: uploadUrl + "RadUploadHandler.ashx"
		}

		var objId = this._parentElementId + "_object";

		Silverlight.createObjectEx({
			source: uploadUrl + "SilverlightUpload.xap",
			parentElement: this._parentElement,
			id: objId,
			properties: {
				width: "250",
				height: "20",
				isWindowless: "true",
				background: "transparent",
				alt: "",
				version: "2.0.31005.0"
			},
			events: {
				onError: this._onSilverlightErrorHandler
				//onLoad: this._onSilverlightLoadHandler
			}, //c:\\inetpub\\wwwroot\\qp_demo_nett\\upload
			initParams: $q.hashToString(params),
			context: "context"    // context helper for onLoad handler.
		});
	},


	_onSilverlightErrorHandler: function (sender, args) {
		var appSource = "";
		if (sender != null && sender != 0) {
			appSource = sender.getHost().Source;
		}
		var errorType = args.ErrorType;
		var iErrorCode = args.ErrorCode;

		var errMsg = "Unhandled Error in Silverlight 2 Application " +
				appSource + "\n";

		errMsg += "Code: " + iErrorCode + "    \n";
		errMsg += "Category: " + errorType + "       \n";
		errMsg += "Message: " + args.ErrorMessage + "     \n";

		if (errorType == "ParserError") {
			errMsg += "File: " + args.xamlFile + "     \n";
			errMsg += "Line: " + args.lineNumber + "     \n";
			errMsg += "Position: " + args.charPosition + "     \n";
		}
		else if (errorType == "RuntimeError") {
			if (args.lineNumber != 0) {
				errMsg += "Line: " + args.lineNumber + "     \n";
				errMsg += "Position: " + args.charPosition + "     \n";
			}
			errMsg += "MethodName: " + args.methodName + "     \n";
		}

		//throw new Error(errMsg);
	},


	set_folderPath: function (value) {
		this._folderPath = value;
	},

	get_folderPath: function () {
		return this._folderPath;
	}
};

// Обработчик окончания загрузки файла на сервер
function Quantumart$QP8$BackendSilverlightUploader$uploadCallback(id, fileName) {
	var $element = jQuery('#' + id);
	if ($element) {
		var component = $element.data('qp_sl_uploader');
		if (component) {
			var eventArgs = new Quantumart.QP8.BackendUploaderEventArgs([fileName]);
			component.notify(EVENT_TYPE_LIBRARY_FILE_UPLOADED, eventArgs);
		}
	}

	$element = null;
	component = null;
};

// Проверяет существование файла
function Quantumart$QP8$BackendSilverlightUploader$checkFileExistence(longfileName) {
	var url = APPLICATION_ROOT_URL + "Library/FullNameFileExists/";
	var obj = $q.getJsonSync(url, { name: longfileName });

	return obj.result;
};

// Разрешает конфликт имен файлов
function Quantumart$QP8$BackendSilverlightUploader$resolveFileName(path, fileName) {
	var url = APPLICATION_ROOT_URL + "Library/ResolveFileName/";
	var obj = $q.getJsonSync(url, { path: path, name: fileName });

	return obj.result;
};

// Проверяет папку на security
function Quantumart$QP8$BackendSilverlightUploader$checkSecurity(path) {
	var url = APPLICATION_ROOT_URL + "Library/CheckSecurity/";
	var obj = $q.getJsonSync(url, { path: path });
	return obj.result;
};


function Quantumart$QP8$BackendSilverlightUploader$returnFolderPath(id) {
	var $element = jQuery('#' + id);
	var result = "";
	if ($element) {
		var component = $element.data('qp_sl_uploader');
		if (component) {
			result = component.get_folderPath();
		}
	}

	$element = null;
	component = null;
	return result;
};

// Локализует Silverlight-uploader
function Quantumart$QP8$BackendSilverlightUploader$localizeUpload() {
	var result = {
		"UploadBrowse": UPLOAD_BROWSE_BUTTON_NAME,
		"UploadTotal": UPLOAD_TOTAL_LABEL,
		"ExtensionMessage": UPLOAD_EXTENSION_MESSAGE,
		"OverwriteMessage": UPLOAD_OVERWRITE_MESSAGE,
		"MaxSizeMessage": UPLOAD_MAX_SIZE_MESSAGE,
		"SecurityMessage": UPLOAD_SECURITY_MESSAGE
	};

	return result;
};

Quantumart.QP8.BackendSilverlightUploader.registerClass("Quantumart.QP8.BackendSilverlightUploader", Quantumart.QP8.Observable, Quantumart.QP8.IBackendUploader);
//#endregion
