const fs = require("fs");
const path = require("path");

const targetExtensions = [".js", ".html"];
const tagRegex = /--(REACT_APP_[A-Z0-9_]+)--/g;
const ignoreFileNames = ["substitute-config.js"];

const shouldProcessFile = (file) =>
  targetExtensions.includes(path.extname(file).toLocaleLowerCase()) &&
  !ignoreFileNames.includes(path.basename(file)) &&
  fs.readFileSync(file).toString().match(tagRegex);

const substituteEnvVarsInFile = (file) => {
  const content = fs.readFileSync(file, "utf8");
  const substitutedContent = content.replace(tagRegex, (match, varName) => {
    const envValue = process.env[varName];
    return envValue === undefined ? match : envValue;
  });
  fs.writeFileSync(file, substitutedContent, "utf8");
};

fs.readdirSync(process.cwd(), { recursive: true })
  .filter(shouldProcessFile)
  .forEach(substituteEnvVarsInFile);
