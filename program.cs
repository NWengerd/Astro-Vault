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

    // Find star tuple associated with user search
    star? tuple = findStar(message);

    // Return 0 if failed to find star
    if (tuple is null) {return Results.Ok(0);}

    // Return star object if found star
    return Results.Ok(tuple);

}
);

// Return star object on success, return on null on failure
star? findStar(string message)
{
    // First query proper_name
    star? tuple = MySQL.queryStarsDB("proper_name", message);
    if (tuple is not null) {return tuple;}

    //FIXME: Should then query all id's in order so that user can search using any common notation

    return null;
}

//MySQL.test();
//CSVParser.test();

app.Run();