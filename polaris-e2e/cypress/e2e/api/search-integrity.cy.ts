/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import {
  WAIT_UNTIL_OPTIONS,
  RAPID_RETRY_WAIT_UNTIL_OPTIONS,
} from "../../support/options"
import { ApiTextSearchResult } from "../../../gateway/ApiTextSearchResult"

const {
  SEARCH_INTEGRITY_TARGET_URN,
  SEARCH_INTEGRITY_TARGET_CASE_ID,
  SEARCH_INTEGRITY_TARGET_TERM,
  SEARCH_INTEGRITY_TARGET_TERM_COUNT,
  SEARCH_INTEGRITY_TOLERANCE,
  PRE_SEARCH_DELAY_MS,
} = Cypress.env()

let routes: ApiRoutes

describe("Search Integrity", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("can observe the exact number of search results expected at the point the tracker is complete", () => {
    cy.clearCaseTracker(
      SEARCH_INTEGRITY_TARGET_URN,
      SEARCH_INTEGRITY_TARGET_CASE_ID
    )
      .api(
        routes.TRACKER_START(
          SEARCH_INTEGRITY_TARGET_URN,
          SEARCH_INTEGRITY_TARGET_CASE_ID
        )
      )
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>({
              ...routes.GET_TRACKER(
                SEARCH_INTEGRITY_TARGET_URN,
                SEARCH_INTEGRITY_TARGET_CASE_ID
              ),
              failOnStatusCode: false,
            })
            .its("body")
            .then(({ status }) => {
              if (status === "Failed") {
                throw new Error("Pipeline failed, ending test")
              }
              return status === "Completed"
            }),
        // RAPID... to try to minimise the gap between when the pipeline is complete and
        //  when we know about it and start searching
        RAPID_RETRY_WAIT_UNTIL_OPTIONS
      )

      .wait(PRE_SEARCH_DELAY_MS)

      .api<ApiTextSearchResult[]>(
        routes.GET_SEARCH(
          SEARCH_INTEGRITY_TARGET_URN,
          SEARCH_INTEGRITY_TARGET_CASE_ID,
          SEARCH_INTEGRITY_TARGET_TERM
        )
      )
      .its("body")
      .then((results) => {
        const matchingWordCount = results.reduce((acc, result) => {
          return (
            acc +
            result.words.filter((word) =>
              word.text
                .toLocaleUpperCase()
                // .includes rather than === because the ocr will have results e.g. "Syracuse,"
                .includes(SEARCH_INTEGRITY_TARGET_TERM.toLocaleUpperCase())
            ).length
          )
        }, 0)

        // Temporary fudge: we have problems with the search index acting in a transactionally
        //  consistent way, so we're going to allow a bit of leeway in the matching word count.
        //  Otherwise our e2e tests fail intermittently but regularly.
        const minMatchingWordCount =
          SEARCH_INTEGRITY_TARGET_TERM_COUNT - SEARCH_INTEGRITY_TOLERANCE
        const maxMatchingWordCount =
          SEARCH_INTEGRITY_TARGET_TERM_COUNT + SEARCH_INTEGRITY_TOLERANCE

        expect(matchingWordCount).to.be.greaterThan(minMatchingWordCount)
        expect(matchingWordCount).to.be.lessThan(maxMatchingWordCount)
      })
  })
})
