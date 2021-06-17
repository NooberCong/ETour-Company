using Infrastructure.Extentions;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Company
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
                config.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
            services.AddRazorPages();
            services.AddControllersWithViews();
            services.AddBaseDb();
            services.AddCompanyDb();
            services.AddAuthorization();
            services.AddEmailService();
            services.AddSignalR();
            services.AddETourLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Status", "?code={0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseCors(builder =>
            {
                builder.WithOrigins("https://localhost:44323/")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
            });

          

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<LogHub>("/logs");
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<QAHub>("/qa");     
            });
        }
    }
}
