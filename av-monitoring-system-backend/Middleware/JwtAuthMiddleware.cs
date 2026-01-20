using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

public class JwtAuthMiddleware : IFunctionsWorkerMiddleware
{
    private readonly JwtTokenValidator _validator;

    public JwtAuthMiddleware(JwtTokenValidator validator)
    {
        _validator = validator;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var req = await context.GetHttpRequestDataAsync();
        if (req is null)
        {
            await next(context);
            return;
        }

        // 🔓 Allow unauthenticated endpoints (fx health)
        if (context.FunctionDefinition.Name == "Health")
        {
            await next(context);
            return;
        }

        if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
        {
            await Reject(context, req);
            return;
        }

        var bearer = authHeaders.FirstOrDefault();
        if (bearer is null || !bearer.StartsWith("Bearer "))
        {
            await Reject(context, req);
            return;
        }

        var token = bearer["Bearer ".Length..];

        try
        {
            _validator.Validate(token);
        }
        catch
        {
            await Reject(context, req);
            return;
        }

        await next(context);
    }

    private static async Task Reject(FunctionContext context, HttpRequestData req)
    {
        var res = req.CreateResponse(HttpStatusCode.Unauthorized);
        context.GetInvocationResult().Value = res;
        await Task.CompletedTask;
    }
}
