
using MinimalAPI.Dominio.Enums;





namespace Dominio.ModelsViews
{
    public record AdmLogado
    {
        public string Perfil { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}