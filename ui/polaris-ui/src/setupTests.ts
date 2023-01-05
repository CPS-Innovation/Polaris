// jest-dom adds custom jest matchers for asserting on DOM nodes.
// allows you to do things like:
// expect(element).toHaveTextContent(/react/i)
// learn more: https://github.com/testing-library/jest-dom
import "@testing-library/jest-dom";

// prevent issue in https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/1840
global.crypto = require("crypto");
