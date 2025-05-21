const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");
const { describe, it, before, after } = require("node:test");
const assert = require("node:assert");

describe("substitute-config.js", () => {
  const tempDir = path.join(__dirname, "temp");
  const fixtureDir = path.join(__dirname, "test-fixtures");

  const readFile = (fileName) =>
    fs.readFileSync(path.join(tempDir, fileName), "utf8");

  before(() => {
    // Delete the temporary directory if it exists
    if (fs.existsSync(tempDir)) {
      fs.rmSync(tempDir, { recursive: true });
    }

    // Create a temporary directory
    fs.mkdirSync(tempDir);

    // Copy the fixture files to the temporary directory recursively
    fs.cpSync(fixtureDir, tempDir, { recursive: true });
  });

  after(() => {
    // Remove the temporary directory
    fs.rmSync(tempDir, { recursive: true });
  });

  it("should replace placeholders with environment variable values", () => {
    // Set the environment variables
    process.env.REACT_APP_API_URL = "https://api.example.com";
    process.env.REACT_APP_API_KEY = "abc123";
    process.env.REACT_APP_EMPTY_VAR = "";
    process.env.NON_REACT_APP_VAR = "something";

    // Run the substitute-config.js script
    execSync(`node ${path.join(__dirname, "substitute-config.js")}`, {
      cwd: tempDir,
    });

    // Read the modified fixture files
    const file1Content = readFile("file1.js");
    const file2Content = readFile("file2.html");
    const file3Content = readFile("file3.txt");
    const file4Content = readFile("folder/folder/file4.js");
    const ignoredFileContent = readFile("substitute-config.js");

    // Tags have been replaced in js files
    assert(file1Content.includes('const apiUrl = "https://api.example.com";'));
    assert(file1Content.includes('const apiKey = "abc123";'));

    // Env vars that are empty strings are still substituted
    assert(file1Content.includes('const emptyVar = "";'));

    // Qualifying tags where the env variable is not present are left in place
    assert(file1Content.includes('const unknown = "--REACT_APP_UNKNOWN--";'));

    // Non-qualifying tags where the env variable is present are left in place
    assert(
      file1Content.includes('const nonReactAppVar = "--NON_REACT_APP_VAR--";')
    );

    // Tags have been replaced in html files
    assert(file2Content.includes('const apiUrl = "https://api.example.com";'));

    // Qualifying tags in non-qualifying file types are left in place
    assert(file3Content.includes('const apiUrl = "--REACT_APP_API_URL--";'));

    // Tags have been replaced in deeply-nested js files
    assert(file4Content.includes('const apiUrl = "https://api.example.com";'));

    // Ignored file tags are left in place
    assert(
      ignoredFileContent.includes('const apiUrl = "--REACT_APP_API_URL--";')
    );
  });
});
