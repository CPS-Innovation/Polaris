# Polaris UI

## Getting Started

You need an `.env` or `.env.development.local` file (see https://create-react-app.dev/docs/adding-custom-environment-variables/#what-other-env-files-can-be-used)
for how `react-scripts` finds `.env` file variants. Also see the `.env.development.local.example` file in this repo - maybe just rename this to get started.

# `env` variables

## Core settings:

Note the following target the deployed dev azure gateway, if you want to target e.g QA, replace `dev` with `QA` in the follwoing instructions.

- `REACT_APP_CLIENT_ID` - search for as-web-polaris-dev in Azure Active Directory, the client ID of that App Registration goes here
- `REACT_APP_TENANT_ID` - CPS AAD tentant Idm, unless something big happens at CPS this will be `00dd0d1d-d7e6-4338-ac51-565339c7088c`
- `REACT_APP_GATEWAY_BASE_URL` - this will be `https://fa-polaris-dev-gateway.azurewebsites.net`
- `REACT_APP_REDACTION_LOG_BASE_URL` - this will be `https://fa-redaction-log-dev-reporting.azurewebsites.net`
- `REACT_APP_GATEWAY_SCOPE` - this will be `https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway/user_impersonation`

## Dev/Test Settings

- `REACT_APP_MOCK_API_SOURCE` - set empty or leave out this setting to hook on to the gateway. `cypress` or `dev` to use local `msw` mocks.
- `REACT_APP_MOCK_AUTH` - set to true if you want to ignore proper AAD auth, only really useful if you are not hitting real endpoints
  i.e. completely using `msw` or are some kind of isolated `cypress` testing.
- `REACT_APP_MOCK_API_MAX_DELAY` - if using the mock api a delay is randomly added to each HTTP call. This helps simulate real-life latency.
  This value sets the upper bound for the delay, the lower bound is 0 and each call is given a random delay between these two bounds.

## Architecture/Code decisions

- using `gds-react-jsx`
- mocking with `msw`

  - driving with `env` and data sources

- using `cypress` for integration testing
  - e2e tests, and where cypress kicks off in the pipelines?
- `prettier` - as per https://prettier.io/docs/en/install.html#summary keep the version number in `package.json` fixed to avoid formatting differences between devs
- pinned `faker` because of maintainer withdrawing code.
