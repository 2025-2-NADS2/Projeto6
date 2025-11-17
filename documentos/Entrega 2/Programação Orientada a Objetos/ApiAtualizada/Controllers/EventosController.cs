using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servidor_PI.DTOs;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;
using SQLitePCL;

namespace Servidor_PI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly IEventosRepo _repo;
        private readonly IWebHostEnvironment _env;

        public EventosController(IEventosRepo repo, IWebHostEnvironment env)
        {
            _repo = repo; //variável utilizada para referenciar os métodos da interface IEventosRepo.
            _env = env; //variável utilizada para aplicar corretamente a navegabilidade entre diretórios internos de pastas (caminhos de upload).
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var eventos = await _repo.Listar(); //utilizado do método listar para pegar todos os objetos 
            return Ok(eventos); // retorna a lista de objetos 

        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var evento = await _repo.BuscarPorId(id);
            if (evento == null) { return NotFound("Atividade não encontrada"); }
            return Ok(evento);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EventosUploadDTO eveUpload)
        {

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return Unauthorized("ID de usuário não encontrado no token.");
            }

            if (eveUpload.ImagemArquivo == null || eveUpload.ImagemArquivo.Length == 0) //verifica se o arquivo existe, ou se esta vazio
            {
                return BadRequest("Nenhuma imagem anexada para Evento");
            }
            // Guid.NewGuid() é uma função que gera um identificador único de 128 bits.
            var nomeUnicoImg = Guid.NewGuid().ToString() + Path.GetExtension(eveUpload.ImagemArquivo.FileName); // Path.GetExtension pega somente a extensão do arquivo.
            var uploadUrl = Path.Combine(_env.WebRootPath, "imagens_eventos"); // Combina o caminho raiz com a subpasta de destino.
            var caminhodoUpload = Path.Combine(uploadUrl, nomeUnicoImg); // Combina o caminho de pastas com o nome único do arquivo.

            if (!Directory.Exists(uploadUrl))
            {
                Directory.CreateDirectory(uploadUrl);
            }

            using (var stream = new FileStream(caminhodoUpload, FileMode.Create)) // FileMode.create cria o arquivo físico no disco.
            {
                await eveUpload.ImagemArquivo.CopyToAsync(stream); // Copia o arquivo recebido para o disco.
            }

            var eve = new Eventos // gera o objeto Evento, que será salvo no banco de dados.
            {
                Titulo = eveUpload.Titulo,
                Descricao = eveUpload.Descricao,
                LocalEvento = eveUpload.LocalEvento,
                LinkImagem = "/imagens_eventos/" + nomeUnicoImg, // Salva o caminho relativo da imagem.
                DataEvento = eveUpload.DataEvento,
                DataPostagem = DateTime.Now,
                UsuarioId = usuarioId

            };

            _repo.AddEve(eve);

            await _repo.Salvar();
            return CreatedAtAction(nameof(BuscarPorId), new { id = eve.Id }, eve);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarEve(int id)
        {
            var eve = await _repo.BuscarPorId(id); // busca a atividade pelo id
            if (eve != null && !string.IsNullOrEmpty(eve.LinkImagem)) // verifica se há conteúdo e link de imagem.
            {
                var caminhoCompletoArquivo = Path.Combine(_env.WebRootPath, eve.LinkImagem.TrimStart('/')); // Pega o caminho completo da imagem no servidor.
                if (System.IO.File.Exists(caminhoCompletoArquivo)) // verifica a existência do arquivo.
                {
                    System.IO.File.Delete(caminhoCompletoArquivo); // deleta a imagem física.
                }
            }

            bool excluido = await _repo.DeletarEve(id); // deletando o objeto no DB.

            if (excluido) // retornando se sucesso
            {
                return Ok(new { message = "Documento excluído com sucesso." });
            }
            else
            {
                return NotFound(new { message = "Documento não encontrado." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarEve(int id, [FromForm] EventosUpdateDTO evento) // novos parâmetros para atualização
        {
            var eveExistente = await _repo.BuscarPorId(id); // buscando

            if (eveExistente == null)
            {
                return NotFound("Documento não encontrado para atualização.");
            }

            if (!string.IsNullOrEmpty(evento.Titulo)) // se for vazio mantém o antigo.
            {
                eveExistente.Titulo = evento.Titulo;
            }

            if (!string.IsNullOrEmpty(evento.Descricao))
            {
                eveExistente.Descricao = evento.Descricao;
            }

            if (!string.IsNullOrEmpty(evento.LocalEvento))
            {
                eveExistente.LocalEvento = evento.LocalEvento;
            }

            if (evento.DataEvento.HasValue) // verifica se há um novo valor para a data.
            {
                eveExistente.DataEvento = evento.DataEvento.Value;
            }

            // Lidar com o NOVO Arquivo img, se enviado
            if (evento.ImagemArquivo != null && evento.ImagemArquivo.Length > 0)
            {
                // Excluir o arquivo antigo do armazenamento físico
                if (!string.IsNullOrEmpty(eveExistente.LinkImagem))
                {
                    // Constrói o caminho completo usando _env.WebRootPath
                    var caminhoArquivoAntigo = Path.Combine(_env.WebRootPath, eveExistente.LinkImagem.TrimStart('/'));

                    if (System.IO.File.Exists(caminhoArquivoAntigo))
                    {
                        System.IO.File.Delete(caminhoArquivoAntigo); // Deleta o arquivo antigo.
                    }
                }

                // Fazer Upload do novo arquivo 
                var nomeUnicoArquivo = Guid.NewGuid().ToString() + Path.GetExtension(evento.ImagemArquivo.FileName);
                // Define o diretório de uploads dentro de wwwroot
                var uploadUrl = Path.Combine(_env.WebRootPath, "imagens_eventos");
                var caminhodoUpload = Path.Combine(uploadUrl, nomeUnicoArquivo);

                // Garante que o diretório exista
                if (!Directory.Exists(uploadUrl))
                {
                    Directory.CreateDirectory(uploadUrl);
                }

                // Salva o novo arquivo fisicamente
                using (var stream = new FileStream(caminhodoUpload, FileMode.Create))
                {
                    await evento.ImagemArquivo.CopyToAsync(stream);
                }

                // C. Atualizar os metadados do DB para o novo arquivo
                eveExistente.LinkImagem = "/imagens_eventos/" + nomeUnicoArquivo;
                eveExistente.DataPostagem = DateTime.Now; // Opcional: Atualiza a data de publicação
            }

            // 3. Salvar as alterações no banco de dados usando o repositório
            bool sucesso = await _repo.AtualizarEve(eveExistente);

            if (sucesso)
            {
                return Ok(eveExistente); // Retorna o objeto atualizado
            }
            else
            {
                return StatusCode(500, "Erro interno ao salvar as alterações no banco de dados.");
            }
        }

    }
}