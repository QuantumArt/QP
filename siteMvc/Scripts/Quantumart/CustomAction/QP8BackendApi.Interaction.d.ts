/**
 * Проверка, что веб-приложение выполняется внутри бекэнда.
 * @param hostUID Уникальный идентификатор текущей вкладки ГПИ.
 * Генерируется бекэндом, передаётся в пользовательское действие в виде одноимённого параметра `QueryString`.
 * @param destination Окно, содержащее основное приложение бекэнда.
 * Обычно нужно передавать window.parent.
 * @param callback Функция, в которую возвращается результат проверки.
 */
export declare function checkHost(
  hostUID: string,
  destination: Window,
  callback: (
    arg: {
      /** результат проверки  */
      success: boolean;
      /** версия бекэнда в случае успешной проверки */
      hostVersion?: string;
      /** текст ошибки в случае неуспешной проверки */
      error?: string;
    }
  ) => void
): void;

/**
 * Закрытие открытого ранее (с помощью `executeBackendAction`) интерфейсного действия.
 * @param actionUID Уникальный идентификатор действия, ранее заданный веб-приложением.
 * @param hostUID Уникальный идентификатор текущей вкладки ГПИ.
 * Генерируется бекэндом, передаётся в пользовательское действие в виде одноимённого параметра `QueryString`.
 * @param destination Окно, содержащее основное приложение бекэнда.
 * Обычно нужно передавать window.parent.
 */
export declare function closeBackendHost(
  actionUID: string,
  hostUID: string,
  destination: Window
): void;

/**
 * Выполнение действия в бекэнде. Поддерживаются как интерфейсные, так и неинтерфейсные действия.
 * Также есть возможность вызова пользовательских и многошаговых действий.
 * @param executeOtions Экземпляр `ExecuteActionOptions` (или объект с аналогичной структурой).
 * @param hostUID Уникальный идентификатор текущей вкладки ГПИ.
 * Генерируется бекэндом, передаётся в пользовательское действие в виде одноимённого параметра `QueryString`.
 * @param destination Окно, содержащее основное приложение бекэнда.
 * Обычно нужно передавать window.parent.
 */
export declare function executeBackendAction(
  executeOtions: ExecuteActionOptions,
  hostUID: string,
  destination: Window
): void;

/** Парамеры сообщения на выполнение BackendAction */
export declare class ExecuteActionOptions {
  /** Код вызываемого действия */
  actionCode: string;
  /** Код типа сущности, к которому относится вызываемое действие */
  entityTypeCode: string;
  /** Идентификатор родительской сущности */
  parentEntityId: number;
  /** Идентификатор сущности */
  entityId: number;
  /**
   * Уникальный идентификатор действия. Задаётся веб-приложением.
   * Использовать в случае, когда осуществляется управление несколькими интерфейсными действиями
   * и в дальнейшем, получая от них callback, требуется возможность различать их.
   */
  actionUID?: string;
  /** Ссылка на метод-обработчик в веб-приложении. Обычно определяется через `BackendEventObserver` */
  callerCallback?: string;
  /**
   * Указатель, выполнять ли интерфейсное действие со сменой содержимого текущей вкладки ГПИ бекэнда.
   * @default false (или открывать новую вкладку).
   */
  changeCurrentTab?: boolean;
  /**
   * Указатель, выполнять ли интерфейсное действие в окне бекэнда.
   * @default false (открывать во вкладке).
   */
  isWindow?: boolean;
  /** Экземпляр `ArticleFormState` (или объект с аналогичной структурой) */
  options?: ArticleFormState;
}

/** Параметры для инициализации формы статьи */
export declare class ArticleFormState {
  /** значения для инициализации полей */
  initFieldValues?: InitFieldValue[];
  /** идентификаторы полей который должны быть disable (массив имен полей) */
  disabledFields?: string[];
  /** идентификаторы полей, которые должны быть скрыты (массив имён полей) */
  hideFields?: string[];
  /** массив Action Code, для которых кнопки в ГПИ бекэнда должны быть скрыты */
  disabledActionCodes?: string[];
  /**
   * дополнительные параметры для выполнения пользовательского действия,
   * которые должны быть в него переданы через QueryString
   */
  additionalParams?: any;
  /** Значение поля */
  static InitFieldValue: typeof InitFieldValue;
}

