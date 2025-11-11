import express from 'express' // Importa o express
import db from '../../config/db.js' // Conexão com o banco
import bcrypt from 'bcrypt' // Para hashing de senha
import { authMiddleware } from '../../middleware/authMiddleware.js' // Middleware JWT
import { authorizeRoles } from '../../middleware/roleMiddleware.js' // Middleware de permissões

const router = express.Router()

// =======================================================
// ✅ USUÁRIO: VER A PRÓPRIA CONTA (precisa vir antes de /:id)
// =======================================================
//Estrutura
//GET - http://localhost:3000/api/users/me
router.get('/me', authMiddleware, async (req, res) => {
    try {
        const userId = req.user.id // Pega ID do token JWT

        const [rows] = await db.execute(
            'SELECT id, name, email, role, created_at FROM users WHERE id = ?',
            [userId]
        )

        res.json(rows[0])
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
})

// =======================================================
// ✅ USUÁRIO: ATUALIZAR SUA PRÓPRIA CONTA
// =======================================================
router.put('/me', authMiddleware, async (req, res) => {
    try {
        const userId = req.user.id
        const { name, email, password } = req.body

        if (!name && !email && !password)
            return res.status(400).json({ message: 'Nada a atualizar!' })

        // Verificar se o novo email já existe em outro usuário
        if (email) {
            const [exists] = await db.execute(
                'SELECT id FROM users WHERE email = ? AND id != ?',
                [email, userId]
            )
            if (exists.length)
                return res.status(400).json({ message: 'E-mail já está sendo utilizado!' })
        }

        const updates = []
        const params = []

        if (name) { updates.push('name = ?'); params.push(name) }
        if (email) { updates.push('email = ?'); params.push(email) }
        if (password) {
            const hash = await bcrypt.hash(password, 10)
            updates.push('password = ?')
            params.push(hash)
        }

        params.push(userId)

        const sql = `UPDATE users SET ${updates.join(', ')} WHERE id = ?`
        await db.execute(sql, params)

        res.json({ message: 'Perfil atualizado com sucesso' })

    } catch (err) {
        res.status(500).json({ error: err.message })
    }
})

// =======================================================
// ✅ USUÁRIO: DELETAR SUA PRÓPRIA CONTA
// =======================================================
router.delete('/me', authMiddleware, async (req, res) => {
    try {
        const userId = req.user.id

        // Deleta o usuário
        await db.execute('DELETE FROM users WHERE id = ?', [userId])

        // Revoga todas as sessões do usuário deletado
        await db.execute(
            'UPDATE sessions SET revoked_at = NOW() WHERE user_id = ?',
            [userId]
        )

        res.json({ message: 'Conta deletada com sucesso' })
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
})

// =======================================================
// ✅ ADMIN: LISTAR TODOS OS USUÁRIOS
// =======================================================
router.get('/', authMiddleware, authorizeRoles('admin'), async (req, res) => {
    try {
        const [rows] = await db.execute(
            'SELECT id, name, email, role, created_at FROM users ORDER BY created_at DESC'
        )
        res.json(rows)
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
})

// =======================================================
// ✅ ADMIN: BUSCAR USUÁRIO POR ID
// =======================================================
router.get('/:id', authMiddleware, authorizeRoles('admin'), async (req, res) => {
    try {
        const { id } = req.params

        const [rows] = await db.execute(
            'SELECT id, name, email, role, created_at FROM users WHERE id = ?',
            [id]
        )

        if (!rows.length)
            return res.status(404).json({ message: 'Usuário não encontrado' })

        res.json(rows[0])
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
})

// =======================================================
// ✅ ADMIN: ATUALIZAR QUALQUER USUÁRIO
// =======================================================
router.put('/:id', authMiddleware, authorizeRoles('admin'), async (req, res) => {
    try {
        const { id } = req.params
        const { name, email, role, password } = req.body

        const updates = []
        const params = []

        if (name) { updates.push('name = ?'); params.push(name) }
        if (email) { updates.push('email = ?'); params.push(email) }
        if (role) { updates.push('role = ?'); params.push(role) }
        if (password) {
            const hash = await bcrypt.hash(password, 10)
            updates.push('password = ?')
            params.push(hash)
        }

        if (!updates.length)
            return res.status(400).json({ message: 'Nada a atualizar!' })

        params.push(id)

        const sql = `UPDATE users SET ${updates.join(', ')} WHERE id = ?`
        await db.execute(sql, params)

        res.json({ message: 'Usuário atualizado com sucesso' })

    } catch (err) {
        res.status(500).json({ error: err.message })
    }
})

// =======================================================
// ✅ ADMIN: DELETAR USUÁRIO POR ID
// =======================================================
router.delete('/:id', authMiddleware, authorizeRoles('admin'), async (req, res) => {
    try {
        const { id } = req.params

        const [exists] = await db.execute(
            'SELECT id FROM users WHERE id = ?',
            [id]
        )

        if (!exists.length)
            return res.status(404).json({ message: 'Usuário não encontrado' })

        await db.execute('DELETE FROM users WHERE id = ?', [id])

        await db.execute(
            'UPDATE sessions SET revoked_at = NOW() WHERE user_id = ?',
            [id]
        )

        res.json({ message: 'Usuário deletado com sucesso' })

    } catch (err) {
        res.status(500).json({ error: err.message })
    }
})

export default router
