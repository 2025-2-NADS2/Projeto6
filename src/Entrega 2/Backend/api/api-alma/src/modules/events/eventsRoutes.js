// src/modules/events/eventsRoutes.js

import express from 'express'
import upload from '../../config/uploadConfig.js'
import { authMiddleware } from '../../middleware/authMiddleware.js'
import { authorizeRoles } from '../../middleware/roleMiddleware.js'

import {
    getAllEvents,
    getEventById,
    createEvent,
    updateEvent,
    deleteEvent
} from './eventsController.js'

const router = express.Router()

// ✅ Rotas públicas
router.get('/', getAllEvents)
router.get('/:id', getEventById)

// ✅ Admin-only
router.post('/', authMiddleware, authorizeRoles('admin'), upload.single('image'), createEvent)
router.put('/:id', authMiddleware, authorizeRoles('admin'), upload.single('image'), updateEvent)
router.delete('/:id', authMiddleware, authorizeRoles('admin'), deleteEvent)

export default router
