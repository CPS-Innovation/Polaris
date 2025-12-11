// Polaris/polaris-e2e/cypress.config.ts
import { defineConfig } from "cypress";
import fs from "fs-extra";
import path from "path";
import { plugin as cypressGrepPlugin } from "@cypress/grep/plugin";

const globalAny: any = global;

// Safer config file loader:
// - keeps dots in names (env.dev.json, env.local.json, etc.)
// - checks existence and throws a helpful error
const getConfigurationByFile = (fileBaseName: string) => {
  const filename = `${fileBaseName}.json`;
  const pathToConfigFile = path.resolve("config", filename);

  if (!fs.pathExistsSync(pathToConfigFile)) {
    throw new Error(
      `Config file not found: ${pathToConfigFile}\n` +
        `Expected a file named '${filename}' under 'config/'.`
    );
  }

  return fs.readJsonSync(pathToConfigFile);
};

export default defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      // Read ENV from Cypress CLI env bag OR OS env and normalize
      const rawEnv =
        (config.env && config.env.ENVIRONMENT) ||
        process.env.ENVIRONMENT ||
        "dev";

      const envName = String(rawEnv).trim(); // avoid trailing spaces, e.g. "dev "

      // Make ENV available to tests via Cypress.env('ENVIRONMENT')
      config.env = { ...config.env, ENVIRONMENT: envName };

      // --- Plugins you already use ---
      require("cypress-timestamps/plugin")(on);
      require("cypress-terminal-report/src/installLogsPrinter")(on);

      // ✅ Correct grep plugin registration (replaces @cypress/grep/src/plugin)
      cypressGrepPlugin(config);

      // --- Tasks you already have ---
      on("task", {
        storeTokenResponseInNode: (tokenResponse: any) => {
          globalAny.tokenResponse = tokenResponse;
          return null;
        },
        retrieveTokenResponseFromNode: () => {
          return globalAny.tokenResponse || null;
        },
        log: (message) => {
          console.log(message);
          return null;
        },
      });

      // --- Load and merge per-env JSON ---
      // base.json (always)
      const baseEnvFromFile = getConfigurationByFile("base");

      // env.<ENV>.json (e.g. env.dev.json, env.local.json)
      const environmentEnvFromFile = getConfigurationByFile(`env.${envName}`);

      const env = { ...baseEnvFromFile, ...environmentEnvFromFile };

      // Prefer BASE_URL from env files
      if (env.BASE_URL) {
        config.baseUrl = env.BASE_URL;
      }

      // Final env bag
      const resolvedEnv = { ...config.env, ...env };
      console.log("Resolved env:", resolvedEnv);

      return { ...config, env: resolvedEnv };
    },

    // Keep your patterns and other settings
    specPattern: "cypress/e2e/**/*.cy.{ts,js}",
    supportFile: "cypress/support/e2e.ts",

    // Defaults (adjust as needed)
    baseUrl: "http://example.org",
    video: true,
    screenshotOnRunFailure: true,
    viewportHeight: 1000,
    viewportWidth: 1500,
    defaultCommandTimeout: 60000,
    trashAssetsBeforeRuns: false,
    experimentalModifyObstructiveThirdPartyCode: true,

    // Grep defaults (optional)
    env: {
      grepOmitFiltered: true,
      grepFilterSpecs: true, // enables spec pre-filtering when grep is used
    },
  },

  videoCompression: false,
});
