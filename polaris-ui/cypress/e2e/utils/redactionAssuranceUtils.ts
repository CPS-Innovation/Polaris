import { RedactionSaveRequest } from "../../../src/app/features/cases/domain/gateway/RedactionSaveRequest";

/**
 * This is to normalize the save redaction request based on the height of the known save request data for each page
 * @param expectedRequest
 * @param redactionRequest
 * @returns
 */
export const getNormalizedRedactionRequest = (
  expectedRequest: RedactionSaveRequest,
  redactionRequest: RedactionSaveRequest
) => {
  const normalizedRedactions = redactionRequest.redactions.map(
    (redaction, index) => {
      const { height, width, redactionCoordinates } = redaction;
      const baseHeight = expectedRequest.redactions[index].height;

      if (height !== baseHeight) {
        const scaleFactor = (baseHeight - height) / height;
        return {
          ...redaction,
          height: baseHeight,
          width: width + width * scaleFactor,
          redactionCoordinates: redactionCoordinates.map((coordinate) => ({
            x1: coordinate.x1 + coordinate.x1 * scaleFactor,
            y1: coordinate.y1 + coordinate.y1 * scaleFactor,
            x2: coordinate.x2 + coordinate.x2 * scaleFactor,
            y2: coordinate.y2 + coordinate.y2 * scaleFactor,
          })),
        };
      }
      return redaction;
    }
  );
  return {
    documentId: redactionRequest.documentId,
    redactions: normalizedRedactions,
  };
};

/**
 * Validator function to compare the redaction values with a precision factor
 * @param expectedRequest
 * @param request
 */
export const redactionRequestAssertionValidator = (
  expectedRequest: RedactionSaveRequest,
  redactionRequest: RedactionSaveRequest
) => {
  //This normalize the redaction save request so that we can do the compare
  const request = getNormalizedRedactionRequest(
    expectedRequest,
    redactionRequest
  );
  //assurance test currently passes if the values falls under particular precision
  const PRECISION_FACTOR = 1.5;
  const PRECISION_FACTOR_Y2 = 3; // When running on the pipeline y2 values shows a higher deviation
  expect(request.documentId).to.equal(expectedRequest.documentId);
  expect(request.redactions.length).to.equal(expectedRequest.redactions.length);
  request.redactions.forEach((redaction, index) => {
    expect(redaction.pageIndex).to.equal(
      expectedRequest.redactions[index].pageIndex
    );
    expect(
      Math.abs(redaction.height - expectedRequest.redactions[index].height)
    ).to.be.lessThan(PRECISION_FACTOR);
    expect(
      Math.abs(redaction.width - expectedRequest.redactions[index].width)
    ).to.be.lessThan(PRECISION_FACTOR);

    const coordinates = redaction.redactionCoordinates;
    const expectedCoordinates =
      expectedRequest.redactions[index].redactionCoordinates;
    coordinates.forEach((coordinate, index) => {
      expect(
        Math.abs(coordinate.x1 - expectedCoordinates[index].x1)
      ).to.be.lessThan(PRECISION_FACTOR);
      expect(
        Math.abs(coordinate.y1 - expectedCoordinates[index].y1)
      ).to.be.lessThan(PRECISION_FACTOR);
      expect(
        Math.abs(coordinate.x2 - expectedCoordinates[index].x2)
      ).to.be.lessThan(PRECISION_FACTOR);
      expect(
        Math.abs(coordinate.y2 - expectedCoordinates[index].y2)
      ).to.be.lessThan(PRECISION_FACTOR_Y2);
    });
  });
};
