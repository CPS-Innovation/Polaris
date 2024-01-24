# This build is targeted at running on `npm serve` and with the app
#  to be running on domain.name/polaris-ui. By setting the PUBLIC_URL 
#  to /polaris-ui, the app looks for its static files in domain.name//polaris-ui/static.
#
# It is essential that server is run with the `-s` flag so that all calls that
#  would be 404s are redirected to `/index.html`.  This lets us deep link into
#  the app e.g. 
#     http://domain.name/polaris-ui
#     http://domain.name/polaris-ui/foo
#     http://domain.name/polaris-ui/foo/bar etc.
# will all be served by the index.html file.  React-routing take cares of the rest.
#
# The final funny here is that the index.html file is moved to the root of the
#  build directory so that this approach works.
#
# Note: I tries to use the rewrites option in a `serve.json` as per the docs.
#  `serve` is very basic, and the rewrite logic does not really cope with what we
#   do.
rm -rf build
PUBLIC_URL=/polaris-ui BUILD_PATH=build/polaris-ui react-scripts build
cp -r build/polaris-ui/static/js build/polaris-ui/static/js-pre-substitution
mv build/polaris-ui/index.html build/index.html
mv build/polaris-ui/run-substitution.sh build/run-substitution.sh