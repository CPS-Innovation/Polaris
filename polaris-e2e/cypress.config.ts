import { defineConfig } from "cypress";

export default defineConfig({
    e2e: {
        setupNodeEvents(on, config) {
            // implement node event listeners here
        },
        // this will be overriden by CYPRESS_BASE_URL environment variable
        baseUrl: "https://as-web-rumpole-dev.azurewebsites.net/",
        video: true,
        // reporter: "junit",
        // reporterOptions: {
        //     mochaFile: "report/test-results-[hash].xml",
        //     attachments: true,
        // },
        viewportHeight: 1000,
        viewportWidth: 1500,
        defaultCommandTimeout: 60000,
    },
});
