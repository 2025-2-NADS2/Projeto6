// src/modules/setup/setupRoutes.js - VERSÃO SEGURA
import express from 'express'
import { setupDatabase, testDatabase } from './setupController.js'

const router = express.Router()

// 🔒 ROTAS DE SETUP - APENAS EM DESENVOLVIMENTO OU COM AUTENTICAÇÃO
if (process.env.NODE_ENV !== 'production') {
  // Setup completo do banco
  router.post('/database', setupDatabase)
  
  // Teste de conexão
  router.get('/test', testDatabase)
  
  // Status do banco
  router.get('/status', async (req, res) => {
    try {
      const result = await db.execute(`
        SELECT 
          (SELECT COUNT(*) FROM users) as users_count,
          (SELECT COUNT(*) FROM activities) as activities_count,
          (SELECT COUNT(*) FROM events) as events_count,
          (SELECT COUNT(*) FROM donations) as donations_count
      `)
      const data = DBCompat.getFirstRow(result)
      res.json(data)
    } catch (error) {
      res.status(500).json({ error: error.message })
    }
  })
} else {
  // Em produção, retornar erro ou desativar
  router.use(['/database', '/test', '/status'], (req, res) => {
    res.status(403).json({ 
      error: 'Setup routes disabled in production',
      hint: 'Use database management tools instead'
    })
  })
}

export default router