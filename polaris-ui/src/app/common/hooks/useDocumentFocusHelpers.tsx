const PDF_HIGHLIGHTER_CLASS = "PdfHighlighter__highlight-layer";

export const getWordStartingIndices = (sentence: string) => {
  //regular expression pattern to find all words in the sentence
  const wordPattern = /\b\w+\b/g;
  const wordStartingIndices = [];
  let match;
  while ((match = wordPattern.exec(sentence)) !== null) {
    wordStartingIndices.push(match.index);
  }

  return wordStartingIndices;
};

export const getNonEmptyTextContentElements = (elements: HTMLCollection) => {
  const filteredElements = Array.from(elements).filter(
    (element) => !element.classList.contains(PDF_HIGHLIGHTER_CLASS)
  );
  const leafSpanElements = filteredElements.reduce(
    (acc, curr) =>
      curr.children.length ? [...acc, ...curr.children] : [...acc, curr],
    [] as Element[]
  );

  const nonEmptyTextContents = leafSpanElements.filter((element) =>
    element.textContent?.trim()
  );
  const groupedTexts = nonEmptyTextContents.reduce((acc, element) => {
    const styles = window.getComputedStyle(element);

    // Access specific styles by property name
    const top = styles.getPropertyValue("top");

    if (!acc[top]) {
      acc[top] = [element];
      return acc;
    }

    acc[top] = [...acc[top], element];
    return acc;
  }, {} as Record<string, Element[]>);

  const keyValueArray = Object.entries(groupedTexts);

  // Sort the array based on the numeric portion of the keys, the style top value
  keyValueArray.sort(([keyA], [keyB]) => {
    const numericA = parseFloat(keyA);
    const numericB = parseFloat(keyB);
    return numericA - numericB;
  });

  // Convert the sorted array back into an object
  const sortedObject = Object.fromEntries(keyValueArray);
  const childSpans = Object.keys(sortedObject).reduce((acc, element) => {
    acc = [...acc, ...sortedObject[element]];
    return acc;
  }, [] as Element[]);

  return childSpans;
};
