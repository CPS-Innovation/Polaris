import { NOTES_ROUTE } from "../../../src/mock-api/routes";
describe("Feature Notes", () => {
  it("Should show and hide blue circle in the notes icon depending on whether the document has notes available or not", () => {
    cy.visit("/case-details/12AB1111111/13401?notes=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("has-note-indicator-1").should("exist");
    cy.findByTestId("has-note-indicator-2").should("exist");
    cy.findByTestId("has-note-indicator-3").should("not.exist");
    cy.findByTestId("has-note-indicator-4").should("not.exist");
    cy.findByTestId("has-note-indicator-5").should("not.exist");
    cy.findByTestId("has-note-indicator-8").should("not.exist");
    cy.findByTestId("has-note-indicator-9").should("not.exist");
    cy.findByTestId("has-note-indicator-10").should("not.exist");
    cy.findByTestId("has-note-indicator-11").should("not.exist");
  });
  it("Should be able to open notes panel and read the notes for that document if there is any", () => {
    cy.visit("/case-details/12AB1111111/13401?notes=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.findByTestId("btn-notes-2").click();
    cy.findByTestId("notes-panel").should("exist");
    cy.findByTestId("notes-panel").contains("Notes - CM01");
    cy.findByTestId("notes-list").find("li").should("have.length", 2);
    cy.findByTestId("notes-panel").find("ul > li").eq(0).as("firstListItem");
    cy.findByTestId("notes-panel").find("ul > li").eq(1).as("secondListItem");
    cy.get("@firstListItem")
      .findByTestId("created-by-2")
      .contains("test_user2");
    cy.get("@firstListItem")
      .findByTestId("added-on-2")
      .contains("11 February 2024");
    cy.get("@firstListItem").findByTestId("note-text-2").contains("text_2");

    cy.get("@secondListItem")
      .findByTestId("created-by-1")
      .contains("test_user1");
    cy.get("@secondListItem")
      .findByTestId("added-on-1")
      .contains("10 February 2024");
    cy.get("@secondListItem").findByTestId("note-text-1").contains("text_1");
    cy.findByTestId("btn-close-notes").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.focused().should("have.id", "btn-notes-2");
    cy.findByTestId("btn-notes-10").click();
    cy.findByTestId("notes-panel").should("exist");
    cy.findByTestId("notes-panel").contains("Notes - PortraitLandscape");
    cy.findByTestId("notes-list").find("li").should("have.length", 0);
    cy.findByTestId("btn-close-notes").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.focused().should("have.id", "btn-notes-10");
  });
  it("Should be able to add new to the document", () => {
    const expectedAddNotePayload = { documentId: 10, text: "note_1" };
    const addNoteRequestObject = { body: "" };
    cy.trackRequestBody(
      addNoteRequestObject,
      "POST",
      "/api/urns/12AB1111111/cases/13401/documents/MGForm/10/notes"
    );
    const refreshPipelineCounter = { count: 0 };
    cy.trackRequestCount(
      refreshPipelineCounter,
      "POST",
      "/api/urns/12AB1111111/cases/13401"
    );

    const trackerCounter = { count: 0 };
    cy.trackRequestCount(
      trackerCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/tracker"
    );
    const doc10GetNotesCounter = { count: 0 };
    cy.trackRequestCount(
      doc10GetNotesCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/documents/MGForm/10/notes"
    );

    cy.visit("/case-details/12AB1111111/13401?notes=true");

    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.waitUntil(() => {
      return trackerCounter.count;
    }).then(() => {
      expect(refreshPipelineCounter.count).to.equal(1);
      expect(trackerCounter.count).to.equal(1);
    });
    cy.findByTestId("notes-panel").should("not.exist");
    cy.findByTestId("btn-notes-10").click();
    cy.findByTestId("notes-panel").should("exist");
    cy.findByTestId("notes-panel").contains("Notes - PortraitLandscape");
    cy.findByTestId("notes-list").find("li").should("have.length", 0);
    cy.window().then(() => {
      expect(doc10GetNotesCounter.count).to.equal(1);
    });
    cy.waitUntil(() => cy.findByTestId("notes-textarea")).then(() =>
      cy.findByTestId("notes-textarea").type("note_1")
    );
    cy.findByTestId("btn-add-note").click();

    //assertion on the add note request
    cy.waitUntil(() => {
      return addNoteRequestObject.body;
    }).then(() => {
      expect(addNoteRequestObject.body).to.deep.equal(
        JSON.stringify(expectedAddNotePayload)
      );
    });

    cy.waitUntil(() => {
      return trackerCounter.count === 2;
    }).then(() => {
      expect(doc10GetNotesCounter.count).to.equal(2);
      expect(trackerCounter.count).to.equal(2);
    });
    cy.findByTestId("btn-close-notes").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.focused().should("have.id", "btn-notes-10");
  });
  it("Should show error message if adding a new note failed, and shouldn't call refreshPipeline and tracker", () => {
    cy.overrideRoute(
      NOTES_ROUTE,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "post"
    );
    const doc10GetNotesCounter = { count: 0 };
    cy.trackRequestCount(
      doc10GetNotesCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/documents/MGForm/10/notes"
    );
    const refreshPipelineCounter = { count: 0 };
    cy.trackRequestCount(
      refreshPipelineCounter,
      "POST",
      "/api/urns/12AB1111111/cases/13401"
    );
    const trackerCounter = { count: 0 };
    cy.trackRequestCount(
      trackerCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/tracker"
    );

    cy.visit("/case-details/12AB1111111/13401?notes=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.findByTestId("btn-notes-10").click();
    cy.findByTestId("notes-panel").should("exist");
    cy.findByTestId("notes-panel").contains("Notes - PortraitLandscape");
    cy.findByTestId("notes-list").find("li").should("have.length", 0);
    cy.waitUntil(() => cy.findByTestId("notes-textarea")).then(() =>
      cy.findByTestId("notes-textarea").type("note_1")
    );
    cy.findByTestId("btn-add-note").click();

    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to add note to the document. Please try again.");
    cy.findByTestId("btn-error-modal-ok").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("notes-textarea").should("have.value", "note_1");
    cy.focused().should("have.id", "btn-close-notes");
    cy.findByTestId("btn-close-notes").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.focused().should("have.id", "btn-notes-10");
    cy.window().then(() => {
      expect(doc10GetNotesCounter.count).to.equal(1);
      expect(refreshPipelineCounter.count).to.equal(1);
      expect(trackerCounter.count).to.equal(1);
    });
  });
  it("Should throw error, if new note crosses the maximum character limit", () => {
    const addNoteCounter = { count: 0 };
    cy.trackRequestCount(
      addNoteCounter,
      "POST",
      "/api/urns/12AB1111111/cases/13401/documents/MGForm/10/notes"
    );
    const notes500CharacterText =
      "Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investigative Agency for correction.Returned to Investig";
    cy.visit("/case-details/12AB1111111/13401?notes=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.findByTestId("btn-notes-10").click();
    cy.findByTestId("notes-panel").should("exist");
    cy.findByTestId("notes-panel").contains("Notes - PortraitLandscape");
    cy.findByTestId("notes-list").find("li").should("have.length", 0);
    cy.waitUntil(() => cy.findByTestId("notes-textarea")).then(() =>
      cy.findByTestId("notes-textarea").type(notes500CharacterText)
    );
    cy.realPress(".");
    cy.get("#notes-textarea-info")
      .contains("You have 1 character too many")
      .should("exist");
    cy.realPress("a");
    cy.get("#notes-textarea-info")
      .contains("You have 2 characters too many")
      .should("exist");
    cy.findByTestId("btn-add-note").click();
    cy.findByTestId("notes-error-summary").find("li").should("have.length", 1);
    cy.findByTestId("notes-textarea-link").should("exist");
    cy.findByTestId("notes-textarea-link").should(
      "have.text",
      "Notes must be 500 characters or less"
    );
    cy.get("#notes-textarea-error").should(
      "have.text",
      "Error: Notes must be 500 characters or less"
    );
    cy.findByTestId("notes-textarea").focus();
    cy.realPress("{backspace}");
    cy.realPress("{backspace}");
    cy.findByTestId("btn-add-note").click();
    cy.get("#notes-textarea-error").should("not.exist");
    cy.findByTestId("notes-error-summary").should("not.exist");
    cy.waitUntil(() => {
      return addNoteCounter.count;
    }).then(() => {
      expect(addNoteCounter.count).to.equal(1);
    });
    cy.findByTestId("btn-close-notes").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.focused().should("have.id", "btn-notes-10");
  });
});