echo "Testing init..."

chmod +x ./GitLite

failed=false


testInit() {
  echo "Test init in an uninitialized directory"
  
  output=$(./GitLite init)
  echo $output
  
  if [[ ! -d ".gitlite" || ! -d ".gitlite/commits" || ! -d ".gitlite/blobs" ]]; then
    echo "Test failed: .gitlite directory not created."
    failed=true
  fi
  
}

testAlreadyInitialized() {
  echo "Test init in an already initialized directory"
  
  output=$(./GitLite init)
  
  if [[ ! $output == "A Gitlet version-control system already exists in the current directory." ]];
    then
      echo "Must not re-initialize in an already initialized directory"
      failed=true
  fi
  
}

testInit
echo
testAlreadyInitialized

if $failed; then
  exit 1
else
  exit 0
fi

