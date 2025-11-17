using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Repositories
{
    public class AtividadesRepo : IAtividadeRepo
    {
        private readonly AppDbContext _context;

        public AtividadesRepo(AppDbContext context)
        {
            _context = context; //instancia o dbcontext
        }
        public async Task<IEnumerable<Atividades>> Listar() { //IEnumerale gera uma list com os objetos citados
            var atividades = await _context.Atividades.ToListAsync();    //função que lista os objetos de forma assincrona
            return atividades; 
        }
        public async Task<Atividades?> BuscarPorId(int id) // busca o objeto em especifico
        {
            var atividade = await _context.Atividades.FindAsync(id);
            return atividade;
        }

        public void AddAtv(Atividades atividade)
        {
           _context.Atividades.Add(atividade); //função do dbcontext que adiciona atividade
           
        }
        public async Task Salvar()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletarAtv(int id)
        {
            var atividade = await _context.Atividades.FindAsync(id);

            if (atividade == null) 
            {
                return false;
            }

            _context.Atividades.Remove(atividade); //deleta a atividade
            await _context.SaveChangesAsync(); // Persiste a exclusão
            return true;
        }

        public async Task<bool> AtualizarAtv(Atividades atividade)
        {
            // Informa ao Contexto que este objeto existe e está pronto para ser atualizado.
            _context.Atividades.Update(atividade);

            // 2. Persiste as mudanças no banco de dados.
            var rowsAffected = await _context.SaveChangesAsync();

            // 3. Retorna se a operação teve sucesso.
            return rowsAffected > 0;
        }
    }
}
