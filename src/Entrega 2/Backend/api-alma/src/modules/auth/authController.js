// src/modules/auth/authController.js - AJUSTES POSTGRESQL
import bcrypt from 'bcrypt'
import jwt from 'jsonwebtoken'
import db from '../../config/db.js'
import { createToken, denyToken } from './tokenUtils.js'
import { createUser, findUserByEmail } from './authService.js'
import { v4 as uuidv4 } from 'uuid'
import { logAction } from '../../utils/logUtils.js'
import { isValidEmail, isDisposableEmail, isStrongPassword } from './validators.js'
import { mailer } from '../../services/emailService.js'
import { DBCompat } from '../../utils/dbCompat.js'

// ===========================
// REGISTRO (Ajustado para PostgreSQL)
// ===========================
export const register = async (req, res) => {
    console.log("BODY NO CONTROLLER:", req.body)

    try {
        const { name, email, password, confirmPassword } = req.body

        if (!name || !email || !password || !confirmPassword) {
            return res.status(400).json({ message: 'Nome, e-mail e senhas são obrigatórios.' })
        }

        const normalizedEmail = String(email).trim().toLowerCase()
        if (!isValidEmail(normalizedEmail)) {
            return res.status(400).json({ message: 'Formato de e-mail inválido.' })
        }

        if (isDisposableEmail(normalizedEmail)) {
            return res.status(400).json({ message: 'E-mails temporários não são permitidos.' })
        }

        if (String(name).trim().length < 3) {
            return res.status(400).json({ message: 'Informe seu nome completo.' })
        }

        if (!/^[a-zA-ZÀ-ÿ'’\- ]{3,}$/.test(name.trim())) {
            return res.status(400).json({ message: 'Nome inválido.' })
        }

        if (password !== confirmPassword) {
            return res.status(400).json({ message: 'As senhas não coincidem.' })
        }

        if (!isStrongPassword(password)) {
            return res.status(400).json({
                message: 'A senha deve ter ao menos 8 caracteres, incluindo uma letra maiúscula, um número e um caractere especial.'
            })
        }

        const existing = await findUserByEmail(normalizedEmail)
        if (existing) {
            // Registrar tentativa de registro com email existente
            await db.execute(
                'INSERT INTO auth_attempts (id, email, ip, user_agent, action, success) VALUES ($1, $2, $3, $4, $5, $6)',
                [uuidv4(), normalizedEmail, req.ip || null, req.get('User-Agent') || null, 'register', false]
            )

            await logAction(existing.id, 'register_attempt_existing_email', 'auth', { attemptedEmail: normalizedEmail }, req)

            return res.status(400).json({ message: 'Este e-mail já está cadastrado.' })
        }

        // Cria usuário
        const user = await createUser({
            name: name.trim(),
            email: normalizedEmail,
            password,
            role: 'user',
            is_verified: false
        })

        // Token JWT para confirmação de email
        const emailToken = jwt.sign(
            { id: user.id },
            process.env.JWT_EMAIL_SECRET,
            { expiresIn: '1h' }
        )

        // Registrar tentativa de registro bem-sucedida
        await db.execute(
            'INSERT INTO auth_attempts (id, email, ip, user_agent, action, success) VALUES ($1, $2, $3, $4, $5, $6)',
            [uuidv4(), normalizedEmail, req.ip || null, req.get('User-Agent') || null, 'register', true]
        )
        
        console.log("TOKEN DE VERIFICAÇÃO:", emailToken);

        await logAction(user.id, 'register', 'auth', { email: normalizedEmail }, req)

        // Envia email de confirmação
        const confirmUrl = `${process.env.FRONTEND_URL}/confirm-email/${emailToken}`

        if (mailer && process.env.MAIL_USER) {
            try {
                await mailer.sendMail({
                    to: normalizedEmail,
                    subject: 'Confirme seu e-mail — Instituto Alma',
                    html: `
                        <p>Olá, ${user.name}!</p>
                        <p>Confirme seu cadastro clicando no link abaixo:</p>
                        <a href="${confirmUrl}">${confirmUrl}</a>
                        <p>O link expira em 1 hora.</p>
                    `
                })
            } catch (emailError) {
                console.log('⚠️ Email não enviado (configuração pendente):', emailError.message)
            }
        } else {
            console.log('⚠️ Email não configurado - token:', emailToken)
        }

        return res.status(201).json({
            message: 'Usuário cadastrado! Verifique seu e-mail para confirmar a conta.'
        })
    } catch (err) {
        console.error('❌ Erro no registro:', err)
        return res.status(500).json({ error: err.message })
    }
}

// ===========================
// LOGIN (Ajustado para PostgreSQL)
// ===========================
export const login = async (req, res) => {
    try {
        const { email, password } = req.body
        if (!email || !password) {
            return res.status(400).json({ message: 'E-mail e senha são obrigatórios.' })
        }

        const normalizedEmail = email.trim().toLowerCase()
        const user = await findUserByEmail(normalizedEmail)
        if (!user) {
            await db.execute(
                'INSERT INTO auth_attempts (id, email, ip, user_agent, action, success) VALUES ($1, $2, $3, $4, $5, $6)',
                [uuidv4(), normalizedEmail, req.ip, req.get('User-Agent'), 'login', false]
            )
            return res.status(401).json({ message: 'Credenciais inválidas.' })
        }
        
        if (!user.is_verified) {
            return res.status(403).json({ message: 'Confirme seu e-mail antes de fazer login.' })
        }

        const isMatch = await bcrypt.compare(password, user.password)
        if (!isMatch) {
            await db.execute(
                'INSERT INTO auth_attempts (id, email, ip, user_agent, action, success) VALUES ($1, $2, $3, $4, $5, $6)',
                [uuidv4(), normalizedEmail, req.ip, req.get('User-Agent'), 'login', false]
            )
            return res.status(401).json({ message: 'Credenciais inválidas.' })
        }

        await db.execute(
            'INSERT INTO auth_attempts (id, email, ip, user_agent, action, success) VALUES ($1, $2, $3, $4, $5, $6)',
            [uuidv4(), user.email, req.ip, req.get('User-Agent'), 'login', true]
        )

        const { token, jti } = createToken({ id: user.id, email: user.email, role: user.role })

        const sessionId = uuidv4()
        await db.execute(
            'INSERT INTO sessions (id, user_id, jti, ip, user_agent) VALUES ($1, $2, $3, $4, $5)',
            [sessionId, user.id, jti, req.ip, req.get('User-Agent')]
        )

        await logAction(user.id, 'login', 'auth', { ip: req.ip })

        return res.json({
            message: 'Login realizado com sucesso!',
            token,
            user: {
                id: user.id,
                name: user.name,
                email: user.email,
                role: user.role
            }
        })
    } catch (err) {
        console.error('❌ Erro no login:', err)
        return res.status(500).json({ error: err.message })
    }
}

// ===========================
// ESQUECI SENHA (Ajustado para PostgreSQL)
// ===========================
export const forgotPassword = async (req, res) => {
    try {
        const { email } = req.body
        if (!email) return res.status(400).json({ message: 'E-mail é obrigatório' })

        const normalizedEmail = email.trim().toLowerCase()
        const user = await findUserByEmail(normalizedEmail)

        if (!user) {
            return res.json({ message: 'Se o e-mail existir, enviaremos um link de recuperação.' })
        }

        const resetToken = uuidv4()
        const expiresAt = new Date(Date.now() + 1000 * 60 * 15) // 15 min

        await db.execute(
            'INSERT INTO password_resets (user_id, token, expires_at) VALUES ($1, $2, $3)',
            [user.id, resetToken, expiresAt]
        )

        const resetLink = `${process.env.FRONTEND_URL}/reset-password/${resetToken}`

        if (mailer && process.env.MAIL_USER) {
            try {
                await mailer.sendMail({
                    to: user.email,
                    subject: 'Recuperação de senha — Instituto Alma',
                    html: 
                    `<p>Olá, ${user.name}!</p>
                    <p>Clique abaixo para redefinir sua senha:</p>
                    <a href="${resetLink}">${resetLink}</a>
                    <p>Este link expira em 15 minutos.</p>
                    <p>Se você não solicitou essa alteração, ignore este e-mail.</p>`
                })
            } catch (emailError) {
                console.log('⚠️ Email de recuperação não enviado:', emailError.message)
            }
        }

        await logAction(user.id, 'forgot_password', 'auth')

        return res.json({ message: 'E-mail enviado se for válido.' })
    } catch (err) {
        console.error('❌ Erro em forgotPassword:', err)
        return res.status(500).json({ error: err.message })
    }
}

// ===========================
// REDEFINIR SENHA (Ajustado para PostgreSQL)
// ===========================
export const resetPassword = async (req, res) => {
    try {
        const { token } = req.params
        const { password } = req.body

        const result = await db.execute(
            'SELECT * FROM password_resets WHERE token = $1 AND expires_at > NOW()',
            [token]
        )

        const rows = DBCompat.getRows(result)
        
        if (!rows || rows.length === 0) {
            return res.status(400).json({ message: 'Token expirado ou inválido.' })
        }

        const reset = rows[0]

        const hash = await bcrypt.hash(password, 10)
        await db.execute('UPDATE users SET password = $1 WHERE id = $2', [hash, reset.user_id])

        await db.execute('DELETE FROM password_resets WHERE token = $1', [token])

        await logAction(reset.user_id, 'reset_password', 'auth')

        return res.json({ message: 'Senha redefinida com sucesso!' })
    } catch (err) {
        console.error('❌ Erro em resetPassword:', err)
        return res.status(500).json({ error: err.message })
    }
}

// ===========================
// LOGOUT (Ajustado para PostgreSQL)
// ===========================
export const logout = async (req, res) => {
    try {
        const authHeader = req.headers.authorization
        if (!authHeader) {
            return res.status(400).json({
                message: 'Token ausente. Use Authorization: Bearer <token>'
            })
        }

        const token = authHeader.split(' ')[1]
        const decoded = jwt.decode(token)
        if (!decoded || !decoded.jti) {
            return res.status(400).json({ message: 'Token inválido' })
        }

        await denyToken(decoded.jti, decoded.exp)

        await db.execute(
            'UPDATE sessions SET revoked_at = NOW() WHERE jti = $1',
            [decoded.jti]
        )

        await logAction(decoded.id, 'logout', 'auth', { jti: decoded.jti })

        res.json({ message: 'Logout realizado com sucesso' })
    } catch (err) {
        console.error('❌ Erro no logout:', err)
        res.status(500).json({ error: err.message })
    }
}