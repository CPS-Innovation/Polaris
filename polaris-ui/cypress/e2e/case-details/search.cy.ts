import {
  allMissingDocsPipelinePdfResults,
  missingDocsPipelinePdfResults,
} from "../../../src/mock-api/data/pipelinePdfResults.cypress";
import { TEXT_SEARCH_ROUTE, TRACKER_ROUTE } from "../../../src/mock-api/routes";

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

        // first highlight is focussed
        cy.findByTestId("btn-focus-highlight-previous").should("not.exist");
        cy.findByTestId("txt-focus-highlight-previous").should("exist");

        cy.findByTestId("txt-focus-highlight-numbers").contains("1/3");

        cy.findByTestId("btn-focus-highlight-next").should("exist");
        cy.findByTestId("txt-focus-highlight-next").should("not.exist");

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

        cy.findByTestId("btn-focus-highlight-previous").should("exist");
        cy.findByTestId("txt-focus-highlight-previous").should("not.exist");

        cy.findByTestId("txt-focus-highlight-numbers").contains("2/3");

        cy.findByTestId("btn-focus-highlight-next").should("exist");
        cy.findByTestId("txt-focus-highlight-next").should("not.exist");

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

        cy.findByTestId("btn-focus-highlight-previous").should("exist");
        cy.findByTestId("txt-focus-highlight-previous").should("not.exist");

        cy.findByTestId("txt-focus-highlight-numbers").contains("3/3");

        cy.findByTestId("btn-focus-highlight-next").should("not.exist");
        cy.findByTestId("txt-focus-highlight-next").should("exist");

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
        cy.findByTestId("txt-focus-highlight-previous").should("not.exist");

        cy.findByTestId("txt-focus-highlight-numbers").contains("2/3");

        cy.findByTestId("btn-focus-highlight-next").should("exist");
        cy.findByTestId("txt-focus-highlight-next").should("not.exist");

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
        cy.findByTestId("txt-focus-highlight-previous").should("exist");

        cy.findByTestId("txt-focus-highlight-numbers").contains("1/3");

        cy.findByTestId("btn-focus-highlight-next").should("exist");
        cy.findByTestId("txt-focus-highlight-next").should("not.exist");

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
        cy.findByTestId("btn-redact").should("have.length", 1);
        cy.findByTestId("btn-redact").click({ force: true });
        cy.findByTestId("btn-save-redaction-0").should("exist");
      });
    });
  });
});

export {};
