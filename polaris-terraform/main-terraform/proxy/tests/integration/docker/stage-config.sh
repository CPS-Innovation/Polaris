#!/bin/sh
# Stage the repo's config/features tree into /etc/nginx/templates/features exactly
# the way the Azure blob mount does, so this stack matches the deployed layout:
#
#   *.conf     -> *.conf.template   20-envsubst-on-templates.sh then renders them
#                                   (substituting ${VARS}) into /etc/nginx/features/,
#                                   where `include features/*/*.conf;` picks them up.
#   *.js       -> copied as-is      envsubst must NOT touch njs: our modules contain
#                                   template literals (${host}, ${args}, ${os} …)
#                                   that it would silently blank.
#   *.test.js  -> skipped           unit tests live beside the module they test and
#                                   are never staged (nor deployed — see the
#                                   .test.js exclusion in app-service-proxy.tf).
#
# The nginx entrypoint runs /docker-entrypoint.d/*.sh in lexical order, so this
# lands before 20-envsubst-on-templates.sh. Net effect: dropping in a new feature
# folder needs NO change to compose, to this script, or to the test runners.
set -e

SRC=/config-features
DST=/etc/nginx/templates/features

[ -d "$SRC" ] || exit 0
mkdir -p "$DST"

find "$SRC" -type f \( -name '*.conf' -o -name '*.js' -o -name '*.html' \)  ! -name '*.test.js' ! -path '*/fixtures/*' | while read -r f; do
  rel="${f#"$SRC"/}"
  case "$rel" in
    *.conf) out="$DST/$rel.template" ;;
    *)      out="$DST/$rel" ;;
  esac
  mkdir -p "$(dirname "$out")"
  cp "$f" "$out"
done
