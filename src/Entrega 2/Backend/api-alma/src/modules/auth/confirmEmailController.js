import jwt from 'jsonwebtoken'
import db from '../../config/db.js'
import { logAction } from '../../utils/logUtils.js'

export const confirmEmailController = async (req, res) => {
    const token = req.params.token

    if (!token) {
        return res.status(400).json({ message: "Token não fornecido." })
    }

    try {
        const decoded = jwt.verify(token, process.env.JWT_EMAIL_SECRET)

        const [rows] = await db.execute(
            'SELECT id, is_verified FROM users WHERE id = ?',
            [decoded.id]
        )

        if (rows.length === 0) {
            return res.status(400).json({ message: "Usuário não encontrado." })
        }

        const user = rows[0]

        if (user.is_verified === 1) {
            return res.status(200).json({ message: "E-mail já confirmado anteriormente." })
        }

        await db.execute(
            'UPDATE users SET is_verified = 1 WHERE id = ?',
            [user.id]
        )

        await logAction(user.id, "email_confirmed", "auth")

        return res.status(200).json({ message: "E-mail confirmado com sucesso!" })

    } catch (err) {
        return res.status(400).json({ message: "Token inválido ou expirado." })
    }
}
