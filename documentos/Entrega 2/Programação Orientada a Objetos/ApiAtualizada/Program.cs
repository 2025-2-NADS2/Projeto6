// --- USINGS (Importações de Bibliotecas) ---

using Microsoft.EntityFrameworkCore; // Necessário para configurar o Entity Framework Core e o DbContext.
using Servidor_PI.Data; // Importa o AppDbContext, sua classe de contexto de banco de dados.
using Servidor_PI.Repositories; // Importa as implementações dos seus Repositórios.
using Servidor_PI.Repositories.Interfaces; // Importa as interfaces dos seus Repositórios.
using Microsoft.AspNetCore.Authentication.JwtBearer; // Biblioteca essencial para autenticação via JWT.
using Microsoft.IdentityModel.Tokens; // Classes para manipulação de tokens e chaves de segurança.
using System.Text; // Usado para codificar a chave secreta (Encoding.UTF8).
using Servidor_PI.Services; // Importa a classe TokenService e o filtro customizado do Swagger.
using Microsoft.OpenApi.Models; // Classes do Swagger/OpenAPI para configuração de esquema de segurança.


// Cria o construtor (builder) da aplicação, que configura serviços e configurações.
var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// Configurações principais da aplicação
// ------------------------------------------------------
builder.Services.AddControllers(); // Adiciona suporte a Controllers com Views (MVC) e Controllers de API.
builder.Services.AddEndpointsApiExplorer(); // Necessário para o Swagger descobrir os endpoints da API.

// ------------------------------------------------------
// Configuração do Swagger 
// ------------------------------------------------------
builder.Services.AddSwaggerGen(c => // Configura o gerador de documentação Swagger.
{
    // Documento principal
    c.SwaggerDoc("v1", new OpenApiInfo // Define o nome, versão e descrição da sua documentação.
    {
        Title = "Instituto Alma - API",
        Version = "v1",
        Description = "API do Instituto Alma com autenticação JWT (visual limpo e organizado)"
    });

    // Evita modelos duplicados e nomes confusos
    c.CustomSchemaIds(type => type.FullName); // Garante que os nomes dos DTOs sejam únicos na documentação.

    // Define o esquema JWT (sem emoji, visual limpo)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme // Configura o botão de "Authorize" para receber o JWT.
    {
        Description = "Autenticação via JWT. Use o formato: **Bearer {seu_token_aqui}**", // Instrução de como inserir o token.
        Name = "Authorization",
        In = ParameterLocation.Header, // O token será enviado no cabeçalho HTTP.
        Type = SecuritySchemeType.Http, // Tipo de esquema HTTP.
        Scheme = "bearer", // Palavra-chave que precede o token.
        BearerFormat = "JWT" // Formato do token.
    });

    // Adiciona o filtro que mostra o cadeado apenas onde há [Authorize]
    c.OperationFilter<Servidor_PI.Services.JwtBearerOperationFilter>(); // Aplica um filtro customizado para organizar a segurança visualmente.
});

// ------------------------------------------------------
// Configuração do Banco de Dados
// ------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options => // Registra o AppDbContext no contêiner de Injeção de Dependência (DI).
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))); // Configura o DBContext para usar SQLite, lendo a string de conexão de 'appsettings.json'.

// ------------------------------------------------------
// Injeção de Dependência (repos e serviços)
// ------------------------------------------------------
// Registra as interfaces e suas implementações concretas no escopo da requisição (Scoped).
builder.Services.AddScoped<IAtividadeRepo, AtividadesRepo>(); // Injeta IAtividadeRepo quando solicitado, usando AtividadesRepo.
builder.Services.AddScoped<ITransparenciaRepo, TransparenciaRepo>(); // Repositório de Transparência.
builder.Services.AddScoped<IOuvidoriaRepo, OuvidoriaRepo>(); // Repositório de Ouvidoria.
builder.Services.AddScoped<IEventosRepo, EventosRepo>(); // Repositório de Eventos.
builder.Services.AddScoped<IUsuarioRepo, UsuarioRepo>(); // Repositório de Usuário.
builder.Services.AddScoped<IDoacaoRepo, DoacaoRepos>(); // Repositório de Doação.
builder.Services.AddScoped<TokenService>(); // Serviço para geração de tokens JWT.
builder.Services.AddHttpClient(); // Adiciona o serviço IHttpClientFactory (usado para chamadas a APIs externas, como o Mercado Pago).

// ------------------------------------------------------
// Configuração do JWT (Autenticação)
// ------------------------------------------------------
var key = builder.Configuration["Jwt:Key"]; // Lê a chave secreta do JWT do arquivo de configuração (appsettings.json).
var issuer = builder.Configuration["Jwt:Issuer"]; // Lê o emissor (Issuer) do JWT.
var audience = builder.Configuration["Jwt:Audience"]; // Lê o público-alvo (Audience) do JWT.

builder.Services.AddAuthentication(options => // Configura o esquema padrão de autenticação.
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Define o esquema padrão para autenticar (Bearer).
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Define o esquema padrão para "desafiar" (retornar 401).
})
.AddJwtBearer(options => // Adiciona o esquema de autenticação JWT Bearer.
{
    options.TokenValidationParameters = new TokenValidationParameters // Define os parâmetros que o token deve satisfazer para ser válido.
    {
        ValidateIssuer = true, // Deve validar se o emissor do token é o correto (lido do 'appsettings.json').
        ValidateAudience = true, // Deve validar se o público-alvo do token é o correto.
        ValidateLifetime = true, // Deve validar o tempo de vida do token (expiração).
        ValidateIssuerSigningKey = true, // Deve validar se a chave secreta usada para assinar o token é a correta.
        ValidIssuer = issuer, // O valor esperado para o Emissor.
        ValidAudience = audience, // O valor esperado para o Público-Alvo.
        // A chave secreta é convertida para bytes UTF8 para ser usada na validação da assinatura do token.
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// ------------------------------------------------------
// Middlewares da aplicação (Pipeline de Requisições)
// ------------------------------------------------------
var app = builder.Build(); // Constrói a aplicação com todas as configurações de serviços.

// Executado apenas em ambiente de desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Adiciona o middleware para gerar a documentação Swagger em JSON.
    app.UseSwaggerUI(c => // Adiciona o middleware para a interface gráfica do Swagger.
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Instituto Alma API v1"); // Define o endpoint do arquivo de documentação.
        c.DocumentTitle = "Instituto Alma - Documentação da API"; // Título da aba do navegador.
        c.DisplayRequestDuration(); // Mostra o tempo de execução da requisição no Swagger UI.
        c.DefaultModelExpandDepth(1); // Define quantos níveis o modelo de dados deve expandir por padrão.
        c.DefaultModelsExpandDepth(-1); // Oculta o painel de "Schemas" na parte inferior para limpar o visual.
        c.EnableFilter(); // Habilita a caixa de filtro para buscar endpoints.
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model); // Define como os modelos são renderizados.
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}




app.UseHttpsRedirection(); // Redireciona requisições HTTP para HTTPS (boa prática de segurança).
app.UseAuthentication(); // Adiciona o middleware de Autenticação (deve sempre vir antes da Autorização).
app.UseAuthorization(); // Adiciona o middleware de Autorização (verifica se o usuário autenticado tem permissão para acessar o recurso).

app.MapControllers(); // Mapeia os endpoints definidos nos seus Controllers para a pipeline da aplicação.

app.Run(); // Inicia o servidor e bloqueia o processo, aguardando requisições.