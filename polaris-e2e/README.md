# Polaris e2e tests

## To run locally:

-   `npm i`
-   either:
    -   create a `scripts/run.sh` shell script by copying `run.example.sh` and adding the secrets to the `env` declarations
    -   in the terminal run `CYPRESS_CLIENTSECRET=SECRET-GOES-HERE CYPRESS_PASSWORD=PASSWORD-GOES-HERE npm run cy:run`

The static configuration values are in two places:

-   `cypress.env.json`
-   `baseUrl` is in `cypress.config.ts`

> At the time of writing I couldn't get the `baseUrl` setting working from within `cypress.env.json`, hence the two separate files to find config in.
> 
