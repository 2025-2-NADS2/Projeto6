import { v4 as uuidv4 } from 'uuid'           // gera ids únicos
import db from '../../config/db.js'          // pool de conexões MySQL
import { mailer } from '../../services/emailService.js' // transporter nodemailer
import { logAction } from '../../utils/logUtils.js'    // registra auditoria

// =============================
// Enviar mensagem de Ouvidoria
// POST /api/feedback
// Body: { name, email, subject, message }
// =============================
export const submitFeedback = async (req, res) => {
  try {
    // 1) extrai e normaliza dados do body
    const { name, email, subject, message } = req.body

    // 2) validações básicas
    if (!name || !email || !message) {
      return res.status(400).json({ message: 'Nome, e-mail e mensagem são obrigatórios.' })
    }

    // 3) opcional: valida email com regex simples (ou use validator.isEmail)
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(String(email).trim().toLowerCase())) {
      return res.status(400).json({ message: 'E-mail inválido.' })
    }

    // 4) cria id da mensagem e persiste no DB
    const id = uuidv4()
    await db.execute(
      'INSERT INTO feedback_messages (id, name, email, subject, message) VALUES (?, ?, ?, ?, ?)',
      [id, String(name).trim(), String(email).trim().toLowerCase(), subject ? String(subject).trim() : null, String(message).trim()]
    )

    // 5) registra log de auditoria (quem enviou — no caso, email enviado, sem user_id)
    await logAction(null, 'submit_feedback', 'feedback', { id, name, email })

    // 6) envia email para o suporte / ouvidoria
    // corpo do e-mail (HTML simples)
    const supportEmail = process.env.SUPPORT_EMAIL
    const mailOptions = {
      from: `"Ouvidoria" <${process.env.MAIL_USER}>`, // remetente (configurado)
      to: supportEmail,                               // para quem vai a mensagem (suporte)
      replyTo: email,                                 // reply-to para facilitar resposta ao remetente
      subject: `[Ouvidoria] ${subject || 'Nova mensagem'}`,
      html: `
        <p><strong>Nome:</strong> ${String(name)}</p>
        <p><strong>E-mail:</strong> ${String(email)}</p>
        <p><strong>Assunto:</strong> ${String(subject || '')}</p>
        <p><strong>Mensagem:</strong></p>
        <div>${String(message).replace(/\n/g,'<br>')}</div>
        <hr />
        <p>Mensagem ID: ${id}</p>
      `
    }

    // 7) envia o email (não bloqueante: trata erro, mas não falha o envio da API)
    mailer.sendMail(mailOptions, (err, info) => {
      if (err) {
        // log local; não interrompe fluxo
        console.error('Erro ao enviar e-mail de ouvidoria:', err)
      } else {
        console.log('E-mail de ouvidoria enviado:', info && info.response)
      }
    })

    // 8) resposta ao usuário (não expor dados internos)
    return res.status(201).json({ message: 'Mensagem enviada com sucesso. Obrigado pelo seu contato.' })
  } catch (err) {
    console.error('Erro submitFeedback:', err)
    return res.status(500).json({ error: err.message })
  }
}


// =============================
// ADMIN: listar mensagens (GET /api/feedback)
// =============================
export const listFeedback = async (req, res) => {
  try {
    // seleciona mensagens mais recentes primeiro
    const [rows] = await db.execute(
      'SELECT id, name, email, subject, message, status, created_at FROM feedback_messages ORDER BY created_at DESC'
    )
    return res.json(rows)
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}


// =============================
// ADMIN: obter mensagem específica (GET /api/feedback/:id)
// =============================
export const getFeedbackById = async (req, res) => {
  try {
    const { id } = req.params
    const [rows] = await db.execute(
      'SELECT id, name, email, subject, message, status, created_at FROM feedback_messages WHERE id = ?',
      [id]
    )
    if (!rows.length) return res.status(404).json({ message: 'Mensagem não encontrada' })

    // opcional: marcar como lida ao abrir (atualiza status)
    await db.execute('UPDATE feedback_messages SET status = ? WHERE id = ?', ['read', id])

    // registra log de leitura (admin fez a leitura)
    await logAction(req.user.id, 'read_feedback', 'feedback', { id })

    return res.json(rows[0])
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}


// =============================
// ADMIN: atualizar status (PUT /api/feedback/:id/status)
// Body: { status: 'new'|'read'|'archived' }
// =============================
export const updateFeedbackStatus = async (req, res) => {
  try {
    const { id } = req.params
    const { status } = req.body

    const allowed = ['new', 'read', 'archived']
    if (!allowed.includes(status)) return res.status(400).json({ message: 'Status inválido' })

    await db.execute('UPDATE feedback_messages SET status = ? WHERE id = ?', [status, id])

    await logAction(req.user.id, 'update_feedback_status', 'feedback', { id, status })

    return res.json({ message: 'Status atualizado com sucesso' })
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}


// =============================
// ADMIN: deletar mensagem (DELETE /api/feedback/:id)
// =============================
export const deleteFeedback = async (req, res) => {
  try {
    const { id } = req.params

    await db.execute('DELETE FROM feedback_messages WHERE id = ?', [id])

    await logAction(req.user.id, 'delete_feedback', 'feedback', { id })

    return res.json({ message: 'Mensagem removida com sucesso' })
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}
