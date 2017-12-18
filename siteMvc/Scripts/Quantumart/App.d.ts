// Quantumart
type Quantumart = {
  QP8?: {
    [ClassName: string]: any;
    Constants?: Constants;
    Enums?: Enums;
  };
};

type Constants = {
  [CONSTANT: string]: string | number;
}

type Enums = {
  [EnumName: string]: {
    [MemberName: string]: number;
  };
}

type Backend = {
  Lang?: Lang;
}

type Lang = {
  [ModuleName: string]: {
    [name: string]: string;
  };
}

declare var $e: Enums;
declare var $l: Lang;
declare var Backend: Backend;
declare var Quantumart: Quantumart;

interface Window {
  [CONSTANT: string]: any;
  $a: any;
  $c: any;
  $e: Enums;
  $l: Lang;
  $o: any;
  $q: any;
  $ctx: any;
  Backend: Backend;
  Quantumart: Quantumart;
}

// Utils
interface ArrayConstructor {
  distinct<T>(array: T[]): T[];
}

interface String {
  left(strLength: number): string;
  right(strLength: number): string;
}

// Vendors
interface JQuery {
  jeegoocontext: any;
  size(): number;
}

interface JQueryStatic {
  browser: any;
  telerik: any;

  css(elem: any, name: any, extra?: any): any;
  nodeName(elem: Element, name: string): boolean;
}

interface SignalR {
  [name: string]: any;
}

declare var pmrpc: any;

// Internet Explorer
declare function escape(input: string): string;

interface Window {
  CollectGarbage(): void;
}

interface EventTarget {
  attachEvent(eventNameWithOn: any, callback: any): any;
  detachEvent(eventNameWithOn: any, callback: any): any;
}

// Suppressed Warnings
interface Element {
  // TODO: fix jQuery.fn.each() usage
  [name: string]: any;
}

// TODO: Set polyfill
declare var Set: any;

interface ObjectConstructor {
  // TODO: Object.values polyfill
  values(object: object): any[];
  // TODO: Object.entries polyfill
  entries(object: object): [string, any][];
}
