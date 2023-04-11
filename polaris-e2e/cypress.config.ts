import { defineConfig } from "cypress"
import fs from "fs-extra"
import path from "path"

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
      const baseEnvFromFile = getConfigurationByFile("base")
      console.log({ baseEnvFromFile })
      const environmentEnvFromFile = getConfigurationByFile(
        "env." + config.env.ENVIRONMENT
      )
      const env = {
        ...baseEnvFromFile,
        ...environmentEnvFromFile,
      }

      config.baseUrl = env.BASE_URL
      return { ...config, env: { ...config.env, ...env } }
    },
    baseUrl: "http://example.org",
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
})
