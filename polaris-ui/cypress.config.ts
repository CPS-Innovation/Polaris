import { defineConfig } from "cypress";

export default defineConfig({
  defaultCommandTimeout: 4000,
  video: false,
  reporter: "junit",
  reporterOptions: {
    mochaFile: "report-cypress/test-results-[hash].xml",
    attachments: true,
  },
  viewportHeight: 1000,
  viewportWidth: 1500,
  e2e: {
    excludeSpecPattern: ["*/**/__coverage__", "*/**/mockServiceWorker.js"],
    // We've imported your old cypress plugins here.
    // You may want to clean this up later by importing these.
    setupNodeEvents(on, config) {
      on("task", {
        log: (message) => {
          console.log(message);
          return null;
        },
        table(message) {
          console.table(message);

          return null;
        },
      });

      const legacyPlugins = require("./cypress/plugins/index.js");
      if (typeof legacyPlugins === "function") {
        legacyPlugins(on, config);
      }

      return config;
      // return require("./cypress/plugins/index.js")(on, config);
    },
    baseUrl: "http://localhost:3000",
  },
});
