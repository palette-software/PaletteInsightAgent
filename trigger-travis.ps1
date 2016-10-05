param(
    [Parameter(Mandatory=$true)][string]$VERSION
)

$BRANCH="master"

$TRAVIS_REQUEST_BODY = @{
  request = @{
    message = "Build insight-agent rpm package v$VERSION"
    branch = "$BRANCH"
    config = @{
      branches = @{
        only = "$BRANCH"
      }
      install = "export PALETTE_AGENT_VERSION=$VERSION"
    }
  }
}
$TRAVIS_REQUEST_BODY_JSON = (ConvertTo-Json -Compress -Depth 10 $TRAVIS_REQUEST_BODY)

$TRAVIS_REQUEST_URI = "https://api.travis-ci.com/repo/palette-software%2FPaletteInsightAgent/requests"

$TRAVIS_REQUEST_HEADER = @{
  "Content-Type"       = "application/json"
  "Accept"             = "application/json"
  "Travis-API-Version" = "3"
  "Authorization"      = "token SUHCsC2vxM_BeE_SCV77hw"
}

Invoke-RestMethod -Method Post -Uri $TRAVIS_REQUEST_URI -Body $TRAVIS_REQUEST_BODY_JSON -Header $TRAVIS_REQUEST_HEADER
