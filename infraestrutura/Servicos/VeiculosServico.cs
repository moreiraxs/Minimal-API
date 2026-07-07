using MinimalAPI.DTOs;
using MinimalAPI.infraestrutura.Db;
using minimal_api.Dominio.Interfaces;
using MinimalAPI.Dominio.entidades;
using Microsoft.EntityFrameworkCore;

namespace MinimalAPI.Dominio.Servicos;

public class VeiculosServico : IVeiculos
{
    private readonly DbContexto _contexto;

    public VeiculosServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
    {
      var query = _contexto.Veiculos.AsQueryable();

      if(!string.IsNullOrEmpty(nome))
      {
        query = query.Where(v => EF.Functions.Like(v.Nome, $"%{nome}%"));
      }
      int itensPerPage = 10;
      if(pagina != null)
      {
        query = query.Skip(((int)pagina- 1) * itensPerPage).Take(itensPerPage);
      }

      return query.ToList();
      
    }

    public void Adicionar(Veiculo veiculo)
    {
        _contexto.Veiculos.Add(veiculo);
        _contexto.SaveChanges();
    }

    public void Update(int id, Veiculo veiculo)
    {
        veiculo.Id = id;
        _contexto.Veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscarPorId(int id)
    {
        return _contexto.Veiculos.Find(id);
    }

    public void Deletar(Veiculo veiculo)
    {
        _contexto.Veiculos.Remove(veiculo);
        _contexto.SaveChanges();
    }

    
}