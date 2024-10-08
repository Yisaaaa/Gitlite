# Default build directory
BUILD = /home/luis/RiderProjects/Gitlite/Gitlite/bin/Release/net6.0/linux-x64/publish/Gitlite

# Testing directory
TESTDIR = ./tests 

FILE = 

.PHONY: all build manual_test test clean

all: 
	cd .. && make clean && make build && make test && make clean

build: 
	cd .. && make build
	
manual_test:
	cd .. && make manual_test
	
test:
	cd .. && make test FILE=${FILE}
	
clean:
	cd .. && make clean