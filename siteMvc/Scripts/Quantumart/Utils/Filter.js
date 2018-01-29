/* eslint-disable arrow-parens, no-plusplus, no-return-assign, valid-jsdoc */
/**
 * Creates a predicate function for Array `.filter()` to:
 * Create a duplicate-free version of an array,
 * in which only the first occurrence of each element is kept.
 */
export const distinct = () => {
  // @ts-ignore
  if (typeof Set === 'undefined') {
    const keys = [];
    return (item) => {
      if (keys.lastIndexOf(item) !== -1) {
        return false;
      }
      keys.push(item);
      return true;
    };
  }
  // @ts-ignore
  const keySet = new Set();
  return (item) => {
    if (keySet.has(item)) {
      return false;
    }
    keySet.add(item);
    return true;
  };
};

/**
 * Creates a predicate function for Array `.filter()` to:
 * Create a duplicate-free version of an array, using value returned by selector function
 * for equality comparisons, in which only the first occurrence of each element is kept.
 */
export const distinctBy = (selector) => {
  // @ts-ignore
  if (typeof Set === 'undefined') {
    const keys = [];
    return (item) => {
      const key = selector(item);
      if (keys.lastIndexOf(key) !== -1) {
        return false;
      }
      keys.push(key);
      return true;
    };
  }
  // @ts-ignore
  const keySet = new Set();
  return (item) => {
    const key = selector(item);
    if (keySet.has(key)) {
      return false;
    }
    keySet.add(key);
    return true;
  };
};

/**
 * Creates a predicate function for Array `.filter()` to:
 * Create a duplicate-free version of an array, using comparer function
 * for equality comparisons, in which only the first occurrence of each element is kept.
 */
export const distinctWith = (comparer) => {
  const keys = [];
  return (item) => {
    let i = keys.length;
    while (i--) {
      if (comparer(keys[i], item)) {
        return false;
      }
    }
    keys.push(item);
    return true;
  };
};

/**
 * Creates a predicate function for Array `.filter()` to:
 * Exclude all given values from an array.
 */
export const without = (values) => {
  // @ts-ignore
  if (typeof Set === 'undefined') {
    return (item) => values.indexOf(item) === -1;
  }
  // @ts-ignore
  const set = new Set(values);
  return (item) => !set.has(item);
};

/**
 * Creates a predicate function for Array `.filter()` to:
 * Return elements from an array as long as a specified condition is true.
 */
export const takeWhile = (predicate) => {
  let shouldTake = true;
  return (item, i) => shouldTake && (shouldTake = predicate(item, i));
};

/**
 * Creates a predicate function for Array `.filter()` to:
 * Bypass elements in a sequence as long as a specified condition is true
 * and then return the remaining elements.
 */
export const skipWhile = (predicate) => {
  let shouldTake = false;
  return (item, i) => shouldTake || (shouldTake = !predicate(item, i));
};
