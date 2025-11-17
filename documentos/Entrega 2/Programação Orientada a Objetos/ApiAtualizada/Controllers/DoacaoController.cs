
using Microsoft.AspNetCore.Authorization; // Necessário para usar atributos como [Authorize] e [AllowAnonymous] (controle de acesso).
using Microsoft.AspNetCore.Mvc; // Contém a classe base ControllerBase e os atributos [ApiController], [HttpGet], etc., fundamentais para criar APIs.
using Servidor_PI.Models; // Importa os modelos de dados da aplicação (suas classes de Doação).
using Servidor_PI.Models.DTOs; // Importa os Data Transfer Objects (DTOs), usados para enviar e receber dados específicos (evita expor o modelo completo).
using Servidor_PI.Repositories.Interfaces; // Importa a interface do Repositório de Doação, usada para interagir com o banco de dados.
using System.Net.Http.Headers; // Necessário para configurar cabeçalhos de requisições HTTP (como o token "Bearer").
using System.Text; // Usado para codificar o JSON para a requisição HTTP (Encoding.UTF8).
using System.Text.Json; // Biblioteca de serialização/desserialização JSON, usada para criar o corpo da requisição do Mercado Pago.


// --- NAMESPACE E CLASSE ---

namespace Servidor_PI.Controllers // Define o agrupamento lógico da classe (o seu módulo de servidor).
{
    [ApiController] // Indica que esta classe é um Controller de API, ativando comportamentos de resposta automática.
    [Route("api/[controller]")] // Define a rota base da API (ex: /api/doacao, pois [controller] é substituído pelo nome da classe sem 'Controller').
    public class DoacaoController : ControllerBase // Declara a classe, herdando de ControllerBase para obter funcionalidades de API.
    {

        private readonly IDoacaoRepo _repo; // Campo privado para armazenar o Repositório de Doação (acesso ao DB).
        private readonly IConfiguration _config; // Campo privado para armazenar as Configurações (acesso a chaves e URLs).
        private readonly IHttpClientFactory _httpClientFactory; // Campo privado para criar instâncias seguras de HttpClient (para chamadas externas).



        public DoacaoController(IDoacaoRepo repo, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _repo = repo; // Repositório injetado para manipulação do banco de dados (buscar, adicionar, atualizar).
            _config = config; // Configurações da aplicação injetadas (ex: chaves de APIs, URLs de retorno).
            _httpClientFactory = httpClientFactory; // Factory injetada para realizar requisições HTTP externas (chamar a API do Mercado Pago).
        }


        [AllowAnonymous] // Permite que esta rota seja acessada mesmo por usuários não autenticados.
        [HttpPost] // Define que esta ação responde a requisições HTTP POST.
        public async Task<IActionResult> CreatePreference([FromBody] DoacaoCreateDTO dto) // Recebe os dados da doação (valor) no corpo da requisição.
        {
            // 1. valida dto
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. pegar id do usuário do token (claims)
            // Inicializa como NULL, permitindo que a doação seja anônima por padrão.
            int? usuarioId = null;
            bool isAnonima = true; // Assumimos que é anônima por padrão

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id"); // Tenta encontrar a informação do ID do usuário no token.

            // Se o token for válido e o ID puder ser convertido, o usuário está logado.
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
            {
                usuarioId = parsedId; // Define o ID do usuário logado.
                isAnonima = false;    // Marca como NÃO anônima.
            }
            // Se userIdClaim for null, o código simplesmente continua, com usuarioId = null. 
            // Isso evita o retorno 'return Unauthorized()'

            // 3. criar objeto Doacao local com status pending (ainda sem pagamento)
            var doacao = new Doacao // Cria um novo objeto Doacao, que será salvo no seu banco de dados.
            {
                Valor = dto.Valor,
                UsuarioId = usuarioId, // Será o ID do usuário (se logado) ou null (se anônimo).
                Anonima = isAnonima, // Propriedade extra para controle de exibição
                Status = "pending",
                DataCriacao = DateTime.UtcNow
            };

            // adiciona ao contexto (não salva ainda)
            await _repo.Adicionar(doacao);
            await _repo.Salvar();

            // 4. Criar preferência no Mercado Pago
            var accessToken = _config["MercadoPago:AccessToken"];
            if (string.IsNullOrEmpty(accessToken)) return StatusCode(500, "Access token do Mercado Pago não configurado.");

            // montar o body conforme API do Mercado Pago
            var preference = new
            {
                items = new[]
                {
            new {
                title = "Doação",
                quantity = 1,
                currency_id = "BRL",
                unit_price = dto.Valor
            }
        },
                external_reference = doacao.Id.ToString(),
                back_urls = new
                {
                    success = _config["Frontend:UrlSuccess"] ?? "https://seufrontend/sucesso",
                    failure = _config["Frontend:UrlFailure"] ?? "https://seufrontend/falha",
                    pending = _config["Frontend:UrlPending"] ?? "https://seufrontend/pendente"
                },
                auto_return = "approved"
            };

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(preference);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // endpoint de criação de preference
            var mpResponse = await client.PostAsync("https://api.mercadopago.com/checkout/preferences", content);
            var responseBody = await mpResponse.Content.ReadAsStringAsync();

            if (!mpResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)mpResponse.StatusCode, responseBody);
            }

