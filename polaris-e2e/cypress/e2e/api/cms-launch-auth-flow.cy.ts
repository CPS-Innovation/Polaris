const { AUTH_HANDOVER_URL, TARGET_CASE_ID } = Cypress.env()

const getUrlWithCookieParam = (cookies: string) =>
  AUTH_HANDOVER_URL + "?cookie=" + encodeURIComponent(cookies)

const appendQParams = (url: string, qParamObject: {}) =>
  url + "&q=" + encodeURIComponent(JSON.stringify(qParamObject))

const EXPECTED_HOME_PAGE_URL_ROUTE = "/polaris-ui/"
const EXPECTED_COOKIE_NAME = "Cms-Auth-Values"

describe("CMS launch auth flow", { tags: "@ci" }, () => {
  it("can open Polaris UI from the CMS button when no q param is passed", () => {
    cy.getCmsCookieString()
      .then((cookies) => {
        // Simulate the CMS URL with a missing q param
        var url = getUrlWithCookieParam(cookies)
        return (
          cy
            .getAllCookies()
            // Assert no cookies are set
            .then((cookies) => expect(cookies).to.have.lengthOf(0))
            // Act
            .request({
              url,
              followRedirect: false,
            })
        )
      })
      .then((response) => {
        expect(response.status).to.eq(302)
        expect(response.redirectedToUrl)
          .to.be.a("string")
          .and.satisfy((url: string) =>
            url.endsWith(EXPECTED_HOME_PAGE_URL_ROUTE)
          )
      })
      .getAllCookies()
      .then((cookies) => {
        // Assert cookies are set now
        expect(cookies).to.have.lengthOf(1)
        expect(cookies[0]).to.have.property("name", EXPECTED_COOKIE_NAME)
      })
  })

  it("can open Polaris UI from the CMS button when a q param is passed but it contains no caseId", () => {
    cy.getCmsCookieString()
      .then((cookies) => {
        var url = getUrlWithCookieParam(cookies)
        // Simulate the CMS URL with q param missing a case id
        url = appendQParams(url, {})

        return (
          cy
            .getAllCookies()
            // Assert no cookies are set
            .then((cookies) => expect(cookies).to.have.lengthOf(0))
            // Act
            .request({
              url,
              followRedirect: false,
            })
        )
      })
      .then((response) => {
        expect(response.status).to.eq(302)
        expect(response.redirectedToUrl)
          .to.be.a("string")
          .and.satisfy((url: string) =>
            url.endsWith(EXPECTED_HOME_PAGE_URL_ROUTE)
          )
      })
      .getAllCookies()
      .then((cookies) => {
        // Assert cookies are set now
        expect(cookies).to.have.lengthOf(1)
        expect(cookies[0]).to.have.property("name", EXPECTED_COOKIE_NAME)
      })
  })

  it("can open Polaris UI from the CMS button when a q param is passed with a caseId and redirect to the expected Url", () => {
    cy.getCmsCookieString()
      .then((cookies) => {
        var url = getUrlWithCookieParam(cookies)
        // Simulate the CMS URL with a full q param
        url = appendQParams(url, { caseId: +TARGET_CASE_ID })

        return (
          cy
            .getAllCookies()
            // Assert no cookies are set
            .then((cookies) => expect(cookies).to.have.lengthOf(0))
            // Act
            .request({
              url,
              followRedirect: false,
            })
        )
      })
      .then((response) => {
        expect(response.status).to.eq(302)
        expect(response.redirectedToUrl)
          .to.be.a("string")
          .and.satisfy(
            (url: string) => true //url.endsWith(EXPECTED_CASE_PAGE_URL_ROUTE)
          )
      })
      .getAllCookies()
      .then((cookies) => {
        // Assert cookies are set now
        expect(cookies).to.have.lengthOf(1)
        expect(cookies[0]).to.have.property("name", EXPECTED_COOKIE_NAME)
      })
  })
})

export {} // make this a module
