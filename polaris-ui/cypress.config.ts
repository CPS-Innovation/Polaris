import { defineConfig } from "cypress";

export default defineConfig({
  defaultCommandTimeout: 60000,
  video: true,
  reporter: "junit",
  reporterOptions: {
    mochaFile: "report-cypress/test-results-[hash].xml",
    attachments: true,
  },
  viewportHeight: 1000,
  viewportWidth: 1500,
  e2e: {
    // We've imported your old cypress plugins here.
    // You may want to clean this up later by importing these.
    setupNodeEvents(on, config) {
      return require("./cypress/plugins/index.js")(on, config);
    },
    baseUrl: "http://127.0.0.1:3000",
  },
});
