using Servidor_PI.Models;

namespace Servidor_PI.Repositories.Interfaces
{
    public interface IOuvidoriaRepo
    {
        Task<IEnumerable<Ouvidoria>> Listar(); //retorna todas as atividades de forma assincrona
        Task<Ouvidoria?> BuscarPorId(int id); //busca atividade por id
        public void AddOuv(Ouvidoria ouvidoria); //adiciona uma nova atividade
        Task<bool> DeletarOuv(int id); // deleta uma atividade

        Task<bool> AtualizarOuv(Ouvidoria ouvidoria);
        Task Salvar(); //salva alterações pendentes, possibilidade realizar varias operações antes de salvar
    }
}