/** Значение поля */
declare class InitFieldValue {
  /** имя поля */
  fieldName: string;
  /** значение (зависит от типа) */
  value: any;
}

/**
 * Открытие всплывающего окна для выбора значения с последующим возвратом результата выбора в веб-приложение.
 * @param openSelectWindowOptions
 * @param hostUID Уникальный идентификатор текущей вкладки ГПИ.
 * Генерируется бекэндом, передаётся в пользовательское действие в виде одноимённого параметра `QueryString`.
 * @param destination Окно, содержащее основное приложение бекэнда.
 * Обычно нужно передавать window.parent.
 */
export declare function openSelectWindow(
  openSelectWindowOptions: OpenSelectWindowOptions,
  hostUID: string,
  destination: Window
): void;

/** Парамеры сообщения на открытие окна выбора из списка */
export declare class OpenSelectWindowOptions {
  /** Код вызываемого действия выбора */
  selectActionCode: string;
  /** Код типа сущности, к которой относится вызываемое действие */
  entityTypeCode: string;
  /** Идентификатор родительской сущности */
  parentEntityId: number;
  /**
   * Указатель, является ли действие выбора множественным
   * @default false
   */
  isMultiple?: boolean;
  /** ID сущностей, выбранные заранее  */
  selectedEntityIDs?: number[];
  /** ID для идентификации окна со списком. Задаётся веб-приложением */
  selectWindowUID?: string;
  /** Ссылка на метод-обработчик в веб-приложении. Обычно определяется через `BackendEventObserver` */
  callerCallback?: string;
  /** Дополнительные опции */
  options?: {
    [key: string]: any;
    /** SQL для фильтрации списка статей (обычно используется Field.RelationCondition) */
    filter?: string;
  };
}

/**
 * Открытие всплывающего окна библиотеки файлов с последующим возвратом результата выбора в веб-приложение.
 * @param openFileLibraryOptions
 * @param hostUID Уникальный идентификатор текущей вкладки ГПИ.
 * Генерируется бекэндом, передаётся в пользовательское действие в виде одноимённого параметра `QueryString`.
 * @param destination Окно, содержащее основное приложение бекэнда.
 * Обычно нужно передавать window.parent.
 */
export declare function openFileLibrary(
  openFileLibraryOptions: OpenFileLibraryOptions,
  hostUID: string,
  destination: Window
): void;

/** Параметры сообщения на открытие окна библиотеки файлов */
export declare class OpenFileLibraryOptions {
  /** Фильтровать по типу файла: только изображения @default false */
  isImage?: boolean;
  /** Использовать библиотеку сайта или библиотеку контента @default false */
  useSiteLibrary?: boolean;
  /** Подкаталой в каталоге библтотеки */
  subFolder?: string;
  /** Id контента (для библиотеки контента) или Id сайта (для библиотеки сайта) */
  libraryEntityId: number;
  /** Id сайта (для библиотеки контента) или 0 (для библиотеки сайта) */
  libraryParentEntityId: number;
  /** ID для идентификации окна со списком. Задаётся веб-приложением */
  selectWindowUID?: string;
  /** Ссылка на метод-обработчик в веб-приложении. Обычно определяется через `BackendEventObserver` */
  callerCallback?: string;
}

/**
 * Предпросмотр изображения, содержащегося в поле статьи
 * @param previewImageOptions
 * @param hostUID Уникальный идентификатор текущей вкладки ГПИ.
 * Генерируется бекэндом, передаётся в пользовательское действие в виде одноимённого параметра `QueryString`.
 * @param destination Окно, содержащее основное приложение бекэнда.
 * Обычно нужно передавать window.parent.
 */
export declare function previewImage(
  previewImageOptions: PreviewImageOptions,
  hostUID: string,
  destination: Window
): void;

/** Параметры просмотра изображения */
export declare class PreviewImageOptions {
  /** Идентификатор сущности */
  entityId: number;
  /** Идентификатор поля */
  fieldId: number;
  /** Имя файла */
  fileName: string;
}

