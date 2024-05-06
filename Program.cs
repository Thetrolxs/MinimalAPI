using chairs_dotnet7_api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("chairlist"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

var chairs = app.MapGroup("api/chair");

//TODO: ASIGNACION DE RUTAS A LOS ENDPOINTS
chairs.MapPost("/",CreateChair);
chairs.MapGet("/",GetAllChairs);
chairs.MapGet("/{name}",GetChairsByName);
chairs.MapPut("/{id}",UpdateChair);
chairs.MapPut("/{id}/stock",IncrementStock);
chairs.MapPost("/purchase",Purchasechair);
chairs.MapDelete("/{id}",DeleteChair);

app.Run();

//TODO: ENDPOINTS SOLICITADOS

//Creacion del objeto silla
static async Task<IResult> CreateChair(Chair chair, DataContext db)
{
    var chairs = await db.Chairs.FindAsync(chair.Nombre);
    if(chairs != null)
    {
        return TypedResults.BadRequest("Ya existe una silla con ese nombre");
    }

    db.Chairs.Add(chair);
    await db.SaveChangesAsync();    

    return TypedResults.Created($"api/chair/{chair.Id}", chair);
}

//Lista de todos los objetos silla
static async Task<IResult> GetAllChairs(DataContext db)
{
    return TypedResults.Ok(await db.Chairs.ToArrayAsync());
}

//Encontrar objeto silla por nombre
static async Task<IResult> GetChairsByName(string name, DataContext db)
{
    var chairs = await db.Chairs.FindAsync(name);
    if(chairs == null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok();
}

//Actualizar datos de un objeto silla
static async Task<IResult> UpdateChair(int Id, string nombre, DataContext db)
{
    var chairs = await db.Chairs.FindAsync(Id);
    if(chairs == null)
    {
        return TypedResults.NotFound();
    }

    chairs.Nombre = nombre;
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

//Incrementar stock del objeto silla
static async Task<IResult> IncrementStock(int Id, int stock, DataContext db)
{
    var chairs = await db.Chairs.FindAsync(Id);
    if(chairs == null)
    {
        return TypedResults.NotFound();
    }

    chairs.Stock = stock;
    await db.SaveChangesAsync();

    return TypedResults.Ok();
}

//compra del objeto silla
static async Task<IResult> Purchasechair(int Id, int stock, int pay, DataContext db)
{
    var chairs = await db.Chairs.FindAsync();
    if(chairs == null)
    {
        return TypedResults.BadRequest("No se encuentra la silla deseada");
    }

    var totalPrice = chairs.Precio*stock;

    if(chairs.Stock > stock && totalPrice>=pay)
    {
        chairs.Stock -= stock;
        await db.SaveChangesAsync();
    
        return TypedResults.Ok("compra realizada con éxito");
    }

    return TypedResults.BadRequest("Error en la compra");
}

//Borrar objeto silla
static async Task<IResult> DeleteChair(int Id, DataContext db)
{
    var chairs = await db.Chairs.FindAsync(Id);
    if(chairs == null)
    {
        return TypedResults.NotFound();
    }

    db.Chairs.Remove(chairs);
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}
