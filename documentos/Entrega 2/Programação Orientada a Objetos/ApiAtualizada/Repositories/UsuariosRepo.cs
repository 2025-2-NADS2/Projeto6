using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Repositories
{
    public class UsuarioRepo : IUsuarioRepo
    {
        private readonly AppDbContext _context;

        public UsuarioRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuarios>> Listar()
        {
            var usuarios =  await _context.Usuarios.ToListAsync();
            return (usuarios);
        }

        public async Task<Usuarios?> BuscarPorId(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            return usuario;
        }

        public async Task<Usuarios?> BuscarPorEmail(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuarios> Adicionar(Usuarios usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            return usuario;
        }

        public async Task<Usuarios> Atualizar(Usuarios usuario)
        {
            _context.Usuarios.Update(usuario);
            return usuario;
        }

        public async Task<bool> Deletar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) { return false; }
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task Salvar()
        {
            await _context.SaveChangesAsync();
        }
    }
}
