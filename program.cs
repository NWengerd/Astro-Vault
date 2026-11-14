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

app.MapGet("/table", (int message) =>
{
    List<star>? table = getTable(message);

    if (table is null) 
        return Results.Ok(0);

    return Results.Ok(table);
});


// Query database for a list of stars within the specified range
// Return list of stars on success, return null on failure
List<star>? getTable(int range)
{
    List<star>? stars = MySQL.queryStarList(range);
    return stars;
}


// Return star object on success, return null on failure
star? findStar(string message)
{
    message = message.Trim();

    // First, search by star name
    star? tuple = MySQL.queryStar("proper_name", message);

    if (tuple is not null)
    {
        return tuple;
    }

    // If the input is numeric, search by ID fields
    if (int.TryParse(message, out _))
    {
        // Search by the star's primary ID
        tuple = MySQL.queryStar("id", message);

        if (tuple is not null)
        {
            return tuple;
        }

        // Search by Hipparcos ID
        tuple = MySQL.queryStar("hipID", message);

        if (tuple is not null)
        {
            return tuple;
        }

        // Search by Henry Draper ID
        tuple = MySQL.queryStar("hdID", message);

        if (tuple is not null)
        {
            return tuple;
        }
    }

    // No matching star was found
    return null;
}

//MySQL.test();
//CSVParser.test();

app.Run();