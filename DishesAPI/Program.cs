using DishesAPI.DbContexts;
using DishesAPI.Extentions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishDBConnectionString"]
    ));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthentication();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminFromBelgium", policy => policy.RequireRole("admin").RequireClaim("country", "Belgium"));

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    //app.UseExceptionHandler(e =>
    //{
    //    e.Run(async context =>
    //    {
    //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    //        context.Response.ContentType = "text/html";
    //        await context.Response.WriteAsync("unException problem");
    //    });
    //});
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.RegisterDishesEndpoints();
app.RegisterIngredientsEndpoints();

//var dishesEndpoints = app.MapGroup("/dishes");
//var dishWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}");
//var ingredientsEndpoints = dishWithGuidIdEndpoints.MapGroup("/ingredients");

//dishesEndpoints.MapGet("", async Task<Ok<IEnumerable<DishDto>>> (DishesDbContext dishesDbContext,
//    ClaimsPrincipal claimsPrincipal, IMapper mapper, [FromQuery] string name) =>
//{
//    Console.WriteLine($"User authenticated? {claimsPrincipal.Identity?.IsAuthenticated}");

//    return TypedResults.Ok(mapper.Map<IEnumerable<DishDto>>(await dishesDbContext.Dishes
//          .Where(d => name == null || d.Name.Contains(name))
//          .ToListAsync()));
//});

//dishWithGuidIdEndpoints.MapGet("", async Task<Results<NotFound, Ok<DishDto>>> (DishesDbContext dishesDbContext, IMapper mapper, Guid disheId) =>
//{
//    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Id == disheId);
//    if (dishEntity is null)
//    {
//        return TypedResults.NotFound();
//    }
//    return TypedResults.Ok(mapper.Map<DishDto>(dishEntity));
//}).WithName("GetDish");

//dishesEndpoints.MapGet("/{disheName}", async Task<Ok<DishDto>> (DishesDbContext dishesDbContext, IMapper mapper, string disheName) =>
//{
//    return TypedResults.Ok(mapper.Map<DishDto>(await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Name == disheName)));
//});

//ingredientsEndpoints.MapGet("", async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>> (DishesDbContext dishesDbContext, IMapper mapper, Guid disheId) =>
//{
//    var dishEntity = await dishesDbContext.Dishes
//     .FirstOrDefaultAsync(d => d.Id == disheId);
//    if (dishEntity == null)
//    {
//        return TypedResults.NotFound();
//    }

//    return TypedResults.Ok(mapper.Map<IEnumerable<IngredientDto>>((await dishesDbContext.Dishes
//        .Include(d => d.Ingredients)
//        .FirstOrDefaultAsync(d => d.Id == disheId))?.Ingredients));
//});

//dishesEndpoints.MapPost("", async Task<CreatedAtRoute<DishDto>> (DishesDbContext dishesDbContext, IMapper mapper, [FromBody] DishForCreationDto dishForCreationDto) =>
//{
//    var dishEntity = mapper.Map<Dish>(dishForCreationDto);
//    dishesDbContext.Add(dishEntity);
//    await dishesDbContext.SaveChangesAsync();
//    var dishReturn = mapper.Map<DishDto>(dishEntity);
//    return TypedResults.CreatedAtRoute(dishReturn, "GetDish", new { disheId = dishReturn.Id });
//});

//dishWithGuidIdEndpoints.MapPut("", async Task<Results<NotFound, NoContent>> (DishesDbContext dishesDbContext, IMapper mapper, Guid disheId, DishForUpdateDto dishForUpdateDto) =>
//{
//    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Id == disheId);
//    if (dishEntity is null)
//    {
//        return TypedResults.NotFound();
//    }
//    mapper.Map(dishForUpdateDto, dishEntity);
//    await dishesDbContext.SaveChangesAsync();
//    return TypedResults.NoContent();
//});

//dishWithGuidIdEndpoints.MapDelete("", async Task<Results<NotFound, NoContent>> (DishesDbContext dishesDbContext, Guid disheId) =>
//{
//    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(x => x.Id == disheId);
//    if (dishEntity is null)
//    {
//        return TypedResults.NotFound();
//    }
//    dishesDbContext.Dishes.Remove(dishEntity);
//    await dishesDbContext.SaveChangesAsync();
//    return TypedResults.NoContent();
//});

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureCreated();
    context.Database.Migrate();
}

app.Run();