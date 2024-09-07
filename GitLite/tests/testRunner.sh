#!/bin/bash

tests=("testInit.sh")

echo "Running GitLite tests..."
# building the project

cd ../
pwd
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

cd ./tests
BUILD_DIR="/home/luis/RiderProjects/GitLite/GitLite/bin/Release/net6.0/linux-x64/publish/GitLite"
cp $BUILD_DIR ./

passed=0
failed=0

for test in "${tests[@]}"; do
  ./$test
  if [[ ! $? ]]; then
    echo "$test passed"
    ((passed++))
  else
    echo "$test failed"
    ((failed++))
  fi
done

rm GitLite