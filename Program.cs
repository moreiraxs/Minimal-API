using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MinimalAPI.DTOs;
using MinimalAPI.infraestrutura.Db;
using minimal_api.Dominio.Interfaces;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Dominio.ModelsViews;
using MinimalAPI.Dominio.entidades;
using MinimalAPI.Dominio.Enums;
using Dominio.ModelsViews;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Unicode;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;


#region Builder

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) jwtKey = "1234";

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IAdm, Administradorservico>();
        builder.Services.AddScoped<IVeiculos, VeiculosServico>();
        builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new()
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

     

        



        builder.Services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                builder.Configuration.GetConnectionString("mysql"),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
            );
        });

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

#endregion

        #region Home

        app.MapGet("/home", () => Results.Json(new Home()))
            .WithTags("Home");

        #endregion

        #region Adm

        string Gerartokenjwt(Adm adm)
        {
            if (string.IsNullOrEmpty(jwtKey)) return string.Empty;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("Email", adm.Email),
                new Claim(ClaimTypes.Role, adm.Perfil),
                new Claim("Perfil", adm.Perfil)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        app.MapPost("Administradores/login", ([FromBody] LoginDTO loginDTO, IAdm admService) =>
        {
            var adm = admService.Login(loginDTO);
            if (adm == null)
                return Results.Problem("Email ou senha incorretos!", statusCode: 401);

            var token = Gerartokenjwt(adm);
            return Results.Ok(new AdmLogado
            {
                Perfil = adm.Perfil,
                Email = adm.Email,
                Token = token
            });
        }).AllowAnonymous()
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
        .WithTags("Adm");

        app.MapPost("/adm", ([FromBody] AdminDTO adminDTO, IAdm admService) =>
        {
            if (string.IsNullOrEmpty(adminDTO.Email) || string.IsNullOrEmpty(adminDTO.Senha))
                return Results.BadRequest("Todos os campos são obrigatórios.");

            var administrador = new Adm
            {
                Perfil = adminDTO.Perfil.ToString(),
                Email = adminDTO.Email,
                Senha = adminDTO.Senha
            };

            admService.Incluir(administrador);
            return Results.Created("", new AdminModelView
            {
                Perfil = administrador.Perfil,
                Email = administrador.Email,
            });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithTags("Adm");
        

        app.MapGet("/adm", ([FromQuery] int? pagina, IAdm admService) =>
        {
            var adms = new List<AdminModelView>();
            var administradores = admService.Todos(pagina);

            foreach (var adm in administradores)
            {
                adms.Add(new AdminModelView
                {
                    Perfil = adm.Perfil,
                    Email = adm.Email,
                });
            }

            return Results.Ok(adms);
        }).RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithTags("Adm");

        app.MapGet("/adm/{id}", ([FromRoute] int id, IAdm admService) =>
        {
            var administrador = admService.BuscarPorId(id);
            if (administrador == null)
                return Results.NotFound("Administrador não encontrado.");

            return Results.Ok(new AdminModelView
            {
                Perfil = administrador.Perfil,
                Email = administrador.Email,
            });
        }).RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithTags("Adm");

        #endregion

        #region Veiculos

        ErrosDeValicao ValidaDTO(VeiculoDTO veiculoDTO)
        {
            var erros = new ErrosDeValicao
            {
                Messages = new List<string>()
            };

            if (string.IsNullOrEmpty(veiculoDTO.Nome))
                erros.Messages.Add("O campo Nome é obrigatório.");

            if (string.IsNullOrEmpty(veiculoDTO.Marca))
                erros.Messages.Add("O campo Marca é obrigatório.");

            if (veiculoDTO.Ano < 1986)
                erros.Messages.Add("O campo Ano deve ser maior que 1980.");

            return erros;
        }

        app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculos veiculosService) =>
        {
            var erros = ValidaDTO(veiculoDTO);
            if (erros.Messages.Count > 0)
                return Results.BadRequest(erros);

            var veiculo = new Veiculo
            {
                Nome = veiculoDTO.Nome,
                Marca = veiculoDTO.Marca,
                Ano = veiculoDTO.Ano
            };

            veiculosService.Adicionar(veiculo);
            return Results.Ok("Veiculo adicionado com sucesso!");
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
        .WithTags("Veiculos");

        app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculos veiculosService) =>
        {
            var veiculos = veiculosService.Todos(pagina ?? 1);
            return Results.Ok(veiculos);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
        .WithTags("Veiculos");

        app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculos veiculosService) =>
        {
            var veiculo = veiculosService.BuscarPorId(id);
            if (veiculo == null)
                return Results.NotFound();

            return Results.Ok(veiculo);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
        .WithTags("Veiculos");

        app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculos veiculosService) =>
        {
            var veiculo = veiculosService.BuscarPorId(id);
            if (veiculo == null)
                return Results.NotFound();

            veiculosService.Deletar(veiculo);
            return Results.Ok("Veiculo deletado com sucesso!");
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithTags("Veiculos");

        #endregion

        #region App

        app.UseSwaggerUI();
        app.UseSwagger();

        app.Run();

        #endregion
    }
}