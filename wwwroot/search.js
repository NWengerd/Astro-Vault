// initialize table of stars
let range = 1;
fillTable();

const starParams = ["ID", "HIP", "HD", "HR", "GL", "Name", "Right Ascension", "Declination", "Distance", 
    "Radial Velocity", "Absolute Magnitude", "Spectrocity", "Bayer Flamsteed", "Constellation", "Luminosity"];

const tableParams = ["proper", "con", "id", "hip", "hd", "hr", "gl"];
    
const output = document.getElementById("output");
const input = document.querySelector("input");
const table = document.getElementById("table-output");
const searchButton = document.querySelector("button");
const backButton = document.getElementById("back-btn");
const nextButton = document.getElementById("next-btn");

// Navigate between table pages
backButton.addEventListener("click", () => {
    range -= 12;
    fillTable();
});
nextButton.addEventListener("click", () => {
    range += 12;
    fillTable();
});

// run searchStar when user presses search button
searchButton.addEventListener("click", searchStar);

// Fetch user's star from server and output it
function searchStar(event) 
{
    // Prevent page reload
    event.preventDefault();

    // Get user input
    const search = input.value;

    // Clear previous output
    output.innerHTML = "";
    
    // Make a GET request to server with user input
    fetch("/search?message=" + search)
    .then(async (res) => {
    
    const data = await res.json();
    console.log(data);
    
    printSearchResults(data, search);

    });
}


// Print search results to document
function printSearchResults(data, search)
{
    if (data != 0) 
    {
        const values = Object.values(data);
        // Remove 'flam' from star data (useful in the future but will be removed for the moment)
        values.splice(13, 1);

        for (let i = 0; i < starParams.length; i++)
        {
            const field = output.appendChild(document.createElement("div"));
            field.className = "star-field";

            const label = field.appendChild(document.createElement("span"));
            label.className = "label";
            label.textContent = starParams[i];

            const value = field.appendChild(document.createElement("span"));
            value.className = "value";
            value.textContent = (values[i] == null) ? "-------" : values[i];
        }
    }
    else
    {
        const message = output.appendChild(document.createElement("p"));
        message.className = "message error";
        message.textContent = "Failed to find \"" + search + "\", please try again.";
    }
}


function fillTable()
{
    // TO-DO: Make it so this only displays named stars (or that the user can filter)
    
    fetch("/table?message=" + range)
    .then(async (res) => {
    
        const data = await res.json();
        console.log(data);
        
        // Reset table contents
        table.innerHTML = "";

        // Add each star to the table
        for (const star of data)
        {
            const row = table.appendChild(document.createElement("tr"));

            for (const param of tableParams)
            {
                const cell = row.appendChild(document.createElement("td"));
                cell.textContent = (star[param] == null) ? "-------" : star[param];
            }
        }
        
    });
}