using Microsoft.OpenApi.Models;
using minimalAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
// This line is creating the web application itself

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<SantaDbContext>(options => options.UseInMemoryDatabase("items"));

builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo {
         Title = "Santa API",
         Description = "Making sure every good kid gets a gift",
         Version = "v1" });
});
// This setups up our swagger info


builder.Services.AddCors();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSantaRole",
        policy => policy.RequireRole("SantaRole"));
    options.AddPolicy("RequireElfRole",
        policy => policy.RequireRole("ElfRole"));
});

var app = builder.Build();
// This line is instantiating the app


app.UseSwagger();
app.UseSwaggerUI(c =>
{
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "Santa API V1");
});
// Now we are telling the app to use swagger and the URL that it is at

app.Urls.Add("http://localhost:5000");
app.Urls.Add("https://localhost:5001");
// Defining the URLs and ports to run on


app.MapGet("/", () => "Hello World!");
// This line is declaring a HTTP GET route at the root that 
// will return the string hello world

// Lock down to Santa
app.MapPost("/Child", async (SantaDbContext db, Child child) => {
    // Add child to db object
    await db.Children.AddAsync(child);
    // Commit change
    await db.SaveChangesAsync();
    // Return created
    return TypedResults.Created($"{child.Id}", child);
}).RequireAuthorization("RequireSantaRole");

app.MapGet("/Child", async (SantaDbContext db) => await db.Children.Include(x => x.wantedItem).ToListAsync())
  .RequireAuthorization("RequireSantaRole");
// Lock down to Santa
app.MapPut("/Child", async (SantaDbContext db, bool isGood, int id) => {
    // Find the right child
    var child = await db.Children.Where(x => x.Id == id).Include(x => x.wantedItem).FirstOrDefaultAsync();
    // If the ID doesn't exist throw not found
    if (child is null) return Results.NotFound();
    
    // If the child is good and good again we can assume they are being checked twice
    if (child.IsGood && isGood){
        child.checkedTwice = true;
        child.wantedItem.shouldBeBuilt = true;
    } else {
        // If child was not previously good or isGood is false update child.IsGood
        child.IsGood = isGood;
    }

    // Whatever we change update it
    await db.SaveChangesAsync();
    // Return http code
    return Results.NoContent();
}).RequireAuthorization("RequireSantaRole");

app.MapGet("/Gift/NeedingBuilt", async (SantaDbContext db) => await db.Gifts.Where(x => x.shouldBeBuilt && !x.startedBuilding).ToListAsync())
    .RequireAuthorization("RequireElfRole");

app.MapPut("/Gift/StartedBuilding/{id}", async (int id, SantaDbContext db) => {
    var giftToUpdate = await db.Gifts.FindAsync(id);

    if (giftToUpdate is null) return Results.NotFound();

    giftToUpdate.startedBuilding = true;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("RequireElfRole");

app.MapPut("/Gift/Finished/{id}", async (int id, SantaDbContext db) => {
    var giftToUpdate = await db.Gifts.FindAsync(id);

    if (giftToUpdate is null) return Results.NotFound();

    var childToUpdate = await db.Children.Include(x => x.wantedItem).FirstOrDefaultAsync(x => x.wantedItem.Id == id);

    if (childToUpdate is null) return Results.NotFound();

    giftToUpdate.isCompleted = true;
    childToUpdate.presentReadyForDelivery = true;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("RequireElfRole");

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
// This line is making sure the app runs and continues running