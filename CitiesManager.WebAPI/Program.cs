using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using CitiesManager.Infrastructure.DatabaseAccess;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ADD SERVICES TO THE CONTAINER
builder.Services.AddControllers(options =>
{
    //Sets the Content-Type of the response body for all action methods
    options.Filters.Add(new ProducesAttribute("application/json"));

    //Sets the Content-Type in the request body that the server will accept
    options.Filters.Add(new ConsumesAttribute("application/json"));

}).AddXmlSerializerFormatters()
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;  // Correct way to disable automatic 400 response
});

//The AddApiVersioning() method adds api versioning related services to IoC container. The method is located in the Microsoft.AspNetCore.Mvc.Versioning package. 
builder.Services.AddApiVersioning(
    config =>
    {
        //Set retrieval method to be from url segment
        config.ApiVersionReader = new UrlSegmentApiVersionReader();

        //The two statements below together will set the default api-version details for Http requests that do NOT contain api-version details
        config.DefaultApiVersion = new ApiVersion(2, 0);
        config.AssumeDefaultVersionWhenUnspecified = true;
    }
 );

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

//Adds the services that is required for Swagger middleware to read the metadata (HTTP method, URL, attributes, etc) of the endpoints (web api action methods). 
builder.Services.AddEndpointsApiExplorer();

//Adds the services that is required for Swagger middleware to read api-versions of endpoints in the application
builder.Services.AddVersionedApiExplorer(options =>
{
    options.SubstituteApiVersionInUrl = true;  //Enables the Swagger middleware to use the api-version of an endpoint to construct the url of the endpoint with {version:ApiVersion} replaced by the actual api version of the endpoint. 
    options.GroupNameFormat = "'ver'VVV";  //Enables the Swagger middleware to use the api-version of the endpoints to categorize the endpoints into groups. Each endpoint will be placed in its corresponding group according to its api-version. The name of the group that an endpoint belongs to can be found by replacing VVV with the endpoint's api-version, i.e. if an endpoint has an api-version of 2.2, then the endpoint will be placed in the "ver2.2" group; if an endpoint has an api-version of 2.0, then the endpoint will be placed in the "ver2" group. Note if this statement is omitted, the default will be placing all the endpoints irrespective of version in the "v1" group. 
});

//Adds the services that is required for Swagger middleware to generate swagger.json file(s) for our api; To use AddSwaggerGen() method, we need to install Swashbuckle.AspNetCore package. 
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));

    //Enables the Swagger middleware to create a swagger.json file representing the group "ver1" in the folder /swagger/ver1/. This /swagger/ver1/swagger.json file will also have additional details of Title = "Cities Web API" and Version = "1.0" which will we eventually end up as the header of the UI page. 
    options.SwaggerDoc("ver1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "1.0" });

    //Enables the Swagger middleware to create a swagger.json file representing the group "ver2" in the folder /swagger/ver2/. This /swagger/ver2/swagger.json file will also have additional details of Title = "Cities Web API" and Version = "2.0" which will we eventually end up as the header of the UI page. 
    options.SwaggerDoc("ver2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "2.0" });
});

//Adds CORS-related services to the ASP.NET Core's Dependency Injection (DI) container.
//Then proceeds to configure these services by establishing a default CORS policy which accepts web requests from the origin http://localhost:4200.
//By default, if the request contains additional headers, it will not be accepted. To accept request with specific additional details we need the WithHeaders() method. 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>());
        policyBuilder.WithHeaders("Authorization", "origin", "accept", "content-type");
        policyBuilder.WithMethods("GET", "POST", "PUT", "DELETE");
    });

    options.AddPolicy("For4100ClientOnly", policyBuilder =>
    {
        policyBuilder.WithOrigins(builder.Configuration.GetSection("AllowedOrigins2").Get<string[]>());
        policyBuilder.WithHeaders("Authorization", "origin", "accept");
        policyBuilder.WithMethods("GET");
    });
});


builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(
    options =>
    {
        options.Password.RequiredLength = 5;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
    }
).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NotAuthenticatedOnly", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return !(context.User.Identity.IsAuthenticated);
        });
    });
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
    
app.UseHsts();
app.UseHttpsRedirection();

app.UseStaticFiles();

//Adds Swagger middleware to the middleware pipeline. Recall the Swaggeer middleware reads metadata of the endpoints in our api and automatically creates the swagger.json file(s) for our api. 
app.UseSwagger();

//Adds SwaggerUI middleware to the middleware pipeline. The SwaggerUI middleware uses the swagger.json file(s) to create an interactive API documentation(s).
app.UseSwaggerUI(options =>
{
    //Use the /swagger/ver1/swagger.json file to generate a UI tab with tab name "1.0". 
    options.SwaggerEndpoint("/swagger/ver1/swagger.json", "1.0");
    //Use the /swagger/ver1/swagger.json file to generate a UI tab with tab name "2.0".
    options.SwaggerEndpoint("/swagger/ver2/swagger.json", "2.0");

});  


app.UseRouting();

//Adds CORS middleware which will make use of the CORS related service to add 'Access-Control-Allow-Origin' header
//to the response allowing request from the origin http://localhost:4200 as configured in the services.
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
