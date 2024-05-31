using MyDotNetAPP.Controllers;
using MyDotNetAPP.Services;
using MyDotNetAPP.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MyDotNetAPP
{
    public static class ServicesConfiguration
    {
        public static void AddChatServices(this IServiceCollection services)
        {
            services.AddSingleton<IQueryBuilderProvider, QueryBuilderProvider>();
            services.AddTransient<UsersService>();
        }
    }
}

