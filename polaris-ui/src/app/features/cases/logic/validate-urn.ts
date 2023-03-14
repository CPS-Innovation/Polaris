const URN_REGEX = /^(\d{2}[A-Z0-9]{2}\d{7})((M)|\(M\)|S|\(S\))?(\/\d{1,2})?$/;

export type ValidateUrnReturnType =
  | { isValid: true; rootUrn: string }
  | { isValid: false; rootUrn: null };

export const validateUrn = (urn: string): ValidateUrnReturnType => {
  var match = urn.match(URN_REGEX);
  var isValid = !!match;

  if (!isValid) {
    return { isValid: false, rootUrn: null };
  }

  var rootUrn = isValid && match?.[1] ? match[1] : "";

  return {
    isValid: true,
    rootUrn,
  };
};
