using System.Collections.Generic;
using MinimalAPI.Dominio.entidades;

namespace minimal_api.Dominio.Interfaces
{
    public interface IVeiculos
    {
        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);

        Veiculo? BuscarPorId(int id);

        void Update(int id, Veiculo veiculo);

        void Adicionar(Veiculo veiculo);

        void Deletar(Veiculo veiculo);
    }
}