// src/modules/admin/adminController.js

import db from "../../config/db.js"

// ==========================================================
// ✅ Visão geral do painel admin
// ==========================================================
export const getAdminDashboard = async (req, res) => {
    try {
        // total de usuários
        const [[users]] = await db.execute("SELECT COUNT(*) AS total FROM users")

        // total de atividades
        const [[activities]] = await db.execute("SELECT COUNT(*) AS total FROM activities")

        // total de eventos
        const [[events]] = await db.execute("SELECT COUNT(*) AS total FROM events")

        // total de arquivos enviados
        const [[uploads]] = await db.execute("SELECT COUNT(*) AS total FROM uploads")

        return res.json({
            message: "Dashboard carregado com sucesso",
            data: {
                users: users.total,
                activities: activities.total,
                events: events.total,
                uploads: uploads.total
            }
        })
    } catch (err) {
        return res.status(500).json({ error: err.message })
    }
}


// ==========================================================
// ✅ Listar logs (visível apenas para ADMIN)
// ==========================================================
export const getAuditLogs = async (req, res) => {
    try {
        const [rows] = await db.execute(
            "SELECT * FROM audit_logs ORDER BY created_at DESC LIMIT 200"
        )

        return res.json(rows)
    } catch (err) {
        return res.status(500).json({ error: err.message })
    }
}


// ==========================================================
// ✅ Buscar logs por usuário
// ==========================================================
export const getLogsByUser = async (req, res) => {
    try {
        const { userId } = req.params

        const [rows] = await db.execute(
            "SELECT * FROM audit_logs WHERE user_id = ? ORDER BY created_at DESC",
            [userId]
        )

        return res.json(rows)
    } catch (err) {
        return res.status(500).json({ error: err.message })
    }
}
