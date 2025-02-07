import {
  allMissingDocsPipelinePdfResults,
  missingDocsPipelinePdfResults,
} from "../../src/mock-api/data/pipelinePdfResults.cypress";
import {
  TEXT_SEARCH_ROUTE,
  TRACKER_ROUTE,
  INITIATE_PIPELINE_ROUTE,
  GET_DOCUMENTS_LIST_ROUTE,
} from "../../src/mock-api/routes";
import pipelinePdfResults from "../../src/mock-api/data/pipelinePdfResults.cypress";
import { getRefreshRedactedDocument } from "../../src/mock-api/data/getDocumentsList.cypress";

describe("Case Details Search", () => {
  describe("Search box", () => {
    it("can search with an empty search term", () => {
      cy.visit("/case-details/12AB1111111/13401");

      cy.findByTestId("btn-search-case").should("not.be.disabled");
      cy.findByTestId("div-search-results").should("not.exist");

      cy.findByTestId("btn-search-case").click();

      cy.findByTestId("div-modal").should("exist");
    });

    it("can search if there is text in the box", () => {
      cy.visit("/case-details/12AB1111111/13401");

      cy.findByTestId("input-search-case").type("a");
      cy.findByTestId("btn-search-case").click();

      cy.findByTestId("div-modal").should("exist");
    });

    it("can search via the enter button", () => {
      cy.visit("/case-details/12AB1111111/13401");

      cy.findByTestId("input-search-case").type("{enter}");

      cy.findByTestId("div-modal").should("exist");
    });
  });

  describe("Search results", () => {
    describe("Searching from the results modal", () => {
      it("can search if the user has opened the results modal with an empty search term", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("btn-search-case").click();

        cy.findByTestId("div-search-result-2").should("not.exist");
        cy.findByTestId("input-results-search-case").type("drink{enter}");
        cy.findByTestId("div-search-result-2").should("exist");
      });

      it("can trigger a search from the results modal by clicking the search button", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("btn-search-case").click();

        cy.findByTestId("div-search-result-2").should("not.exist");
        cy.findByTestId("input-results-search-case").type("drink");
        cy.findByTestId("btn-results-search-case").click();

        cy.findByTestId("div-search-result-2").should("exist");
      });
    });

    describe("Results information", () => {
      it("can display header information when there are results", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("div-results-header").contains(
          "8 results in 3 documents"
        );

        cy.findByTestId("div-sanitized-search").should("not.exist");
      });

      it("can display header information when there are no results", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("DOES-NOT-EXIST{enter}");

        cy.contains("No documents found matching 'DOES-NOT-EXIST'");
      });

      it("can display header warning if user has attempted to use multiple wards", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink drive{enter}");

        cy.findByTestId("div-results-header").contains(
          "8 results in 3 documents"
        );

        cy.findByTestId("div-sanitized-search").should("exist");
      });

      it("can display results context line and meta data", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("div-search-result-2").contains(
          "drink and has left in her car."
        );
        cy.findByTestId("div-search-result-2").contains("Uploaded:02 Jun 2020");
        cy.findByTestId("div-search-result-2").contains("Type:MG12");

        cy.findByTestId("div-search-result-2")
          .findByTestId("details-expand-search-results")
          .should("not.exist");

        // if there are multiple occurrences of the word in the line
        cy.findByTestId("div-search-result-3").contains(
          "DRINK THE SCENE IN HER CAR IN DRINK"
        );
        cy.findByTestId("div-search-result-3").contains("Uploaded:03 Jun 2020");
        cy.findByTestId("div-search-result-3").contains("Type:MG13");
      });

      it("can display results that have multiple occurrences across several lines", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("div-search-result-1")
          .findByTestId("details-expand-search-results")
          .should("exist")
          .contains("View 3 more")
          .should(
            "not.contain",
            "Drink xrive xorms xoadside / hospital / station"
          )
          .should(
            "not.contain",
            "Drink drink zorms zoadside / hospital / station"
          );

        cy.findByTestId("div-search-result-1")
          .findByTestId("details-expand-search-results")
          .find("summary")
          .click();

        cy.findByTestId("div-search-result-1")
          .findByTestId("details-expand-search-results")
          .should("contain", "Drink xrive xorms xoadside / hospital / station")
          .should("contain", "Drink drink zorms zoadside / hospital / station");
      });
    });

    describe("Search ordering", () => {
      const assertResultsOrderHelper = (idsInExpectedOrder: string[]) => {
        cy.get("[data-testid^=div-search-result-")
          .should("have.length", 3)
          .then(($items) => {
            return $items.toArray().map((item) => {
              return item.getAttribute("data-testid");
            });
          })
          .should("deep.eq", idsInExpectedOrder);
      };
      it("can display search results in default date descending order", () => {
        cy.visit("/case-details/12AB1111111/13401");

        cy.findByTestId("input-search-case").type("drink{enter}");

        assertResultsOrderHelper([
          "div-search-result-3",
          "div-search-result-2",
          "div-search-result-1",
        ]);
      });

      it("can change results ordering to number of occurrences descending then back to date descending", () => {
        cy.visit("/case-details/12AB1111111/13401");

        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("select-result-order").select(
          "byOccurancesPerDocumentDesc"
        );

        assertResultsOrderHelper([
          "div-search-result-1",
          "div-search-result-3",
          "div-search-result-2",
        ]);

        cy.findByTestId("select-result-order").select("byDateDesc");

        assertResultsOrderHelper([
          "div-search-result-3",
          "div-search-result-2",
          "div-search-result-1",
        ]);
      });

      it("can not display the sort option dropdown if there are no results", () => {
        cy.visit("/case-details/12AB1111111/13401");

        cy.findByTestId("input-search-case").type("DOES-NOT-EXIST{enter}");
        // make sure we have our results UI...
        cy.findByTestId("div-results-header");

        // ... before asserting that the drop down is not there

        cy.findByTestId("select-result-order").should("not.exist");
      });
    });

    describe("Filtering", () => {
      it("can filter results using one of the filters", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2");
        cy.findByTestId("div-search-result-3");

        cy.findByTestId("checkboxes-doc-type").get("#MG11").check();

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2").should("not.exist");
        cy.findByTestId("div-search-result-3").should("not.exist");

        cy.findByTestId("checkboxes-doc-type").get("#MG12").check();

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2");
        cy.findByTestId("div-search-result-3").should("not.exist");

        cy.findByTestId("checkboxes-doc-type").get("#MG13").check();

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2");
        cy.findByTestId("div-search-result-3");

        cy.findByTestId("checkboxes-doc-type").get("#MG11").uncheck();

        cy.findByTestId("div-search-result-1").should("not.exist");
        cy.findByTestId("div-search-result-2");
        cy.findByTestId("div-search-result-3");

        cy.findByTestId("checkboxes-doc-type").get("#MG12").uncheck();

        cy.findByTestId("div-search-result-1").should("not.exist");
        cy.findByTestId("div-search-result-2").should("not.exist");
        cy.findByTestId("div-search-result-3");

        cy.findByTestId("checkboxes-doc-type").get("#MG13").uncheck();

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2");
        cy.findByTestId("div-search-result-3");
      });

      it("can combine filters across both filter types and show results that match any filter", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("checkboxes-doc-type").get("#MG11").check();

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2").should("not.exist");

        cy.findByTestId("checkboxes-category").get("#Exhibits").check();

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2");

        cy.findByTestId("checkboxes-category").get("#Exhibits").uncheck();

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2").should("not.exist");
      });

      it("can hide filters if there are no results", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("checkboxes-doc-type").should("exist");
        cy.findByTestId("checkboxes-category").should("exist");

        cy.findByTestId("input-results-search-case").type("drink{enter}");

        cy.findByTestId("txt-filter-heading").should("exist");

        cy.findByTestId("checkboxes-doc-type").should("not.exist");

        cy.findByTestId("checkboxes-category").should("not.exist");
      });
    });

    describe("Filtering lifecycle", () => {
      it("retains filter options when changing sort order", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("checkboxes-doc-type").get("#MG11").check();

        cy.findByTestId("select-result-order").select(
          "byOccurancesPerDocumentDesc"
        );

        cy.findByTestId("checkboxes-doc-type")
          .get("#MG11")
          .should("be.checked");
      });

      it("resets filter options when searching for another term", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("checkboxes-doc-type").get("#MG11").check();

        cy.findByTestId("input-results-search-case").clear();
        cy.findByTestId("input-results-search-case").type(
          "DOES-NOT-EXIST{enter}"
        );

        cy.findByTestId("input-results-search-case").clear();
        cy.findByTestId("input-results-search-case").type("drink{enter}");

        cy.findByTestId("checkboxes-doc-type")
          .get("#MG11")
          .should("not.be.checked");
      });

      it("can close results modal and reopen in the same state", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("checkboxes-doc-type").get("#MG11").check();

        cy.findByTestId("select-result-order").select(
          "byOccurancesPerDocumentDesc"
        );

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2").should("not.exist");
        cy.findByTestId("div-search-result-3").should("not.exist");

        cy.findByTestId("btn-modal-close").click();
        cy.findByTestId("btn-search-case").click();

        cy.findByTestId("input-results-search-case").should(
          "have.value",
          "drink"
        );

        cy.findByTestId("checkboxes-doc-type")
          .get("#MG11")
          .should("be.checked");

        cy.findByTestId("select-result-order").should(
          "have.value",
          "byOccurancesPerDocumentDesc"
        );

        cy.findByTestId("div-search-result-1");
        cy.findByTestId("div-search-result-2").should("not.exist");
        cy.findByTestId("div-search-result-3").should("not.exist");
      });
    });

    describe("Missing documents", () => {
      it("can list documents that have not been processed by the pipeline", () => {
        cy.overrideRoute(TRACKER_ROUTE, {
          body: missingDocsPipelinePdfResults,
        });

        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("txt-missing-doc-4");
        cy.findByTestId("txt-missing-doc-5");

        cy.findByTestId("details-expand-missing-docs").should("not.exist");
      });

      it("can list many documents that have not been processed by the pipeline", () => {
        cy.overrideRoute(TRACKER_ROUTE, {
          body: allMissingDocsPipelinePdfResults,
        });

        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("txt-missing-doc-1");
        cy.findByTestId("txt-missing-doc-2");
        cy.findByTestId("txt-missing-doc-3");
        cy.findByTestId("txt-missing-doc-4");

        cy.findByTestId("details-expand-missing-docs").should("exist");

        cy.findByTestId("details-expand-missing-docs")
          .should("exist")
          .contains("View 1 more")
          .findByTestId("txt-missing-doc-5")
          .should("not.exist");

        cy.findByTestId("details-expand-missing-docs").find("summary").click();

        cy.findByTestId("details-expand-missing-docs")
          .findByTestId("txt-missing-doc-5")
          .should("exist");
      });
    });

    describe("Loading... (long running calls)", () => {
      it("can show the user the 'Loading...' content for a long running search call", () => {
        cy.overrideRoute(TEXT_SEARCH_ROUTE, { type: "delay", timeMs: 1500 });
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("div-please-wait");
      });

      it("can show the user the 'Loading...' content for a long running pipeline call", () => {
        cy.overrideRoute(TRACKER_ROUTE, { type: "delay", timeMs: 1500 });
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");

        cy.findByTestId("div-please-wait");
      });
    });

    describe("Viewing search results", () => {
      it("Can use the previous/next buttons to focus on highlights", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("input-search-case").type("drink{enter}");
        cy.findByTestId("link-result-document-3").click();
        cy.findByTestId("div-pdfviewer-0")
          .should("exist")
          .contains("Officer’s certification");

        // first highlight is focussed
        cy.findByTestId("btn-focus-highlight-previous").should("not.exist");
        cy.findByTestId("txt-focus-highlight-numbers").contains("1/3");
        cy.findByTestId("btn-focus-highlight-next").should("exist");

        cy.findByTestId("div-highlight-0").should(
          "have.attr",
          "data-test-isfocussed",
          "true"
        );
        cy.findByTestId("div-highlight-1").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );
        cy.findByTestId("div-highlight-2").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );

        // focus second highlight
        cy.findByTestId("btn-focus-highlight-next").click();
        cy.wait(500);
        cy.findByTestId("btn-focus-highlight-previous").should("exist");
        cy.findByTestId("txt-focus-highlight-numbers").contains("2/3");
        cy.findByTestId("btn-focus-highlight-next").should("exist");

        cy.findByTestId("div-highlight-0").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );
        cy.findByTestId("div-highlight-1").should(
          "have.attr",
          "data-test-isfocussed",
          "true"
        );
        cy.findByTestId("div-highlight-2").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );

        // focus third highlight
        cy.findByTestId("btn-focus-highlight-next").click();
        cy.wait(500);
        cy.findByTestId("btn-focus-highlight-previous").should("exist");
        cy.findByTestId("txt-focus-highlight-numbers").contains("3/3");
        cy.findByTestId("btn-focus-highlight-next").should("not.exist");

        cy.findByTestId("div-highlight-0").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );
        cy.findByTestId("div-highlight-1").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );
        cy.findByTestId("div-highlight-2").should(
          "have.attr",
          "data-test-isfocussed",
          "true"
        );
        // back to second highlight
        cy.findByTestId("btn-focus-highlight-previous").click();
        cy.findByTestId("btn-focus-highlight-previous").should("exist");
        cy.findByTestId("txt-focus-highlight-numbers").contains("2/3");
        cy.findByTestId("btn-focus-highlight-next").should("exist");

        cy.findByTestId("div-highlight-0").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );
        cy.findByTestId("div-highlight-1").should(
          "have.attr",
          "data-test-isfocussed",
          "true"
        );
        cy.findByTestId("div-highlight-2").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );

        // back to first highlight
        cy.findByTestId("btn-focus-highlight-previous").click();
        cy.findByTestId("btn-focus-highlight-previous").should("not.exist");
        cy.findByTestId("txt-focus-highlight-numbers").contains("1/3");
        cy.findByTestId("btn-focus-highlight-next").should("exist");

        cy.findByTestId("div-highlight-0").should(
          "have.attr",
          "data-test-isfocussed",
          "true"
        );
        cy.findByTestId("div-highlight-1").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );
        cy.findByTestId("div-highlight-2").should(
          "have.attr",
          "data-test-isfocussed",
          "false"
        );
      });
    });

    describe("Search term redaction", () => {
      it("User can successfully complete redactions, by clicking on the search results highlighted in the document", () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("btn-search-case").click();

        cy.findByTestId("div-search-result-1").should("not.exist");
        cy.findByTestId("input-results-search-case").type("drink{enter}");
        cy.findByTestId("div-search-result-1").should("exist");
        cy.findByTestId("link-result-document-1").click();
        cy.findByTestId("div-pdfviewer-0")
          .should("exist")
          .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
        cy.get(".PdfLinearHighlight_Highlight__part__search__KLMnH")
          .first()
          .click({ force: true });
        cy.findByTestId("btn-save-redaction-0").should("not.exist");
        cy.findByTestId("btn-redact").should("be.disabled");
        cy.findByTestId("select-redaction-type").should("have.length", 1);
        cy.findByTestId("select-redaction-type").select("2");
        cy.findByTestId("btn-redact").click({ force: true });
        cy.findByTestId("btn-save-redaction-0").should("exist");
      });
    });
  });

  describe("Pipeline refresh", () => {
    it("Should start pipeline refresh when the user starts typing in the search box", () => {
      const initiatePipelineCounter = { count: 0 };
      cy.trackRequestCount(
        initiatePipelineCounter,
        "POST",
        "/api/urns/12AB1111111/cases/13401"
      );
      const trackerCounter = { count: 0 };
      cy.trackRequestCount(
        trackerCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/tracker"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("input-search-case").type("a");
      cy.waitUntil(() => {
        return trackerCounter.count > 0;
      }).then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
    });

    it("Should stop polling pipeline tracker, if the user moves away from the case details page", () => {
      const pipelineDocuments = pipelinePdfResults()[0];
      cy.overrideRoute(TRACKER_ROUTE, {
        type: "break",
        timeMs: 300,
        httpStatusCode: 200,
        body: JSON.stringify({
          ...pipelineDocuments,
          status: "DocumentsRetrieved",
        }),
      });

      const trackerCounter = { count: 0 };
      cy.trackRequestCount(
        trackerCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/tracker"
      );
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("input-search-case").type("a");

      // navigating user away from case-details page
      cy.waitUntil(() => {
        return trackerCounter.count === 1;
      }).then(() => {
        cy.findAllByTestId("link-back-link").click();
      });
      cy.wait(1000);
      cy.window().then(() => {
        expect(trackerCounter.count).to.equal(1);
      });
    });

    it("Should not call again the initiate pipeline, if the previous call return 423 status during pipeline refresh flow, and continue polling the tracker", () => {
      cy.visit("/case-details/12AB1111111/13401");

      cy.overrideRoute(
        INITIATE_PIPELINE_ROUTE,
        {
          type: "break",
          timeMs: 300,
          httpStatusCode: 423,
          body: JSON.stringify({
            trackerUrl:
              "https://mocked-out-api/api/urns/12AB1111111/cases/13401/tracker",
          }),
        },
        "post"
      );

      const initiatePipelineRequestCounter = { count: 0 };
      cy.trackRequestCount(
        initiatePipelineRequestCounter,
        "POST",
        "/api/urns/12AB1111111/cases/13401"
      );
      const trackerCounter = { count: 0 };
      cy.trackRequestCount(
        trackerCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/tracker"
      );
      cy.findByTestId("input-search-case").type("a");

      cy.waitUntil(() => {
        return trackerCounter.count === 1;
      }).then(() => {
        expect(initiatePipelineRequestCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
    });

    it("Should  call the initiate pipeline, but stop polling if the user navigates away from case detail page", () => {
      cy.visit("/case-details/12AB1111111/13401");

      const initiatePipelineRequestCounter = { count: 0 };
      cy.trackRequestCount(
        initiatePipelineRequestCounter,
        "POST",
        "/api/urns/12AB1111111/cases/13401"
      );
      const trackerCounter = { count: 0 };
      cy.trackRequestCount(
        trackerCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/tracker"
      );
      cy.findByTestId("input-search-case").type("a");

      cy.findAllByTestId("link-back-link").click();

      cy.window().then(() => {
        expect(initiatePipelineRequestCounter.count <= 1);
        expect(trackerCounter.count).to.equal(0);
      });
    });

    it("Should not trigger a pipeline refresh after user typed the first letter in the search box until the user action cause update of a document", () => {
      const documentList = getRefreshRedactedDocument("1", 2);
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[0],
      });
      const initiatePipelineCounter = { count: 0 };
      cy.trackRequestCount(
        initiatePipelineCounter,
        "POST",
        "/api/urns/12AB1111111/cases/13401"
      );
      const trackerCounter = { count: 0 };
      cy.trackRequestCount(
        trackerCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/tracker"
      );
      const documentListCounter = { count: 0 };
      cy.trackRequestCount(
        documentListCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/documents"
      );
      cy.visit("/case-details/12AB1111111/13401");
      //user typing first letter
      cy.findByTestId("input-search-case").type("a");
      cy.waitUntil(() => {
        return trackerCounter.count > 0;
      }).then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
      //user typing second letter
      cy.findByTestId("input-search-case").type("b");
      cy.wait(1000);
      cy.window().then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
      //user clicking on search btn
      cy.findByTestId("btn-search-case").click();
      cy.window().then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
      //user typing in the search again
      cy.findByTestId("input-results-search-case").type("c");
      cy.window().then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
      //user clicking on search btn
      cy.findByTestId("btn-results-search-case").click();
      cy.window().then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
      cy.findByTestId("btn-modal-close").click();
      cy.findByTestId("div-modal").should("not.exist");

      //user doing a redaction for document update
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[1],
      });
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.focused().should("have.id", "select-redaction-type");
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click({ force: true });
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("be.visible");
      cy.findByTestId("rl-under-redaction-content").should("be.visible");
      cy.findByTestId("btn-save-redaction-log").click();
      cy.waitUntil(() => {
        return cy
          .findByTestId("div-modal")
          .should("not.exist")
          .then(() => true);
      }).then(() => {
        cy.wait(500);
        cy.findByTestId("input-search-case").type("e");
      });
      cy.waitUntil(() => {
        return trackerCounter.count > 1;
      }).then(() => {
        expect(initiatePipelineCounter.count).to.equal(2);
        expect(trackerCounter.count).to.equal(2);
      });
      cy.findByTestId("input-search-case").type("d");
      cy.wait(500);
      cy.window().then(() => {
        expect(initiatePipelineCounter.count).to.equal(2);
        expect(trackerCounter.count).to.equal(2);
      });
    });

    it("Should trigger a pipeline refresh if user typed in the search box then user action cause update of a document, then clicked on search button without updating the search text", () => {
      const documentList = getRefreshRedactedDocument("1", 2);
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[0],
      });
      const initiatePipelineCounter = { count: 0 };
      cy.trackRequestCount(
        initiatePipelineCounter,
        "POST",
        "/api/urns/12AB1111111/cases/13401"
      );
      const trackerCounter = { count: 0 };
      cy.trackRequestCount(
        trackerCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/tracker"
      );
      const documentListCounter = { count: 0 };
      cy.trackRequestCount(
        documentListCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/documents"
      );
      cy.visit("/case-details/12AB1111111/13401");
      //user typing first letter
      cy.findByTestId("input-search-case").type("a");
      cy.waitUntil(() => {
        return trackerCounter.count > 0;
      }).then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(1);
      });
      //user doing a redaction for document update
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[1],
      });
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.focused().should("have.id", "select-redaction-type");
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click({ force: true });
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("be.visible");
      cy.findByTestId("rl-under-redaction-content").should("be.visible");
      cy.findByTestId("btn-save-redaction-log").click();
      cy.waitUntil(() => {
        return cy
          .findByTestId("div-modal")
          .should("not.exist")
          .then(() => true);
      }).then(() => {
        cy.wait(500);
        cy.findByTestId("btn-search-case").click();
      });
      cy.waitUntil(() => {
        return trackerCounter.count > 1;
      }).then(() => {
        expect(initiatePipelineCounter.count).to.equal(2);
        expect(trackerCounter.count).to.equal(2);
        expect(documentListCounter.count).to.equal(2);
      });
      cy.findByTestId("input-results-search-case").type("d");
      cy.wait(500);
      cy.window().then(() => {
        expect(initiatePipelineCounter.count).to.equal(2);
        expect(trackerCounter.count).to.equal(2);
        expect(documentListCounter.count).to.equal(2);
      });
    });

    it("Should only call the search api after finishing the pipelineRefresh if it require one", () => {
      const pipelineDocuments = pipelinePdfResults()[0];
      cy.overrideRoute(TRACKER_ROUTE, {
        type: "break",
        timeMs: 300,
        httpStatusCode: 200,
        body: JSON.stringify({
          ...pipelineDocuments,
          status: "DocumentsRetrieved",
        }),
      });
      const initiatePipelineCounter = { count: 0 };
      cy.trackRequestCount(
        initiatePipelineCounter,
        "POST",
        "/api/urns/12AB1111111/cases/13401"
      );
      const trackerCounter = { count: 0 };
      cy.trackRequestCount(
        trackerCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/tracker"
      );
      const searchCounter = { count: 0 };
      cy.trackRequestCount(
        searchCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/search/"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("input-search-case").type("a");
      cy.findByTestId("btn-search-case").click();

      cy.waitUntil(() => {
        return trackerCounter.count > 0;
      }).then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(searchCounter.count).to.equal(0);
      });
      //this makes sure the search api is been called after finishing the tracker calls(here 2)
      cy.waitUntil(() => {
        return searchCounter.count === 1;
      }).then(() => {
        expect(initiatePipelineCounter.count).to.equal(1);
        expect(trackerCounter.count).to.equal(2);
        expect(searchCounter.count).to.equal(1);
      });
    });

    it("Should show the loading percentage and the tracker summary if the pipeline refresh tracker is called more than once during the search", () => {
      cy.visit("/case-details/12AB1111111/13401");
      const pipelineDocuments = pipelinePdfResults()[0];
      cy.overrideRoute(TRACKER_ROUTE, {
        type: "break",
        timeMs: 300,
        httpStatusCode: 200,
        body: JSON.stringify({
          ...pipelineDocuments,
          status: "DocumentsRetrieved",
        }),
      });
      cy.findByTestId("input-search-case").type("a");
      cy.findByTestId("btn-search-case").click();
      cy.findByTestId("loading-percentage").should("exist");
      cy.findByTestId("loading-percentage").should(
        "contain.text",
        "Loading... 0%"
      );
      cy.waitUntil(() => {
        return cy
          .findByTestId("loading-percentage")
          .should("contain.text", "Loading... 100%");
      });
      cy.findByTestId("div-modal").should(
        "contain.text",
        "Total documents: 11"
      );
      cy.findByTestId("div-modal").should(
        "contain.text",
        "Documents ready to read: 11"
      );
      cy.findByTestId("div-modal").should(
        "contain.text",
        "Documents indexed: 11"
      );
      cy.findByTestId("input-results-search-case").type("d");
      cy.findByTestId("btn-results-search-case").click();
      cy.findByTestId("loading-percentage").should("not.exist");
      cy.findByTestId("btn-modal-close").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("input-search-case").type("e");
      cy.findByTestId("btn-search-case").click();
      cy.findByTestId("loading-percentage").should("not.exist");
    });
  });
});

export {};
