# Default build directory
BUILD = /home/luis/RiderProjects/Gitlite/Gitlite/bin/Release/net6.0/linux-x64/publish/Gitlite

# Testing directory
TESTDIR = ./tests 

FILE = 

.PHONY: all build manual_test test clean

all: clean build test clean

build: $(shell find src -type f)
	echo "Building Gitlite on ${TESTDIR}"
	dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ${TESTDIR}
	cd ${TESTDIR} && rm Gitlite.pdb
	
manual_test: clean build
	cd ${TESTDIR} && \
	mkdir test_tmp_dir && \
	cp Gitlite ./test_tmp_dir/
	
test: build
	source venv/bin/activate && pytest ${FILE} -s -v
	
clean:
	cd ${TESTDIR} && if [ -f Gitlite ]; then rm Gitlite; fi && if [ -d "test_tmp_dir" ]; then rm -rf test_tmp_dir; fi
	