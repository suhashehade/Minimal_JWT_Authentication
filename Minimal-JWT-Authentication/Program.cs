namespace Minimal_JWT_Authentication;

internal static class  Program
{
    private static void Main( string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}