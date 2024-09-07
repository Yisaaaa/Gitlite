echo "Testing init..."

chmod +x ./GitLite
./GitLite init

if [[ ! -d ".gitlite" ]]; then
  echo "Test failed: .gitlite directory not created."
  exit 1
  else
    echo "Test passed: .gitlite directory created."
  exit 0
fi