#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const fileStrings = {};

fs.readdirSync(__dirname).forEach((fileName) => {
  if (fileName.endsWith(".js") || fileName.endsWith(".json")) {
    return;
  }

  const fileNameNoExtension = path.parse(fileName).name;

  fileStrings[fileNameNoExtension] = fs.readFileSync(
    path.resolve(__dirname, fileName),
    "base64"
  );
});

fs.writeFileSync(
  path.resolve(__dirname, "pdf-strings.json"),
  JSON.stringify(fileStrings, null, 2)
);
