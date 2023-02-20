# Polaris e2e tests

## To run locally:

- `npm i`
- either:
  - create a `scripts/run.sh` shell script by copying `run.example.sh` and adding the secrets to the `FOO=bar`
    env variable declarations
  - in the terminal run `CYPRESS_CLIENTSECRET=SECRET-GOES-HERE CYPRESS_PASSWORD=PASSWORD-GOES-HERE npm run cy:run`
    (see below for more details)

## Config

The static configuration values are in two places:

- `baseUrl` is in `cypress.config.ts`
- everything else is in `cypress.env.json`

> At the time of writing I couldn't get the `baseUrl` setting working from within `cypress.env.json`, hence the two separate files to find config in.

### In `cypress.config.ts`:

```
baseUrl: "https://as-web-polaris-dev.azurewebsites.net/" // is the root URL of the Polaris UI deployment under test
```

### In `cypress.env.json`

```
{
    "AUTHORITY": "https://login.microsoftonline.com/00dd0d1d-d7e6-4338-ac51-565339c7088c",
    // should never change from the above, embeds the CPS tenant id

    "CLIENTID": "3649c1c8-00cf-4b8f-a671-304bc074937c",
    // look in Azure AD for the app registration of the app service running the UI under test
    // CLIENTSECRET is passed in from the outside for security purposes

    "APISCOPE": "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway/user_impersonation",
    // make sure `fa-polaris-dev-gateway` is changed to reference the gateway of the env under test

    "AD_USERNAME": "AutomationUser.ServiceTeam2@cps.gov.uk",
    // PASSWORD is passed in from the outside for security purposes

    "CMS_USERNAME": "stefanstachow.cin3",
    // CMS_PASSWORD is passed in from the outside for security purposes

    // settings specific to logging in to CMS
    "CMS_LOGIN_PAGE_URL": "https://polaris-dev-cmsproxy.azurewebsites.net/dev-login",
    "CMS_USERNAME_FIELD_LOCATOR": "input[name='username']",
    "CMS_PASSWORD_FIELD_LOCATOR": "input[name='password']",
    "CMS_SUBMIT_BUTTON_LOCATOR": "input[type='submit']",
    "CMS_LOGGED_IN_CONFIRMATION_LOCATOR": "strong[data-testid='login-ok']",

    // specific details for the test run below
    "TARGET_URN": "45CV2911222",
    "TARGET_DEFENDANT_NAME": "HUSBANDTWO, Bad",
    "TARGET_DOCUMENT_ID": "8611594",
    "TARGET_DOCUMENT_NAME": "MG 4 - Charges",
    "TARGET_DOCUMENT_TEXT_FRAGMENT": "Multi Media Evidence",
    "TARGET_SEARCH_TEXT": "evidence"
}
```

### Passed-in values

- `CYPRESS_CLIENTSECRET` - if you do not know this you will need to generate a new secret on the `Certificates & secrets`
  page of the app registration for the app service under test i Azure AD

- `CYPRESS_PASSWORD` - get this from someone who knows!
