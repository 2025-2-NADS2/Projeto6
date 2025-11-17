using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Servidor_PI.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Servidor_PI.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        // Construtor para injetar as configurações (appsettings)
        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        // Método que gera o token JWT
        public string GerarToken(Usuarios usuario)
        {
            // 1️ Define as "claims" — informações contidas no token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email), // Email do usuário
                new Claim("id", usuario.Id.ToString()),                 // ID do usuário
                new Claim("isAdmin", usuario.IsAdmin.ToString()),
                new Claim(ClaimTypes.Role, usuario.IsAdmin ? "Admin" : "User") // Flag de admin
            };

            // 2️ Pega a chave secreta do appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            // 3️ Cria as credenciais de assinatura
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4️ Define o token em si
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],      // quem emite
                audience: _config["Jwt:Audience"],  // quem consome
                claims: claims,                     // dados internos
                expires: DateTime.Now.AddHours(2),  // tempo de expiração
                signingCredentials: creds           // assinatura digital
            );

            // 5️ Gera o token em formato string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

