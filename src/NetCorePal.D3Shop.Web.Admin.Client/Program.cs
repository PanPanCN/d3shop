using AntDesign.ProLayout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NetCorePal.D3Shop.Admin.Shared.Authorization;
using NetCorePal.D3Shop.Web.Admin.Client.Auth;
using NetCorePal.D3Shop.Web.Admin.Client.Services;
using Newtonsoft.Json;

namespace NetCorePal.D3Shop.Web.Admin.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddRefitClient<IAccountService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

            var ser = new NewtonsoftJsonContentSerializer(new JsonSerializerSettings { });
            var settings = new RefitSettings(ser);
            builder.Services.AddRefitClient<IRolesService>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddRefitClient<IPermissionsService>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

            #region 身份认证和授权

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IPermissionChecker, ClientPermissionChecker>();

            #endregion

            builder.Services.AddAntDesign();

            builder.Services.Configure<ProSettings>(builder.Configuration.GetSection("ProSettings"));

            await builder.Build().RunAsync();
        }
    }
}