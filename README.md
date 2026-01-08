# AspireExperiments

This experimental web application explores dotnet aspire with a dotnet backend, angular frontend, angular custom element, cookie authentication with csrf protection, yarp gateway with static files, open telemetry with prometheus and grafana.

## Problems

Currently the yarp gateway does not directly support a static port.
<https://github.com/dotnet/aspire/issues/13674>

We can workaround this by registering a `BeforeStartEvent` in which we set the port.
