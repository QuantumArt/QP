/**
 * Creates a comparsion function for Array `.sort()` that:
 * Compares values returned by selector function in ascending order.
 */
export declare function asc<T>(
  selector?: (item: T) => any
): (left: T, right: T) => number;

/**
 * Creates a comparsion function for Array `.sort()` that:
 * Compares values returned by selector function in descending order.
 */
export declare function desc<T>(
  selector?: (item: T) => any
): (left: T, right: T) => number;

/**
 * Creates a comparsion function for Array `.sort()` that:
 * Combines multiple other comparsion functions.
 */
export declare function by<T>(
  ...comparers: ((left: T, right: T) => number)[]
): (left: T, right: T) => number;
