using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servidor_PI.DTOs;
using Servidor_PI.Models;
using Servidor_PI.Repositories.Interfaces;

namespace Servidor_PI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtividadesController : ControllerBase
    {
        private readonly IAtividadeRepo _repo;
        private readonly IWebHostEnvironment _env;

        public AtividadesController(IAtividadeRepo repo, IWebHostEnvironment env)
        {
            _repo = repo; //variavel utilizada para referenciar os motodos da interface IAtividadeRepo
            _env = env; //varival utilizada para aplicar corretamente a navegabilidade entre diretorios internos de pastas
        }

        [AllowAnonymous] //permite qualquer pessoa ter acesso
        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var atividade = await _repo.Listar(); //utilizado do motodo listar para pegar todos os objetos ouvidoria
            return Ok(atividade); // retorna a lista de objetos ordenados

        }

        [AllowAnonymous] //permite qualquer pessoa ter acesso
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id) //busca uma atividade em especifica pelo id
        {
            var atividade = await _repo.BuscarPorId(id);
            if(atividade == null) { return NotFound("Atividade não encontrada"); }
            return Ok(atividade);
        }


        [Authorize(Roles = "Admin")] // somente admins tem acesso a essa rota
        [HttpPost]
        public async Task<IActionResult> Create([FromForm]AtividadesUploadDTO atvUpload) //passagem de parametros para criação de nova atividade
        {
            // EXTRAI O ID DO USUÁRIO LOGADO DO TOKEN JWT
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return Unauthorized("ID de usuário não encontrado no token.");
            }

            if (atvUpload.ImagemArquivo == null || atvUpload.ImagemArquivo.Length == 0) //verifica se o arquivo existe, ou se esta vazio
            {
                return BadRequest("Nenhuma imagem anexada para Atividade"); // retorna erro caso condição true
            }
            //Guild.newGuild é uma função que gera um identificador unico de 128 bits alfanumericos. toString é usado para converte-lo a uma string
            var nomeUnicoImg = Guid.NewGuid().ToString() + Path.GetExtension(atvUpload.ImagemArquivo.FileName); // Path.GetExtension, é um metodo que pega somente a extensão do arquivo, no caso seria um .pdf
            var uploadUrl = Path.Combine(_env.WebRootPath, "imagens_atividades"); //combina o arquivo raiz de pastas com a subpasta que criamos
            var caminhodoUpload = Path.Combine(uploadUrl, nomeUnicoImg); // combina nosso caminho de pastas com nosso nome unico, gerando o caminho correto de upload

            if (!Directory.Exists(uploadUrl)) // verifica se existe o repositorio para as imagens
            {
                Directory.CreateDirectory(uploadUrl); // caso não exista cria um
            }

            using (var stream = new FileStream(caminhodoUpload, FileMode.Create)) // FileMode.create cria o arquivo fisico
            {
                await atvUpload.ImagemArquivo.CopyToAsync(stream); // faz uma copia fisica no bd
            }

            var atv = new Atividades //gera o objeto, e atualiza o bd
            {
                Titulo = atvUpload.Titulo,
                Descricao = atvUpload.Descricao,
                LinkImagem = "/imagens_atividades/" + nomeUnicoImg, // Salva o caminho relativo para o download
                DataPostagem = DateTime.Now,
                UsuarioId = usuarioId

            };

            _repo.AddAtv(atv); //metodo para adicionar a atividade

            await _repo.Salvar(); // salva de forma assincrona, permetuando a operação no bd
            return CreatedAtAction(nameof(BuscarPorId), new { id = atv.Id }, atv); // preenche o id do objeto, e retorna o metodo utilizado para buscar esse objeto ja criado
        }

        [Authorize(Roles = "Admin")] //somente adms autorizados
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarAtv(int id) //deletar um id
        {
            var atv = await _repo.BuscarPorId(id); //busca a atividade pelo id
            if (atv != null && !string.IsNullOrEmpty(atv.LinkImagem)) //verifica se há conteudo
            {
                var caminhoCompletoArquivo = Path.Combine(_env.WebRootPath, atv.LinkImagem.TrimStart('/')); //Pega o caminho da imagem
                if (System.IO.File.Exists(caminhoCompletoArquivo)) //verifica a existencia
                {
                    System.IO.File.Delete(caminhoCompletoArquivo); //deleta a imagem
                }
            }

            bool excluido = await _repo.DeletarAtv(id); //deletando o objeto

            if (excluido) //returnando se sucesso
            {
                return Ok(new { message = "Documento excluído com sucesso." }); // se a condição for verdadeira, retorna sucesso
            }
            else
            {
                return NotFound(new { message = "Documento não encontrado." }); // caso não retorna o erro
            }
        }


        [Authorize(Roles = "Admin")] // somente adms liberados
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarAtv(int id,[FromForm] AtividadesUpdateDTO atividade) //novos parametros para atualização usando DTO para captação dos atributos
        {
            var atvExistente = await _repo.BuscarPorId(id); //buscando

            if (atvExistente == null) //verificando se o id esta vazio "null"
            {
                return NotFound("Documento não encontrado para atualização."); // caso sim, retorna o erro
            }

            if (!string.IsNullOrEmpty(atividade.Titulo)) // caso novo parametro for vazio, mantem o atributo antigo
            {
                atvExistente.Titulo = atividade.Titulo; // atualizando o existente pelo novo
            }

            if (!string.IsNullOrEmpty(atividade.Descricao))// caso novo parametro vazio, mantem o atributo anterior
            {
                atvExistente.Descricao = atividade.Descricao;// atualizando o existente com o novo parametro
            }

            // Lidar com o NOVO Arquivo img, se enviado
            if (atividade.ImagemArquivo != null && atividade.ImagemArquivo.Length > 0)
            {
                // Excluir o arquivo antigo do armazenamento físico
                if (!string.IsNullOrEmpty(atvExistente.LinkImagem))
                {
                    // Constrói o caminho completo usando _env.WebRootPath
                    var caminhoArquivoAntigo = Path.Combine(_env.WebRootPath, atvExistente.LinkImagem.TrimStart('/'));

                    if (System.IO.File.Exists(caminhoArquivoAntigo)) //verifica a existencia deste arquivo
                    {
                        System.IO.File.Delete(caminhoArquivoAntigo); // caso exista, executa um delete nele. abrindo espaço para o novo arquivo
                    }
                }

                // Fazer Upload do novo arquivo 
                var nomeUnicoArquivo = Guid.NewGuid().ToString() + Path.GetExtension(atividade.ImagemArquivo.FileName);
                // Define o diretório de uploads dentro de wwwroot
                var uploadUrl = Path.Combine(_env.WebRootPath, "imagens_atividades"); // combina a pasta padrão, com a especifica para imagens de atividades
                var caminhodoUpload = Path.Combine(uploadUrl, nomeUnicoArquivo); // crie o caminho especifico para encontrar nossa imagem

                // Garante que o diretório exista
                if (!Directory.Exists(uploadUrl))
                {
                    Directory.CreateDirectory(uploadUrl); // cria o diretorio caso não existe
                }

                // Salva o novo arquivo fisicamente
                using (var stream = new FileStream(caminhodoUpload, FileMode.Create))
                {
                    await atividade.ImagemArquivo.CopyToAsync(stream);
                }

                // C. Atualizar os metadados do DB para o novo arquivo
                atvExistente.LinkImagem = "/imagens_atividades/" + nomeUnicoArquivo;
                atvExistente.DataPostagem = DateTime.Now; // Opcional: Atualiza a data de publicação
            }

            // 3. Salvar as alterações no banco de dados usando o repositório
            bool sucesso = await _repo.AtualizarAtv(atvExistente);

            if (sucesso)
            {
                return Ok(atvExistente); // Retorna o objeto atualizado
            }
            else
            {
                return StatusCode(500, "Erro interno ao salvar as alterações no banco de dados."); // retorna erro
            }
        }

    }
}

