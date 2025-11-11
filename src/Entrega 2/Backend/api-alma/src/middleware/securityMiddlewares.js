// src/config/securityMiddlewares.js
// Middlewares de segurança para Express — importe e chame applySecurity(app) em app.js

import helmet from 'helmet'                 // Conjunto de headers de segurança
import cors from 'cors'                     // Controla origem das requisições
import rateLimit from 'express-rate-limit'  // Limita taxa de requests (DOS / brute force)
import compression from 'compression'       // Compressão de respostas para performance
import hpp from 'hpp'                       // Protege contra HTTP Parameter Pollution
import cookieParser from 'cookie-parser'    // Necessário caso use cookies (CSRF, refresh tokens)

/*
  applySecurity(app)
  - app: instância do express
  Uso: importe e chame em app.js antes de montar rotas
*/
export const applySecurity = (app) => {
  // 1) Helmet: adiciona diversos headers de segurança automaticamente
  app.use(helmet())

  // src/middleware/securityMiddlewares.js
app.use(cors({
  origin: [
    'http://localhost:5173',
    'https://seu-frontend.onrender.com',
    process.env.FRONTEND_URL
  ],
  credentials: true
}));

  // 2) CORS: só permitir origem(s) do seu frontend em produção
  app.use(cors({
    origin: process.env.FRONTEND_URL || 'http://localhost:5173', // ajuste para seu frontend
    methods: ['GET','POST','PUT','DELETE','OPTIONS'],
    allowedHeaders: ['Content-Type','Authorization'],
    credentials: true // se for usar cookies (refresh token via cookie)
  }))

  // 3) Rate limiter global — proteção básica contra abuso (ajuste valores conforme necessidade)
  const globalLimiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutos
    max: 200,                 // limite global por IP por janela
    standardHeaders: true,
    legacyHeaders: false,
    message: { error: 'Muitas requisições. Tente novamente mais tarde.' }
  })
  app.use(globalLimiter)

  // 4) Compression para respostas (reduz tráfego + melhora perf.)
  app.use(compression())

  // 5) HPP — evita parâmetros repetidos e pollution
  app.use(hpp())


  // 7) Cookie parser — necessário se usar cookies (refresh tokens, CSRF)
  app.use(cookieParser())

  // Observação: CSRF (csurf) NÃO é aplicado aqui por padrão.
  // Se você usar autenticação por cookie/session, ative csurf configurando cookie/session adequadamente.
}
