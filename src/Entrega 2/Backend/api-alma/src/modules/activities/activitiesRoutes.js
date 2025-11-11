import express from 'express'
import upload from '../../config/uploadConfig.js'
import { authMiddleware } from '../../middleware/authMiddleware.js'
import { authorizeRoles } from '../../middleware/roleMiddleware.js'
import { createActivity, getAllActivities, getActivityById, updateActivity, deleteActivity } from './activitiesController.js'

const router = express.Router()

// ✅ Usuário comum pode ver
router.get('/', getAllActivities)
router.get('/:id', getActivityById)

// ✅ Admin cria atividade com imagem
router.post( '/', authMiddleware, authorizeRoles('admin'), upload.single('image'), createActivity )

// ✅ Admin atualiza atividade
router.put( '/:id', authMiddleware, authorizeRoles('admin'), upload.single('image'), updateActivity )

// ✅ Admin deleta atividade
router.delete( '/:id', authMiddleware, authorizeRoles('admin'), deleteActivity )

export default router
