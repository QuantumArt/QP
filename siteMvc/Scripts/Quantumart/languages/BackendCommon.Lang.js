if (typeof Backend == 'undefined') {
	Backend = {};
}

if (typeof Backend.Lang == 'undefined') {
	Backend.Lang = {};
}

Backend.Lang.Common = {
	ajaxGenericErrorMessage: "An error {0} has occurred!",
	ajaxDataReceivingErrorMessage: "When data recieving error occurred!",
	ajaxUserSessionExpiredErrorMessage: "Your user session has expired!\nNeed to be re-authenticated.",
	error500Title: "An error occurred!",
	error500Text: "<p>On requested page encountered an error.<br />\nWe apologize for any inconvenience.</p>",
	error404Title: "Page not found!",
	error404Text: "<p>Requested page does not exist.<br />\nPerhaps you made a mistake when writing url address or page has been deleted.</p>",
	eventTypeNotSpecified: "You do not specify event type!",
	eventArgsNotSpecified: "You do not specify arguments of event \"{0}\"!",
	observerIsNotFunctionOrObject: "Attaching observer is not an object or function!",
	observerIsNotImplementedInterface: "Attaching observer does not support interface Quantumart.QP8.IObserver!",
	firstComponentInMediatorNotSpecified: "You do not specify first component!",
	secondComponentInMediatorNotSpecified: "You do not specify second component!",
	parentDomElementNotSpecified: "You do not specify parent DOM-element!",
	unknownDateTimePickerMode: "You specify unknown mode of AnyTime plugin!",
	methodNotImplemented: "Method not implemented!",
	andUnion: "And",
	actionNotSpecified: "You do not specify action-object!",
	targetEventArgsNotSpecified: "You do not specify target-event arguments!",
	sourceEventArgsNotSpecified: "You do not specify source-event arguments!",
	sourceEventArgsIvalidType: "Source-event arguments has an invalid type!",
	untitledTabText: "Untitled Tab",
	untitledWindowTitle: "Untitled Window",
    nextTitleForSettings: "Next step"
};

window.$l = Backend.Lang;
