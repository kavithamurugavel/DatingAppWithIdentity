using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Helpers
{
    // ActionFilters can be applied when an action is being executed or after a controller action has been executed
    // These can be added at the action level (i.e. the get, post, put methods below), or at the controller level 
    // or at the application level (where we can use it for some sort of logging, for instance)
    // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.2#action-filters
    public class LogUserActivity : IAsyncActionFilter
    {
        // ActionExecuting: when the action is being executed
        // ActionExecutionDelegate: after the action has been executed
        // we are writing this ActionFilter to update the lastActive time of the user in their profile. Instead
        // of writing individual, duplicate code in each of the methods/actions in UsersController, we are using ActionFilter
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            // same as ControllerBase.User.FindFirst part in UsersController
            var userID = int.Parse(resultContext.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier).Value);
            
            // Dating repo is provided as a service in our dependency injection container
            // inside our Startup.cs. GetService will get an instance of that service
            // https://www.devtrends.co.uk/blog/dependency-injection-in-action-filters-in-asp.net-core
            // From the link: If we cannot inject a component into our attribute, it seems as though the next best option is to request the 
            // component from our IoC container (either directly or via a wrapper). In .NET Core, we can use service location
            // to resolve components from the built-in IoC container by using RequestServices.GetService
            var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();
            
            var user = await repo.GetUser(userID, true);
            user.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }
}