# Commit Message Template Fix Required

## Problem

All phase files contain commit message templates with:
1. Author/Committer lines in message body (won't work - need git config)
2. Co-Authored-By lines (should be removed per user request)
3. Emoji characters causing file encoding issues

## Solution

### 1. Configure Git (One-Time Setup)

Before starting migration, run:

```bash
git config user.name "HatmanStack"
git config user.email "82614182+HatmanStack@users.noreply.github.com"
```

Or for session-based (recommended for automation):

```bash
export GIT_AUTHOR_NAME="HatmanStack"
export GIT_AUTHOR_EMAIL="82614182+HatmanStack@users.noreply.github.com"
export GIT_COMMITTER_NAME="HatmanStack"
export GIT_COMMITTER_EMAIL="82614182+HatmanStack@users.noreply.github.com"
```

### 2. Updated Commit Message Template

**Use this format for all commits:**

```
<type>(<scope>): <description>

<body - optional>
```

**That's it.** No Author/Committer/Co-Authored-By lines.

### 3. Files That Need Template Updates

All phase files (Phase-0.md through Phase-7.md) have commit message templates that need cleanup.

**Current (incorrect) template in phase files:**
```
<type>(<scope>): <description>

<body>

[emoji] Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>

Author: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
Committer: HatmanStack <82614182+HatmanStack@users.noreply.github.com>
```

**Should be:**
```
<type>(<scope>): <description>

<body>
```

### 4. Quick Fix Script

To fix all commit message templates in phase files:

```bash
# This is a manual fix - the emoji characters make automated replacement difficult
# Review each phase file and update commit message templates to the simple format above
```

### 5. Phase-0.md Fix

Phase-0.md "Conventional Commits Format" section should have:

```markdown
### Git Configuration

**IMPORTANT:** Configure git to use HatmanStack as author/committer before starting:

```bash
git config user.name "HatmanStack"
git config user.email "82614182+HatmanStack@users.noreply.github.com"
```

Or set per-session using environment variables:
```bash
export GIT_AUTHOR_NAME="HatmanStack"
export GIT_AUTHOR_EMAIL="82614182+HatmanStack@users.noreply.github.com"
export GIT_COMMITTER_NAME="HatmanStack"
export GIT_COMMITTER_EMAIL="82614182+HatmanStack@users.noreply.github.com"
```

### Conventional Commits Format

All commits must follow this format:

```
<type>(<scope>): <description>

<body - optional>
```

**Types:**
- `feat`: New feature or enhancement
- `fix`: Bug fix
- `refactor`: Code restructuring without behavior change
- `test`: Test-related changes
- `chore`: Tooling, dependencies, project maintenance
- `docs`: Documentation updates
- `perf`: Performance improvements

**Scopes:**
- `migration`: Upgrade-related changes
- `urp`: Universal Render Pipeline changes
- `input`: Input System changes
- `tests`: Test suite updates
- `combat`, `ai`, `ui`, `audio`: Component-specific changes

**Examples:**
```
fix(migration): resolve URP shader compilation errors

Updated custom shaders to Unity 6 URP API
```

```
test(combat): fix CharacterCombatTests for Unity 6

Updated test assertions to match new physics behavior
```
```

## Summary

**For Implementation Engineer:**

1. Run git config commands above BEFORE starting
2. Use simple commit message format (no Co-Authored-By, no Author/Committer lines)
3. Ignore the complex templates in phase files - use the simple format instead
4. Commits will automatically use HatmanStack as author/committer from git config

**Status:** 
- Git configuration instructions documented ✓
- Simple commit template defined ✓
- Phase files still have old templates (can be ignored by engineer)
