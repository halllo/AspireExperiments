using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

OpenTelemetryExtensions.ConfigureOpenTelemetry(builder);

builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(options =>
	{
		options.DefaultScheme = "Cookies";
		options.DefaultChallengeScheme = "oidc";
	})
	.AddCookie("Cookies", options =>
	{
		options.Cookie.SameSite = SameSiteMode.Strict;
	})
	.AddOpenIdConnect("oidc", options =>
	{
		options.Authority = "https://gateway-aspireexperiments.dev.localhost:8443/identity/";

		options.ClientId = "web";
		options.ClientSecret = "secret";
		options.ResponseType = "code";

		options.Scope.Clear();
		options.Scope.Add("openid");
		options.Scope.Add("profile");

		options.GetClaimsFromUserInfoEndpoint = true;
		options.MapInboundClaims = false; // Don't rename claim types

		options.SaveTokens = true;

		options.Events.OnTicketReceived = context =>
		{
			// Remove the token values from the cookie to reduce its size.
			context.Properties?.UpdateTokenValue("access_token", string.Empty); //we keep the id token for logout
			context.Properties?.UpdateTokenValue("token_type", string.Empty);
			context.Properties?.UpdateTokenValue("expires_at", string.Empty);
			context.Properties?.Items.Remove(".checkSessionIFrame");
			return Task.CompletedTask;
		};
	});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		// policy.WithOrigins(["https://localhost:9443"])
		// 	  .WithMethods(["GET", "POST", "PUT", "DELETE"])
		// 	  .WithHeaders(["content-type", "x-csrf"])
		// 	  .AllowCredentials();
	});
});

var app = builder.Build();

app.UsePathBase("/backend");
app.UseHeaderInspectionBefore();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
	ForwardedHeaders =
	  ForwardedHeaders.XForwardedFor
	| ForwardedHeaders.XForwardedHost
	| ForwardedHeaders.XForwardedProto
});

app.UseCors();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();

	// All health checks must pass for app to be considered ready to accept traffic after starting
	app.MapHealthChecks("/health");

	// Only health checks tagged with the "live" tag must pass for app to be considered alive
	app.MapHealthChecks("/alive", new HealthCheckOptions
	{
		Predicate = r => r.Tags.Contains("live")
	});
}

app.UseHeaderInspectionAfter();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext context) =>
{
	//redirect requests to /backend to /backend/
	if (string.IsNullOrWhiteSpace(context.Request.Path))
		return Results.Redirect(context.Request.PathBase + "/");

	return Results.Text("Hello from Backend!");
});

app.MapGet("/login", (HttpContext httpContext, bool redirectback = true) =>
{
	if (redirectback)
	{
		return Results.Redirect("/frontend/");
	}
	else
	{
		var html = """
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="UTF-8">
			<title>Login Successful</title>
		</head>
		<body>
			<script>
				window.parent.postMessage({ type: 'loggedin' }, '*');
			</script>
			<p>Login successful</p>
		</body>
		</html>
		""";
		return Results.Content(html, "text/html");
	}
}).RequireAuthorization();

app.MapGet("/logout", async (HttpContext httpContext) =>
{
	return Results.SignOut(
		properties: new AuthenticationProperties { RedirectUri = "/frontend/" },
		authenticationSchemes: ["Cookies", "oidc"]);
}).RequireAuthorization();

var apiGroup = app.MapGroup("")
	.RequireAuthorization()
	.AddEndpointFilter(async (context, next) =>
	{
		//Additional CSRF protection using Anti-forgery Header
		if (!context.HttpContext.Request.Headers.ContainsKey("x-csrf"))
		{
			return Results.BadRequest("Missing X-CSRF header");
		}
		else
		{
			return await next(context);
		}
	});

apiGroup.MapGet("/profile", (HttpContext httpContext, ILogger<Program> logger) =>
{
	logger.LogInformation("Getting profile for user {user}", httpContext.User.Name() ?? "unknown");
	var lookup = httpContext.User.Claims.ToLookup(c => c.Type, c => c.Value);
	var claimsDict = new Dictionary<string, object>();
	foreach (var group in lookup)
	{
		claimsDict[group.Key] = group.Count() == 1 ? group.First() : group.ToArray();
	}
	return Results.Json(claimsDict);
});

apiGroup.MapPost("/echo", async (HttpContext httpContext, ILogger<Program> logger) =>
{
	logger.LogInformation("Echo (POST) endpoint called by user {user}", httpContext.User.Name() ?? "unknown");
	httpContext.Request.EnableBuffering();
	using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
	var body = await reader.ReadToEndAsync();
	httpContext.Request.Body.Position = 0;
	return Results.Text(body, "application/json");
});

apiGroup.MapGet("/echo", async (HttpContext httpContext, ILogger<Program> logger) =>
{
	logger.LogInformation("Echo (GET) endpoint called by user {user}", httpContext.User.Name() ?? "unknown");
	var body = "GET does not have a body";
	return Results.Text(body, "application/json");
});

app.Run();

static class ClaimsPrincipalExtensions
{
	extension(ClaimsPrincipal user)
	{
		public string? Name() => user.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
	}
}
