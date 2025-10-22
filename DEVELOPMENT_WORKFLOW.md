# Development Workflow - Feature Branch Strategy

## Branch Structure

```
main (production)
  ↑ PR/MR
development (integration/testing)
  ↑ PR/MR
feature (feature development)
```

## Golden Rules

1. **NEVER commit directly to `main`** - Always use Pull Requests
2. **NEVER commit directly to `development`** - Always use Pull Requests
3. **Work on `feature` branch** or create feature branches from it
4. **Test before creating PR** - Ensure tests pass locally
5. **One feature = One PR** - Keep PRs focused and small

---

## Daily Development Workflow

### Step 1: Start Working (Every Day)

```powershell
# Switch to feature branch
git checkout feature

# Pull latest changes from remote
git pull origin feature

# Verify you're on feature branch
git branch
# Should show: * feature
```

### Step 2: Make Your Changes

```powershell
# Make your code changes in Visual Studio or VS Code
# ...

# Check what files changed
git status

# View your changes
git diff
```

### Step 3: Test Your Changes Locally

```powershell
# Start test environment
.\scripts\run-visual-studio-tests.ps1

# Run tests from Visual Studio or command line
dotnet test

# Run full pipeline to ensure everything works
.\scripts\run-local-pipeline.ps1
```

### Step 4: Commit Your Changes

```powershell
# Stage all changes
git add .

# Or stage specific files
git add path/to/specific/file.cs

# Commit with meaningful message
git commit -m "feat: Add user profile management"

# Follow conventional commit format:
# - feat: New feature
# - fix: Bug fix
# - docs: Documentation
# - refactor: Code refactoring
# - test: Add/update tests
# - chore: Maintenance
```

### Step 5: Push to Feature Branch

```powershell
# Push your changes to remote feature branch
git push origin feature

# Verify push succeeded
git log --oneline -5
```

---

## Creating Pull Requests / Merge Requests

### Step 1: Feature → Development (Testing/Integration)

#### A. Push Feature Branch
```powershell
# Ensure feature branch is up to date
git checkout feature
git pull origin feature

# Push your latest changes
git push origin feature
```

#### B. Create Pull Request on GitHub

1. **Go to GitHub**:
   - Visit: https://github.com/santoshrohin/erpbe-api
   
2. **Create PR**:
   - Click "Pull requests" tab
   - Click "New pull request"
   - Set: `base: development` ← `compare: feature`
   - Click "Create pull request"

3. **Fill PR Details**:
   ```
   Title: feat: User profile management module
   
   Description:
   ## Changes
   - Added user profile CRUD operations
   - Implemented profile validation
   - Added unit tests for profile service
   
   ## Testing
   - ✅ All unit tests passing (51/51)
   - ✅ Integration tests passing
   - ✅ Manual testing completed
   
   ## Screenshots (if applicable)
   [Add screenshots]
   
   ## Checklist
   - [x] Tests added/updated
   - [x] Documentation updated
   - [x] No breaking changes
   - [x] Code reviewed locally
   ```

4. **Request Review** (if team members exist):
   - Add reviewers on the right sidebar
   - Assign yourself
   - Add labels: "enhancement", "feature", etc.

5. **Wait for CI/CD Checks**:
   - GitHub Actions will run automatically
   - All checks must pass before merging

6. **Merge PR**:
   - Once approved and checks pass, click "Merge pull request"
   - Choose merge strategy:
     - **"Create a merge commit"** (Recommended - keeps full history)
     - "Squash and merge" (Combines all commits into one)
     - "Rebase and merge" (Linear history)
   - Click "Confirm merge"
   - **Delete feature branch** (optional): Click "Delete branch" button

### Step 2: Development → Main (Production Release)

#### A. After Thorough Testing on Development

```powershell
# Pull latest development branch
git checkout development
git pull origin development

# Run comprehensive tests
.\scripts\run-local-pipeline.ps1

# If all tests pass, proceed to create PR
```

#### B. Create Pull Request on GitHub

1. **Go to GitHub**:
   - Visit: https://github.com/santoshrohin/erpbe-api/pulls
   
2. **Create PR**:
   - Click "New pull request"
   - Set: `base: main` ← `compare: development`
   - Click "Create pull request"

