export const statementNumberText = (values: number[]) => {
  return values.reduce((acc, value, index) => {
    if (index === values.length - 1 && values.length > 1) {
      acc = `${acc} and #${value}`;
      return acc;
    }
    acc = `${acc} #${value}`;
    return acc;
  }, "Already in use");
};
