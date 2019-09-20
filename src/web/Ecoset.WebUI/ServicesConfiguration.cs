using System;
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

            services.AddOptions();
            services.Configure<Options.EcosetAppOptions>(configuration.GetSection("EcosetApp"));
            services.Configure<ReportContentOptions>(configuration);
            services.Configure<EmailOptions>(configuration);
            services.Configure<PhantomOptions>(configuration);
            services.Configure<FileSystemPersistenceOptions>(configuration);
            services.Configure<PaymentOptions>(configuration);
            services.Configure<SeedOptions>(configuration.GetSection("Seed"));
            services.ConfigureOptions(typeof(UIConfigureOptions));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole("Admin");
                });
            });

            // 3. Add application services.
            var ecoset = new EcosetConnection(configuration["EcosetEndpoint"]);
            services.AddSingleton<IGeoSpatialConnection>(ecoset);
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<IJobService, JobService>();
            services.AddTransient<IJobProcessor, EcoSetJobProcessor>();
            services.AddTransient<IPaymentService, WorldPayPaymentService>();
            services.AddTransient<INotificationService, NotificationService>(); 
            services.AddTransient<IOutputPersistence, FileSystemOutputPersistence>(); 
            services.AddTransient<IReportGenerator, PhantomReportGenerator>(); 
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // 4. Hangfire for queues
            services.AddHangfire(config => 
                config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LEFT Data Package API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement { 
                    { 
                        new OpenApiSecurityScheme { 
                            Reference = new OpenApiReference { 
                                Type = ReferenceType.SecurityScheme, 
                                Id = "Bearer" 
                            } 
                        }, new string[] {}

                    }});
            });
        }

    }

    /// <summary>
    /// Routing technique for accessing wwwroot static items from this
    /// Razor Class Library from a dependent application. 
    /// See: https://stackoverflow.com/questions/51610513/can-razor-class-library-pack-static-files-js-css-etc-too 
    /// </summary>
    public class UIConfigureOptions : IConfigureOptions<StaticFileOptions>
    {
        public UIConfigureOptions(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public void Configure(StaticFileOptions options)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            // Basic initialization in case the options weren't initialized by any other component
            options.ContentTypeProvider = options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
            if (options.FileProvider == null && Environment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".scss"] = "text/plain";
            options.ContentTypeProvider = provider;

            options.FileProvider = options.FileProvider ?? Environment.WebRootFileProvider;

            var basePath = "wwwroot";

            var filesProvider = new ManifestEmbeddedFileProvider(GetType().Assembly, basePath);
            options.FileProvider = new CompositeFileProvider(options.FileProvider, filesProvider);
        }
    }

}