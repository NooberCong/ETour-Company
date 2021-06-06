using System;
using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Extentions;
using Microsoft.AspNetCore.Identity.UI.Services;

[assembly: HostingStartup(typeof(Company.Areas.Identity.IdentityHostingStartup))]
namespace Company.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddEmployeesIdentity(op =>
                {
                    op.SignIn.RequireConfirmedEmail = true;
                    op.SignIn.RequireConfirmedAccount = true;
                    op.Password.RequireNonAlphanumeric = false;
                    op.Password.RequireUppercase = false;
                    op.User.RequireUniqueEmail = true;
                }).AddDefaultUI();
                services.Configure<SecurityStampValidatorOptions>(op =>
                {
                    op.ValidationInterval = TimeSpan.Zero;
                });
                services.AddTransient<IEmailSender, EmailSender>();
                services.AddScoped<IUserClaimsPrincipalFactory<Employee>, EmployeeClaimsFactory>();
            });
        }
    }
}