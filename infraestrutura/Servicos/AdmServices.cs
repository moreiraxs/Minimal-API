using MinimalAPI.Dominio.entidades;
using MinimalAPI.DTOs;
using MinimalAPI.infraestrutura.Db;
using minimal_api.Dominio.Interfaces;
using MinimalAPI.Dominio.ModelsViews;

namespace MinimalAPI.Dominio.Servicos;

public class Administradorservico : IAdm
{
    private readonly DbContexto _contexto;

    public Administradorservico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public Adm? Login(LoginDTO loginDTO)
    {
         var adm =  _contexto.Administradores.FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
         return adm;
    }

     public void Incluir(Adm adm)
    {
        _contexto.Administradores.Add(adm);
        _contexto.SaveChanges();
    }

    public void Update(int id, AdminDTO adminDTO)
    {
        _contexto.Administradores.Where(a => a.Id == id).ToList().ForEach(a =>
        {
            a.Perfil = adminDTO.Perfil.ToString();
            a.Email = adminDTO.Email;
            a.Senha = adminDTO.Senha;
        });
    }

    public List<Adm> Todos(int? pagina)
    {
        int tamanhoPagina = 10;
        int paginaAtual = pagina ?? 1;

        var administradores = _contexto.Administradores
            .Skip((paginaAtual - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        return administradores;
    }

      public Adm? BuscarPorId(int id)
    {
        return _contexto.Administradores.Find(id);
    }

}
    
