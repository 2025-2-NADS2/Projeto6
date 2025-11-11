

# ✅ **README.md — Instituto Alma API**

````md
# 🌱 Instituto Alma – API Oficial  
API RESTful construída em **Node.js + Express**, com suporte completo para **MySQL/PostgreSQL**, autenticação JWT, upload de arquivos, sistema de logs, auditoria e módulos de gestão para:  
✅ Usuários  
✅ Autenticação  
✅ Sessões  
✅ Uploads (Imagens, Vídeos, Documentos, Outros)  
✅ Doações  
✅ Eventos  
✅ Atividades  
✅ Transparência  
✅ Administração  
✅ Feedback  
✅ Setup do sistema

---

## 🚀 Tecnologias Utilizadas
- **Node.js**
- **Express.js**
- **MySQL ou PostgreSQL (Render-ready)**
- **Multer (upload de arquivos)**
- **Nodemailer (e-mail)**
- **JWT (autenticação)**
- **BCrypt (hash de senha)**
- **Helmet / Rate-limit / CORS (segurança)**
- **Logger + Auditoria (“audit_logs”)**
- **UUID para IDs únicos**

---

# 📁 Estrutura de Pastas

```bash
src/
├── app.js
├── server.js
├── healthCheck.js
├── config/
│   ├── db.js
│   ├── uploadConfig.js
│   └── redisConfig.js
├── middleware/
│   ├── authMiddleware.js
│   ├── roleMiddleware.js
│   ├── rateLimiter.js
│   ├── securityMiddlewares.js
│   ├── requestLogger.js
│   └── errorLogger.js
├── modules/
│   ├── auth/
│   │   ├── authController.js
│   │   ├── authService.js
│   │   ├── authRoutes.js
│   │   └── sessionRoutes.js
│   ├── users/
│   │   └── userRoutes.js
│   ├── uploads/
│   │   └── uploadRoutes.js
│   ├── events/
│   │   ├── eventsController.js
│   │   └── eventsRoutes.js
│   ├── donations/
│   ├── activities/
│   ├── transparency/
│   ├── feedback/
│   ├── setup/
│   └── admin/
├── public/
│   └── uploads/ (imagens, videos, documentos...)
└── utils/
    ├── logger.js
    ├── logUtils.js
    └── dbCompat.js
````

---

# ⚙️ Configuração do Ambiente (.env)

Crie um arquivo `.env`:

```env
# SERVER
PORT=3000
NODE_ENV=development

# DATABASE
DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASS=
DB_NAME=alma

# POSTGRES (Render)
DATABASE_URL=

# JWT
JWT_SECRET=minha_chave_super_secreta
JWT_EXPIRES_IN=1d

# EMAIL (Nodemailer)
MAIL_HOST=smtp.gmail.com
MAIL_PORT=587
MAIL_USER=seuemail@gmail.com
MAIL_PASS=suasenha
```

---

# 🛠️ Instalação

```bash
git clone https://github.com/2025-2-NADS2/Projeto6.git
cd instituto-alma-api
npm install
```

---

# ▶️ Rodando Localmente

```bash
npm run dev
```

A API sobe em:

```
http://localhost:3000
```

---

# 🔥 Rodando em Produção (Render)

Render executa:

```bash
node src/server.js
```

A porta é automaticamente substituída por `process.env.PORT`.

---

# ✅ Testes no Render

Você pode testar qualquer rota usando:

### ✅ Testes com Postman/Insomnia:

Exemplo:

**Login**

```
POST https://instituto-alma-backend.onrender.com/api/auth/login
Body → JSON:
{
  "email": "admin@email.com",
  "password": "Senha123"
}
```

**Feedback**

```
POST https://instituto-alma-backend.onrender.com/api/feedback
```

---

# ✅ Como Testar Cada Rota

---

## 🧪 **Autenticação**

### ✅ Registrar

```
POST /api/auth/register
```

### ✅ Login

```
POST /api/auth/login
```

### ✅ Logout

```
POST /api/auth/logout
Authorization: Bearer TOKEN
```

---

## 🧪 **Usuário – Perfil**

### ✅ Ver perfil

```
GET /api/users/me
Bearer token
```

### ✅ Atualizar perfil

```
PUT /api/users/me
```

### ✅ Excluir conta

```
DELETE /api/users/me
```

---

## 🧪 **Admin – Usuários**

```
GET    /api/users/
GET    /api/users/:id
PUT    /api/users/:id
DELETE /api/users/:id
```

---

## 🧪 **Uploads (Imagens, vídeos, PDFs)**

### ✅ Upload

```
POST /api/files
Body → form-data
key: file (File)
```

### ✅ Atualizar arquivo

```
PUT /api/files/:id
```

### ✅ Deletar arquivo

```
DELETE /api/files/:id
```

---

## 🧪 **Eventos**

### ✅ Criar evento (admin)

```
POST /api/events
form-data:
title: ...
description: ...
event_date: YYYY-MM-DD
image: (file)
```

### ✅ Atualizar evento

```
PUT /api/events/:id
```

### ✅ Deletar evento

```
DELETE /api/events/:id
```

### ✅ Listar todos

```
GET /api/events
```

### ✅ Buscar por ID

```
GET /api/events/:id
```

---

## 🧪 **Feedback público**

```
POST /api/feedback
{
  "name": "teste",
  "message": "API funcionando!"
}
```

---

## 🧪 **Health Check (Render)**

Essencial para monitoramento:

```
GET /
GET /health
GET /api/health
```

Retorna:

```json
{
  "status": "OK",
  "service": "Instituto Alma API",
  "checks": {
    "database": {"status": "OK"},
    "memory": {...},
    "system": {...}
  }
}
```

---

# 🛡️ Segurança

A API aplica:

✅ Helmet
✅ Rate limiting
✅ Sanitização de inputs
✅ CORS configurado
✅ Auditoria completa (IP, action, resource, user)
✅ JWT com expiração

---

# 📜 Logs de Auditoria

Toda ação crítica gera registro em:

```
audit_logs
```

Com:

* user_id
* action
* resource
* ip
* user_agent
* payload




# ✅ Autor

API criada para o **Instituto Alma** – por grupo 6.

```

---

