using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servidor_PI.DTOs;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OuvidoriaController : ControllerBase
    {
        private readonly IOuvidoriaRepo _repo;//importa a nossa interface para utilizar seus metodos
        private readonly IEmailService _emailService; // Injeção do Serviço de E-mail

        public OuvidoriaController (IOuvidoriaRepo repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task <IActionResult> GetAll()
        {
            var ouvidoria = await _repo.Listar(); //utilizado do motodo listar para pegar todos os objetos ouvidoria
            return Ok(ouvidoria); // retorna a lista de objetos
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var ouvidoria1 = await _repo.BuscarPorId(id);
            if(ouvidoria1 == null)
            {
                return NotFound("Ouvidoria não encontrada");
            }
            return Ok(ouvidoria1);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] OuvidoriaCreateDTO ouv) // Usando o DTO com EmailRemetente
        {
            // Obter e validar o ID do usuário logado (necessário para a FK)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            // Se o ID não for encontrado no token, é um erro de token/configuração.
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int usuarioId))
            {
                // Este erro só deve ocorrer se o token JWT for inválido ou não contiver o 'id'
                return Forbid("Informação de ID de usuário ausente ou inválida no token.");
            }

            // 2. Salvar no Banco de Dados (ignora EmailRemetente do DTO)
            var ouvidoria = new Ouvidoria
            {
                Titulo = ouv.Titulo,
                Descricao = ouv.Descricao,
                DataOuvidoria = DateTime.Now,
                UsuarioId = usuarioId // FK preenchida com o ID do usuário logado
            };

            _repo.AddOuv(ouvidoria);
            await _repo.Salvar();

            // 3. Enviar E-mail de Notificação 
            try
            {
                var emailBody = $"Nova Mensagem de Ouvidoria (#id: {ouvidoria.Id}) recebida.\n\n" +
                                $"Título: {ouv.Titulo}\n" +
                                $"Descrição:\n{ouv.Descricao}\n\n" +
                                $"--- Detalhes do Contato ---\n" +
                                $"Remetente (Email): {ouv.EmailRemetente}\n" +
                                $"Usuário ID (FK): {usuarioId}";

                await _emailService.SendEmailAsync(
                    "andre3000ferreira@gmail.com", // EMAIL DESTINO DA SUA INSTITUIÇÃO
                    $"OUVIDORIA: {ouv.Titulo}",
                    emailBody
                );
            }
            catch (Exception ex)
            {
                // Se o e-mail falhar, a mensagem ainda foi salva no DB. Logue o erro.
                // Não queremos que a falha do e-mail impeça a criação do registro 201.
                Console.WriteLine($"Erro ao enviar e-mail de ouvidoria: {ex.Message}");
            }

            return CreatedAtAction(nameof(BuscarPorId), new { id = ouvidoria.Id }, ouvidoria);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarOuv(int id, [FromForm]OuvidoriaUpdateDTO dados)
        {
            var ouvExiste = await _repo.BuscarPorId(id); //busca o id
            
            if (ouvExiste == null) { return NotFound("Ouvidoria não encontrada."); }

            if (!string.IsNullOrEmpty(dados.Titulo))
            {
                ouvExiste.Titulo = dados.Titulo;
            }

            if (!string.IsNullOrEmpty(dados.Descricao))
            {
                ouvExiste.Descricao = dados.Descricao;    
            }

            ouvExiste.DataOuvidoria = DateTime.Now;
            bool sucesso = await _repo.AtualizarOuv(ouvExiste);

            if (sucesso)
            {
                return Ok(ouvExiste); // Retorna o objeto atualizado
            }
            else
            {
                return StatusCode(500, "Erro interno ao salvar as alterações no banco de dados.");
            }
        }
    }
}
