node cypress/assurance-input/convertToStrings && 
BUILD_PATH='./build-cypress' env-cmd -f .env.cypress react-scripts build &&
cypress verify &&
cypress info &&
start-server-and-test 'serve -l 3000 -s build-cypress' http-get://localhost:3000 'env-cmd -f .env.assurance cypress run --spec "cypress/e2e/assurance/**/*"'