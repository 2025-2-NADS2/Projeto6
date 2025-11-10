using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servidor_PI.DTOs;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;
using Servidor_PI.Services;

namespace Servidor_PI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepo _repo;
        private readonly TokenService _tokenService;

        // Construtor - injeta a dependência do repositório
        public UsuariosController(IUsuarioRepo repo, TokenService tokenService)
        {
            _repo = repo; // Guarda a referência do repositório para uso nos métodos abaixo
            _tokenService = tokenService;
        }

        // ===========================================
        // ========== LISTAR TODOS OS USUÁRIOS =======
        // ===========================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _repo.Listar(); // Busca todos os registros no banco

            // Monta uma lista de visualização (ViewDTO)
            var lista = usuarios.Select(u => new UsuarioViewDTO
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                IsAdmin = u.IsAdmin
            }).ToList();

            return Ok(lista); // Retorna a lista de usuários simplificada, sem a senha para segurança
        }

        // ===========================================
        // ========== BUSCAR USUÁRIO POR ID ==========
        // ===========================================
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var usuario = await _repo.BuscarPorId(id); // Busca o usuário no banco

            if (usuario == null)
            {
                return NotFound("Usuário não encontrado."); // Retorna erro 404 se não existir
            }

            // Cria o DTO de visualização
            var view = new UsuarioViewDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                IsAdmin = usuario.IsAdmin
            };

            return Ok(view); // Retorna o usuário encontrado sem a senha para segurança
        }

        
        // Cadastrar novo usuario 
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> CadastrarUsuario([FromBody] UsuarioCreateDTO dto)
        {
            // Valida se os campos obrigatórios estão corretos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica se o e-mail já está cadastrado
            var existente = await _repo.BuscarPorEmail(dto.Email);
            if (existente != null)
            {
                return BadRequest(new { message = "E-mail já cadastrado." });
            }

            // Cria o novo objeto de usuário
            var usuario = new Usuarios
            {
                Nome = dto.Nome,
                Email = dto.Email,
                // Gera um hash seguro da senha
                Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                DataNascimento = dto.DataNascimento ?? DateTime.MinValue, // Evita erro de nulo
                Endereco = dto.Endereco,
                IsAdmin = false
            };

            // Adiciona e salva no banco
            await _repo.Adicionar(usuario);
            await _repo.Salvar();

            // Cria DTO de visualização para retorno
            var view = new UsuarioViewDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                IsAdmin = usuario.IsAdmin
            };

            // Retorna 201 (Created) com o link para o novo recurso
            return CreatedAtAction(nameof(BuscarPorId), new { id = usuario.Id }, view);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto, [FromServices] TokenService tokenService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar o usuário pelo email
            var usuario = await _repo.BuscarPorEmail(dto.Email);
            if (usuario == null)
            {
                return Unauthorized(new { message = "Usuário não encontrado." });
            }

            // Validar senha com BCrypt
            bool senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha);
            if (!senhaCorreta)
            {
                return Unauthorized(new { message = "Senha incorreta." });
            }

            // Gerar token JWT
            var token = tokenService.GerarToken(usuario);

            // Retornar token e dados básicos do usuário
            return Ok(new
            {
                token,
                usuario = new UsuarioViewDTO
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    IsAdmin = usuario.IsAdmin
                }
            });
        }


        
        // Atualizar novo usuario
        
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarUsuario(int id, [FromBody] UsuarioUpdateDTO dto)
        {
            // Validação de entrada
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Busca o usuário existente
            var usuario = await _repo.BuscarPorId(id);
            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // Atualiza apenas os campos que foram enviados (parcial update)
            if (!string.IsNullOrEmpty(dto.Nome)) { usuario.Nome = dto.Nome; }
            if (!string.IsNullOrEmpty(dto.Email)) { usuario.Email = dto.Email; }
            if (dto.DataNascimento.HasValue) { usuario.DataNascimento = dto.DataNascimento.Value; }
            if (!string.IsNullOrEmpty(dto.Endereco)) { usuario.Endereco = dto.Endereco; }

            // Atualiza senha se enviada (hash novamente)
            if (!string.IsNullOrEmpty(dto.Senha))
            {
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            }

            // Salva alterações
            await _repo.Atualizar(usuario);
            await _repo.Salvar();

            // Monta o DTO de resposta
            var view = new UsuarioViewDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                IsAdmin = usuario.IsAdmin
            };

            return Ok(view);
        }

        //Deletar usuario
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarUsuario(int id)
        {
            // Tenta deletar
            var deletado = await _repo.Deletar(id);

            if (!deletado)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            return Ok(new { message = "Usuário deletado com sucesso." });
        }
    }
}


