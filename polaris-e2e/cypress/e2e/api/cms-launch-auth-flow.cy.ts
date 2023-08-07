const {
  CMS_USERNAME,
  CMS_PASSWORD,
  CMS_LOGIN_PAGE_URL,
  AUTH_HANDOVER_URL,
  TARGET_URN,
  TARGET_CASE_ID,
} = Cypress.env()

const getUrlWithCookieParam = (cookies: string) =>
  AUTH_HANDOVER_URL + "?cookie=" + encodeURIComponent(cookies)

const appendQParams = (url: string, qParamObject: {}) =>
  url + "&q=" + encodeURIComponent(JSON.stringify(qParamObject))

describe("CMS launch auth flow", () => {
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
          .and.satisfy((url: string) => url.endsWith("/polaris-ui/"))
      })
      .getAllCookies()
      .then((cookies) => {
        // Assert cookies are set now
        expect(cookies).to.have.lengthOf(1)
        expect(cookies[0]).to.have.property("name", "Cms-Auth-Values")
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
          .and.satisfy((url: string) => url.endsWith("/polaris-ui/"))
      })
      .getAllCookies()
      .then((cookies) => {
        // Assert cookies are set now
        expect(cookies).to.have.lengthOf(1)
        expect(cookies[0]).to.have.property("name", "Cms-Auth-Values")
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
          .and.satisfy((url: string) =>
            url.endsWith(
              `/polaris-ui/case-details/${TARGET_URN}/${TARGET_CASE_ID}`
            )
          )
      })
      .getAllCookies()
      .then((cookies) => {
        // Assert cookies are set now
        expect(cookies).to.have.lengthOf(1)
        expect(cookies[0]).to.have.property("name", "Cms-Auth-Values")
      })
  })
})
