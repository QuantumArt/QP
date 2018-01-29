/**
 * Creates a reducer function for Array `.reduce()` that:
 * Flattens array a single level deep.
 */
export declare function flatten<T>(): (result: T[], item: T[]) => T[];

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Groups the elements of an array according to a specified key selector function.
 * The elements of each group are projected by using a specified value selector function.
 */
export declare function groupBy<T, K, U = T>(
  keySelector: (item: T) => K,
  valueSelector?: (item: T) => U
): (result: Grouping<K, U>[], item: T) => Grouping<K, U>[];

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Creates an object composed of keys generated from the results of running
 * each element of array thru key selector function.
 * The order of grouped values is determined by the order they occur in collection.
 * The corresponding value of each key is an array of elements responsible for generating the key.
 * The elements of each group are projected by using a specified value selector function.
 */
export declare function toLookup<T, U = T>(
  keySelector: (item: T) => number,
  valueSelector?: (item: T) => U
): (result: NumberMap<U[]>, item: T) => NumberMap<U[]>;
export declare function toLookup<T, U = T>(
  keySelector: (item: T) => any,
  valueSelector?: (item: T) => U
): (result: StringMap<U[]>, item: T) => StringMap<U[]>;

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Creates an object composed of keys generated from the results of running
 * each element of collection thru key selector function.
 * The corresponding value of each key is the last element responsible for generating the key.
 * The values of an object are projected by using a specified value selector function.
 */
export declare function toDictionary<T, U = T>(
  keySelector: (item: T) => number,
  valueSelector?: (item: T) => U
): (result: NumberMap<U[]>, item: T) => NumberMap<U[]>;
export declare function toDictionary<T, U = T>(
  keySelector: (item: T) => any,
  valueSelector?: (item: T) => U
): (result: StringMap<U>, item: T) => StringMap<U>;

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Finds element of the array that having max value of specified selector function.
 */
export declare function maxBy<T>(
  selector: (item: T) => any
): (result: T, item: T) => T;

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Finds element of the array that having min value of specified selector function.
 */
export declare function minBy<T>(
  selector: (item: T) => any
): (result: T, item: T) => T;

/**
 * An Array with extra `key` property.
 * Represents a collection of objects that have a common key.
 */
interface Grouping<K, T> extends Array<T> {
  readonly key: K;
}

/** Represents a dictinoary with `number` keys. */
interface NumberMap<T> {
  [key: number]: T;
}

/** Represents a dictinoary with `string` keys. */
interface StringMap<T> {
  [key: string]: T;
}
