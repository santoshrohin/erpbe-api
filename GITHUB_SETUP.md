# GitHub Setup Guide

## Current Status

✅ **Git Configured**: Repository initialized with remote
✅ **Branches Created**: `main`, `development`, `feature`
✅ **Initial Commit**: All code committed to main branch
❌ **Authentication**: Need to set up GitHub authentication

## Authentication Options

### Option 1: Personal Access Token (Recommended)

1. **Generate a Personal Access Token**
   - Go to: https://github.com/settings/tokens
   - Click "Generate new token" → "Generate new token (classic)"
   - Give it a name: "ErpBE API Development"
   - Select scopes:
     - ✅ `repo` (Full control of private repositories)
     - ✅ `workflow` (Update GitHub Action workflows)
   - Click "Generate token"
   - **Copy the token immediately** (you won't see it again!)

2. **Configure Git to Use Token**
   ```powershell
   # Use token as password when prompted
   git push -u origin main
   # Username: your-github-username
   # Password: paste-your-token-here
   ```

3. **Store Credentials (Optional)**
   ```powershell
   # Windows Credential Manager will remember your token
   git config --global credential.helper wincred
   ```

### Option 2: GitHub CLI (Easiest)

1. **Install GitHub CLI**
   ```powershell
   winget install --id GitHub.cli
   ```

2. **Authenticate**
   ```powershell
   gh auth login
   # Follow the prompts
   ```

3. **Push Code**
   ```powershell
   git push -u origin main
   ```

### Option 3: SSH Key (Most Secure)

1. **Generate SSH Key**
   ```powershell
   ssh-keygen -t ed25519 -C "your-email@example.com"
   # Press Enter to accept default location
   # Enter a passphrase (optional but recommended)
   ```

2. **Add SSH Key to GitHub**
   ```powershell
   # Copy public key
   cat ~/.ssh/id_ed25519.pub | clip
   
   # Go to: https://github.com/settings/keys
   # Click "New SSH key"
   # Paste your key and save
   ```

3. **Update Remote URL**
   ```powershell
   git remote set-url origin git@github.com:santoshrohin/erpbe-api.git
   ```

4. **Push Code**
   ```powershell
   git push -u origin main
   ```

## After Authentication

Once authenticated, push all branches:

```powershell
# Push main branch
git push -u origin main

# Push development branch
git push -u origin development

# Push feature branch
git push -u origin feature

# Verify all branches are pushed
git branch -r
```

## Set Up Branch Protection (Recommended)

After pushing, configure branch protection on GitHub:

1. Go to: https://github.com/santoshrohin/erpbe-api/settings/branches
2. Click "Add rule"
3. For `main` branch:
   - ✅ Require pull request reviews before merging
   - ✅ Require status checks to pass before merging
   - ✅ Require branches to be up to date before merging
   - ✅ Include administrators
4. For `development` branch:
   - ✅ Require pull request reviews before merging
   - ✅ Require status checks to pass before merging

## GitHub Actions Setup

The repository includes CI/CD workflows. After pushing, they will automatically run on:

- **Push to any branch**: Test pipeline
- **Push to main/development**: Full CI/CD pipeline
- **Database changes**: Schema sync workflow

## Troubleshooting

### "Authentication failed"
- Make sure you're using a Personal Access Token, not your GitHub password
- Token must have `repo` scope enabled
- Check if token is expired

### "Permission denied (publickey)"
- SSH key not added to GitHub
- SSH agent not running
- Wrong SSH key being used

### "Remote repository not found"
- Check repository URL: `git remote -v`
- Verify you have access to the repository
- Repository might be private

## Quick Start (After Authentication)

```powershell
# 1. Push all branches
git push -u origin main
git push -u origin development
git push -u origin feature

# 2. Verify branches
git branch -r

# 3. Check GitHub Actions
# Go to: https://github.com/santoshrohin/erpbe-api/actions

# 4. Set up branch protection
# Go to: https://github.com/santoshrohin/erpbe-api/settings/branches
```

## Next Steps

After pushing to GitHub:

1. ✅ Set up branch protection rules
2. ✅ Configure GitHub Actions secrets (database connection strings)
3. ✅ Invite team members (if any)
4. ✅ Create first pull request from feature → development
5. ✅ Review and test the CI/CD pipeline

## Useful Commands

```powershell
# Check remote configuration
git remote -v

# Check current branch
git branch

# View all branches (local and remote)
git branch -a

# Pull latest changes
git pull origin main

# Switch branches
git checkout development

# Create and switch to new branch
git checkout -b feature/new-feature
```
