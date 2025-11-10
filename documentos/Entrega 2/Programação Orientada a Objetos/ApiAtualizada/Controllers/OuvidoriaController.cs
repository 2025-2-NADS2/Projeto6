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
        
        public OuvidoriaController (IOuvidoriaRepo repo)
        {
            _repo = repo;
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
        public async Task<IActionResult> Create([FromForm]OuvidoriaUpdateDTO ouv)
        {
            var ouvidoria = new Ouvidoria 
            {
                Titulo = ouv.Titulo,
                Descricao = ouv.Descricao,
                DataOuvidoria = DateTime.Now,
            };

            _repo.AddOuv(ouvidoria);
            await _repo.Salvar();
            return CreatedAtAction(nameof(BuscarPorId), new { id = ouvidoria.Id }, ouvidoria);
            //CreatedAtAction =  Indica que o recurso foi criado com sucesso httpo201 e cria um Location (URL que indica de forma certeira o caminho para encontrar o objeto criado)
            //nameof (BuscarPorId) = mostra qual action usada pra visualizar o novo objeto "BuscarPorId"
            // new {id = ouvidoria.id} = gera automaticamente o id do objeto
            // ouvidoria = retorna o objeto completo, com id adicionado
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarOuv(int id)
        {
            bool excluido = await _repo.DeletarOuv(id); //executa o delete

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
