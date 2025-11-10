// --- USINGS (Importações de Bibliotecas) ---

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
        // --- DEPENDÊNCIAS (Campos Privados) ---

        private readonly IDoacaoRepo _repo; // Campo privado para armazenar o Repositório de Doação (acesso ao DB).
        private readonly IConfiguration _config; // Campo privado para armazenar as Configurações (acesso a chaves e URLs).
        private readonly IHttpClientFactory _httpClientFactory; // Campo privado para criar instâncias seguras de HttpClient (para chamadas externas).

        // --- CONSTRUTOR (Injeção de Dependência) ---

        /// <summary>
        /// Construtor da classe DoacaoController.
        /// Recebe e armazena as dependências necessárias para operação.
        /// </summary>
        public DoacaoController(IDoacaoRepo repo, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _repo = repo; // Repositório injetado para manipulação do banco de dados (buscar, adicionar, atualizar).
            _config = config; // Configurações da aplicação injetadas (ex: chaves de APIs, URLs de retorno).
            _httpClientFactory = httpClientFactory; // Factory injetada para realizar requisições HTTP externas (chamar a API do Mercado Pago).
        }

        // --- ENDPOINT: Criar Preferência de Pagamento ---

        [AllowAnonymous] // Permite que esta rota seja acessada mesmo por usuários não autenticados (se o seu fluxo permitir).
        [HttpPost] // Define que esta ação responde a requisições HTTP POST (geralmente para criar um novo recurso).
        // A função é assíncrona (Task<...>) porque faz chamadas de I/O (DB e API externa) e retorna um resultado (IActionResult).
        public async Task<IActionResult> CreatePreference([FromBody] DoacaoCreateDTO dto) // Recebe os dados da doação (valor) no corpo da requisição.
        {
            // 1. valida dto
            if (!ModelState.IsValid) return BadRequest(ModelState); // Verifica se o DTO recebido é válido (conforme Data Annotations no DTO). Retorna 400 se inválido.

            // 2. pegar id do usuário do token (claims)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id"); // Tenta encontrar a informação do ID do usuário no token de autenticação (Claims).
            if (userIdClaim == null) return Unauthorized(); // Se não encontrar o ID (usuário não autenticado), retorna 401.
            int usuarioId = int.Parse(userIdClaim.Value); // Converte o ID encontrado (que é uma string) para um inteiro.

            // 3. criar objeto Doacao local com status pending (ainda sem pagamento)
            var doacao = new Doacao // Cria um novo objeto Doacao, que será salvo no seu banco de dados.
            {
                Valor = dto.Valor, // Pega o valor da doação do DTO recebido.
                UsuarioId = usuarioId, // Associa a doação ao ID do usuário autenticado.
                Status = "pending", // Define o status inicial como pendente (ainda não foi pago).
                DataCriacao = DateTime.UtcNow // Registra a data de criação da doação.
            };

            // adiciona ao contexto (não salva ainda)
            await _repo.Adicionar(doacao); // Prepara a doação para ser salva no banco de dados.
            await _repo.Salvar(); // Salva as alterações no banco, garantindo que a variável 'doacao' tenha agora um ID único (`doacao.Id`).

            // 4. Criar preferência no Mercado Pago
            var accessToken = _config["MercadoPago:AccessToken"]; // Lê o token de acesso (chave secreta) do Mercado Pago das configurações.
            if (string.IsNullOrEmpty(accessToken)) return StatusCode(500, "Access token do Mercado Pago não configurado."); // Erro interno se a chave não estiver configurada.

            // montar o body conforme API do Mercado Pago
            var preference = new // Cria um objeto anônimo (JSON) com as informações que o Mercado Pago exige para criar um checkout.
            {
                items = new[] // Seção para listar os itens a serem cobrados (neste caso, apenas um item "Doação").
                {
                    new {
                        title = "Doação", // Nome do item no checkout.
                        quantity = 1, // Apenas uma unidade.
                        currency_id = "BRL", // Moeda (Real Brasileiro).
                        unit_price = (decimal) dto.Valor // O valor da doação.
                    }
                },
                external_reference = doacao.Id.ToString(), // **MUINTO IMPORTANTE:** Este campo é o ID da sua doação no seu banco, usado para vincular o pagamento do MP com sua base de dados.
                back_urls = new // URLs para onde o usuário será redirecionado após o pagamento.
                {
                    success = _config["Frontend:UrlSuccess"] ?? "https://seufrontend/sucesso", // URL de sucesso (lida das configurações, com fallback).
                    failure = _config["Frontend:UrlFailure"] ?? "https://seufrontend/falha", // URL de falha.
                    pending = _config["Frontend:UrlPending"] ?? "https://seufrontend/pendente" // URL de pendente.
                },
                auto_return = "approved" // Configura para retornar automaticamente à URL de sucesso após a aprovação.
            };

            var client = _httpClientFactory.CreateClient(); // Cria um novo HttpClient (instância para fazer requisição externa).
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken); // Define o cabeçalho de autorização (o token secreto do MP).
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // Indica que a requisição espera uma resposta em formato JSON.

            var json = JsonSerializer.Serialize(preference); // Converte o objeto 'preference' criado acima em uma string JSON.
            var content = new StringContent(json, Encoding.UTF8, "application/json"); // Cria o conteúdo da requisição HTTP (o corpo JSON codificado).

            // endpoint de criação de preference
            var mpResponse = await client.PostAsync("https://api.mercadopago.com/checkout/preferences", content); // Faz a chamada POST assíncrona para a API do Mercado Pago.
            var responseBody = await mpResponse.Content.ReadAsStringAsync(); // Lê a resposta completa (o JSON retornado) como uma string.

            if (!mpResponse.IsSuccessStatusCode) // Verifica se a requisição ao Mercado Pago não foi bem-sucedida (status code diferente de 2xx).
            {
                // registrar log aqui se quiser
                return StatusCode((int)mpResponse.StatusCode, responseBody); // Retorna o código de erro do MP para o seu cliente.
            }

            // parse do retorno (acessa init_point e sandbox_init_point)
            using var doc = JsonDocument.Parse(responseBody); // Analisa a string JSON de resposta do MP em uma estrutura de documento.
            var root = doc.RootElement; // Pega o elemento raiz do JSON.

            string checkoutLink = ""; // Variável para armazenar o link de checkout (onde o usuário pagará).
            if (root.TryGetProperty("init_point", out var initPoint)) // Tenta buscar a propriedade 'init_point' (link de produção).
            {
                checkoutLink = initPoint.GetString() ?? ""; // Armazena o link.
            }
            else if (root.TryGetProperty("sandbox_init_point", out var sandboxInit)) // Tenta buscar 'sandbox_init_point' (link de teste, se o token for de teste).
            {
                checkoutLink = sandboxInit.GetString() ?? ""; // Armazena o link.
            }

            // 5. atualizar a doacao com o link de checkout e salvar
            doacao.CheckoutLink = checkoutLink; // Armazena o link de checkout na sua doação no banco de dados.
            await _repo.Atualizar(doacao); // Atualiza o registro no banco com o link do MP.
            // (UpdateAsync no repo faz SaveChanges dentro no exemplo acima)

            // 6. retornar o link para o cliente
            var result = new DoacaoViewDTO // Cria o DTO de retorno para o cliente.
            {
                Id = doacao.Id, // ID da sua doação.
                Valor = doacao.Valor,
                Status = doacao.Status,
                CheckoutLink = doacao.CheckoutLink, // O link que o cliente deve usar para redirecionar para o pagamento.
                DataCriacao = doacao.DataCriacao
            };

            return Ok(result); // Retorna 200 OK com o objeto contendo o link de pagamento.
        }

        // --- ENDPOINT: Webhook (Receber Notificações de Pagamento) ---

        // =========================
        // Webhook: Mercado Pago envia notificações aqui
        // =========================
        [AllowAnonymous] // Deve ser anônimo, pois o Mercado Pago é quem faz a chamada, não um usuário autenticado.
        [HttpPost("webhook")] // Define a rota específica para o webhook (ex: /api/doacao/webhook).
        public async Task<IActionResult> Webhook([FromBody] JsonElement body) // Recebe o corpo da notificação (webhook) como um elemento JSON genérico.
        {
            // 1. --- VALIDAÇÃO DE ASSINATURA SECRETA ---
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

            // 3. COMPARA AS CHAVES
            // Use String.Equals para comparação segura entre strings
            if (secretValue == null || !secretValue.Equals(secretConfig, StringComparison.Ordinal)) // Compara o valor recebido com o valor configurado (validação de segurança).
            {
                // Resposta 401: Rejeitar a requisição com segredo incorreto
                return Unauthorized("Assinatura secreta inválida."); // Retorna 401: a assinatura de segurança não confere.
            }
            // --- FIM DA VALIDAÇÃO ---


            // O Mercado Pago envia backgrounds notifications que podem conter 'id' e 'topic' (ou resource)
            // Vamos pegar as informações e consultar a API do Mercado Pago para detalhes do pagamento.
            try
            {
                Console.WriteLine("Webhook recebido do Mercado Pago:"); // Loga o recebimento.
                Console.WriteLine(body.ToString()); // Loga o corpo da requisição para inspeção.
                // extrair id do recurso enviado (varia conforme configuração do MP)
                // Exemplo de corpo: { "id": "123456", "topic": "payment" }
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
                return Ok(); // **MUITO IMPORTANTE:** Retorna 200 OK para o Mercado Pago, indicando que a notificação foi processada com sucesso.
            }
            catch (Exception ex) // Bloco de tratamento de erros.
            {
                // log do erro...
                Console.WriteLine($" Erro no webhook: {ex.Message}"); // Loga o erro.
                return StatusCode(500, ex.Message); // Retorna 500 para o MP em caso de falha interna.
            }
        }

        // --- ENDPOINT: Listar Doações do Usuário ---

        // =========================
        // Listar doações do usuário autenticado
        // =========================
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
    }
}