3. **Fill PR Details**:
   ```
   Title: Release v1.2.0 - User Management Features
   
   Description:
   ## Release Notes
   
   ### New Features
   - User profile management
   - Role-based dashboard
   - Export functionality
   
   ### Bug Fixes
   - Fixed login timeout issue
   - Resolved pagination bug
   
   ### Changes
   - Updated API documentation
   - Improved error handling
   
   ## Testing
   - ✅ All 51 tests passing
   - ✅ Integration tests passed
   - ✅ Manual QA completed
   - ✅ Performance testing done
   
   ## Deployment Checklist
   - [x] Database migrations ready
   - [x] Environment variables updated
   - [x] Rollback plan prepared
   - [x] Documentation updated
   ```

4. **Require Approvals**:
   - Main branch should require 2+ approvals
   - All checks must pass
   - No merge conflicts

5. **Merge to Production**:
   - After approvals, click "Merge pull request"
   - Use "Create a merge commit" for production releases
   - Click "Confirm merge"
   - **DO NOT delete development branch**

6. **Post-Merge Actions**:
   ```powershell
   # Pull latest main branch
   git checkout main
   git pull origin main
   
   # Tag the release
   git tag -a v1.2.0 -m "Release version 1.2.0"
   git push origin v1.2.0
   
   # Sync development with main
   git checkout development
   git merge main
   git push origin development
   
   # Sync feature with development
   git checkout feature
   git merge development
   git push origin feature
   ```

---

## Creating Feature Branches (Optional Advanced Workflow)

For larger features, create dedicated feature branches:

### Creating Feature Branch

```powershell
# Start from feature branch
git checkout feature
git pull origin feature

# Create new feature branch
git checkout -b feature/user-profile-management

# Work on your feature
# ... make changes ...

# Commit and push
git add .
git commit -m "feat: Add user profile API endpoints"
git push origin feature/user-profile-management
```

### Merging Feature Branch

1. **Create PR**: `feature/user-profile-management` → `feature`
2. **After merge**, delete the feature branch:
   ```powershell
   git checkout feature
   git pull origin feature
   git branch -d feature/user-profile-management
   git push origin --delete feature/user-profile-management
   ```

---

## Handling Merge Conflicts

### When Creating PR to Development

```powershell
# 1. Update your feature branch with latest development
git checkout feature
git pull origin development

# 2. If conflicts occur, resolve them
# Open conflicted files in VS Code/Visual Studio
# Look for conflict markers:
# <<<<<<< HEAD
# your changes
# =======
# their changes
# >>>>>>> development

# 3. After resolving, stage and commit
git add .
git commit -m "fix: Resolve merge conflicts with development"

# 4. Push updated branch
git push origin feature

# 5. PR will automatically update
```

### When Creating PR to Main

```powershell
# 1. Update development with latest main
git checkout development
git pull origin main

# 2. Resolve conflicts if any
# ... resolve conflicts ...
git add .
git commit -m "fix: Resolve merge conflicts with main"

# 3. Push updated branch
git push origin development

# 4. PR will automatically update
```

---

## Commit Message Guidelines

### Format
```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation only
- **style**: Code style (formatting, semicolons, etc.)
- **refactor**: Code refactoring
- **perf**: Performance improvements
- **test**: Adding/updating tests
- **chore**: Maintenance (dependencies, build, etc.)

### Examples

```bash
# Good
git commit -m "feat(auth): Add JWT token refresh functionality"
git commit -m "fix(api): Resolve null reference in user controller"
git commit -m "docs: Update API documentation for user endpoints"
git commit -m "test(unit): Add tests for user service"

# With body
git commit -m "feat(auth): Add OAuth2 authentication

- Implemented OAuth2 client
- Added Google and GitHub providers
- Updated authentication middleware

Closes #123"
```

---

## Pull Request Review Checklist

### For Reviewer

- [ ] Code follows project conventions
- [ ] No unnecessary code changes
- [ ] Tests are included and passing
- [ ] Documentation updated (if needed)
- [ ] No security vulnerabilities
- [ ] Performance impact considered
- [ ] No breaking changes (or properly documented)

### For Author

- [ ] Self-reviewed the code
- [ ] All tests passing locally
- [ ] No console errors/warnings
- [ ] Documentation updated
- [ ] Commit messages are clear
- [ ] PR description is detailed
- [ ] Screenshots added (if UI changes)

---

## Common Scenarios

### Scenario 1: Quick Bug Fix

```powershell
# 1. Switch to feature branch
git checkout feature
git pull origin feature

