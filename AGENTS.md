# Copilot instructions

This repository is set up to use Aspire. Aspire is an orchestrator for the entire application and will take care of configuring dependencies, building, and running the application. The resources that make up the application are defined in `apphost.cs` including application code and external dependencies.

## General recommendations for working with Aspire

1. Before making any changes always run the apphost using `aspire run` and inspect the state of resources to make sure you are building from a known state.
1. Changes to the _apphost.cs_ file will require a restart of the application to take effect.
1. Make changes incrementally and run the aspire application using the `aspire run` command to validate changes.
1. Use the Aspire MCP tools to check the status of resources and debug issues.

## Using the application

When the application is running, access the frontend at <https://localhost:8443/frontend/>.
