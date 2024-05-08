/// <reference types="cypress" />
// ***********************************************************
// This example plugins/index.js can be used to load plugins
//
// You can change the location of this file or turn off loading
// the plugins file with the 'pluginsFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/plugins-guide
// ***********************************************************

// This function is called when a project is opened or re-opened (e.g. due to
// the project's config changing)

/**
 * @type {Cypress.PluginConfig}
 */
// eslint-disable-next-line no-unused-vars

const _ = require("lodash");
const del = require("del");

module.exports = (on, config) => {
  require("@cypress/code-coverage/task")(on, config);
  // `on` is used to hook into various events Cypress emits
  // `config` is the resolved Cypress config

  // to allow out tests to dictate and/or override their own msw mock api route
  config.env.REACT_APP_GATEWAY_BASE_URL =
    process.env.REACT_APP_GATEWAY_BASE_URL;

  config.env.REACT_APP_REDACTION_LOG_BASE_URL =
    process.env.REACT_APP_REDACTION_LOG_BASE_URL;

  config.env.REACT_APP_MOCK_API_MAX_DELAY =
    process.env.REACT_APP_MOCK_API_MAX_DELAY;

  config.env.REACT_APP_MOCK_API_SOURCE = process.env.REACT_APP_MOCK_API_SOURCE;

  on("after:spec", (spec, results) => {
    if (results && results.video) {
      // Do we have failures for any retry attempts?
      const failures = _.some(results.tests, (test) => {
        return _.some(test.attempts, { state: "failed" });
      });
      if (!failures) {
        // delete the video if the spec passed and no tests retried
        return del(results.video);
      }
    }
  });

  return config;
};
