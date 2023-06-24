import { defineConfig } from "cypress"
import fs from "fs-extra"
import path from "path"

const globalAny: any = global

const getConfigurationByFile = (file: string) => {
  const pathToConfigFile = path.resolve("config", `${file}.json`)
  return fs.readJsonSync(pathToConfigFile)
}

export default defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      if (!config.env.ENVIRONMENT) {
        throw new Error("Please provide an ENVIRONMENT variable")
      }
      require("cypress-terminal-report/src/installLogsPrinter")(on, {
        printLogsToConsole: "always",
      })

      on("task", {
        storeTokenResponseInNode: (tokenResponse: any) => {
          globalAny.tokenResponse = tokenResponse
          return null
        },
        retrieveTokenResponseFromNode: () => {
          return globalAny.tokenResponse
        },
      })

      const baseEnvFromFile = getConfigurationByFile("base")
      const environmentEnvFromFile = getConfigurationByFile(
        "env." + config.env.ENVIRONMENT
      )
      const env = {
        ...baseEnvFromFile,
        ...environmentEnvFromFile,
      }

      config.baseUrl = env.BASE_URL

      const resolvedEnv = { ...config.env, ...env }
      console.log("Resolved env: ", resolvedEnv)

      return { ...config, env: resolvedEnv }
    },
    baseUrl: "http://example.org",
    video: true,
    screenshotOnRunFailure: true,
    // reporter: "junit",
    // reporterOptions: {
    //     mochaFile: "report/test-results-[hash].xml",
    //     attachments: true,
    // },
    viewportHeight: 1000,
    viewportWidth: 1500,
    defaultCommandTimeout: 60000,
  },
})
