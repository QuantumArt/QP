/* eslint-disable arrow-parens, id-length, no-plusplus, no-param-reassign, valid-jsdoc */
const identity = x => x;

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Flattens array a single level deep.
 */
export const flatten = () => (result, item) => {
  result.push(...item);
  return result;
};

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Groups the elements of an array according to a specified key selector function.
 * The elements of each group are projected by using a specified value selector function.
 */
export const groupBy = (keySelector, valueSelector = identity) => {
  const hashTable = Object.create(null);

  return (result, item) => {
    if (!Array.isArray(result)) {
      throw new TypeError('initialValue should be an Array');
    }

    const key = keySelector(item);
    const value = valueSelector(item);

    const bucket = hashTable[key] || (hashTable[key] = []);

    let group;
    let i = bucket.length;
    while (i--) {
      group = bucket[i];
      if (group.key === key) {
        break;
      }
    }
    if (i === -1) {
      group = [value];
      Object.defineProperty(group, 'key', { value: key });
      bucket.push(group);
      result.push(group);
    } else {
      group.push(value);
    }

    return result;
  };
};


/**
 * Creates a reducer function for Array `.reduce()` that:
 * Creates an object composed of keys generated from the results of running
 * each element of array thru key selector function.
 * The order of grouped values is determined by the order they occur in collection.
 * The corresponding value of each key is an array of elements responsible for generating the key.
 * The elements of each group are projected by using a specified value selector function.
 */
export const toLookup = (keySelector, valueSelector = identity) => (result, item) => {
  if (typeof result !== 'object' || Array.isArray(result)) {
    throw new TypeError('initialValue should be an Object');
  }
  const key = keySelector(item);
  const value = valueSelector(item);

  if (Object.prototype.hasOwnProperty.call(result, key)) {
    result[key].push(value);
  } else {
    result[key] = [value];
  }
  return result;
};

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Creates an object composed of keys generated from the results of running
 * each element of collection thru key selector function.
 * The corresponding value of each key is the last element responsible for generating the key.
 * The values of an object are projected by using a specified value selector function.
 */
export const toDictionary = (keySelector, valueSelector = identity) => (result, item) => {
  if (typeof result !== 'object' || Array.isArray(result)) {
    throw new TypeError('initialValue should be an Object');
  }
  const key = keySelector(item);
  const value = valueSelector(item);

  result[key] = value;
  return result;
};

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Finds element of the array that having max value of specified selector function.
 */
export const maxBy = (selector) => {
  let max;
  return (result, item) => {
    const value = selector(item);
    if (max >= value) {
      return result;
    }
    max = value;
    return item;
  };
};

/**
 * Creates a reducer function for Array `.reduce()` that:
 * Finds element of the array that having min value of specified selector function.
 */
export const minBy = (selector) => {
  let min;
  return (result, item) => {
    const value = selector(item);
    if (min <= value) {
      return result;
    }
    min = value;
    return item;
  };
};
