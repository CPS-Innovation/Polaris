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
    (element) => !element.classList.contains("PdfHighlighter__highlight-layer")
  );
  const leafSpanElements = filteredElements.reduce((acc: any[], curr) => {
    if (curr.children.length) {
      acc = [...acc, ...curr.children];
      return acc;
    }
    acc = [...acc, curr];
    return acc;
  }, []);

  return leafSpanElements.filter((element) => element.textContent?.trim());
};
