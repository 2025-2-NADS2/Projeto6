// src/modules/admin/adminRoutes.js

import express from 'express'
import { authMiddleware } from '../../middleware/authMiddleware.js'
import { authorizeRoles } from '../../middleware/roleMiddleware.js'

import * as activities from '../activities/activitiesController.js'
import * as events from '../events/eventsController.js'
import * as transparency from '../transparency/transparencyController.js'
import upload from '../../config/uploadConfig.js'

const router = express.Router()

router.use(authMiddleware, authorizeRoles('admin'))

// ATIVIDADES
router.post('/activities', upload.single('image'), activities.createActivity)
router.put('/activities/:id', upload.single('image'), activities.updateActivity)
router.delete('/activities/:id', activities.deleteActivity)
router.get('/activities', activities.getAllActivities)

// EVENTOS
router.post('/events', upload.single('image'), events.createEvent)
router.put('/events/:id', upload.single('image'), events.updateEvent)
router.delete('/events/:id', events.deleteEvent)
router.get('/events', events.getAllEvents)

// TRANSPARÊNCIA
router.post('/files', upload.single('file'), transparency.uploadTransparencyFile)
router.delete('/files/:id', transparency.deleteTransparencyFile)
router.get('/files', transparency.listAllFilesForAdmin)


// LOGS
router.get('/logs', async (req, res) => {
    const [rows] = await db.execute(
        "SELECT * FROM audit_logs ORDER BY created_at DESC"
    )
    res.json(rows)
})

export default router