# 2. Fix the bug
# ... make changes ...

# 3. Test
dotnet test

# 4. Commit and push
git add .
git commit -m "fix: Resolve login timeout issue"
git push origin feature

# 5. Create PR: feature → development
# 6. After merge and testing, create PR: development → main
```

### Scenario 2: New Feature Development

```powershell
# 1. Create feature branch
git checkout feature
git checkout -b feature/user-dashboard

# 2. Develop feature over multiple days
git add .
git commit -m "feat: Add dashboard layout"
git push origin feature/user-dashboard

# ... next day ...
git add .
git commit -m "feat: Add dashboard widgets"
git push origin feature/user-dashboard

# 3. Create PR: feature/user-dashboard → feature
# 4. After merge, create PR: feature → development
# 5. After testing, create PR: development → main
```

### Scenario 3: Hotfix to Production

```powershell
# 1. Create hotfix branch from main
git checkout main
git pull origin main
git checkout -b hotfix/critical-security-fix

# 2. Fix the issue
# ... make changes ...

# 3. Test thoroughly
.\scripts\run-local-pipeline.ps1

# 4. Commit and push
git add .
git commit -m "fix: Critical security vulnerability in auth"
git push origin hotfix/critical-security-fix

# 5. Create PR: hotfix/critical-security-fix → main
# 6. After merge to main, also merge to development and feature
git checkout development
git merge main
git push origin development

git checkout feature
git merge development
git push origin feature
```

---

## Branch Protection Rules (GitHub Settings)

### Main Branch Protection

1. Go to: https://github.com/santoshrohin/erpbe-api/settings/branches
2. Add rule for `main`:
   - ✅ Require pull request reviews (2 reviewers)
   - ✅ Dismiss stale PR approvals when new commits pushed
   - ✅ Require status checks to pass
   - ✅ Require branches to be up to date
   - ✅ Require conversation resolution before merging
   - ✅ Include administrators
   - ✅ Restrict who can push (only via PR)

### Development Branch Protection

1. Add rule for `development`:
   - ✅ Require pull request reviews (1 reviewer)
   - ✅ Require status checks to pass
   - ✅ Require branches to be up to date
   - ✅ Include administrators

### Feature Branch

- No protection needed
- Direct commits allowed
- Can force push if needed

---

## Quick Reference Commands

```powershell
# Daily start
git checkout feature
git pull origin feature

# Check status
git status
git log --oneline -10

# Make changes and commit
git add .
git commit -m "feat: Your feature description"
git push origin feature

# Create PR on GitHub:
# feature → development → main

# Update feature branch with latest development
git checkout feature
git pull origin development
git push origin feature

# Update development with latest main
git checkout development
git pull origin main
git push origin development

# View all branches
git branch -a

# Delete local branch
git branch -d feature/old-feature

# Delete remote branch
git push origin --delete feature/old-feature
```

---

## Important Notes

1. **Always work on `feature` branch** - Your main development branch
2. **Use PRs for everything** - Never push directly to development or main
3. **Test before creating PR** - Save reviewers' time
4. **Keep PRs small** - Easier to review and merge
5. **Update often** - Pull from development regularly to avoid conflicts
6. **Clear commit messages** - Help others understand your changes
7. **Delete merged branches** - Keep repository clean
8. **Tag releases** - Mark production releases with version tags

---

## Troubleshooting

### "Your branch is behind"
```powershell
git pull origin feature
```

### "Merge conflicts"
```powershell
# Pull the target branch
git pull origin development
# Resolve conflicts in files
# Stage and commit
git add .
git commit -m "fix: Resolve merge conflicts"
git push origin feature
```

### "PR shows unwanted commits"
```powershell
# Ensure your branch is up to date
git checkout feature
git pull origin development
git push origin feature
```

### "Need to undo last commit"
```powershell
# Undo commit but keep changes
git reset --soft HEAD~1

# Undo commit and discard changes (dangerous!)
git reset --hard HEAD~1
```

---

**Remember: Communication is key! Discuss with your team before merging large changes.**
