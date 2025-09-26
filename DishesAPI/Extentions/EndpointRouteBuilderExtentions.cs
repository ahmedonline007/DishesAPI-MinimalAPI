using DishesAPI.EndPointFilter;
using DishesAPI.EndPointHandlers;

namespace DishesAPI.Extentions
{
    public static class EndpointRouteBuilderExtentions
    {
        public static void RegisterDishesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var dishesEndpoints = endpointRouteBuilder.MapGroup("/dishes").RequireAuthorization();
            var dishWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}").RequireAuthorization();

            dishesEndpoints.MapGet("", DishesHandlers.GetDishesAsync);
            dishWithGuidIdEndpoints.MapGet("", DishesHandlers.GetDishByIdAsync).WithName("GetDish");
            dishesEndpoints.MapGet("/{dishName}", DishesHandlers.GetDishByNameAsync).AllowAnonymous();//للسماح بعدم الحصول على اذن
            dishesEndpoints.MapPost("", DishesHandlers.CreateDishAsync);
            dishWithGuidIdEndpoints.MapPut("", DishesHandlers.UpdateDishAsync)
                .AddEndpointFilter<RendangDishIsLockedFilter>();
            dishWithGuidIdEndpoints.MapDelete("", DishesHandlers.DeleteDishAsync);
        }

        public static void RegisterIngredientsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var ingredientsEndpoints = endpointRouteBuilder.MapGroup("/dishes/{dishId:guid}/ingredients").RequireAuthorization();
            ingredientsEndpoints.MapGet("", IngredientsHandlers.GetIngredientsAsync).RequireAuthorization();
        }
    }
}