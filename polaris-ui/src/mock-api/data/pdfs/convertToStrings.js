#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const fileStrings = {};

fs.readdirSync(__dirname).forEach((fileName) => {
  if (fileName.endsWith(".js") || fileName.endsWith(".json")) {
    return;
  }

  const regexp = /[/./-/_/\\]/gm;
  const file = fileName.replace(regexp, "");
  const fileNameNoExtension = path.parse(file).name;

  fileStrings[fileNameNoExtension] = fs.readFileSync(
    path.resolve(__dirname, fileName),
    "base64"
  );
});

fs.writeFileSync(
  path.resolve(__dirname, "file-strings.json"),
  JSON.stringify(fileStrings, null, 2)
);
