export const selectPDFTextElement = (matchString: string, targetCount = 0) => {
  cy.get(`.markedContent > span:contains(${matchString})`).each(
    (element, index) => {
      if (index === targetCount) {
        cy.wrap(element)
          .trigger("mousedown")
          .then(() => {
            const el = element[0];
            const document = el.ownerDocument;
            const range = document.createRange();
            range.selectNodeContents(el);
            document.getSelection()?.removeAllRanges();
            document.getSelection()?.addRange(range);
          })
          .trigger("mouseup");
        cy.document().trigger("selectionchange");
        return false;
      }
    }
  );
};
