# GitHub Copilot Instructions

This repository uses Super-Linter for automated code quality checks. Make sure your changes pass all linter rules before submitting.

`script/lint` has been provided to allow you to run `super-linter` locally, with the same configuration as the workflow. You must
use this script to ensure that all of your changes are correctly linted / formatted **before** committing your changes, this step
is not optional.

## **CRITICAL: autofix First Policy**

**Before manually addressing any linting issues, you MUST first run the autofix mode:**

```bash
./script/lint --fix
```

**This is mandatory and non-negotiable.** The autofix mode will automatically resolve most formatting and many linting issues, including:

- Shell script formatting (shfmt)
- C# formatting (dotnet format)
- CSS/SCSS formatting and linting
- JavaScript/TypeScript formatting and linting
- JSON formatting
- Markdown formatting
- YAML formatting
- Python formatting (black, isort, ruff)

**Only after running autofix should you manually address any remaining issues.** Manual fixes should be surgical and minimal - focusing only on issues that cannot be automatically resolved.

**Workflow:**

1. **First**: Run `./script/lint --fix` to autofix all possible issues
2. **Second**: Review and commit the auto-fixed changes
3. **Third**: Run `./script/lint` to identify remaining issues
4. **Finally**: Manually fix only the remaining issues that cannot be auto-fixed

**Note**: EditorConfig violations cannot be auto-fixed and must be addressed manually by ensuring proper file endings, whitespace, and indentation.

## Code Formatting Guidelines

This repository uses an `.editorconfig` file to define coding standards. When making code changes, ensure compliance with the following formatting rules:

### General Formatting Rules

- **Trailing whitespace**: Remove all trailing whitespace from lines
- **Final newline**: Always insert a final newline at the end of files
- **Charset**: Use UTF-8 encoding
- **Line endings**: Use auto line endings
- **Indentation**: Use consistent indentation as defined in `.editorconfig`

### C# Specific Rules

- **Indentation**: Use 4 spaces (no tabs) for indentation
- **Braces**: Place opening braces on new lines
- **Spacing**: Follow standard C# spacing conventions
- **Naming conventions**:
  - Use PascalCase for public members and types
  - Use camelCase with underscore prefix for private fields
  - Follow the naming conventions defined in `.editorconfig`
- **Type usage**: Prefer to use var unless the resulting type is not obvious, e.g. `var foo = new Bar();` is acceptable
- **Global usings**: Never enable global usings
- **Dynamic typing**: Never use the `dynamic` keyword
- **Null safety**: Never use null forgiving operator without having first tried other, more robust, methods
- **Structure**: Each source file must only ever define one type, e.g. class, interface, enum, etc.

### Before Committing

Always ensure your code passes the linting checks:

- Remove trailing whitespace from all lines
- Ensure files end with a single newline
- Verify proper indentation (4 spaces for C#)
- Check that formatting matches the `.editorconfig` specifications
- Follow the existing code style and conventions in the repository

## Testing Guidelines

- **One test, one assert**: Each test method should verify a single behavior with a single assertion
- **Use parameterized tests**: When testing similar scenarios with different inputs, use `[TestCaseSource]` to eliminate code duplication
- **Test naming**: Use descriptive test method names that clearly indicate what is being tested and the expected outcome
- **Avoid code duplication**: If you find yourself copying test code, refactor to use parameterized tests instead
- **No reflection**: Always look for a clean solution that does not require the use of reflection

## Code Duplication Prevention

- Before creating similar classes or methods, consider if they can be generalized or parameterized
- Use inheritance or composition patterns when appropriate
- Extract common functionality into shared base classes or helper methods
- For test data, use test case sources rather than duplicating test methods

## EditorConfig Compliance

- Respect all settings defined in `.editorconfig`
- Let Git automatically manage line endings unless there are specific tool compatibility issues
- Follow indentation rules (spaces vs tabs as configured per file type)
- Maintain consistent formatting across all file types
