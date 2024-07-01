DEBUG=instrument-cra CYPRESS_INSTRUMENT_PRODUCTION=true BUILD_PATH='./build-cypress' env-cmd -f .env.cypress react-scripts -r @cypress/instrument-cra build &&
(cd ./build-cypress && rm serve.json) &&
rimraf report-cypress &&
cypress verify &&
cypress info &&
start-server-and-test 'serve -l 3000 -s build-cypress' http-get://localhost:3000 "$1" &&
xunit-viewer -r report-cypress -o report-cypress/index.html