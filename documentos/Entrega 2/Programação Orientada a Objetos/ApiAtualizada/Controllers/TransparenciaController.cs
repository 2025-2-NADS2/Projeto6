using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servidor_PI.DTOs;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransparenciaController : ControllerBase
    {
        private readonly ITransparenciaRepo _repo;
        private readonly IWebHostEnvironment _env;

        public TransparenciaController(ITransparenciaRepo repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var docs = await _repo.Listar(); // Utilizando o metodo Listar da minha interface ITransparenciRepo
            return Ok(docs); //retornando a chamada com todos os objetos ordenados
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var transparencia = await _repo.BuscarPorId(id);
            if(transparencia == null) { return NotFound("Documento de Transparencia não encontrado"); }
            return Ok(transparencia);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TransparenciaUploadDTO docUpload)
        {

            // EXTRAI O ID DO USUÁRIO LOGADO DO TOKEN JWT
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return Unauthorized("ID de usuário não encontrado no token.");
            }

            if (docUpload.PdfFile == null || docUpload.PdfFile.Length == 0) //verifica se o arquivo existe, ou se esta vazio
            {
                return BadRequest("Nenhum arquivo enviado");
            }
            // definindo o caminho aonde o arquivo sera salvo no servidor
            var nomeUnicoArquivo = Guid.NewGuid().ToString() + Path.GetExtension(docUpload.PdfFile.FileName); //guild gera um nome unico em binario, entao deve ser convertido para string. getextension, pega o nome da extensão no caso .pdf
            var uploadUrl = Path.Combine(_env.WebRootPath, "uploads_transparencia"); //webrooth pega a pasta padrão que ira salvar os arquivos de upload. path combine, junta o nome desta pasta padrão com a subpasta ao qual nosso documento sera anexado.
            var caminhodoUpload = Path.Combine(uploadUrl, nomeUnicoArquivo); //combina o nome unico com a url gerada do caminho de upload, gerando o caminho completo do nosso arquivo dentro do servidor

            if (!Directory.Exists(uploadUrl)) //confirma se existe a subpasta uploadUrl, caso não ele a cria
            {
                Directory.CreateDirectory(uploadUrl);
            }

            using (var stream = new FileStream(caminhodoUpload, FileMode.Create)) // FileMode.create cria o arquivo fisico
            {
                await docUpload.PdfFile.CopyToAsync(stream);
            }

            var doc = new Transparencia //gera o objeto, que ira ser salvo no banco de dados
            {
                Titulo = docUpload.Titulo,
                Descricao = docUpload.Descricao,
                LinkDownload = "/uploads_transparencia/" + nomeUnicoArquivo, // Salva o caminho relativo para o download
                NomeOriginal = docUpload.PdfFile.FileName, //salva o nome original do arquivo 
                DataPublicacao = DateTime.Now,
                UsuarioId = usuarioId

            };

            _repo.AddDoc(doc);
            await _repo.Salvar();
            return CreatedAtAction(nameof(BuscarPorId), new { id = doc.Id }, doc);
        }

        [AllowAnonymous]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            // Busca o documento no banco de dados
            var doc = await _repo.BuscarPorId(id);
            if (doc == null || string.IsNullOrEmpty(doc.LinkDownload))
            {
                return NotFound();
            }

            // Constrói o caminho completo no servidor
            var filePath = Path.Combine(_env.WebRootPath, doc.LinkDownload.TrimStart('/'));

            // Verifica se o arquivo existe fisicamente
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Retorna o arquivo
            var fileStream = System.IO.File.OpenRead(filePath);
            var mimeType = "application/pdf";
            var fileName = doc.NomeOriginal;

            return File(fileStream, mimeType, fileName);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoc(int id)
        {
            var doc = await _repo.BuscarPorId(id);
            if (doc != null && !string.IsNullOrEmpty(doc.LinkDownload)) //verifica se o documento existe ou esta vazio
            {
                var caminhoCompletoArquivo = Path.Combine(_env.WebRootPath, doc.LinkDownload.TrimStart('/')); // pega o caminho completo aonde esta o arquivo
                if (System.IO.File.Exists(caminhoCompletoArquivo)) //verifica a existencia 
                {
                    System.IO.File.Delete(caminhoCompletoArquivo);
                }
            }

            bool excluido = await _repo.DeleteDoc(id); //executa o delete

            if (excluido) //retorna se foi excluido 
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
        public async Task<IActionResult> AtualizarDoc(int id, [FromForm] TransparenciaUpdateDTO transparencia)
        {
            var docExistente = await _repo.BuscarPorId(id); //validação se o objeto existe

            if (docExistente == null)
            {
                return NotFound("Documento não encontrado para atualização.");
            }

            
            // manter o valor antigo se o novo for nulo/vazio
            if (!string.IsNullOrEmpty(transparencia.Titulo))
            {
                docExistente.Titulo = transparencia.Titulo;
            }
            
            if (!string.IsNullOrEmpty(transparencia.Descricao))
            {
                docExistente.Descricao = transparencia.Descricao;
            }

            // 2. Lidar com o NOVO Arquivo PDF, se enviado
            if (transparencia.PdfFile != null && transparencia.PdfFile.Length > 0)
            {
                // A. Excluir o arquivo antigo do armazenamento físico
                if (!string.IsNullOrEmpty(docExistente.LinkDownload))
                {
                    // Constrói o caminho completo usando _env.WebRootPath
                    var caminhoArquivoAntigo = Path.Combine(_env.WebRootPath, docExistente.LinkDownload.TrimStart('/'));

                    if (System.IO.File.Exists(caminhoArquivoAntigo))
                    {
                        System.IO.File.Delete(caminhoArquivoAntigo);
                    }
                }

                // Upload do novo arquivo (reutilizando sua lógica do CREATE)
                var nomeUnicoArquivo = Guid.NewGuid().ToString() + Path.GetExtension(transparencia.PdfFile.FileName);
                // Define o diretório de uploads dentro de wwwroot
                var uploadUrl = Path.Combine(_env.WebRootPath, "uploads_transparencia");
                var caminhodoUpload = Path.Combine(uploadUrl, nomeUnicoArquivo);

                // Garante que o diretório exista
                if (!Directory.Exists(uploadUrl))
                {
                    Directory.CreateDirectory(uploadUrl);
                }

                // Salva o novo arquivo fisicamente
                using (var stream = new FileStream(caminhodoUpload, FileMode.Create))
                {
                    await transparencia.PdfFile.CopyToAsync(stream);
                }

                // C. Atualizar os metadados do DB para o novo arquivo
                docExistente.LinkDownload = "/uploads_transparencia/" + nomeUnicoArquivo;
                docExistente.NomeOriginal = transparencia.PdfFile.FileName; // Atualiza o nome original
                docExistente.DataPublicacao = DateTime.Now; // Opcional: Atualiza a data de publicação
            }

            // Salva as alterações no banco de dados usando o repositório
            bool sucesso = await _repo.AtualizarDoc(docExistente);

            if (sucesso)
            {
                return Ok(docExistente); // Retorna o objeto atualizado
            }
            else
            {
                return StatusCode(500, "Erro interno ao salvar as alterações no banco de dados.");
            }
        }
    }
}

