using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API
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
            // giving a MySql config for Production mode
            // the ConfigureWarnings part is included to ignore the MySql's Include warnings when the app is run
            // https://docs.microsoft.com/en-us/ef/core/querying/related-data#ignored-includes
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder.configurewarnings?view=efcore-2.1
            // Section 17 Lecture 182
            // services.AddDbContext<DataContext>(x => 
            // x.UseMySql(Configuration.GetConnectionString("DefaultConnection"))
            // .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning)));

            // the foll.line of code are part of normal DBContext code in an ASP.NET Web App, say. Unsure of another way 
            // to add this code other than physically type it out in a ASP.NET Web API
            // plus the Configuration. is part of the Configuration declared in the ctor
            // -- Note: This particular config was commented out for Section 17 Lecture 181, when we swapped SQLite for MySql (above)
            // The instructor wanted to copy the entire ConfigureServices method, paste it with a new name called ConfigureDevelopmentServices
            // because MVC would recognize the config as the config for Development mode, based on convention based naming
            // But I was having trouble trying to get the application to take this particular ConfigureServices method for Production, so I am
            // just commenting the below line out instead of having another separate ConfigureDevelopmentServices
            services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            //------------ Identity based configurations-----------------
            // AddIdentityCore: Adds and configures the identity system for the specified User type.
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.identityservicecollectionextensions.addidentitycore?view=aspnetcore-2.2
            // We are choosing AddIdentityCore over AddIdentity here. This is because AddIdentity is more pro-MVC .Net Core Project
            // i.e. for eg: it has provisions for cookie based authentication (We use JWT Token authentication), its own redirects to other pages in case of
            // bad login (based on Razor view logic of MVC). Since we are not using Razor views (we use Angular), we go with AddIdentityCore.
            IdentityBuilder builder = services.AddIdentityCore<User>(opt =>
            {
                // we are making these conditions very rudimentary so that we can use our weak passwords for demonstration
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 4;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;

            });

            // we do the foll. to query the users and their roles
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identitybuilder.-ctor?view=aspnetcore-2.2
            // the first two params are the types used for users and roles, and the 3rd is the The IServiceCollection to attach to.
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            // AddEntityFrameworkStores - Adds an Entity Framework implementation of identity information stores.
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.identityentityframeworkbuilderextensions.addentityframeworkstores?view=aspnetcore-2.2
            // we are basically telling Identity that we are going to use EntityFramework as our store
            // this will then store all our user classes in our database (the tables created can be seen during migration) using the DataContext
            builder.AddEntityFrameworkStores<DataContext>();

            // we have to explicitly configure the following because we are using AddIdentityCore instead of AddIdentity
            // (which already has these defined by default) 
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identitybuilder?view=aspnetcore-2.2
            // above link has links to the foll. 3 methods
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();

            //------------ End of Identity based configurations-----------------

            // Section 3, Lecture 34
            // letting the system know what authentication we need if we put the annotation [Authorize] in our controller
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes( Configuration.GetSection("AppSettings:Token").Value)),
                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                    });

            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-2.2#policy-based-role-checks
            services.AddAuthorization(options => {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
                options.AddPolicy("VIPOnly", policy => policy.RequireRole("VIP"));
            });
            
            services.AddMvc(options => 
            {
                // adding an AuthorizationFilter so that every request is authenticated (instead of using Authorize
                // data annotation in every controller i.e. for us, we can remove the annotation from the User, Photos and Message controllers)
                // also means that, all controller actions which are not marked with [AllowAnonymous] 
                // will require the user is authenticated with the default authentication scheme.
                // https://joonasw.net/view/apply-authz-by-default
                // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.authorizationpolicybuilder?view=aspnetcore-2.2
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

                // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.authorization.authorizefilter?view=aspnetcore-2.2
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddJsonOptions(opt => {
                // to bypass the self referencing loop error between user reference in photo and photo reference in user, for eg. 
                // Json.NET will ignore objects in reference loops and not serialize them. The first time an object is encountered it will be serialized 
                // as usual but if the object is encountered as a child object of itself the serializer will skip serializing it.
                // Link: https://stackoverflow.com/questions/11979637/what-does-referenceloophandling-ignore-in-newtonsoft-json-exactly-do
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddCors();

            // for cloudinary, the configure maps the properties in the CloudinarySettings class with the appsettings part
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

            // we do the reset below because the ConfigureServices() is being called twice when we drop a database (for reasons unaware)
            // so the AutoMapper is being called twice. So we call reset as a stopgap solution. We should only do these kind of hacks in
            // Development mode. Otherwise, we should ideally do a try/catch here to catch that error.
            Mapper.Reset();
            services.AddAutoMapper();
            
            // Transient lifetime services are created each time they're requested. This lifetime works best for lightweight, stateless services.
            services.AddTransient<Seed>(); // adding the data seed part as a service

            services.AddScoped<IDatingRepository, DatingRepository>();
            
            services.AddScoped<LogUserActivity>(); // ActionFilter has to be registered here as a service in order to use it in our controller
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment()) // development mode
            {
                app.UseDeveloperExceptionPage();
            }
            else // in production mode
            {
                // section 5 lecture 49
                // handling error globally by asp core rather than tediously decorating all the code with try catch blocks
                // builder because UseExceptionHandler returns IApplicationBuilder
                // accessing context inside the Run method
                app.UseExceptionHandler(builder => {
                    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.requestdelegate?view=aspnetcore-2.1
                    // The Run method takes a RequestDelegate, which is a delegate that in turn takes a HTTPContext (see link above). That's why the 'context' variable below is 
                    // automatically understood by the compiler as an HttpContext variable.
                    builder.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;// we can control the status code here

                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if(error != null)
                        {
                            // Read this to see how extensions work: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
                            context.Response.AddApplicationError(error.Error.Message); // from Extensions class                            
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
                // app.UseHsts();
            }

            seeder.SeedUsers(); // just for the seeding data part

            // app.UseHttpsRedirection();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            
            // this basically makes the application to run the default file like index.html (in our case) or default.aspx, etc.
            // Setting a default home page provides visitors a logical starting point when visiting your site.
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-2.2#serve-a-default-document
            app.UseDefaultFiles(); 
            
            // Static files, such as HTML, CSS, images, and JavaScript, are assets an ASP.NET Core app serves directly to clients.
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-2.2
            app.UseStaticFiles(); // looks inside the wwwroot folder and serves the content from there
            
            // middleware, sits between client request and API end point. We give this configuration so that mvc knows the routes of the SPA
            // for eg: visiting localhost:5000/members would mean that the API would know the SPA route /members is where it should be redirected
            // i.e. It's for when you want to handle the 404 within your front-end application (let all Routes go through so your front-end can handle them)
            // https://github.com/aspnet/JavaScriptServices/issues/973
            app.UseMvc(routes => {
                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new {controller = "Fallback", action = "Index"}
                );
            }); 
        }
    }
}
