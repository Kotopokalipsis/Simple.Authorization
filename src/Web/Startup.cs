using System;
using System.Linq;
using System.Text;
using Application;
using Application.Common.Interfaces.Infrastructure.Services;
using Domain.Entities;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Services.JwtGenerator;
using Infrastructure.Services.JwtReader;
using Infrastructure.Services.JwtValidator;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Web.Filters;

namespace Web
{
    public class Startup
    {
        public IWebHostEnvironment HostingEnvironment { get; }
        
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            HostingEnvironment = environment;
        }

        private IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureModules(services);
            
            ConfigureJwt(services);
            ConfigureIdentity(services);
            ConfigureCors(services);
            ConfigureMediatr(services);
            ConfigureMvc(services);
            ConfigureSwagger(services);
        }

        private void ConfigureModules(IServiceCollection services)
        {
            services.AddInfrastructureModule(Configuration);
            services.AddApplicationModule(Configuration);
        }

        private void ConfigureMediatr(IServiceCollection services)
        {
            services.AddScoped<Mediator>();
            services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
        }
        
        private void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("Cors", corsPolicyBuilder =>
                {
                    corsPolicyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            ));
        }
        
        private void ConfigureJwt(IServiceCollection services)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Identity:AccessApiKey"]!));
            
            services
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(
                    opt =>
                    {
                        opt.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = key,
                            ValidateAudience = false,
                            ValidateIssuer = false,
                        };
                    });
            
            services.AddScoped<IJwtGenerator, JwtGenerator>();
            services.AddScoped<IJwtReader, JwtReader>();
            services.AddScoped<IJwtValidator, JwtValidator>();
        }

        private void ConfigureIdentity(IServiceCollection services)
        {
            var builder = services.AddIdentityCore<User>();
            
            new IdentityBuilder(builder.UserType, builder.Services)
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddSignInManager<SignInManager<User>>()
                .AddUserManager<UserManager<User>>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 0;
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
                
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "API", Version = "v1"});
            });
        }

        private void ConfigureMvc(IServiceCollection services)
        {
            services
                .AddControllers(x =>
                {
                    x.Filters.Add(new ValidationFilter());
                    x.Filters.Add(new ExceptionFilter(HostingEnvironment));
                })
                .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    }
                )
                .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; })
                .AddFluentValidation(x 
                    => x.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            app.ApplyMigrations();
        }
    }
}