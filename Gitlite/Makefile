# Default build directory
BUILD = /home/luis/RiderProjects/Gitlite/Gitlite/bin/Release/net6.0/linux-x64/publish/Gitlite

# Testing directory
TESTDIR = ./tests 

all: build clean

build: $(shell find src -type f)
	echo "Building Gitlite on ${TESTDIR}"
	dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ${TESTDIR}
	cd ${TESTDIR} && rm Gitlite.pdb
	
clean:
	cd ${TESTDIR} && rm Gitlite
	