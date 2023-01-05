// this file is required for setting config values in deployed code see #12977
const fs = require("fs");

const folder = "static/js";

const REACT_APP_CONFIGS = Object.entries(process.env).filter(([key]) =>
  key.startsWith("REACT_APP_")
);

fs.readdirSync(folder).forEach((file) => {
  const filePath = folder + "/" + file;

  let content = fs.readFileSync(filePath, { encoding: "utf8", flag: "r" });
  REACT_APP_CONFIGS.forEach(([key, value]) => {
    content = content.replace(new RegExp(`--${key}--`, "g"), value);
  });
  fs.writeFileSync(filePath, content);
});
