const convertToRanges = (arr: number[]): string => {
  if (arr.length === 0) return "";
  arr.sort((a, b) => a - b);

  const result = arr.reduce<{ ranges: string[]; start: number; end: number }>(
    (acc, curr, value) => {
      if (value === 0) return acc;
      if (curr === acc.end + 1) {
        acc.end = curr;
      } else {
        if (acc.end - acc.start >= 2) {
          acc.ranges.push(`#${acc.start} - #${acc.end}`);
        } else {
          for (let j = acc.start; j <= acc.end; j++) {
            acc.ranges.push(`#${j}`);
          }
        }
        acc.start = curr;
        acc.end = curr;
      }
      return acc;
    },
    { ranges: [], start: arr[0], end: arr[0] }
  );

  if (result.end - result.start >= 2) {
    result.ranges.push(`#${result.start} - #${result.end}`);
  } else {
    for (let j = result.start; j <= result.end; j++) {
      result.ranges.push(`#${j}`);
    }
  }

  return result.ranges.join(", ");
};

export const statementNumberText = (values: number[]) => {
  if (!values.length) {
    return "";
  }
  const formattedValues = convertToRanges(values);
  return `Already in use ${formattedValues}`;
};
