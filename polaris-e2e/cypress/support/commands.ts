import "@testing-library/cypress/add-commands";
import { injectTokens } from "./inject-tokens";

declare global {
    namespace Cypress {
        interface Chainable<Subject> {
            login(): Chainable<any>;
            requestToken(): Chainable<Response<unknown>>;
        }
    }
}

const { AUTHORITY, CLIENTID, CLIENTSECRET, APISCOPE, USERNAME, PASSWORD } =
    Cypress.env();

const AUTOMATION_LANDING_PAGE_URL = "/?automation-test-first-visit=true";

let cachedTokenExpiryTime = new Date().getTime();
let cachedTokenResponse = null;

Cypress.Commands.add("login", () => {
    let getToken =
        cachedTokenResponse && cachedTokenExpiryTime > new Date().getTime()
            ? cy.visit(AUTOMATION_LANDING_PAGE_URL).then(() => ({
                  body: cachedTokenResponse,
              }))
            : cy.visit(AUTOMATION_LANDING_PAGE_URL).requestToken();

    return getToken
        .then((response) => {
            injectTokens(response.body);
            cachedTokenResponse = response.body;
            cachedTokenExpiryTime = new Date().getTime() + 20 * 60 * 1000; // 20 mins ok for a start?
        })
        .reload()
        .visit("/");
});

Cypress.Commands.add(
    "requestToken",
    {
        prevSubject: ["optional"],
    },
    () =>
        cy.request({
            url: AUTHORITY + "/oauth2/v2.0/token",
            method: "POST",
            body: {
                grant_type: "password",
                client_id: CLIENTID,
                client_secret: CLIENTSECRET,
                scope: ["openid profile"].concat([APISCOPE]).join(" "),
                USERNAME,
                PASSWORD,
            },
            form: true,
        })
);
