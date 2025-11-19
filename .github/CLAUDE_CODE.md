# Claude Code GitHub Integration

## Overview

This repository uses Claude Code for AI-assisted code review. Claude will only respond when explicitly mentioned in PR comments.

## Usage

### Triggering Claude Review

To trigger a Claude Code review on a pull request:

1. **Comment on the PR** with `@claude` anywhere in the comment
2. Claude will react with 👀 to acknowledge
3. Claude will analyze the PR and provide feedback

**Example comments:**
```
@claude please review this PR

@claude can you check the performance implications?

Hey @claude, what do you think about this approach?
```

### What Claude Reviews

Claude can help with:
- Code quality and best practices
- Potential bugs or issues
- Performance concerns
- Documentation suggestions
- Architecture feedback
- Test coverage analysis

## Configuration

### Required Secrets

For the GitHub Action to work, you need to set up repository secrets:

1. **`ANTHROPIC_API_KEY`** - Your Anthropic API key
   - Go to Settings > Secrets and variables > Actions
   - Click "New repository secret"
   - Name: `ANTHROPIC_API_KEY`
   - Value: Your API key from [Anthropic Console](https://console.anthropic.com/)

2. **`GITHUB_TOKEN`** - Automatically provided by GitHub Actions

### Workflow Configuration

The workflow is defined in `.github/workflows/claude-code.yml`:

- **Trigger**: Only on comments containing `@claude`
- **Scope**: Pull requests only (will notify if used on regular issues)
- **Permissions**: Read contents, write to PRs and issues

### Customizing Claude's Behavior

Edit `.github/workflows/claude-code.yml` to customize:

```yaml
- name: Run Claude Code review
  uses: anthropics/anthropic-review-action@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    anthropic-api-key: ${{ secrets.ANTHROPIC_API_KEY }}
    model: claude-sonnet-4-5-20250929  # Specify model
    max-tokens: 8000                    # Adjust response length
```

## Alternative: GitHub App Integration

If you installed the Claude Code GitHub App (via `/install-github-app`), it may work differently:

1. **App-based**: The app listens for comments automatically
2. **No workflow needed**: The app handles everything server-side
3. **Configuration**: Done through GitHub App settings

### Checking App Installation

1. Go to Repository Settings > GitHub Apps
2. Look for "Claude Code" or "Anthropic Claude"
3. Configure app permissions and behavior there

### App vs Action

- **GitHub App**: Simpler setup, managed by Anthropic
- **GitHub Action**: More control, runs in your Actions

You may want to **choose one approach** to avoid duplicate reviews.

## Disabling Automatic Runs

If you have an existing setup that runs on every push, you can:

### Option 1: Remove push trigger from workflow

If you have a workflow with:
```yaml
on:
  push:
    branches: [main]
  pull_request:
```

Change to:
```yaml
on:
  issue_comment:
    types: [created]
```

### Option 2: Disable workflow temporarily

1. Go to Actions tab
2. Select the workflow
3. Click "Disable workflow"

### Option 3: Configure GitHub App

1. Go to GitHub App settings
2. Adjust "Subscribe to events" to only issue comments
3. Uncheck "Push" event

## Best Practices

### When to Use @claude

✅ **Good times to mention Claude:**
- After significant code changes
- When you need a second opinion
- Before merging important features
- When addressing complex issues

❌ **Avoid mentioning Claude for:**
- Minor typo fixes
- Merge commits
- Automated bot commits
- Simple documentation updates

### Rate Limiting

Be mindful of:
- API usage costs (Claude API calls)
- GitHub Actions minutes (if using Actions)
- Review response time (Claude needs time to analyze)

## Troubleshooting

### Claude doesn't respond

1. **Check the comment**: Did you include `@claude`?
2. **Check the PR**: Is it a pull request (not a regular issue)?
3. **Check Actions**: Go to Actions tab to see if workflow ran
4. **Check logs**: Click on the failed workflow for error details

### Common Errors

**"ANTHROPIC_API_KEY not set"**
- Add the secret in repository settings

**"Insufficient permissions"**
- Ensure workflow has `pull-requests: write` permission

**"API rate limit exceeded"**
- Wait for rate limit to reset or upgrade API tier

## Examples

### Request specific review
```
@claude please focus on the error handling in AuthService.cs
```

### Ask about performance
```
@claude are there any performance concerns with this database query?
```

### Get architectural feedback
```
@claude what do you think about this new component structure?
```

### Request test suggestions
```
@claude what additional tests should I add for this feature?
```

## Workflow Visualization

```
PR Comment with @claude
        ↓
GitHub detects comment
        ↓
Workflow triggered (or App notified)
        ↓
Claude analyzes PR diff
        ↓
Claude posts review comment
        ↓
Developer reads feedback
```

## Support

- **Claude Code Docs**: [claude.ai/docs](https://claude.ai/docs)
- **GitHub Actions**: [docs.github.com/actions](https://docs.github.com/actions)
- **Anthropic Support**: [support.anthropic.com](https://support.anthropic.com)

## Version

- **Workflow Version**: 1.0.0
- **Last Updated**: 2025-11-19
- **Claude Model**: claude-sonnet-4-5-20250929
