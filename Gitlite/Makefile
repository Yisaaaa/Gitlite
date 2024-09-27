# Default build directory
BUILD = /home/luis/RiderProjects/Gitlite/Gitlite/bin/Release/net6.0/linux-x64/publish/Gitlite

# Testing directory
TESTDIR = ./tests 

all: build test clean

build: $(shell find src -type f)
	echo "Building Gitlite on ${TESTDIR}"
	dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ${TESTDIR}
	cd ${TESTDIR} && rm Gitlite.pdb
	
manual_test: build
	cd ${TESTDIR} && \
	mkdir test_temp_dir && \
	cp Gitlite test_temp_dir
	
test: build
	venv/bin/pytest -s -v
	
clean:
	cd ${TESTDIR} && if [ -f Gitlite ]; then rm Gitlite; fi && if [ -d "test_tmp_dir" ]; then rm -rf test_tmp_dir; fi
	