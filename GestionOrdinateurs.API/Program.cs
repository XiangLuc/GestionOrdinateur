using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddDbContext<ParcInformatiqueContext>();
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<IReparationService, ReparationService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<JournalService>();
builder.Services.AddEndpointsApiExplorer();

// ajouter swagger JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {token}"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Configuration Keycloak : l'API valide les tokens émis par Keycloak
var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"];
var keycloakRealm = builder.Configuration["Keycloak:Realm"];
var keycloakHttpClient = new HttpClient();
JsonWebKeySet? keycloakKeys = null;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    // Garde les noms de claims de Keycloak ("roles", "preferred_username") sans les convertir
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // Récupère les clés publiques de Keycloak (mises en cache tant que la clé du token est connue)
        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
        {
            if (keycloakKeys == null || !keycloakKeys.Keys.Any(k => k.Kid == kid))
            {
                var json = keycloakHttpClient
                    .GetStringAsync($"{keycloakBaseUrl}/realms/{keycloakRealm}/protocol/openid-connect/certs")
                    .GetAwaiter().GetResult();
                keycloakKeys = new JsonWebKeySet(json);
            }
            return keycloakKeys.GetSigningKeys();
        },
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Keycloak:ValidIssuer"],
        ValidateAudience = true,
        ValidAudience = "account",
        // Les rôles sont dans le claim "roles" (mapper Keycloak) et le nom dans "preferred_username"
        RoleClaimType = "roles",
        NameClaimType = "preferred_username"
    };
    options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Utilisateur utilisant un Token Valide --- {context.Principal?.Identity?.Name ?? "Anonymous"}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Token Invalide");
                return Task.CompletedTask;
            }
        };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//Initialisation de la base de données
using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();

    db.Database.Migrate();

    if(!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Login="user1", Role=RoleEnum.Admin },
            new User { Login="user2", Role=RoleEnum.Gestionnaire },
            new User { Login="user3", Role=RoleEnum.Gestionnaire }
        );

        db.SaveChanges();
    }
}

//Activer CORS
app.UseCors("AllowBlazorClient");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
