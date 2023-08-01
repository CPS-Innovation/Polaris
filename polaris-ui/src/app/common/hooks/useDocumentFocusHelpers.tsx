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

  return leafSpanElements.filter((element) => element.textContent?.trim());
};
