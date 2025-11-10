using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Repositories
{
    public class TransparenciaRepo : ITransparenciaRepo
    {
        private readonly AppDbContext _context;

        public TransparenciaRepo(AppDbContext context)
        {
            _context = context; //instancia o dbcontext
        }

        public async Task<IEnumerable<Transparencia>> Listar() //IEnumerale gera uma list com os objetos citados
        {
           var ListaDoc = await _context.Transparencias.ToListAsync();  //função que lista os objetos de forma assincrona
            return ListaDoc; //retorna a lista de atividades
        }
        public async Task<Transparencia?> BuscarPorId(int id) // busca o objeto em especifico
        {
            var doc = await _context.Transparencias.FindAsync(id);
            return doc;
        }
        public void AddDoc(Transparencia doc)
        {
            _context.Transparencias.Add(doc); //função do dbcontext que adiciona atividade
        }
        public async Task Salvar() 
        { 
            await _context.SaveChangesAsync(); 
        }

        public async Task<bool> DeleteDoc(int id)
        {
            var documento = await _context.Transparencias.FindAsync(id);
            if (documento == null) { return false; }
            _context.Transparencias.Remove(documento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AtualizarDoc(Transparencia doc)
        {
           _context.Update(doc);
           var rowsAffected = await _context.SaveChangesAsync();
           return rowsAffected > 0;

        }
    }
}
