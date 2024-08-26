using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using Ecoset.GeoTemporal.Remote;
using Ecoset.WebUI.Data;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;
using Ecoset.WebUI.Services.Concrete;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Ecoset.WebUI {

    /// <summary>
    /// Methods to add modules of Ecoset functionality into a custom
    /// web application.
    /// </summary>
    public static class ServicesConfiguration {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddEcosetUI(this IServiceCollection services, IConfiguration configuration) {

            // 1. Add database connection
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // 2. Add user accounts
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddCookie(cfg => cfg.SlidingExpiration = true);
            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Home/Login");

            services.AddOptions();
            services.Configure<Options.EcosetAppOptions>(configuration.GetSection("EcosetApp"));
            services.Configure<ReportContentOptions>(configuration);
            services.Configure<EmailOptions>(configuration.GetSection("Email"));
            services.Configure<PhantomOptions>(configuration);
            services.Configure<FileSystemPersistenceOptions>(configuration);
            services.Configure<SeedOptions>(configuration.GetSection("Seed"));
            services.ConfigureOptions(typeof(UIConfigureOptions));
            services.Configure<TextLookup>((settings) =>
            {
                configuration.GetSection("CustomText").Bind(settings);
            });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole("Admin");
                });

            // 3. Add application services.
            var ecoset = new EcosetConnection(configuration["EcosetEndpoint"]);
            services.AddSingleton<IGeoSpatialConnection>(ecoset);
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<IJobService, JobService>();
            services.AddTransient<IJobProcessor, EcoSetJobProcessor>();
            services.AddSingleton<IDataRegistry, EcosetDataRegistry>();
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddTransient<IPaymentService, WorldPayPaymentService>();
            services.AddTransient<INotificationService, NotificationService>(); 
            services.AddTransient<IDataFormatter, TiffDataFormatter>(); 
            services.AddTransient<IOutputPersistence, FileSystemOutputPersistence>(); 
            services.AddSingleton<IReportGenerator, DinkReportGenerator>(); 
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // 4. Hangfire for queues
            services.AddHangfire(config => {
                config
                    .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"))
                    .WithJobExpirationTimeout(TimeSpan.FromDays(30));
                }
            );
        }

        public static void AddEcosetDataPackageAPI(this IServiceCollection services, IConfiguration configuration) {

            services.AddAuthentication()
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters() 
                    {
                        ValidIssuer = configuration["Tokens:Issuer"],
                        ValidAudience = configuration["Tokens:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Tokens:Key"]))
                    };
                });

            services.AddSwaggerGen(c => 
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "LEFT Data Package API", 
                    Version = "v1"
                });
                c.AddSecurityDefinition("jwt_auth", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Specify the authorisation token",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement { 
                    { 
                        new OpenApiSecurityScheme { 
                            Reference = new OpenApiReference { 
                                Type = ReferenceType.SecurityScheme, 
                                Id = "jwt_auth" 
                            } 
                        }, new string[] {}

                    }});
                    
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public static void UseEcosetMigrations(this IApplicationBuilder app, IConfiguration configuration) {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.Migrate();
        }

        public static void UseEcosetRoles(this IApplicationBuilder app, IConfiguration configuration)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            {
                RoleManager<IdentityRole> roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

                string[] roleNames = { "Admin" };
                foreach (string roleName in roleNames)
                {
                    bool roleExists = roleManager.RoleExistsAsync(roleName).Result;
                    if (!roleExists)
                    {
                        IdentityRole identityRole = new IdentityRole(roleName);
                        IdentityResult identityResult = roleManager.CreateAsync(identityRole).Result;
                    }
                }
            }
        }

        public static void UseEcosetAdminUser(this IApplicationBuilder app, IConfiguration configuration)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var userManager = (UserManager<ApplicationUser>)scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>));
            var user = userManager.FindByNameAsync(configuration["Account:Admin:DefaultAdminUserName"]).Result;
            if (user == null)
            {
                user = new ApplicationUser()
                {
                    UserName = configuration["Account:Admin:DefaultAdminUserName"],
                    FirstName = "Primary",
                    Surname = "Administrator",
                    OrganisationName = "Ecoset",
                    OrganisationType = OrganisationType.NonCommercial,
                    Credits = 9999,
                    EmailConfirmed = true,
                    Email = configuration["Account:Admin:DefaultAdminUserName"]
                };
                userManager.CreateAsync(user, configuration["Account:Admin:DefaultAdminPassword"]).Wait();
                userManager.AddToRoleAsync(user, "Admin").Wait();
            }
        }

    }

    /// <summary>
    /// Routing technique for accessing wwwroot static items from this
    /// Razor Class Library from a dependent application. 
    /// See: https://stackoverflow.com/questions/51610513/can-razor-class-library-pack-static-files-js-css-etc-too 
    /// </summary>
    public class UIConfigureOptions(IWebHostEnvironment environment) : IConfigureOptions<StaticFileOptions>
    {
        public IWebHostEnvironment Environment { get; } = environment;

        public void Configure(StaticFileOptions options)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            // Basic initialization in case the options weren't initialized by any other component
            options.ContentTypeProvider ??= new FileExtensionContentTypeProvider();
            if (options.FileProvider == null && Environment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".scss"] = "text/plain";
            options.ContentTypeProvider = provider;

            options.FileProvider ??= Environment.WebRootFileProvider;

            var basePath = "wwwroot";

            var filesProvider = new ManifestEmbeddedFileProvider(GetType().Assembly, basePath);
            options.FileProvider = new CompositeFileProvider(options.FileProvider, filesProvider);
        }
    }

    public class TextLookup
    {
        public Dictionary<string, string> Values {get;set;}
    }

}