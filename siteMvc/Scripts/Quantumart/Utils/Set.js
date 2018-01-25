/**
 * Determines if given arrays contains the same set of elements.
 * @template T
 * @param {T[]} first
 * @param {T[]} second
 * @returns {boolean}
 */
export const setEquals = function (first, second) {
  // @ts-ignore
  if (typeof Set === 'undefined') {
    return first.every(el => second.indexOf(el) !== -1)
      && second.every(el => first.indexOf(el) !== -1);
  }
  // @ts-ignore
  const firstSet = new Set(first);
  // @ts-ignore
  const secondSet = new Set(second);
  return firstSet.length === secondSet.length
    && first.every(el => secondSet.has(el))
    && second.every(el => firstSet.has(el));
};

/**
 * Creates an array of unique array values included in the other given array.
 * @template T
 * @param {T[]} first
 * @param {T[]} second
 * @returns {T[]}
 */
export const setIntersection = function (first, second) {
  const result = [];
  // @ts-ignore
  if (typeof Set === 'undefined') {
    first.forEach(el => {
      if (result.lastIndexOf(el) === -1 && second.indexOf(el) !== -1) {
        result.push(el);
      }
    });
  } else {
    // @ts-ignore
    const firstSet = new Set(first);
    // @ts-ignore
    const secondSet = new Set(second);
    firstSet.forEach(el => {
      if (secondSet.has(el)) {
        result.push(el);
      }
    });
  }
  return result;
};

/**
 * Creates an array of unique array values not included in the other given array.
 * @template T
 * @param {T[]} first
 * @param {T[]} second
 * @returns {T[]}
 */
export const setDifference = function (first, second) {
  const result = [];
  // @ts-ignore
  if (typeof Set === 'undefined') {
    first.forEach(el => {
      if (result.lastIndexOf(el) === -1 && second.indexOf(el) === -1) {
        result.push(el);
      }
    });
  } else {
    // @ts-ignore
    const firstSet = new Set(first);
    // @ts-ignore
    const secondSet = new Set(second);
    firstSet.forEach(el => {
      if (!secondSet.has(el)) {
        result.push(el);
      }
    });
  }
  return result;
};

/**
 * Creates an array of unique values from two given arrays.
 * @template T
 * @param {T[]} first
 * @param {T[]} second
 * @returns {T[]}
 */
export const setUnion = function (first, second) {
  // @ts-ignore
  if (typeof Set === 'undefined') {
    const result = [];
    first.forEach(el => {
      if (result.lastIndexOf(el) === -1) {
        result.push(el);
      }
    });
    second.forEach(el => {
      if (result.lastIndexOf(el) === -1) {
        result.push(el);
      }
    });
    return result;
  }
  // @ts-ignore
  const resultSet = new Set(first);
  second.forEach(el => {
    resultSet.add(el);
  });
  return [...resultSet];
};

/**
 * Creates an array of unique values that is the symmetric difference of the given arrays.
 * @template T
 * @param {T[]} first
 * @param {T[]} second
 * @returns {T[]}
 */
export const setSymmetricDifference = function (first, second) {
  const result = [];
  // @ts-ignore
  if (typeof Set === 'undefined') {
    first.forEach(el => {
      if (result.lastIndexOf(el) === -1 && second.indexOf(el) === -1) {
        result.push(el);
      }
    });
    second.forEach(el => {
      if (result.lastIndexOf(el) === -1 && first.indexOf(el) === -1) {
        result.push(el);
      }
    });
  } else {
    // @ts-ignore
    const firstSet = new Set(first);
    // @ts-ignore
    const secondSet = new Set(second);
    firstSet.forEach(el => {
      if (!secondSet.has(el)) {
        result.push(el);
      }
    });
    secondSet.forEach(el => {
      if (!firstSet.has(el)) {
        result.push(el);
      }
    });
  }
  return result;
};
