window.Backend = window.Backend || {};
window.Backend.Lang = window.Backend.Lang || {};
window.Backend.Lang.Common = {
  ajaxGenericErrorMessage: 'Произошла ошибка {0}!',
  ajaxDataReceivingErrorMessage: 'При получении данных произошла ошибка!',
  ajaxUserSessionExpiredErrorMessage: 'Ваша пользовательская сессия истекла!\nПройдите повторную аутентификацию.',
  error500Title: 'Произошла ошибка!',
  error500Text: '<p>На запрашиваемой Вами странице произошла ошибка.<br />'
  + '\nПриносим свои извинения за причиненные неудобства.</p>',
  error404Title: 'Страница не найдена!',
  error404Text: '<p>Запрашиваемая Вами страница не существует.<br />'
  + '\nВозможно, Вы ошиблись при написании адреса или страница была удалена.</p>',
  eventTypeNotSpecified: 'Вы не задали тип события!',
  eventArgsNotSpecified: 'Вы не задали аргументы события "{0}"!',
  observerIsNotFunctionOrObject: 'Привязываемый наблюдатель не является объектом или функцией!',
  observerIsNotImplementedInterface: 'Привязываемый наблюдатель не поддерживает интерфейс Quantumart.QP8.IObserver!',
  firstComponentInMediatorNotSpecified: 'Вы не задали первый компонент!',
  secondComponentInMediatorNotSpecified: 'Вы не задали второй компонент!',
  parentDomElementNotSpecified: 'Вы не задали родительский DOM-элемент!',
  unknownDateTimePickerMode: 'Вы задали неизвестный режим работы плагина AnyTime!',
  methodNotImplemented: 'Метод не реализован!',
  andUnion: 'И',
  actionNotSpecified: 'Вы не задали объект-действие!',
  targetEventArgsNotSpecified: 'Вы не задали аргументы целевого события!',
  sourceEventArgsNotSpecified: 'Вы не задали аргументы события-источника!',
  sourceEventArgsIvalidType: 'Аргументы события-источника имеют недопустимый тип!',
  untitledTabText: 'Безымянный таб',
  untitledWindowTitle: 'Безымянное окно',
  nextTitleForSettings: 'Далее'
};

window.$l = window.Backend.Lang;
