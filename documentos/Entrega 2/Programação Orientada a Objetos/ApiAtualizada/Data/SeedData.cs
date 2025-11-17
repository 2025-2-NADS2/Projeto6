using Microsoft.EntityFrameworkCore;
using Servidor_PI.Data;
using Servidor_PI.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
        {
            // Se já existir um admin, não cria outro
            if (context.Usuarios.Any(u => u.IsAdmin))
                return;

            var admin = new Usuarios
            {
                Nome = "Administrador",
                Email = "dede@gmail.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("mudar@1101"),
                DataNascimento = new DateTime(1990, 1, 1),
                Endereco = "Sistema",
                IsAdmin = true
            };

            context.Usuarios.Add(admin); // ja cria por padrao um administrador
            context.SaveChanges();
        }
    }
}
