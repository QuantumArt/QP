/**
 * Creates a predicate function for Array `.filter()` to:
 * Create a duplicate-free version of an array,
 * in which only the first occurrence of each element is kept.
 */
export declare function distinct<T>(): (item: T) => boolean;

/**
 * Creates a predicate function for Array `.filter()` to:
 * Create a duplicate-free version of an array, using value returned by selector function
 * for equality comparisons, in which only the first occurrence of each element is kept.
 */
export declare function distinctBy<T>(
  selector: (item: T) => any
): (item: T) => boolean;

/**
 * Creates a predicate function for Array `.filter()` to:
 * Create a duplicate-free version of an array, using comparer function
 * for equality comparisons, in which only the first occurrence of each element is kept.
 */
export declare function distinctWith<T>(
  comparer: (a: T, b: T) => boolean
): (item: T) => boolean;

/**
 * Creates a predicate function for Array `.filter()` to:
 * Return elements from an array as long as a specified condition is true.
 */
export declare function takeWhile<T>(
  predicate: (item: T, i: number) => boolean
): (item: T, i: number) => boolean;

/**
 * Creates a predicate function for Array `.filter()` to:
 * Bypass elements in a sequence as long as a specified condition is true
 * and then return the remaining elements.
 */
export declare function skipWhile<T>(
  predicate: (item: T, i: number) => boolean
): (item: T, i: number) => boolean;
