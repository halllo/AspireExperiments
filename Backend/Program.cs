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
	.AddCookie("Cookies")
	.AddOpenIdConnect("oidc", options =>
	{
		options.Authority = "https://localhost:8443/identity/";

		options.ClientId = "web";
		options.ClientSecret = "secret";
		options.ResponseType = "code";

		options.Scope.Clear();
		options.Scope.Add("openid");
		options.Scope.Add("profile");

		options.GetClaimsFromUserInfoEndpoint = true;
		options.MapInboundClaims = false; // Don't rename claim types

		options.SaveTokens = true;
	});

builder.Services.AddAuthorization();

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

app.MapGet("/profile", (HttpContext httpContext) =>
{
	var lookup = httpContext.User.Claims.ToLookup(c => c.Type, c => c.Value);
	var claimsDict = new Dictionary<string, object>();
	foreach (var group in lookup)
	{
		claimsDict[group.Key] = group.Count() == 1 ? group.First() : group.ToArray();
	}
	return Results.Json(claimsDict);
}).RequireAuthorization();

app.MapGet("/login", (HttpContext httpContext) =>
{
	return Results.Redirect("/frontend/");
}).RequireAuthorization();

app.MapGet("/logout", async (HttpContext httpContext) =>
{
	return Results.SignOut(
		properties: new AuthenticationProperties { RedirectUri = "/frontend/" }, 
		authenticationSchemes: ["Cookies", "oidc"]);
}).RequireAuthorization();

app.MapPost("/echo", async (HttpContext httpContext) =>
{
	httpContext.Request.EnableBuffering();
	using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
	var body = await reader.ReadToEndAsync();
	httpContext.Request.Body.Position = 0;
	return Results.Text(body, "application/json");
}).RequireAuthorization();

app.Run();
