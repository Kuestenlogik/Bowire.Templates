---
name: Bug report
about: The template produced bad output, won't restore, won't build, or produces
       failing tests / packages.
title: ''
labels: bug
assignees: ''
---

## What happened

<!-- One or two sentences describing the symptom. -->

## How to reproduce

The exact `dotnet new bowire-plugin` command you used:

```bash
dotnet new bowire-plugin -n ... --Preset ... --IncludeDuplexChannel ...
```

What you did next (`dotnet restore`, `dotnet build`, `dotnet test`, ...):

```bash
# commands that reproduce the bug
```

## Expected vs actual

- **Expected**: <!-- what you thought would happen -->
- **Actual**: <!-- what actually happened, including error messages -->

## Environment

- `Kuestenlogik.Bowire.Templates` version: <!-- e.g. 0.9.4 -->
- `Kuestenlogik.Bowire` version being referenced: <!-- from Directory.Packages.props or the csproj -->
- `dotnet --version`:
- OS (Windows / macOS / Linux + version):
- IDE (if UI-related): <!-- Visual Studio / Rider / VS Code / CLI-only -->

## Additional context

<!-- Smoke-test variant that fails (if applicable), link to a minimal repro repo,
     error logs, etc. -->
