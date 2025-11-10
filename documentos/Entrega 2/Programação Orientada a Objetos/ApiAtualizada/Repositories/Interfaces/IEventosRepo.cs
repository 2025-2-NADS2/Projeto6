using Servidor_PI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servidor_PI.Repositories.Interfaces
{
    public interface IEventosRepo
    {
        Task<IEnumerable<Eventos>> Listar(); //retorna todos  os eventos de forma assincrona
        Task<Eventos?> BuscarPorId(int id); //busca evento por id
        public void AddEve(Eventos evento); //adiciona um novo evento
        Task<bool> DeletarEve(int id); // deleta um evento

        Task<bool> AtualizarEve(Eventos evento); //atualiza um evento existente
        Task Salvar(); //salva alterações pendentes, possibilidade realizar varias operações antes de salvar
    }
}
