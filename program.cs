var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serve index.html and other files from wwwroot (boiler-plate)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/search", (string? message) =>
{
    // Ensure query from front-end is not null
    if (message is null) { return Results.BadRequest();}

    // Trim whitespace
    message = message.Trim();
    // FIXME: message should be made lower case, as should everything stored in the database for ease of use

    star? tuple = findStar(message);
    if (tuple is null) {return Results.Ok(0);}
    // Temporarily just return original search
    return Results.Ok(tuple);

}
);

star? findStar(string message)
{
    // First query proper_name
    star? tuple = MySQL.queryStarsDB("proper_name", message);
    if (tuple is not null) {return tuple;}

    // more queries after initial failure
    return null;
}

//MySQL.test();
//CSVParser.Main();

app.Run();