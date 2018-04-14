using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BitHub.Authorizations.Requirements;
using Microsoft.AspNetCore.Authorization;

using BitHub.Data;
using BitHub.Services;
using BitHub.Options;
using LibGit2Sharp;
using BitHub.Authorizations.Handlers;

namespace BitHub
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
            //services.AddDbContext<ApplicationDbContext>(options =>
               //options.UseSqlServer(Configuration.GetConnectionString("LocalUserAccountConnection")));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("MacOSLocalConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 7;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                //options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });


            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Accounts/Manage");
                    options.Conventions.AuthorizePage("/Accounts/Logout");
                });


            //services.ConfigureApplicationCookie(options =>
            //{
            //    // Cookie settings
            //    options.Cookie.HttpOnly = true;
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            //    // If the LoginPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/Login.
            //    options.LoginPath = "/Account/Login";
            //    // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/AccessDenied.
            //    options.AccessDeniedPath = "/Account/AccessDenied";
            //    options.SlidingExpiration = true;
            //});

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IDirectoryManager, LocalDirectoryManager>();
            services.AddSingleton<IFileManager, LocalFileManager>();
            services.AddScoped<IFileInfoManager, LocalFileInfo>();
            services.AddScoped<IAuthorizationHandler, SignInHandler>();
            services.AddScoped<IAuthorizationHandler, RepositoryOwnerHandler>();
            // container will call Dispose for IDisposable types it creates
            //services.AddScoped<Repository>();


            services.AddAuthorization(options =>
            {
                options.AddPolicy("SignedIn", policy =>
                    policy.Requirements.Add(new SignInRequirement(true)));
                options.AddPolicy("RepoOwner", policy =>
                    policy.Requirements.Add(new RepositoryOwnerRequirement(true)));
            });


            services.AddDbContext<Models.MovieContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("MovieContext")));

            services.Configure<FileDirectoryOptions>(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
