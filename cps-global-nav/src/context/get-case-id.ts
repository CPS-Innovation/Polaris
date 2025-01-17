const caseIdLocators = [
  // Polaris format e.g. : http://some-domain/polaris-ui/case-details/45CV2911222/2149310
  "/polaris-ui/[^/]+/[^/]+/(\\d+)",
  // Last chance: fall back to getting the last integer group in the address
  "(\\d+)(?!.*\\d)",
].map(reg => new RegExp(reg));

export const getCaseId = (window: Window) => {
  const address = window.location.href;

  for (let i = 0; i < caseIdLocators.length; i++) {
    const reg = caseIdLocators[i];
    const result = address.match(reg);
    console.debug(`Checking ${address} against ${reg} with result ${result}`);
    if (result?.[1]) {
      return parseInt(result?.[1], 10);
    }
  }

  return null;
};
