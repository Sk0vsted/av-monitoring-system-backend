using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Net;

public class CorsMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var req = await context.GetHttpRequestDataAsync();

        // Handle preflight
        if (req != null && req.Method == "OPTIONS")
        {
            var preflight = req.CreateResponse(HttpStatusCode.OK);
            preflight.Headers.Add("Access-Control-Allow-Origin", "*");
            preflight.Headers.Add("Access-Control-Allow-Headers", "*");
            preflight.Headers.Add("Access-Control-Allow-Methods", "*");
            await preflight.WriteStringAsync("OK");

            context.GetInvocationResult().Value = preflight;
            return;
        }

        await next(context);

        // Add CORS headers to all responses
        var res = context.GetHttpResponseData();
        if (res != null)
        {
            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Headers", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "*");
        }
    }
}
