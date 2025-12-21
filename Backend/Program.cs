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
        options.Authority = "http://localhost:8080/identity/";
		options.RequireHttpsMetadata = false;

        options.ClientId = "web";
        options.ClientSecret = "secret";
        options.ResponseType = "code";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");

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

app.MapGet("/weather", () =>
{
	string[] summaries =
	[
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	];
	return Results.Json(Enumerable.Range(1, 5).Select(index => new WeatherForecast
	{
		Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
		TemperatureC = Random.Shared.Next(-20, 55),
		Summary = summaries[Random.Shared.Next(summaries.Length)]
	})
	.ToArray());
});

app.MapGet("/me", (HttpContext httpContext) =>
{
	var user = httpContext.User.Claims.Select(c => new { c.Type, c.Value });
	return Results.Json(user);
}).RequireAuthorization();

app.Run();

public class WeatherForecast
{
	public DateOnly Date { get; set; }

	public int TemperatureC { get; set; }

	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

	public string? Summary { get; set; }
}