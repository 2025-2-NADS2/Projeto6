import express from 'express'
import { register, login, forgotPassword, resetPassword } from './authController.js'
import { loginLimiter } from '../../middleware/rateLimiters.js'
import { sanitizeInput } from '../../middleware/sanitizeInput.js'
import { registerLimiter } from '../../middleware/rateLimiters.js'
import { confirmEmailController } from './confirmEmailController.js'

const router = express.Router()

// REGISTRO

router.post('/register', registerLimiter, sanitizeInput, register)

// LOGIN
router.post('/login', loginLimiter, sanitizeInput, login)

// ESQUECI MINHA SENHA
router.post('/forgot-password', sanitizeInput, forgotPassword)

// RESET DE SENHA
router.post('/reset-password/:token', sanitizeInput, resetPassword)

router.get('/confirm-email/:token', confirmEmailController)

export default router
