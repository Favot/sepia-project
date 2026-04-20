#!/bin/bash
set -a
source .env.prod
set +a
node_modules/.bin/alchemy deploy
