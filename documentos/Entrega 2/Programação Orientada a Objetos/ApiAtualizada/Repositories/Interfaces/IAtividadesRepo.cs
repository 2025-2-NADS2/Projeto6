using Servidor_PI.Models;

namespace Servidor_PI.Repositories.Interfaces
{
    public interface IAtividadeRepo
    {
        Task<IEnumerable<Atividades>> Listar(); //retorna todas as atividades de forma assincrona
        Task<Atividades?> BuscarPorId(int id); //busca atividade por id
        public void AddAtv(Atividades atividade); //adiciona uma nova atividade
        Task<bool> DeletarAtv(int id); // deleta uma atividade

        Task<bool> AtualizarAtv(Atividades atividade);
        Task Salvar(); //salva alterações pendentes, possibilidade realizar varias operações antes de salvar
    }
}
