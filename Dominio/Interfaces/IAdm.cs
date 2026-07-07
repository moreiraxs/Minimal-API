using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using MinimalAPI.Dominio.entidades;
using MinimalAPI.Dominio.ModelsViews;
using MinimalAPI.DTOs;


namespace minimal_api.Dominio.Interfaces
{
    public interface IAdm
    {
        Adm? Login(LoginDTO loginDTO);

        void Incluir(Adm adm);

        void Update(int id, AdminDTO adminDTO);

        List<Adm> Todos(int? pagina);

        Adm? BuscarPorId(int id);
}
}