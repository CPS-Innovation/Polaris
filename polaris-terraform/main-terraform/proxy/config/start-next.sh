#!/bin/sh
# Boots the proxy on the NEXT config (this proxy/config tree) instead of the live monolith.
#
# Deployed by terraform as the `start-next.sh` blob at the content-container root (mounted at
# /etc/nginx/templates), and run by the App Service Startup Command `sh /etc/nginx/templates/start-next.sh`
# when var.proxy_next_config_enabled is true (app-service-proxy*.tf). Empty Startup Command = the
# image default = the live monolith.
#
# Why a script: the Startup Command field splits on whitespace and does NOT honour quotes, so
# `-g "daemon off;"` can't be passed there (it becomes `-g` `"daemon` `off;"` and nginx exits).
# The field runs this whitespace-free command; the quoting lives here, in a real shell.
#
# Why through /docker-entrypoint.sh: with `nginx` as its first argument the image's entrypoint
# runs its /docker-entrypoint.d hooks (envsubst renders the *.template files, among them
# nginx-next.conf.template -> /etc/nginx/nginx-next.conf) and then execs nginx — exactly the
# image's default startup, only pointed at the next entry file. The integration stack boots
# through this same script (docker-compose.next.yml).
exec /docker-entrypoint.sh nginx -c /etc/nginx/nginx-next.conf -g "daemon off;"
