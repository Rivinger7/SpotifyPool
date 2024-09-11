//using Data_Access_Layer.Implement.UnitOfWork;
//using DataAccessLayer.Interface.Interface.IUnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer.DependencyInjection.Dependency_Injections
{
    public static class BusinessDependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddServices(configuration);
            //services.AddRepositories();
        }

        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {

        }

        //public static void AddRepositories(this IServiceCollection services)
        //{
        //    services.AddScoped<IUnitOfWork, UnitOfWork>();
        //}
    }
}
