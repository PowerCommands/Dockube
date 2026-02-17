# Git Module Version 1.0

GitModule provides lightweight Git CLI integration inside CommandPrompt.

The module contains a single command: **GitCommand**.

All Git operations are executed using the system Git CLI through `ShellService` and run against the repository configured in:

`Configuration.Core.Modules.Git.DefaultRepositoryPath`

---

## Purpose

GitModule allows developers to execute common Git operations directly from CommandPrompt without introducing any abstraction layer over Git.

It delegates directly to the Git executable to ensure **predictable and standard behavior**.

---

## Supported Operations

### Basic Commands

- `git status`
- `git log`
- `git push`
- `git commit "message"`

If no commit message is provided, the default message used is:

`refactoring`

---

### Branch Management

- `git branch --create my-branch`
- `git branch --change my-branch`
- `git branch --delete my-branch`
- `git branch --merge my-branch`
- `git branch --main`

Branch operations internally use standard Git commands such as:

- `checkout`
- `branch`
- `merge`
- `push --set-upstream`

Deleting a branch will:

1. Switch to `main`
2. Delete the branch locally
3. Prompt for optional remote deletion

---

### Merge Behavior

`git merge "feature-x"`

The command always switches to `main` before performing the merge.

---

### Relative Path Detection

`git --relative-path`

Traverses upward from the application base directory until a `.git` folder is found and prints the relative path.

Traversal depth is limited to prevent infinite loops.

---

## Configuration

GitModule requires repository path configuration:

```yaml
Core:
  Modules:
    Git:
      DefaultRepositoryPath: "C:\\Projects\\MyRepository"
```

The configured path must:

- Exist
- Contain a valid `.git` directory
- Be accessible by the process

---

## Design Principles

- No Git abstraction layer
- No internal repository model
- Direct CLI delegation
- Minimal surface area
- Predictable behavior

GitModule is intentionally simple and transparent, designed to integrate Git workflows directly into CommandPrompt with minimal complexity.
