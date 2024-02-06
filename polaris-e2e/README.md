# Polaris e2e tests

## To run locally:

- `npm i`
- either:
  - create a `scripts/run.sh` shell script by copying `run.example.sh` and adding the secrets to the `FOO=bar`
    env variable declarations
  - in the terminal run `CYPRESS_CLIENTSECRET=SECRET-GOES-HERE CYPRESS_PASSWORD=PASSWORD-GOES-HERE CYPRESS_ENVIRONMENT=dev npm run cy`
    (see below for more details)

## Config

See `config` folder. `base.json` holds all non-environment-specific settings. Each environment then has a
file e.g. `env.dev.json`. The settings in the environment-specific file are then added to the base settings.

### Passed-in values

- `CYPRESS_CLIENTSECRET` - if you do not know this you will need to generate a new secret on the `Certificates & secrets`
  page of the app registration for the app service under test in Azure AD

- `CYPRESS_PASSWORD` - the password of the AD account, get this from someone who knows!

- `CYPRESS_ENVIRONMENT` - `local`, `dev` or `qa`

# CI tests
Tests marked with the tag `@ci` will be ran as part of the CI pipeline