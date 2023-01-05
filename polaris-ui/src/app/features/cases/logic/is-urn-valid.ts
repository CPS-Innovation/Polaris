const URN_REGEX = /^\d{2}[A-Z0-9]{2}\d{7}((M)|\(M\)|S|\(S\))?(\/\d{1,2})?$/;

export const isUrnValid = (urn: string) => !!urn.match(URN_REGEX);
