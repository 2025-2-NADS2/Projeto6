import express from 'express'
import { authMiddleware } from '../../middleware/authMiddleware.js'
import { authorizeRoles } from '../../middleware/roleMiddleware.js'

import { 
    createDonation, 
    approveDonation, 
    listMyDonations, 
    listAllDonations, 
    getDonationById 
} from './donationsController.js'

const router = express.Router()

// ✅ Criar doação
router.post('/', authMiddleware, createDonation)

// ✅ Minhas doações
router.get('/me', authMiddleware, listMyDonations)

// ✅ Admin aprova pagamento
router.post('/:id/approve', authMiddleware, authorizeRoles('admin'), approveDonation)

// ✅ Buscar doação por ID (coloque ANTES da listagem geral)
router.get('/:id', authMiddleware, getDonationById)

// ✅ Admin lista todas (rota genérica, por último)
router.get('/', authMiddleware, authorizeRoles('admin'), listAllDonations)

export default router
