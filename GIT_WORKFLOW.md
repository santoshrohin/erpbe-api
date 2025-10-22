# Git Workflow Guide

## Branch Structure

This repository uses a three-branch workflow:

```
main (production)
  └── development (integration)
       └── feature (feature development)
```

### Branch Purposes

1. **`main`** - Production-ready code
   - Only merge from `development` after thorough testing
   - Protected branch (requires pull request reviews)
   - Automatically deploys to production

2. **`development`** - Integration branch
   - Merge feature branches here for testing
   - Integration testing happens here
   - Merge to `main` when stable

3. **`feature`** - Feature development
   - Create feature branches from here
   - Individual features are developed here
   - Merge back to `development` when complete

## Workflow

### 1. Starting a New Feature

```bash
# Switch to feature branch
git checkout feature

# Pull latest changes
git pull origin feature

# Create a new feature branch
git checkout -b feature/your-feature-name
```

### 2. Working on Your Feature

```bash
# Make changes
# ...

# Stage changes
git add .

# Commit with meaningful message
git commit -m "feat: Add new feature description"

# Push to remote
git push origin feature/your-feature-name
```

### 3. Merging Feature to Development

```bash
# Switch to development
git checkout development

# Pull latest changes
git pull origin development

# Merge your feature
git merge feature/your-feature-name

# Run tests
.\scripts\start-tests.ps1

# If tests pass, push to development
git push origin development
```

### 4. Merging Development to Main

```bash
# Switch to main
git checkout main

# Pull latest changes
git pull origin main

# Merge development
git merge development

# Run full CI/CD pipeline
.\scripts\run-local-pipeline.ps1

# If all tests pass, push to main
git push origin main
```

## Commit Message Convention

Follow conventional commits format:

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, etc.)
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

Examples:
```bash
git commit -m "feat: Add user authentication"
git commit -m "fix: Resolve login timeout issue"
git commit -m "docs: Update API documentation"
git commit -m "test: Add unit tests for UserService"
```

## Pull Request Workflow

### Creating a Pull Request

1. Push your feature branch to GitHub
2. Go to GitHub repository
3. Click "New Pull Request"
4. Select your feature branch → target branch
5. Fill in PR description
6. Request reviewers
7. Wait for CI/CD checks to pass

### PR Checklist

- [ ] All tests pass locally
- [ ] Code follows project conventions
- [ ] Documentation updated (if needed)
- [ ] No merge conflicts
- [ ] CI/CD pipeline passes
- [ ] Code reviewed and approved

## Branch Protection Rules

### Main Branch
- Requires pull request reviews (2 reviewers)
- Requires status checks to pass
- No force push allowed
- No direct commits allowed

### Development Branch
- Requires pull request reviews (1 reviewer)
- Requires status checks to pass
- No force push allowed

### Feature Branch
- No restrictions
- Can be force pushed during development

## CI/CD Pipeline

### Automated Checks on Push

1. **Build** - Code compiles successfully
2. **Tests** - All unit and integration tests pass
3. **Linting** - Code style checks pass
4. **Security** - No security vulnerabilities

### Deployment

- **Main branch** → Automatic deployment to production
- **Development branch** → Automatic deployment to staging
- **Feature branches** → No automatic deployment

## Common Git Commands

### Check Current Branch
```bash
git branch
```

### Switch Branches
```bash
git checkout main
git checkout development
git checkout feature
```

### Pull Latest Changes
```bash
git pull origin main
git pull origin development
git pull origin feature
```

### View Commit History
```bash
git log --oneline --graph --all
```

### Undo Last Commit (keep changes)
```bash
git reset --soft HEAD~1
```

### Discard Local Changes
```bash
git checkout -- .
```

### View Branch Differences
```bash
git diff main development
```

## Troubleshooting

### Merge Conflicts

1. Pull latest changes from target branch
2. Resolve conflicts in your editor
3. Stage resolved files: `git add .`
4. Complete merge: `git commit`
5. Push changes

### Accidentally Committed to Wrong Branch

```bash
# Undo last commit (keep changes)
git reset --soft HEAD~1

# Switch to correct branch
git checkout correct-branch

# Commit changes
git add .
git commit -m "Your message"
```

### Need to Update Feature Branch with Latest Development

```bash
# Switch to your feature branch
git checkout feature/your-feature

# Pull latest development changes
git pull origin development

# Resolve any conflicts
# Push updated feature branch
git push origin feature/your-feature
```

## Best Practices

1. **Commit Often** - Small, focused commits are easier to review
2. **Pull Before Push** - Always pull latest changes before pushing
3. **Test Before Merge** - Run tests before merging to development/main
4. **Write Clear Messages** - Use descriptive commit messages
5. **Review Code** - Always review your own code before creating PR
6. **Keep Branches Updated** - Regularly sync with development branch
7. **Delete Merged Branches** - Clean up feature branches after merging

## Quick Reference

```bash
# Create new feature
git checkout feature
git checkout -b feature/new-feature

# Work on feature
git add .
git commit -m "feat: description"
git push origin feature/new-feature

# Merge to development
git checkout development
git pull origin development
git merge feature/new-feature
git push origin development

# Merge to main (after testing)
git checkout main
git pull origin main
git merge development
git push origin main
```

## GitHub Actions Workflows

This repository has automated workflows:

- **Test Pipeline** - Runs on all branches
- **CI/CD Pipeline** - Runs on main and development
- **Database Schema Sync** - Runs on database changes

All workflows must pass before merging to main.
