using AutoMapper;
using DishesAPI.DbContexts;
using DishesAPI.Entities;
using DishesAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishDBConnectionString"]
    ));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var dishesEndPoint = app.MapGroup("/dishes");
var dishesEndPointGuid = dishesEndPoint.MapGroup("/{disheId:guid}");

app.MapGet("/dishes", async Task<Ok<IEnumerable<DishDto>>> (DishesDbContext dishesDbContext,
    ClaimsPrincipal claimsPrincipal, IMapper mapper, [FromQuery] string name) =>
{
    Console.WriteLine($"User authenticated? {claimsPrincipal.Identity?.IsAuthenticated}");

    return TypedResults.Ok(mapper.Map<IEnumerable<DishDto>>(await dishesDbContext.Dishes
          .Where(d => name == null || d.Name.Contains(name))
          .ToListAsync()));
});

app.MapGet("/dishes/{disheId:guid}", async Task<Results<NotFound, Ok<DishDto>>> (DishesDbContext dishesDbContext, IMapper mapper, Guid disheId) =>
{
    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Id == disheId);
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(mapper.Map<DishDto>(dishEntity));
}).WithName("GetDish");

app.MapGet("/dishes/{disheName}", async Task<Ok<DishDto>> (DishesDbContext dishesDbContext, IMapper mapper, string disheName) =>
{
    return TypedResults.Ok(mapper.Map<DishDto>(await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Name == disheName)));
});

app.MapGet("/dishes/{disheId}/ingredients", async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>> (DishesDbContext dishesDbContext, IMapper mapper, Guid disheId) =>
{
    var dishEntity = await dishesDbContext.Dishes
     .FirstOrDefaultAsync(d => d.Id == disheId);
    if (dishEntity == null)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(mapper.Map<IEnumerable<IngredientDto>>((await dishesDbContext.Dishes
        .Include(d => d.Ingredients)
        .FirstOrDefaultAsync(d => d.Id == disheId))?.Ingredients));
});

app.MapPost("/dishes", async Task<CreatedAtRoute<DishDto>> (DishesDbContext dishesDbContext, IMapper mapper, [FromBody] DishForCreationDto dishForCreationDto) =>
{
    var dishEntity = mapper.Map<Dish>(dishForCreationDto);
    dishesDbContext.Add(dishEntity);
    await dishesDbContext.SaveChangesAsync();
    var dishReturn = mapper.Map<DishDto>(dishEntity);
    return TypedResults.CreatedAtRoute(dishReturn, "GetDish", new { disheId = dishReturn.Id });
});

app.MapPut("/dishes/{disheId:guid}", async Task<Results<NotFound, NoContent>> (DishesDbContext dishesDbContext, IMapper mapper, Guid disheId, DishForUpdateDto dishForUpdateDto) =>
{
    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Id == disheId);
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    mapper.Map(dishForUpdateDto, dishEntity);
    await dishesDbContext.SaveChangesAsync();
    return TypedResults.NoContent();
});

app.MapDelete("/dishes/{disheId:guid}", async Task<Results<NotFound, NoContent>> (DishesDbContext dishesDbContext, Guid disheId) =>
{
    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Id == disheId);
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    dishesDbContext.Dishes.Remove(dishEntity);
    await dishesDbContext.SaveChangesAsync();
    return TypedResults.NoContent();
});

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureCreated();
    context.Database.Migrate();
}

app.Run();