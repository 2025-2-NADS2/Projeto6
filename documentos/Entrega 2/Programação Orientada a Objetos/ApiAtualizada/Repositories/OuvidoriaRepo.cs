using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Repositories
{
    public class OuvidoriaRepo : IOuvidoriaRepo
    {
        private readonly AppDbContext _context;

        public OuvidoriaRepo(AppDbContext context)
        {
            _context = context; //instancia o dbcontext
        }

        public async Task<IEnumerable<Ouvidoria>> Listar()
        {                       //IEnumerale gera uma list com os objetos citados
            var ouvidorias = await _context.Ouvidorias.ToListAsync();    //função que lista os objetos de forma assincrona
            return ouvidorias; //retorna a lista de objetos ouvidoria
        }

        public async Task<Ouvidoria?> BuscarPorId(int id)
        {
            var ouv = await _context.Ouvidorias.FindAsync(id);
            return ouv;
        }

        public void AddOuv(Ouvidoria ouvidoria)
        {
            _context.Ouvidorias.AddAsync(ouvidoria);    
        }

        public async Task Salvar()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletarOuv(int id)
        {
            var ouvidoria = await _context.Ouvidorias.FindAsync(id); //busca se o objeto existe, caso não exista retorna false
            if (ouvidoria == null)
            {
                return false;
            }
            _context.Ouvidorias.Remove(ouvidoria); // remove o objeto
            await _context.SaveChangesAsync(); //salva as alterações
            return true; //retorna o resultado 
        }

        public async Task<bool> AtualizarOuv(Ouvidoria ouv)
        {
            _context.Update(ouv);
            var linhasAfetadas = _context.SaveChanges(); //verifica se foi afetada algum elemento do meu objeto
            return linhasAfetadas > 0; //caso ao menos 1 linha tenha sido afetada, retorna true
        }
    }
}
