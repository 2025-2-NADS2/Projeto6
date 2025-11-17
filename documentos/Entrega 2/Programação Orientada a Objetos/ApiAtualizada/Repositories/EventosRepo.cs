using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Repositories
{
    public class EventosRepo : IEventosRepo
    {
        private readonly AppDbContext _context;

        public EventosRepo(AppDbContext context)
        {
            _context = context; //instancia o dbcontext
        }
        public async Task<IEnumerable<Eventos>> Listar()
        { //IEnumerale gera uma list com os objetos citados
            var eventos = await _context.Eventos.ToListAsync();    //função que lista os objetos de forma assincrona
            return eventos;
        }
        public async Task<Eventos?> BuscarPorId(int id) // busca o objeto em especifico
        {
            var eventos = await _context.Eventos.FindAsync(id);
            return eventos;
        }

        public void AddEve(Eventos evento)
        {
            _context.Eventos.Add(evento); //função do dbcontext que adiciona atividade

        }
        public async Task Salvar()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletarEve(int id)
        {
            var eventos = await _context.Eventos.FindAsync(id);

            if (eventos == null) // retorna caso a atividade exista
            {
                return false;
            }

            _context.Eventos.Remove(eventos); //deleta a atividade
            await _context.SaveChangesAsync(); // Persiste a exclusão
            return true;
        }

        public async Task<bool> AtualizarEve(Eventos evento)
        {
            // Informa ao Contexto que este objeto existe e está pronto para ser atualizado.
            _context.Eventos.Update(evento);

            // Persiste as mudanças no banco de dados.
            var rowsAffected = await _context.SaveChangesAsync();

            // Retorna se a operação teve sucesso.
            return rowsAffected > 0;
        }
    }
}