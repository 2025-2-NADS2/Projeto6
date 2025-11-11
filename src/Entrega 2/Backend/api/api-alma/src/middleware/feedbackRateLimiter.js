import rateLimit from 'express-rate-limit'

// Limita envios de formulários de contato para evitar spam (ex: 5 por hora por IP)
export const feedbackLimiter = rateLimit({
    windowMs: 60 * 60 * 1000, // 1 hora
    max: 5,                   // máximo 5 requisições por IP por janela
    message: { message: 'Muitas mensagens enviadas desse IP. Tente novamente mais tarde.' },
    standardHeaders: true,
    legacyHeaders: false
})
