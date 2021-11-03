#!/bin/bash
if [ ! $# == 1 ]; then
  echo "Specify a version number, e.g. deploy.sh 0.99"
  exit
fi
version="$1"
echo "Assembling snap."
sed -i '' -e "s/version:.*/version: '${version}'/" ubuntu-core-app/snap/snapcraft.yaml
cd ubuntu-core-app
snapcraft
echo "Uploading version ${version} to snapcraft."
snapcraft upload rivermonitor_${version}_multi.snap
echo "Done."