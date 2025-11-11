import express from 'express'
import db from '../../config/db.js'
import { authMiddleware } from '../../middleware/authMiddleware.js'
import { authorizeRoles } from '../../middleware/roleMiddleware.js'
import { denyToken } from './tokenUtils.js'

const router = express.Router()

// ✅ Usuário visualiza suas próprias sessões
router.get('/me/sessions', authMiddleware, async (req, res) => {
    const userId = req.user.id
    const [rows] = await db.execute(
        'SELECT id, jti, ip, user_agent, created_at, revoked_at FROM sessions WHERE user_id = ? ORDER BY created_at DESC',
        [userId]
    )
    res.json(rows)
})

// ✅ ADM: revogar sessão específica
router.post('/sessions/:jti/revoke', authMiddleware, authorizeRoles('admin'), async (req, res) => {
    const { jti } = req.params

    denyToken(jti)
    await db.execute(
        'UPDATE sessions SET revoked_at = NOW() WHERE jti = ?',
        [jti]
    )

    res.json({ message: 'Sessão revogada' })
})

// ✅ ADM: listar todas as sessões
router.get('/sessions', authMiddleware, authorizeRoles('admin'), async (req, res) => {
    const [rows] = await db.execute(
        'SELECT s.id, s.jti, s.ip, s.user_agent, s.created_at, s.revoked_at, u.email, u.role FROM sessions s JOIN users u ON u.id = s.user_id ORDER BY s.created_at DESC'
    )
    res.json(rows)
})

export default router
