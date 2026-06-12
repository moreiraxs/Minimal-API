using Microsoft.AspNetCore.SignalR;
using MinimalAPI.DTOs;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Olaa");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if(loginDTO.Email == "Adm@teste.com" && loginDTO.Senha == "123456")
    return Results.Ok("login efetuado com sucesso!");
    else
    return Results.Unauthorized();
});



app.Run();

