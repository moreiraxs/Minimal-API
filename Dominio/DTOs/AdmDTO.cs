using MinimalAPI.Dominio.Enums;


namespace MinimalAPI.Dominio.ModelsViews;



public class AdminDTO
{

    public Perfil? Perfil {get; set;}
    public string Email {get; set;}

    public string Senha {get; set;}
}