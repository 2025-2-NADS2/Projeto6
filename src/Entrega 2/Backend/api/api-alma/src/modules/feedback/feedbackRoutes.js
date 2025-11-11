// src/modules/feedback/feedbackRoutes.js
import express from 'express'
import { 
  submitFeedback, 
  listFeedback, 
  getFeedbackById, 
  updateFeedbackStatus, 
  deleteFeedback 
} from './feedbackController.js'
import { authMiddleware } from '../../middleware/authMiddleware.js'
import { authorizeRoles } from '../../middleware/roleMiddleware.js'
import { feedbackLimiter } from '../../middleware/feedbackRateLimiter.js'
import { sanitizeInput } from '../../middleware/sanitizeInput.js'

const router = express.Router()

// ✅ ROTA PÚBLICA - enviar mensagem (SEM authMiddleware)
router.post('/', feedbackLimiter, sanitizeInput, submitFeedback)

// ✅ ROTAS ADMIN - precisam de autenticação
router.get('/', authMiddleware, authorizeRoles('admin'), listFeedback)
router.get('/:id', authMiddleware, authorizeRoles('admin'), getFeedbackById)
router.put('/:id/status', authMiddleware, authorizeRoles('admin'), updateFeedbackStatus)
router.delete('/:id', authMiddleware, authorizeRoles('admin'), deleteFeedback)

export default router