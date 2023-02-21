
docker run \
  --rm \
  -ti \
  -v `pwd`../../polaris-terraform/ui-terraform/nginx.conf:/etc/nginx/templates/nginx.conf.template \
  -v `pwd`../../polaris-terraform/ui-terraform/nginx.js:/etc/nginx/templates/nginx.js \
  -p 8080:80 \
  -e NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx \
  -e RESOLVER=8.8.8.8 \
  -e WEBSITE_HOSTNAME=localhost \
  -e UPSTREAM_CMS_DOMAIN_NAME=example.org \
  -e ENDPOINT_HTTP_PROTOCOL=https \
  -e AUTH_HANDOVER_ENDPOINT_DOMAIN_NAME=example.org \
  -e API_ENDPOINT_DOMAIN_NAME=example.org \
  -e DDEI_ENDPOINT_DOMAIN_NAME=example.org \
  -e DDEI_ENDPOINT_FUNCTION_APP_KEY=not-used-in-local-development \
  --name nginx \
  nginx