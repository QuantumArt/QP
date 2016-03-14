Quantumart.QP8.BackendDocumentContext.prototype.initHandler = function (editor, $elem) {
    editor._hideFields = jQuery.isArray(editor._hideFields) ? editor._hideFields.concat(this.fieldsToHide) : this.fieldsToHide;
};

