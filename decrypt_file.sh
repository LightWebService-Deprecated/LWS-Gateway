#!/bin/bash

# Write File
echo "$K3S_ALTUSER_CONFIG" > ./k3s.config.base64
base64 --decode ./k3s.config.base64 > ./k3s.config.gpg

# --batch to prevent interactive command
# --yes to assume "yes" for questions
gpg --quiet --batch --yes --decrypt --passphrase="$K3S_PASSWORD" \
--output ./k3s_config.yaml ./k3s.config.gpg
