// this file is required for setting config values in deployed code see #12977
const fs = require("fs");
const util = require("util");

const cleanFolder = "polaris-ui/static/js-pre-substitution";
const workingFolder = "polaris-ui/static/js-temp";
const liveFolder = "polaris-ui/static/js";

console.log("Copying from %s to %s", cleanFolder, workingFolder);
fs.cpSync(cleanFolder, workingFolder, { recursive: true });
console.log("Copied from %s to %s", cleanFolder, workingFolder);

const REACT_APP_CONFIGS = Object.entries(process.env).filter(([key]) =>
  key.startsWith("REACT_APP_")
);
console.log(
  "Read REACT_APP_ env vars",
  util.inspect(REACT_APP_CONFIGS, false, null, true /* enable colors */)
);

fs.readdirSync(workingFolder).forEach((file) => {
  const filePath = workingFolder + "/" + file;

  console.log("Reading %s", filePath);
  let content = fs.readFileSync(filePath, { encoding: "utf8", flag: "r" });
  console.log("Read %s", filePath);

  REACT_APP_CONFIGS.forEach(([key, value]) => {
    content = content.replace(new RegExp(`--${key}--`, "g"), value);
  });

  console.log("Writing %s", filePath);
  fs.writeFileSync(filePath, content);
  console.log("Written %s", filePath);
});

console.log("Copying from %s to %s", workingFolder, liveFolder);
fs.cpSync(workingFolder, liveFolder, { recursive: true });
console.log("Copied from %s to %s", workingFolder, liveFolder);

console.log("Deleting %s", workingFolder);
fs.rmSync(workingFolder, { recursive: true, force: true });
console.log("Deleted %s", workingFolder);
