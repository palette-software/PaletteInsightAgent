#!/bin/bash

if [ $# -ne 1 ]; then
  echo "Usage $0 <version>"
  exit 1
fi

VERSION="$1"
BRANCH="master"

TRAVIS_REQUEST_BODY="{
\"request\": {
  \"message\": \"Build insight-agent rpm package v${VERSION}\",
  \"branch\": \"${BRANCH}\",
  \"config\": {
    \"branches\": {
      \"only\": \"${BRANCH}\"
    },
    \"install\": \"export PALETTE_AGENT_VERSION=${VERSION}\"
  }
}}"

curl -s -X POST \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Travis-API-Version: 3" \
  -H "Authorization: token SUHCsC2vxM_BeE_SCV77hw" \
  -d "${TRAVIS_REQUEST_BODY}" \
  https://api.travis-ci.com/repo/palette-software%2FPaletteInsightAgent/requests
