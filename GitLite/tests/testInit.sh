echo "Testing init..."

pwd

if [[ -d "TestRepo" ]]; then
  rm -rf TestRepo
fi
  
mkdir TestRepo
cd TestRepo
chmod +x ../GitLite
../GitLite init

if [[ ! -d ".gitlite" ]]; then
  echo "Test failed: .gitlite directory not created."
  exit 1
  else
    echo "Test passed: .gitlite directory created."
fi

cd ../
rm -rf TestRepo
