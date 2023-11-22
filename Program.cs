using demowebapiminima.Domain;
using demowebapiminma.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WpmDbContext>();

var app = builder.Build();

var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetService<WpmDbContext>();
db.Database.EnsureCreated();
db.Dispose();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var speciesGroup = app.MapGroup("/api/species");

speciesGroup.MapGet("/", async (WpmDbContext wpmDbContext) =>
{
    var species = await wpmDbContext.Species.ToListAsync();
    return Results.Ok(species);
}).Produces<IEnumerable<Species>>().WithOpenApi(op => new(op)
{
    Summary = "Todas las especies",
    Description = "Este endpoint regresa todas las especies de la base de datos"
});

speciesGroup.MapGet("/{id}/breeds", async (int id, WpmDbContext wpmDbContext) =>
{
    var breeds = await wpmDbContext.Breeds.Where(b => b.SpeciesId == id).ToListAsync();
    return breeds.Count != 0 ? Results.Ok(breeds) : Results.NotFound();
}).Produces<IEnumerable<Breed>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

speciesGroup.MapPost("/", async (SpeciesModel speciesModel, WpmDbContext wpmDbContext) =>
{
    wpmDbContext.Species.Add(new Species() { Name = speciesModel.Name });
    var result = await wpmDbContext.SaveChangesAsync();
    return result == 1 ? Results.Ok() : Results.BadRequest();

}).Produces(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound);

app.Run();

record SpeciesModel(string Name);