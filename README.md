## Server-Side
C# / ASP.NET <br/>
Runs on server, moderates communication between front-end to MySQL database <br/>

## Start-up Instructions (for development)
wwwroot is considered the web root directory, where all static files live, such as js, html, and css, which is what we're using for the front-end. These files are served to the client by asp.net when the client goes to our url, which in our case is http://localhost:5075 (only accessible to your own computer). The server-side code exists outside of the wwwroot directory. <br/>
While in the Astro-Vault directory you can run `dotnet run` to start the server (of course you need .NET installed on your system), then by going to http://localhost:5075 in whatever browser you will be served index.html from wwwroot. <br/>
index.html (front-end) communicates to the back-end via a fetch() in the JavaScript portion of the file, this sends an HTTP GET request to /search, which is an access point defined in program.cs.
