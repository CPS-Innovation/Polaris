export const EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX = /[^ .'`|a-z|A-Z-]/g;
//Note: We got the EXHIBIT_TEXT_VALIDATION_REGEX validation rule from Modern code base, but this validation is not operational in Modern but we are applying it.
export const EXHIBIT_TEXT_VALIDATION_REGEX =
  /[^$"*/0-9 !£_+(),?@#~;:%&.'`\r\n|a-z|A-Z-]/g;

export const handleTextValidation = (text: string, regex: RegExp) => {
  const invalidChars = text.match(regex);
  if (invalidChars) {
    const uniqueInvalidChars = Array.from(new Set(invalidChars));

    return `invalid ${
      uniqueInvalidChars.length > 1 ? "characters" : "character"
    } ${uniqueInvalidChars.join("")}`;
  }
  return "";
};
