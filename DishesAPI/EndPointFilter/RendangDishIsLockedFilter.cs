namespace DishesAPI.EndPointFilter
{
    public class RendangDishIsLockedFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            Guid dishId = Guid.NewGuid();
            if (context.HttpContext.Request.Method == "PUT")
            {
                dishId = context.GetArgument<Guid>(2);
            }
            else if (context.HttpContext.Request.Method == "DELETE")
            {
                dishId = context.GetArgument<Guid>(1);
            }

            var rendangId = new Guid("");
            if (dishId == rendangId)
            {
                return TypedResults.Problem(new()
                {
                    Status = 400,
                    Title = "Cannot Be Change",
                    Detail = "Cannot Be Change"
                });
            }

            var result = await next.Invoke(context);
            return result;
        }
    }
}