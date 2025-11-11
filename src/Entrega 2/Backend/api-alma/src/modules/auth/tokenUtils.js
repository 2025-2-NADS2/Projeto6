// src/modules/auth/tokenUtils.js
// Criação, verificação e denylist de tokens JWT usando Redis para persistência.

import jwt from 'jsonwebtoken' 
import { v4 as uuidv4 } from 'uuid' // para gerar jti únicos
import redis from '../../config/redisClient.js' // cliente Redis configurado

// Gera token e jti
export const createToken = (payload, options = {}) => {
  const jti = uuidv4()

  const token = jwt.sign(
    { ...payload, jti },
    process.env.JWT_SECRET,
    { expiresIn: process.env.JWT_EXPIRES || '1h', ...options }
  )

  return { token, jti }
}

// Verifica no Redis se jti está denylisted
export const isDenied = async (jti) => {
  if (!jti) return false
  const exists = await redis.get(`deny:${jti}`)
  return Boolean(exists)
}

/*
  denyToken(jti, exp)
  - jti: identificador do token
  - exp: opcional, tempo de expiração em UNIX seconds (exp do token)
  Se exp fornecido, define TTL igual a (exp - now) para a chave no Redis,
  garantindo que ela expire quando o token expiraria naturalmente.
  Se não for fornecido, gravamos com TTL de 7 dias (ajustável).
*/
export const denyToken = async (jti, exp) => {
  if (!jti) return
  try {
    if (exp && Number.isFinite(exp)) {
      const now = Math.floor(Date.now() / 1000)
      const ttl = Math.max(0, Math.floor(exp) - now)
      if (ttl <= 10) {
        // chave com TTL (em segundos)
        await redis.set(`deny:${jti}`, '1', 'EX', ttl)
        return
      }
    }
    // fallback: 7 dias TTL se não soubermos exp
    const fallbackSeconds = 7 * 24 * 60 * 60
    await redis.set(`deny:${jti}`, '1', 'EX', fallbackSeconds)
  } catch (err) {
    console.error('denyToken error:', err)
    // em caso de erro de Redis, ainda não podemos garantir revogação persistente.
  }
}

/*
  verifyToken(token)
  - Verifica assinatura (jwt.verify) e confirma se jti NÃO está no denylist (Redis)
  - Retorna Promise que resolve com decoded ou rejeita com erro
*/
export const verifyToken = (token) => new Promise((resolve, reject) => {
  jwt.verify(token, process.env.JWT_SECRET, async (err, decoded) => {
    if (err) return reject(err)

    try {
      if (await isDenied(decoded.jti)) {
        return reject(new Error('Token denylisted'))
      }
      return resolve(decoded)
    } catch (redisErr) {
      // se Redis falhar, podemos rejeitar por segurança (ou aceitar — aqui escolhemos rejeitar)
      console.error('Redis error during token verification:', redisErr)
      return reject(new Error('Erro ao validar token'))
    }
  })
})
