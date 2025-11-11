import db from '../config/db.js'
import { v4 as uuidv4 } from 'uuid'

export const logAction = async (userId, action, resource, details = {}, req = null) => {
    try {
        // captura IP e user-agent quando disponível pela requisição
        const ip = req?.ip || req?.headers?.['x-forwarded-for'] || null
        const userAgent = req?.get?.('User-Agent') || null

        const payload = {
        ...details,
        ip,
        user_agent: userAgent
        }

        // insere no audit_logs (presume que tabela audit_logs existe)
        await db.execute(
        'INSERT INTO audit_logs (id, user_id, action, resource, details) VALUES (?, ?, ?, ?, ?)',
        [uuidv4(), userId, action, resource, JSON.stringify(payload)]
        )
    } catch (err) {
        // não lança erro (evita bloquear fluxo principal), apenas loga no console
        console.error('Erro ao registrar log:', err)
    }
}
