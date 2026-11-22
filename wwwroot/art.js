const numConstellations = 88;
let constellationID = 1;
let constellationName;
let values = [];
let width = 800;
let height = 500;

// Get canvas element
const canvas = document.getElementById('constell');
// Get constellation title element
const title = document.getElementById('constellation-name');
// Get buttons
const backButton = document.getElementById("back-btn");
const nextButton = document.getElementById("next-btn");

// Navigate between constellation Art
backButton.addEventListener("click", () => {
    step(-1);
});
nextButton.addEventListener("click", () => {
    step(1);
});

function step(direction)
{
    // next button pressed
    if (direction == 1)
    {
        if (constellationID < numConstellations)
            constellationID = constellationID + direction;
        getConstArt(direction);
    }
    // back button pressed
    else if (direction == -1)
    {
        if (constellationID > 1)
            constellationID = constellationID + direction;
        getConstArt(direction);
    }
}


function draw()
{
    // Set constellation title
    title.innerHTML = constellationName;
    
    // Access canvas context
    const ctx = canvas.getContext('2d');

    // Solid filled rectangle (x, y, width, height)
    ctx.fillStyle = 'black';
    ctx.fillRect(0, 0, width, height);
    
    ctx.fillStyle = 'white';

    // Draw star points
    for (let i = 0; i < values.length; i++)
    {
        ctx.beginPath();
        ctx.arc(values[i].rightAsc1, values[i].declination1, 3, 0, Math.PI * 2);
        ctx.arc(values[i].rightAsc2, values[i].declination2, 3, 0, Math.PI * 2);
        ctx.fill();
    }

    // Draw lines between star points
    for (let i = 0; i < values.length; i++)
    {
        ctx.beginPath();
        ctx.moveTo(values[i].rightAsc1, values[i].declination1);
        ctx.lineTo(values[i].rightAsc2, values[i].declination2);
        ctx.strokeStyle = 'white';
        ctx.stroke();
    }
}


function getConstArt(direction = 1)
{    
    // Make a GET request to server with user input
    fetch("/art?message=" + constellationID)
    .then(async (res) => {

        const data = await res.json();
        values = data.lines;
        constellationName = data.constFullName;

        // Make sure constellation art exists before projecting and drawing
        if (values != null && values.length > 0) 
        {
            rectProjectConstellation();
            draw();
        }
        else
        {
            // Use existing direction to step to next constellation if current one doesn't have art
            step(direction);
        }
    });

}


function rectProjectConstellation()
{
    // FIXME: Andromenda is warped by rectangular projection, may need more sophisticated projection
    
    const minRA = Math.min(...values.map(o => o.rightAsc1), ...values.map(o => o.rightAsc2));
    const maxRA = Math.max(...values.map(o => o.rightAsc1), ...values.map(o => o.rightAsc2));
    const minDec = Math.min(...values.map(o => o.declination1), ...values.map(o => o.declination2));
    const maxDec = Math.max(...values.map(o => o.declination1), ...values.map(o => o.declination2));

    const margin = 30;
    // RA hours to degrees, shrunk by cos(dec)
    const k = 15 * Math.cos(((minDec + maxDec) / 2) * Math.PI / 180); 

    // both spans are now in degrees
    const spanX = (maxRA - minRA) * k;   
    const spanY = maxDec - minDec;

    // one scale for both axes, so the shape isn't distorted
    const scale = Math.min((width - 2 * margin) / spanX, (height - 2 * margin) / spanY);

    // offsets to center the constellation in the rectangle
    const offX = (width - spanX * scale) / 2;
    const offY = (height - spanY * scale) / 2;

    // maxRA - ra flips x so east is on the left
    const toX = ra  => offX + (maxRA - ra) * k * scale;   
    const toY = dec => offY + (maxDec - dec) * scale;

    for (const v of values) {
        v.rightAsc1   = toX(v.rightAsc1);
        v.rightAsc2   = toX(v.rightAsc2);
        v.declination1 = toY(v.declination1);
        v.declination2 = toY(v.declination2);
    }
}

getConstArt();