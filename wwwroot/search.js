const starParams = ["ID", "HIP", "HD", "HR", "GL", "Name", "Right Ascension", "Declination", "Distance", 
    "Radial Velocity", "Absolute Magnitude", "Spectrocity", "Bayer Flamsteed", "Constellation", "Luminosity"];
    
const output = document.getElementById("output");
const input = document.querySelector("input");
const searchButton = document.querySelector("button");

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

    if (data != 0) 
    {
        // Turn return object into an array
        let values = Object.values(data);
        
        // Print all parameters to the document in seperate <p> tags
        for (let i = 0; i < 15; i++)
        {
        const p = output.appendChild(document.createElement("p"));
        p.textContent = starParams[i] + ": " + values[i];
        }
    }
    else
    {
        output.textContent = "Failed to find \"" + search + "\", please try again.";
    }
    });
}