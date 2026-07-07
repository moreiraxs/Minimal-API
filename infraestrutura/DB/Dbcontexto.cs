using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.entidades;
using Microsoft.Extensions.Configuration;

namespace MinimalAPI.infraestrutura.Db;

public class DbContexto : DbContext
{
    private readonly IConfiguration _configuration;

    public DbContexto(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public DbSet<Adm> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adm>().HasData(
            new Adm
            {
                Id = 1,
                Email  = "Lucas.moreira@gmail.com",
                Senha = "1234567",
                Perfil = "Administrador"

            }
        );
    }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("mysql");

            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString));
            }
        }
    }
}