namespace PasteBinASP.Middlewares;

public class ServerIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _serverId;

    public ServerIdMiddleware(RequestDelegate next)
    {
        _next = next;
        _serverId = new Random().Next(1, 100);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/server")
            await context.Response.WriteAsync($"Server {_serverId}");
        else
            await _next.Invoke(context);
    }
}
