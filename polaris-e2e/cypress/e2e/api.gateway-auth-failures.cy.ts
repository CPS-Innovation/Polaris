import { PresentationDocumentProperties } from "../../gateway/PipelineDocument";
import { CorrelationId } from "../support/correlation-ids";
import { makeApiRoutes } from "../support/helpers/make-routes";

const {
  HAPPY_PATH_URN,
  HAPPY_PATH_CASE_ID,
  HAPPY_PATH_DOCUMENT_ID,
  HAPPY_PATH_TARGET_SEARCH_TEXT,
} = Cypress.env();

const statusCodes = {
  BAD_REQUEST_400: 400,
  UNAUTHORIZED_401: 401,
  FORBIDDEN_403: 403,
} as const;

type Headers = Record<string, string>;

const getRoutesToTest = (
  versionId: number,
  correlationId: CorrelationId,
  headers: Headers
) => {
  const routeGenerators = makeApiRoutes(headers);

  const routesToTest = [
    routeGenerators.LIST_CASES(HAPPY_PATH_URN, correlationId),
    routeGenerators.GET_CASE(HAPPY_PATH_URN, HAPPY_PATH_CASE_ID, correlationId),
    routeGenerators.TRACKER_START(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      correlationId
    ),
    routeGenerators.GET_TRACKER(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      correlationId
    ),
    routeGenerators.GET_SEARCH(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_TARGET_SEARCH_TEXT,
      correlationId
    ),
    routeGenerators.GET_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      versionId,
      correlationId
    ),
    routeGenerators.CHECKOUT_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      versionId,
      correlationId
    ),
    routeGenerators.CANCEL_CHECKOUT_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      versionId,
      correlationId
    ),
    routeGenerators.SAVE_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      versionId,
      correlationId
    ),
  ];

  return routesToTest;
};

const getVersionIdHelper = (
  headers: Headers,
  restOfTest: (versionId: number) => void
) => {
  const getDocumentRoute = makeApiRoutes(headers).GET_DOCUMENTS(
    HAPPY_PATH_URN,
    HAPPY_PATH_CASE_ID
  );

  cy.api<PresentationDocumentProperties[]>(getDocumentRoute).then(
    ({ body }) => {
      const match = body.find(
        (doc) => doc.documentId === HAPPY_PATH_DOCUMENT_ID
      );
      if (!match || typeof match.versionId !== "number") {
        throw new Error(
          `Could not find versionId for documentId=${String(
            HAPPY_PATH_DOCUMENT_ID
          )}`
        );
      }
      restOfTest(match.versionId);
    }
  );
};

describe(
  "Gateway endpoint auth failures",
  { tags: ["@ci", "@ci-chunk-4"] },
  () => {
    (["EMPTY", "UNDEFINED", "NOT_A_GUID"] as CorrelationId[]).forEach(
      (correlationId) => {
        xit(`rejects calls that have a token but do not have an appropriate correlation id: (${correlationId} correlation id)`, () => {
          cy.getADTokens().then((adTokens) => {
            cy.getAuthHeaders().then((headers) => {
              getVersionIdHelper(headers, (versionId) => {
                const routesToTest = getRoutesToTest(versionId, correlationId, {
                  Authorization: `Bearer ${adTokens.access_token}`,
                });

                for (const route of routesToTest) {
                  cy.log(
                    `Testing ${String(route.headers?.["correlation-id"])}`
                  );
                  cy.api({
                    ...route,
                    failOnStatusCode: false,
                  }).then((response) =>
                    expect(response.status).to.equal(
                      statusCodes.BAD_REQUEST_400
                    )
                  );
                }
              });
            });
          });
        });
      }
    );

    describe("Broken AD auth", () => {
      xit("rejects calls that have a correlation id but do not have a token", () => {
        cy.getAuthHeaders().then((headers) => {
          getVersionIdHelper(headers, (versionId) => {
            const routesToTest = getRoutesToTest(
              versionId,
              "BLANK" as CorrelationId,
              {}
            );

            for (const route of routesToTest) {
              cy.api({
                ...route,
                failOnStatusCode: false,
                followRedirect: false, // either a redirect or a fail
              }).then((response) =>
                expect(response.status).to.be.greaterThan(299)
              );
            }
          });
        });
      });

      xit("rejects calls that have a correlation id but do not have a valid token", () => {
        cy.getADTokens().then((adTokens) => {
          const brokenToken = adTokens.access_token.slice(0, -1);
          cy.getAuthHeaders().then((headers) => {
            getVersionIdHelper(headers, (versionId) => {
              const routesToTest = getRoutesToTest(
                versionId,
                "BLANK" as CorrelationId,
                {
                  Authorization: `Bearer ${brokenToken}`,
                }
              );

              for (const route of routesToTest) {
                cy.api({
                  ...route,
                  failOnStatusCode: false,
                  followRedirect: false, // either a redirect or a fail
                }).then((response) =>
                  expect(response.status).to.be.greaterThan(299)
                );
              }
            });
          });
        });
      });
    });
  }
);
