/**
 * Utilities for strings
 */

/**
 * Convert a number to an abbreviated string
 * @param {any} number
 */
export function numToAbbrString(number) {
  if (Math.abs(number) > 1000000000) {
    return (number / 1000000000.0).toFixed(1).toString() + "G";
  }
  if (Math.abs(number) > 1000000) {
    return (number / 1000000.0).toFixed(1).toString() + "M";
  }
  if (Math.abs(number) > 1000) {
    return (number / 1000.0).toFixed(1).toString() + "K";
  }
  return number.toFixed(0).toString();
};