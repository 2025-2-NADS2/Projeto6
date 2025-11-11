// src/middleware/loginRateLimiter.js
// Rate limiter para rota de login usando store Redis (rate-limit-redis)

import rateLimit from 'express-rate-limit'
import RedisStore from 'rate-limit-redis'
import redisClient from '../config/redisClient.js' // nosso cliente ioredis

export const loginLimiter = rateLimit({
  store: new RedisStore({
    // rate-limit-redis aceita 'client' ou 'sendCommand' (usando ioredis)
    client: redisClient
  }),
  windowMs: 15 * 60 * 1000, // 15 minutos
  max: 5,                   // 5 tentativas por janela por IP
  message: { error: 'Muitas tentativas de login. Aguarde 15 minutos e tente novamente.' },
  standardHeaders: true,
  legacyHeaders: false
})
