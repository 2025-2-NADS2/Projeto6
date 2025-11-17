using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Repositories
{
    public class DoacaoRepos : IDoacaoRepo
    {
        private readonly AppDbContext _context;

        public DoacaoRepos(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Doacao>> BuscarTodas()
        {
            return await _context.Doacao
                .Include(d => d.Usuario)
                .ToListAsync();
        }


        public async Task<Doacao> Adicionar(Doacao doacao)
        {
            await _context.Doacao.AddAsync(doacao);
            return doacao;
        }

        public async Task<Doacao?> BuscarPorId(int id)
        {
            return await _context.Doacao.Include(d => d.Usuario).FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Doacao>> BuscarPorUsuario(int usuarioId)
        {
            return await _context.Doacao.Where(d => d.UsuarioId == usuarioId).ToListAsync();
        }

        public async Task<bool> Atualizar(Doacao doacao)
        {
            _context.Doacao.Update(doacao);
            var affected = await _context.SaveChangesAsync();
            return affected > 0;
        }

        public async Task Salvar()
        {
            await _context.SaveChangesAsync();
        }
    }
}

