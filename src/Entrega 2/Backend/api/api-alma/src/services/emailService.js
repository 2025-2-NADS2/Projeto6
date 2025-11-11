// src/services/emailService.js
import nodemailer from 'nodemailer'

// Cria e exporta o transporter do nodemailer configurado via .env
export const mailer = nodemailer.createTransport({
    host: process.env.MAIL_HOST,          // host SMTP do provedor
    port: Number(process.env.MAIL_PORT),  // porta (587, 465, etc.)
    secure: Number(process.env.MAIL_PORT) === 465, // true se porta 465 (SMTPS)
    auth: {
    user: process.env.MAIL_USER,        // usuário SMTP
    pass: process.env.MAIL_PASS         // senha SMTP
    }
})
