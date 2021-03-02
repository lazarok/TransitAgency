using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TransitAgency.Infrastructure.BlazorUI.Helpers;
using TransitAgency.Infrastructure.BlazorUI.Services;

namespace TransitAgency.Infrastructure.BlazorUI
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBlazorUI(this IServiceCollection services, WebAssemblyHostBuilder builder)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<IHttpService, HttpService>();
            services.AddScoped<ILocalStorageService, LocalStorageService>();

            // configure http client
            services.AddScoped(x =>
            {
                var apiUrl = new Uri($"{builder.HostEnvironment.BaseAddress}api/");

                // use fake backend if "fakeBackend" is "true" in appsettings.json
                if (builder.Configuration["fakeBackend"] == "true")
                {
                    var fakeBackendHandler = new FakeBackendHandler(x.GetService<ILocalStorageService>());
                    return new HttpClient(fakeBackendHandler) { BaseAddress = apiUrl };
                }

                return new HttpClient() { BaseAddress = apiUrl };
            });

            return services;
        }
    }
}
