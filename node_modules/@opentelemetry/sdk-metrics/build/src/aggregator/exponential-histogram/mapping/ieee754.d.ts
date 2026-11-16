/**
 * MIN_NORMAL_EXPONENT is the minimum exponent of a normalized
 * floating point: -1022.
 */
export declare const MIN_NORMAL_EXPONENT: number;
/**
 * MAX_NORMAL_EXPONENT is the maximum exponent of a normalized
 * floating point: 1023.
 */
export declare const MAX_NORMAL_EXPONENT = 1023;
/**
 * MIN_VALUE is the smallest normal number
 */
export declare const MIN_VALUE: number;
/**
 * getNormalBase2 extracts the normalized base-2 fractional exponent.
 * This returns k for the equation f x 2**k where f is
 * in the range [1, 2).  Note that this function is not called for
 * subnormal numbers.
 * @param {number} value - the value to determine normalized base-2 fractional
 *    exponent for
 * @returns {number} the normalized base-2 exponent
 */
export declare function getNormalBase2(value: number): number;
/**
 * isPowerOfTwo reports whether value is an exact power of two, e.g. its 52-bit
 * significand is all zeros. Only valid for positive, finite values.
 * @param {number} value - the floating point number to test
 * @returns {boolean} true if value is an exact power of two
 */
export declare function isPowerOfTwo(value: number): boolean;
//# sourceMappingURL=ieee754.d.ts.map