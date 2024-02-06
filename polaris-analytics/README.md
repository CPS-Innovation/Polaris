# Exporting log analytics queries

## Purpose

A way to keep safe copies of the functions and queries that we have built in log analytics (until a better approach surfaces).
Useful for disaster recovery and tracking changes.

## Steps

- Make sure the `const`s at the top of the `Program.cs` file are accurate (there shouldn't be much reason for these to change).
- In the terminal use `az login` to log in to our tenant.
- Verify you are attached to the prod subscription with `az account show`.
  - If not attached to prod then `az account set --subscription Innovation_Production`
- `cd app` then `dotnet run` to run the export.
