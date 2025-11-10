using Servidor_PI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servidor_PI.Repositories.Interfaces
{
    public interface IUsuarioRepo
    {
        Task<IEnumerable<Usuarios>> Listar();
        Task<Usuarios?> BuscarPorId(int id);
        Task<Usuarios?> BuscarPorEmail(string email);
        Task<Usuarios> Adicionar(Usuarios usuario);
        Task<Usuarios> Atualizar(Usuarios usuario);
        Task<bool> Deletar(int id);
        Task Salvar();
    }
}