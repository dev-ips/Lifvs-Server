using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Lifvs.Common;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Filters;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility;
using Lifvs.Common.Utility.Interfaces;
using Lifvs.Common.Validators;
using Lifvs.Common.Validators.Interfaces;
using Lifvs.Controllers;
using log4net;
using StackExchange.Profiling;
using System.Data;
using System.Web.Http;
using System.Web.Mvc;

namespace Lifvs.App_Start
{
    public class IoCConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.Register(ctx => DbHelper.GetOpenConnection()).As<IDbConnection>().InstancePerLifetimeScope();

            ConfigureLog4Net(builder);
            ConfigureProfiler(builder);
           

            //Controllers
            builder.RegisterType<AccessTokenController>().InstancePerRequest();
            builder.RegisterType<BaseDataController>().InstancePerRequest();
            builder.RegisterType<EmailController>().InstancePerRequest();
            builder.RegisterType<EmployeeController>().InstancePerRequest();
            builder.RegisterType<InventoryController>().InstancePerRequest();
            builder.RegisterType<InventoryWebController>().InstancePerRequest();
            builder.RegisterType<StoreController>().InstancePerRequest();
            builder.RegisterType<UserController>().InstancePerRequest();
            builder.RegisterType<UsersController>().InstancePerRequest();
            builder.RegisterType<LoginController>().InstancePerRequest();
            builder.RegisterType<SalesController>().InstancePerRequest();
            builder.RegisterType<StoreController>().InstancePerRequest();
            builder.RegisterType<StoresController>().InstancePerRequest();
            builder.RegisterType<UserCodeController>().InstancePerRequest();
            builder.RegisterType<StoreInventoryController>().InstancePerRequest();
            builder.RegisterType<CartController>().InstancePerRequest();
            builder.RegisterType<ReceiptController>().InstancePerRequest();

            //area controllers

            builder.RegisterType<Areas.Customer.Controllers.LoginController>().InstancePerRequest();
            builder.RegisterType<Areas.Customer.Controllers.DashboardController>().InstancePerRequest();
            builder.RegisterType<Areas.Customer.Controllers.CustomerShopViewController>().InstancePerRequest();


            //Filters
            builder.RegisterType<AccessTokenAuthenticationAttribute>().PropertiesAutowired();

            //Helpers
            builder.RegisterType<ExceptionManager>().As<IExceptionManager>();
            
            //Repositories
            builder.RegisterType<AccessTokenRepository>().As<IAccessTokenRepository>();
            builder.RegisterType<EmployeeRepository>().As<IEmployeeRepository>();
            builder.RegisterType<InventoryRepository>().As<IInventoryRepository>();
            builder.RegisterType<UserRepository>().As<IUserRepository>();
            builder.RegisterType<StoreRepository>().As<IStoreRepository>();
            builder.RegisterType<UserCodeRepository>().As<IUserCodeRepository>();
            builder.RegisterType<StoreInventoryRepository>().As<IStoreInventoryRepository>();
            builder.RegisterType<CartRepository>().As<ICartRepository>();
            builder.RegisterType<ReceiptRepository>().As<IReceiptRepository>();

            //Services
            builder.RegisterType<AccessTokenService>().As<IAccessTokenService>();
            builder.RegisterType<BaseDataService>().As<IBaseDataService>();
            builder.RegisterType<EmployeeService>().As<IEmployeeService>();
            builder.RegisterType<InventoryService>().As<IInventoryService>();
            builder.RegisterType<StoreService>().As<IStoreService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<UserCodeService>().As<IUserCodeService>();
            builder.RegisterType<StoreInventoryService>().As<IStoreInventoryService>();
            builder.RegisterType<CartService>().As<ICartService>();
            builder.RegisterType<ReceiptService>().As<IReceiptService>();


            //Utilities
            builder.RegisterType<CacheHelper>().As<ICacheHelper>();
            builder.RegisterType<CryptoGraphy>().As<ICryptoGraphy>();
            builder.RegisterType<EmailNotifier>().As<IEmailNotifier>();
            builder.RegisterType<FileManager>().As<IFileManager>();
            builder.RegisterType<ServerPathMapper>().As<IPathMapper>();
            builder.RegisterType<SetupUser>().As<ISetupUser>();

            //Validators
            builder.RegisterType<LoginUserCredentialValidator>().As<IValidatorService<AudienceCredentials>>();
            builder.RegisterType<UserRegistrationValidator>().As<IValidatorService<RegisterModel>>();
            builder.RegisterType<ChangePasswordValidator>().As<IValidatorService<RecoveryCode>>();
            builder.RegisterType<UserCardDetailValidator>().As<IValidatorService<UserCardDetailModel>>();
            builder.RegisterType<ChangeUserProfileValidator>().As<IValidatorService<UserProfileViewModel>>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
        private static void ConfigureProfiler(ContainerBuilder builder)
        {
            builder.Register(ctx => MiniProfiler.Current).As<MiniProfiler>();
        }
        private static void ConfigureLog4Net(ContainerBuilder builder)
        {
            log4net.Config.XmlConfigurator.Configure();
            var loggerForWebSite = LogManager.GetLogger("Lifvs");
            builder.RegisterInstance(loggerForWebSite).As<ILog>();
        }
    }
}
