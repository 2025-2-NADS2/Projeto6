using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data.Map;
using Servidor_PI.Models;

namespace Servidor_PI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Atividades> Atividades { get; set; }
        public DbSet<Transparencia> Transparencias { get; set; }
        public DbSet<Ouvidoria> Ouvidorias { get; set; }
        public DbSet<Eventos> Eventos { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Doacao> Doacao { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AtividadesMap()); //Instancia nosso Map para que o banco de dados crie as colunas seguindo o requisitado
            modelBuilder.ApplyConfiguration(new TransparenciaMap());
            modelBuilder.ApplyConfiguration(new OuvidoriaMap());
            modelBuilder.ApplyConfiguration(new EventosMap());
            modelBuilder.ApplyConfiguration(new UsuariosMap());
            modelBuilder.ApplyConfiguration(new DoacaoMap());

            base.OnModelCreating(modelBuilder); 
        }
    }
}
