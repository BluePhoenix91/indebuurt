# indebuurt
A web app that lets you check out new neighbourhoods and tries to help you find the best matching neighbourhoods in Flanders.

## Developer Setup

### Git Hooks (optional but recommended)

We use [Lefthook](https://github.com/evilmartians/lefthook) for git hooks. Install it once:

```bash
# macOS
brew install lefthook

# Windows (scoop)
scoop install lefthook

# Or via npm (global)
npm install -g lefthook
```

Then activate the hooks:

```bash
lefthook install
```

This sets up a pre-commit hook that validates agent schemas when you modify them.
