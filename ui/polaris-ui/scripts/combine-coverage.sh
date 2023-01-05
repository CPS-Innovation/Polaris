rm -rf coverage-merged
mkdir coverage-merged
cp coverage/coverage-final.json coverage-merged/coverage-final-unit.json
cp coverage-cypress/coverage-final.json coverage-merged/coverage-final-cypress.json
#nyc merge coverage-merged coverage-merged/coverage.json
nyc report \
  --temp-dir ./coverage-merged \
  --reporter cobertura \
  --reporter lcov \
  --report-dir ./coverage-merged