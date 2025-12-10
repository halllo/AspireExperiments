public static class MiddlewareForRequestDiagnosticForDebugging
{
    static readonly string inspectPath = "/show-me-my-headers";
    
    public static void UseHeaderInspectionBeforeAfter(this IApplicationBuilder app, Action<IApplicationBuilder> inbetween)
    {
        app.UseHeaderInspectionBefore();
        inbetween(app);
        app.UseHeaderInspectionAfter();
    }

    public static void UseHeaderInspectionBefore(this IApplicationBuilder app)
    {
        app.Use(Before(inspectPath));
    }

    public static void UseHeaderInspectionAfter(this IApplicationBuilder app)
    {
        app.Use(After(inspectPath));
    }





    private static Func<HttpContext, Func<Task>, Task> Before(string inspectPath)
    {
        return async (context, next) =>
        {
            if (context.Request.Path.Equals(inspectPath))
            {
                var beforeHeaders = context.Request.Headers.ToArray();
                var beforePath = context.Request.Path;
                var beforePathBase = context.Request.PathBase;

                await next.Invoke();

                var afterHeaders = context.Request.Headers.ToArray();
                var afterPath = context.Request.Path;
                var afterPathBase = context.Request.PathBase;

                await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
<head>
<title>MiddlewareForRequestDiagnosticForDebugging</title>
</head>
<body>
<h1>MiddlewareForRequestDiagnosticForDebugging</h1>
<br>
<h2>path before</h2>
{beforePath}<br>{beforePathBase}
<h2>headers before</h2>
{string.Join("<br>", beforeHeaders.Select(h => $"{h.Key}: {h.Value}"))}
<h2>path after</h2>
{afterPath}<br>{afterPathBase}
<h2>headers after</h2>
{string.Join("<br>", afterHeaders.Select(h => $"{h.Key}: {h.Value}"))}
</body>
</html>"
                );
            }
            else
            {
                //dont inspect anything
                await next.Invoke();
            }
        };
    }

    private static Func<HttpContext, Func<Task>, Task> After(string inspectPath)
    {
        return async (context, next) =>
        {
            if (!context.Request.Path.Equals(inspectPath))
            {
                await next.Invoke();
            }
        };
    }
}
