We have forked `react-pdf-highlighter` from the github repository at https://github.com/agentcooper/react-pdf-highlighter.

The fork was taken from the latest published version at the time: 5.3.0.

The library is not suitable in its published form as it is opinionated about the appearance of the visual
components it provides. We need control over our look-and-feel hence we fork the library rather than
importing from npm.

The `src/` folder from the repo has been copied here and the following npm dependencies of `react-pdf-highlighter`
have been added directly to our own `package.json`:

```
	"@types/lodash.debounce": "^4.0.6",
    "@types/pdfjs-dist": "^2.7.4",
    "lodash.debounce": "^4.0.8",
    "pdfjs-dist": "2.11.338",
    "react-rnd": "^10.3.7"
```


## Comparing releases of `react-pdf-highlighter`
https://github.com/agentcooper/react-pdf-highlighter/compare/e9e8e00..1d1c768

## Area highlighting drops by a pixel
See https://github.com/agentcooper/react-pdf-highlighter/issues/160#issuecomment-942854641

## css notes

