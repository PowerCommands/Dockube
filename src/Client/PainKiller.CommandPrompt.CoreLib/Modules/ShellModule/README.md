# Shell Module Version 1.0
The ShellModule is designed to facilitate navigation and basic file operations within the CommandPrompt environment. It provides several commands to change directories, list content, manage files, and interact with the operating system.

## Commands:

CdCommand

DirCommand

FileCommand

## Services:

### IOService

Description: Provides file and folder management operations, including copying entire folders.

Methods:

CopyFolder: Recursively copies files and subdirectories from a source folder to a destination folder.

### ShellService
Description: Executes shell operations such as opening directories, running programs, and opening files with the default system application.

### ZipService
Handles Zip archives, zip, unzip.