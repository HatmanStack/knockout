# GitHub Configuration

This directory contains GitHub-specific configuration for the Knockout repository.

## Contents

- **workflows/** - GitHub Actions workflows
  - `claude-code.yml` - Comment-triggered Claude review (using hypothetical Anthropic action)
  - `claude-pr-review.yml` - Comment-triggered Claude review (using direct API) ⭐ **Recommended**

- **CLAUDE_CODE.md** - Documentation for Claude Code integration

## Quick Setup

### 1. Add Required Secret

Go to: **Settings > Secrets and variables > Actions**

Add secret:
- **Name**: `ANTHROPIC_API_KEY`
- **Value**: Your API key from [console.anthropic.com](https://console.anthropic.com/)

### 2. Choose Workflow

We have two workflows available:

#### Option A: Direct API (Recommended)
Uses `claude-pr-review.yml` - Makes direct API calls to Anthropic.

**Pros:**
- Simple, reliable
- No dependency on external actions
- Easy to customize

**Cons:**
- Requires API key secret
- API usage costs

#### Option B: Anthropic Action
Uses `claude-code.yml` - Attempts to use official Anthropic GitHub Action.

**Note**: This workflow assumes an action exists at `anthropics/anthropic-review-action@v1`. If this doesn't exist, use Option A instead or update the action reference.

### 3. Disable Unused Workflow

If you only want one workflow active:

1. Go to **Actions** tab
2. Select the workflow you don't want
3. Click **"⋯" menu > Disable workflow**

Or simply delete the unused `.yml` file.

### 4. Test It

1. Create a pull request
2. Comment: `@claude please review this`
3. Watch for:
   - 👀 reaction (acknowledged)
   - Review comment from bot
   - 🚀 reaction (completed)

## Usage

### Triggering Reviews

Comment on any PR with `@claude` to trigger a review:

```
@claude review this please

@claude what do you think about the error handling?

Hey @claude, any performance concerns here?
```

### What Gets Reviewed

Claude will receive:
- Full PR diff
- Your comment/question
- Repository context

And provide feedback on:
- Code quality
- Potential bugs
- Best practices
- Performance
- Test coverage
- Documentation

## Workflow Details

### Comment-Triggered Only

Both workflows are configured to **only** run when:
1. A comment is created on a **pull request**
2. The comment contains `@claude`

**They will NOT run on:**
- Every push
- Every PR creation
- Regular issue comments (will notify user)
- Commits

### Permissions Required

Workflows need:
- `contents: read` - Read repository code
- `pull-requests: write` - Post review comments
- `issues: write` - React to comments

These are configured in each workflow file.

## Customization

### Change Claude Model

Edit the workflow file:

```yaml
model: "claude-sonnet-4-5-20250929"  # Current
# or
model: "claude-opus-4-20250514"      # More powerful
```

### Adjust Response Length

```yaml
max_tokens: 8000  # Default
# or
max_tokens: 4000  # Shorter responses
```

### Add Custom Instructions

In `claude-pr-review.yml`, edit the PROMPT section to add instructions:

```bash
PROMPT="You are a senior code reviewer specializing in Unity/C#.

Review this PR focusing on:
- Unity best practices
- Performance optimization
- Memory management

PR Diff:
$(cat pr_diff.txt)
"
```

## Troubleshooting

### Workflow doesn't trigger

- ✅ Check you commented on a **pull request** (not regular issue)
- ✅ Check comment contains `@claude`
- ✅ Check workflow is enabled in Actions tab

### API errors

- ✅ Verify `ANTHROPIC_API_KEY` secret is set
- ✅ Check API key is valid in Anthropic console
- ✅ Check API quota/rate limits

### No response

- ✅ Check Actions tab for workflow run logs
- ✅ Look for error messages in failed steps
- ✅ Verify PR has changes (diff not empty)

## Cost Considerations

### API Usage

Each `@claude` mention triggers:
- 1 API call to Anthropic
- Input tokens: ~1000-10000 (depends on PR size)
- Output tokens: ~1000-8000 (depends on response)

**Estimate**: $0.02-$0.50 per review depending on PR size

### GitHub Actions Minutes

Each workflow run uses:
- ~30-60 seconds of Actions minutes
- Free tier: 2000 minutes/month
- Typical usage: 100-200 reviews/month = 50-200 minutes

## Best Practices

### When to use @claude

✅ **Use for:**
- Significant feature PRs
- Complex refactors
- Security-sensitive changes
- Before merging to main

❌ **Skip for:**
- Typo fixes
- Dependency updates
- Generated code
- Trivial changes

### Review Workflow

1. Create PR
2. Self-review first
3. Address obvious issues
4. Tag `@claude` for AI review
5. Address Claude's feedback
6. Request human review
7. Merge

## GitHub App Alternative

If you installed Claude Code via `/install-github-app`, you may have a GitHub App instead of (or in addition to) these workflows.

**To check:**
1. Go to Settings > GitHub Apps
2. Look for "Claude Code" or similar
3. Configure app behavior there

**Note:** You may want to choose **either** the GitHub App **or** these workflows to avoid duplicate reviews.

## Support

- **Workflow Issues**: Check `.github/CLAUDE_CODE.md`
- **API Issues**: [Anthropic Support](https://support.anthropic.com)
- **GitHub Actions**: [GitHub Docs](https://docs.github.com/actions)

## Version History

- **v1.0.0** (2025-11-19) - Initial setup with comment-triggered workflows
