// this file is required for setting config values in deployed code see #12977
const fs = require("fs");
const util = require("util");

const cleanFolder = "polaris-ui/static/js-pre-substitution";
const workingFolder = "polaris-ui/static/js-temp";
const liveFolder = "polaris-ui/static/js";

// Set up a sideways folder to do our work in. Lets do all the work
//  first and then copy over to the prod location at the last minute.
// This approach helps mitigate against a scenario seen in incident
//  #26718 where a failure in this script leaves the polaris-ui/static/js
//  empty when the script is runs subsequently to a fresh deployment run
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

  let doesFileContainEnvKey = false;

  REACT_APP_CONFIGS.forEach(([key, value]) => {
    const thisKey = `--${key}--`;
    const firstKeyPos = content.indexOf(thisKey);

    if (firstKeyPos !== -1) {
      console.log(
        "File %s contains %s, first position %d",
        file,
        thisKey,
        firstKeyPos
      );
      doesFileContainEnvKey = true;
    } else {
      // nothing to do in this file, exit this iteration
      return;
    }

    content = content.replace(new RegExp(thisKey, "g"), value);

    console.log(
      "File %s now %s contains %s",
      file,
      content.includes(thisKey) ? "still" : "no longer",
      thisKey
    );
  });

  if (doesFileContainEnvKey) {
    console.log("Writing %s", filePath);
    fs.writeFileSync(filePath, content);
    console.log("Written %s", filePath);
  } else {
    console.log("%s did not contain env keys", filePath);
  }
});

// Now we have all files substituted in the working folder, at the last
//  moment copy over the prod location. No need to remove the prod folder
//  or files as `cpSync` copies over the top of destination files.
console.log("Copying from %s to %s", workingFolder, liveFolder);
fs.cpSync(workingFolder, liveFolder, { recursive: true });
console.log("Copied from %s to %s", workingFolder, liveFolder);

// Tidy up the working folder (this isn't critical, doesn't matter if it fails)
console.log("Deleting %s", workingFolder);
fs.rmSync(workingFolder, { recursive: true, force: true });
console.log("Deleted %s", workingFolder);