/**
 * Скачать файл, содержащийся в поле статьи
 * @param downloadFileOptions
 * @param hostUID Уникальный идентификатор текущей вкладки ГПИ.
 * Генерируется бекэндом, передаётся в пользовательское действие в виде одноимённого параметра `QueryString`.
 * @param destination Окно, содержащее основное приложение бекэнда.
 * Обычно нужно передавать window.parent.
 */
export declare function downloadFile(
  downloadFileOptions: DownloadFileOptions,
  hostUID: string,
  destination: Window
): void;

/** Параметры скачивания файла */
export declare class DownloadFileOptions {
  /** Идентификатор сущности */
  entityId: number;
  /** Идентификатор поля */
  fieldId: number;
  /** Имя файла */
  fileName: string;
}

/** Observer сообщений от хоста */
export declare class BackendEventObserver {
  /**
   * @param callbackProcName регистрирует callback c именем callbackProcName в библиотеке PMRPC
   * @param callback результат взаимодействия с бекэндом
   */
  constructor(
    callbackProcName: string,
    callback: (
      eventType: typeof BackendEventTypes[keyof typeof BackendEventTypes],
      args: HostUnbindedArgs &
        ActionExecutedArgs &
        EntitiesSelectedArgs &
        SelectWindowClosedArgs &
        FileSelectedArgs
    ) => void
  );

  dispose(): void;

  /** Типы событий backend'а */
  static EventType: typeof BackendEventTypes;

  static HostUnbindingReason: typeof HostUnbindingReason;
}

/** Типы событий backend'а */
export declare const BackendEventTypes: {
  /** вызванный хост был отсоединён */
  HostUnbinded: 1;
  /** вызванное действие было выполнено */
  ActionExecuted: 2;
  /** сущности были выбраны */
  EntitiesSelected: 3;
  /** окно выбора было закрыто */
  SelectWindowClosed: 4;
  /** файл был выбран */
  FileSelected: 5;
};

export interface HostUnbindedArgs {
  /** причина отсоединения */
  reason?: typeof HostUnbindingReason[keyof typeof HostUnbindingReason];
}

/** причина отсоединения */
declare const HostUnbindingReason: {
  Closed: "closed";
  Changed: "changed";
};

export interface ActionExecutedArgs {
  actionCode?: string;
  actionTypeCode?: string;
  entityTypeCode: string;
  parentEntityId?: number;
  entityId?: string;
  isMultipleAction?: boolean;
}

export interface EntitiesSelectedArgs {
  /** идентификатор окна, в котором произошёл выбор */
  selectWindowUID?: string;
  /** массив идентификаторов выбранных сущностей */
  selectedEntityIDs?: number[];
}

export interface SelectWindowClosedArgs {
  /** идентификатор окна, в котором произошёл выбор */
  selectWindowUID?: string;
}

export interface FileSelectedArgs {
  /** имя выбранного файла */
  filePath?: string;
}

/** Типы сообщений backend'у */
export declare const ExternalMessageTypes: {
  ExecuteAction: 1;
  CloseBackendHost: 2;
  OpenSelectWindow: 3;
  CheckHost: 4;
  PreviewImage: 5;
  DownloadFile: 6;
  OpenFileLibrary: 7;
};

declare var Quantumart: {
  QP8: {
    Interaction: {
      checkHost: typeof checkHost;
      closeBackendHost: typeof closeBackendHost;
      executeBackendAction: typeof executeBackendAction;
      openSelectWindow: typeof openSelectWindow;
      previewImage: typeof previewImage;
      downloadFile: typeof downloadFile;
      openFileLibrary: typeof openFileLibrary;
      ExecuteActionOptions: typeof ExecuteActionOptions;
      ArticleFormState: typeof ArticleFormState;
      OpenSelectWindowOptions: typeof OpenSelectWindowOptions;
      BackendEventObserver: typeof BackendEventObserver;
      ExternalMessageTypes: typeof ExternalMessageTypes;
      BackendEventTypes: typeof BackendEventTypes;
    };
  };
};
