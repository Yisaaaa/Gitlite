# GitLite Design
This file shows and explain the design of this project.

## Classes
This projects has notable Classes or Objects to work.

* Program - The main file that runs everything. Basically the main entry point of this program.
* Commit - object that represents a single commit. It contains the metadata of a commit including its parent (and possibly a second parent) reference.
* Repository - Contains the methods for a gitlite repository to work -- the main logic.
* Utils - Contains other helper methods such as reading/writing to files, file/dir operations, and error reporting.

## Persistence
The directory structure looks like this.

```
CWD                                     <== The current working directory
  |- .gitlite                           <== All persistent data are stored here
    |- .blobs/                          <== Where blobs are stored
        |- 2a34faalb234a487b4e...
        |- ...
    |- .branches/                       <== All branches are stored here
        |- master
    |- .commits/                        <== All commits are stored here.
        |- 51786ddb3c335f8b7ea...
        |- ...
    |- HEAD                             <== HEAD pointer that tells where we are in
                                            the commit tree. It's a file.
    
```