            // parse do retorno (acessa init_point e sandbox_init_point)
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            string checkoutLink = "";
            if (root.TryGetProperty("init_point", out var initPoint))
            {
                checkoutLink = initPoint.GetString() ?? "";
            }
            else if (root.TryGetProperty("sandbox_init_point", out var sandboxInit))
            {
                checkoutLink = sandboxInit.GetString() ?? "";
            }

            // 5. atualizar a doacao com o link de checkout e salvar
            doacao.CheckoutLink = checkoutLink;
            await _repo.Atualizar(doacao);

            // 6. retornar o link para o cliente
            var result = new DoacaoViewDTO // Assumindo que seu DTO tem o campo CheckoutLink
            {
                Id = doacao.Id,
                Valor = doacao.Valor,
                Status = doacao.Status,
                CheckoutLink = doacao.CheckoutLink,
                DataCriacao = doacao.DataCriacao
            };

            return Ok(result);
        }

        // Webhook: Mercado Pago envia notificações aqui
        [AllowAnonymous] // Deve ser anônimo, pois o Mercado Pago é quem faz a chamada, não um usuário autenticado.
        [HttpPost("webhook")] // Define a rota específica para o webhook (ex: /api/doacao/webhook).
        public async Task<IActionResult> Webhook([FromBody] JsonElement body) // Recebe o corpo da notificação (webhook) como um elemento JSON genérico.
        {
            // VALIDAÇÃO DE ASSINATURA SECRETA 
            var secretConfig = _config["MercadoPago:WebhookSecret"]; // Lê o segredo de validação do webhook (chave de segurança) das configurações.

            // Tenta obter o cabeçalho "X-Secret" que o MP envia
            // Request.Headers.TryGetValue retorna um array de strings (StringValues)

            // método de validação
            if (!Request.Headers.TryGetValue("x-signature", out var signatureHeader)) // Tenta ler o cabeçalho 'x-signature' que contém a chave secreta.
            {
                Console.WriteLine("Header x-signature ausente"); // Loga a ausência do cabeçalho.
                return Unauthorized("Assinatura ausente"); // Retorna 401: a requisição não tem a assinatura de segurança esperada.
            }


            Console.WriteLine($"Header X-Secret recebido: {signatureHeader}"); // Loga o valor recebido para depuração.

            // Pega o primeiro valor do header e compara com a configuração
            var secretValue = signatureHeader.FirstOrDefault(); // Extrai a string real do valor do cabeçalho.

            // COMPARA AS CHAVES
            // Use String.Equals para comparação segura entre strings
            if (secretValue == null || !secretValue.Equals(secretConfig, StringComparison.Ordinal)) // Compara o valor recebido com o valor configurado (validação de segurança).
            {
                // Resposta 401: Rejeitar a requisição com segredo incorreto
                return Unauthorized("Assinatura secreta inválida."); // Retorna 401: a assinatura de segurança não confere.
            }

            // O Mercado Pago envia backgrounds notifications que podem conter 'id' e 'topic' (ou resource)
            // Vamos pegar as informações e consultar a API do Mercado Pago para detalhes do pagamento.
            try
            {
                Console.WriteLine("Webhook recebido do Mercado Pago:"); // Loga o recebimento.
                Console.WriteLine(body.ToString()); // Loga o corpo da requisição para inspeção.
                string resourceId = ""; // ID do pagamento ou recurso no Mercado Pago.
                string topic = ""; // Tipo de evento (ex: payment, merchant_order).

                if (body.TryGetProperty("type", out var typeProp)) // Tenta extrair o campo "type" (em algumas configurações de webhook).
                {
                    // Algumas versões usam "type" e "data.id"
                    topic = typeProp.GetString() ?? "";
                }
                if (body.TryGetProperty("id", out var idProp)) // Tenta extrair o ID do pagamento/recurso do corpo.
                {
                    resourceId = idProp.GetString() ?? "";
                }

                // Outras notificações podem ter data.id
                if (string.IsNullOrEmpty(resourceId)) // Se o ID não foi encontrado no campo "id"...
                {
                    if (body.TryGetProperty("data", out var dataProp) && dataProp.TryGetProperty("id", out var dId)) // ...tenta buscar em "data.id".
                    {
                        resourceId = dId.GetString() ?? "";
                    }
                }

                // Se não encontramos um id, respondemos 400
                if (string.IsNullOrEmpty(resourceId)) // Se o ID do pagamento/recurso ainda estiver vazio.
                {
                    return BadRequest("Requisição webhook inválida - sem id."); // Retorna 400: Não é possível processar sem o ID do recurso.
                }

                Console.WriteLine($"ID do pagamento recebido: {resourceId}"); // Loga o ID encontrado.

                // Consultar o endpoint de pagamentos do Mercado Pago para obter status
                var accessToken = _config["MercadoPago:AccessToken"]; // Pega o token de acesso novamente.
                var client = _httpClientFactory.CreateClient(); // Cria um novo cliente HTTP.
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken); // Define a autorização.

                // GET /v1/payments/{id}
                var pagoRes = await client.GetAsync($"https://api.mercadopago.com/v1/payments/{resourceId}"); // Faz uma requisição GET para a API do MP para obter detalhes do pagamento.
                var pagoBody = await pagoRes.Content.ReadAsStringAsync(); // Lê a resposta (detalhes do pagamento).

                if (!pagoRes.IsSuccessStatusCode) // Se a consulta ao MP falhar.
                {
                    Console.WriteLine($"Erro ao consultar pagamento: {pagoRes.StatusCode}"); // Loga o erro.
                    Console.WriteLine(pagoBody);
                    // possível que o recurso seja outro tipo (e.g. merchant_order) — trate conforme necessidade
                    return StatusCode((int)pagoRes.StatusCode, pagoBody); // Retorna o erro.
                }

                using var pagoDoc = JsonDocument.Parse(pagoBody); // Analisa a resposta JSON dos detalhes do pagamento.
                var pagoRoot = pagoDoc.RootElement;

                // Extrair status e external_reference
                var status = pagoRoot.GetProperty("status").GetString() ?? ""; // Extrai o status final do pagamento (ex: approved, rejected).
                string externalReference = "";
                if (pagoRoot.TryGetProperty("external_reference", out var extRef)) // Tenta extrair a referência externa (onde guardamos o ID da nossa doação).
                {
                    externalReference = extRef.GetString() ?? "";
                }

                // external_reference contém nosso doacao.Id porque definimos assim ao criar
                if (int.TryParse(externalReference, out int doacaoId)) // Tenta converter a referência externa (que é nosso ID local) para inteiro.
                {
                    var doacao = await _repo.BuscarPorId(doacaoId); // Busca o registro da doação no seu banco de dados usando o ID local.
                    if (doacao != null) // Se a doação foi encontrada...
                    {
                        // Atualizar campos locais conforme status
                        doacao.MercadoPagoPaymentId = resourceId; // Salva o ID do pagamento do Mercado Pago na sua doação.
                        doacao.Status = status; // Atualiza o status local com o status final do MP.
                        if (status.Equals("approved", StringComparison.OrdinalIgnoreCase)) // Se o pagamento foi APROVADO.
                        {
                            doacao.DataPagamento = DateTime.UtcNow; // Registra a data de pagamento.
                        }

                        await _repo.Atualizar(doacao); // Salva todas as alterações no banco de dados.
                        Console.WriteLine($" Doação {doacaoId} atualizada com sucesso."); // Loga o sucesso.
                    }
                    else
                    {
                        Console.WriteLine($" Nenhuma doação encontrada com ID {doacaoId}"); // Loga se a doação não for encontrada (erro de referência).
                    }
                }

                // responder 200 OK para Mercado Pago (ele entende que recebemos)
                return Ok(); // Retorna 200 OK para o Mercado Pago, indicando que a notificação foi processada com sucesso.
            }
            catch (Exception ex) // Bloco de tratamento de erros.
            {
                // log do erro...
                Console.WriteLine($" Erro no webhook: {ex.Message}"); // Loga o erro.
                return StatusCode(500, ex.Message); // Retorna 500 para o MP em caso de falha interna.
            }
        }

   


        // Listar doações do usuário autenticado==
        [Authorize] // **IMPORTANTE:** Define que esta rota SÓ pode ser acessada por um usuário AUTENTICADO.
        [HttpGet("me")] // Define a rota GET específica para listar as doações do usuário logado (ex: /api/doacao/me).
        public async Task<IActionResult> MyDonations() // Ação que retorna uma lista de doações.
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id"); // Pega o ID do usuário logado através do token.
            if (userIdClaim == null) return Unauthorized(); // Se não tiver ID no token, retorna 401 (já protegido pelo [Authorize], mas é uma checagem de segurança extra).
            int usuarioId = int.Parse(userIdClaim.Value); // Converte o ID.

            var list = await _repo.BuscarPorUsuario(usuarioId); // Usa o Repositório para buscar todas as doações associadas a este ID de usuário.
            var dtoList = list.Select(d => new DoacaoViewDTO // Projeta a lista de modelos de Doação para uma lista de DTOs de visualização (DTOList).
            {
                Id = d.Id,
                Valor = d.Valor,
                Status = d.Status,
                CheckoutLink = d.CheckoutLink,
                DataCriacao = d.DataCriacao,
                DataPagamento = d.DataPagamento
            }).ToList(); // Converte a lista para um tipo List<DoacaoViewDTO>.

            return Ok(dtoList); // Retorna 200 OK com a lista de doações do usuário.
        }

        // GET: api/Doacao/admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> ListAll()
        {
            var lista = await _repo.BuscarTodas();
            return Ok(lista);
        }
    }
}