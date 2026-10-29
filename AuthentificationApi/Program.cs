using AuthentificationApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000") // URL de ton client Blazor
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Service qui récupère le token auprès de Keycloak
builder.Services.AddHttpClient();
builder.Services.AddScoped<KeycloakAuthService>();

builder.Services.AddHttpClient("GestionOrdinateursApi", client =>
{
    client.BaseAddress = new Uri("http://gestionordinateurs-api:5299/api/");
});

var app = builder.Build();

//Activer CORS
app.UseCors("AllowBlazorClient");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();