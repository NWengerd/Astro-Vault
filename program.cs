using System.Text.RegularExpressions;

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

app.MapGet("/art", (int message) =>
{
    constellationArt? table = getConstArt(message);

    if (table is null) 
        return Results.Ok(0);

    return Results.Ok(table);
});

app.MapGet("/calculate", IResult (string message, string operation) =>
{
    var selectedStar = findStar(message);
    if (selectedStar is null)
        return Results.NotFound(new { error = "Star not found." });

    var result = StarCalculator.Calculate(selectedStar, operation);
    if (result is null)
        return Results.BadRequest(new { error = "Calculation unavailable for this star." });

    return Results.Ok(new { value = result.Value.Value, unit = result.Value.Unit });
});


// Return list of lines in a constellation if successful, return null if unsuccessful
constellationArt? getConstArt(int constellation)
{
    constellationArt? table = MySQL.queryConstellationArt(constellation);
    return table;
}


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
    message = message.ToLower();

    // Initialize return var
    star? tuple = null;

    // Regex for identifying if the user is searching by an ID
    Match match = Regex.Match(message, @"^(hip|hd|hr)[\s\-]*(\d+)$");

    // If user is searching by a valid ID
    if (match.Success)
    {
        string idSpecifier = match.Groups[1].Value;
        string idNumber = match.Groups[2].Value;
        
        switch (idSpecifier)
        {
            case "hip":
                // Search by Hipparcos ID
                tuple = MySQL.queryStar("hipID", idNumber);
                break;
            case "hd":
                // Search by Henry Draper ID
                tuple = MySQL.queryStar("hdID", idNumber);
                break;
            case "hr":
                // Search by Harvard Revised ID
                tuple = MySQL.queryStar("hrID", idNumber);
                break;
            default:
                break;
        }
    }
    
    // If star was found using ID return it, otherwise return search by proper_name
    return (tuple is not null) ? tuple : MySQL.queryStar("proper_name", message);
}

//artReader.ReadConstellationArt(); // Uncomment me to fill constellationArt table

app.Run();