// src/app.js - VERSÃO FINAL PARA RENDER
import express from 'express'
import path from 'path'
import { fileURLToPath } from 'url'
import 'dotenv/config'
import { applySecurity } from './middleware/securityMiddlewares.js'
import { requestLogger, errorLogger } from './middleware/requestLogger.js'

// Importar rotas
import uploadRoutes from './modules/uploads/uploadRoutes.js'
import authRoutes from './modules/auth/authRoutes.js'
import userRoutes from './modules/users/userRoutes.js'
import sessionRoutes from './modules/auth/sessionRoutes.js'
import transparencyRoutes from './modules/transparency/transparencyRoutes.js'
import donationRoutes from './modules/donations/donationsRoutes.js'
import feedbackRoutes from './modules/feedback/feedbackRoutes.js'
import eventRoutes from './modules/events/eventsRoutes.js'
import adminRoutes from './modules/admin/adminRoutes.js'
import activitiesRoutes from './modules/activities/activitiesRoutes.js'
import setupRoutes from './modules/setup/setupRoutes.js'
import { healthCheck } from './healthCheck.js'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

const app = express()

// -------------- CONFIGURAÇÕES BÁSICAS --------------
app.use(express.json({ limit: '100kb' }))
app.use(express.urlencoded({ extended: true, limit: '10kb' }))

// -------------- SEGURANÇA --------------
applySecurity(app)

// -------------- LOGGING --------------
app.use(requestLogger)

// -------------- STATIC FILES --------------
app.use('/uploads', express.static(path.join(__dirname, 'public/uploads')))

// -------------- ROTAS --------------
app.use('/api', uploadRoutes)
app.use('/api/auth', authRoutes)
app.use('/api/users', userRoutes)
app.use('/api/feedback', feedbackRoutes)
app.use('/api/auth', sessionRoutes)
app.use('/api/transparency', transparencyRoutes)
app.use('/api/donations', donationRoutes)
app.use('/api/events', eventRoutes)
app.use('/api/admin', adminRoutes)
app.use('/api/activities', activitiesRoutes)
app.use('/api/setup', setupRoutes)

// -------------- HEALTH CHECK --------------
app.get('/', healthCheck)
app.get('/health', healthCheck)
app.get('/api/health', healthCheck)

// -------------- ERROR HANDLING --------------
app.use((err, req, res, next) => {
  console.error('Erro não tratado:', err)
  const status = err.status || 500
  const message = process.env.NODE_ENV === 'production' 
    ? 'Erro interno do servidor' 
    : err.message
  
  res.status(status).json({ error: message })
})

app.use(errorLogger)

export default app