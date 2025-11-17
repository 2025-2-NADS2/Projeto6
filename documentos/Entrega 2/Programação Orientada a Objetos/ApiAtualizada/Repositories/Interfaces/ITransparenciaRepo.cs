using Servidor_PI.Models;

namespace Servidor_PI.Repositories.Interfaces
{
    public interface ITransparenciaRepo
    {
        Task<IEnumerable<Transparencia>> Listar();
        Task<Transparencia?> BuscarPorId(int id);
        public void AddDoc(Transparencia doc);
        Task <bool> DeleteDoc(int id);
        Task <bool> AtualizarDoc(Transparencia doc);
        Task Salvar();
    }
}
