var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serve index.html and other files from wwwroot (boiler-plate)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/search", (string? message) =>
{
    // Ensure query from front-end is not null
    if (message == null) { return Results.BadRequest();} // Does not work?

    // Trim whitespace
    message = message.Trim();

    // Querying database would go here...

    // Temporarily just return original search
    return Results.Ok(message);

}
);

// MySQL.Main();
CSVParser.Main();

app.Run();