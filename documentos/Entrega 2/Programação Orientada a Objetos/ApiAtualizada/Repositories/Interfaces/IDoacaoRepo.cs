using Servidor_PI.Models;

namespace Servidor_PI.Repositories.Interfaces
{
    public interface IDoacaoRepo
    {
        Task<IEnumerable<Doacao>> BuscarTodas();
        Task<IEnumerable<Doacao>> BuscarPorUsuario(int usuarioId);
        Task<Doacao?> BuscarPorId(int id);
        Task<Doacao> Adicionar(Doacao doacao);
        Task<bool> Atualizar(Doacao doacao);
        Task Salvar();
    }
}

