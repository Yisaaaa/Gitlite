#!/bin/bash

tests=("../test_init.sh")

echo "Running GitLite tests..."
echo

# building the project
cd ../
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
if [[ ! $? -eq 0 ]]; then
  echo "Building failed."
  exit 1
fi 

echo
clear

cd ./tests

# TODO: Refactor this. We don't want hardcoded path
BUILD_DIR="/home/luis/RiderProjects/GitLite/GitLite/bin/Release/net6.0/linux-x64/publish/GitLite"


if [[ -d "TestRepo" ]]; then
  rm -rf TestRepo
fi
mkdir TestRepo
cd TestRepo

cp $BUILD_DIR ./

passed=0
failed=0

for test in "${tests[@]}"; do
  ./$test
  test_exit_code=$?
  echo
  
  if [[ $test_exit_code -eq 0 ]]; then
    echo "$test passed"
    ((passed++))
  else
    echo "$test failed"
    ((failed++))
  fi
done

rm GitLite
cd ../
rm -rf TestRepo
