// src/middleware/sanitizeInput.js

// Middleware leve que limpa campos string em req.body: trim e remove tags simples
export const sanitizeInput = (req, res, next) => {
  // percorre todas as chaves do body
    if (req.body && typeof req.body === 'object') {
        for (const key of Object.keys(req.body)) {
            const val = req.body[key]
        // apenas strings são sanitizadas aqui
            if (typeof val === 'string') {
        // remove espaços nas extremidades
            let s = val.trim()
        // remove tags HTML básicas para proteção (simples)
            s = s.replace(/<[^>]*>/g, '')
            req.body[key] = s
            }
        }
    }
    next()
}
