using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SalasReuniaoApi.Auth;
using SalasReuniaoApi.Data;
using SalasReuniaoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------- CONFIGURAÇÃO ----------------------------

// Chave usada para assinar e validar o token JWT.
const string chaveJwt = "MINHA_CHAVE_SUPER_SECRETA_SALAS_REUNIAO_2026";

// Usuário fixo (sem banco de dados de usuários, conforme solicitado na avaliação).
const string usuarioEmail = "teste@teste.com";
const string usuarioSenha = "123";

// ---------------------------- SERVIÇOS ----------------------------

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Digite: Bearer {seu token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Aplica automaticamente as migrations e cria o banco SQLite na primeira execução.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ---------------------------- LOGIN ----------------------------

app.MapPost("/login", (LoginRequest login) =>
{
    if (login.Email == usuarioEmail && login.Senha == usuarioSenha)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(chaveJwt);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, login.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Results.Ok(new { token = tokenString });
    }

    return Results.Unauthorized();
});

// ---------------------------- CRUD SALAS DE REUNIÃO ----------------------------

// GET /salas - lista todas as salas
app.MapGet("/salas", async (AppDbContext db) =>
    await db.SalasReuniao.ToListAsync())
    .RequireAuthorization();

// GET /salas/{id} - busca uma sala por id
app.MapGet("/salas/{id}", async (AppDbContext db, int id) =>
{
    var sala = await db.SalasReuniao.FindAsync(id);
    return sala is not null ? Results.Ok(sala) : Results.NotFound();
})
    .RequireAuthorization();

// POST /salas - cria uma nova sala
app.MapPost("/salas", async (AppDbContext db, SalaReuniao sala) =>
{
    db.SalasReuniao.Add(sala);
    await db.SaveChangesAsync();
    return Results.Created($"/salas/{sala.Id}", sala);
})
    .RequireAuthorization();

// PUT /salas/{id} - atualiza uma sala existente
app.MapPut("/salas/{id}", async (AppDbContext db, int id, SalaReuniao salaAtualizada) =>
{
    var sala = await db.SalasReuniao.FindAsync(id);

    if (sala is null)
    {
        return Results.NotFound();
    }

    sala.Nome = salaAtualizada.Nome;
    sala.Capacidade = salaAtualizada.Capacidade;
    sala.PossuiProjetor = salaAtualizada.PossuiProjetor;

    await db.SaveChangesAsync();

    return Results.Ok(sala);
})
    .RequireAuthorization();

// DELETE /salas/{id} - remove uma sala
app.MapDelete("/salas/{id}", async (AppDbContext db, int id) =>
{
    var sala = await db.SalasReuniao.FindAsync(id);

    if (sala is null)
    {
        return Results.NotFound();
    }

    db.SalasReuniao.Remove(sala);
    await db.SaveChangesAsync();

    return Results.Ok();
})
    .RequireAuthorization();

app.Run();
