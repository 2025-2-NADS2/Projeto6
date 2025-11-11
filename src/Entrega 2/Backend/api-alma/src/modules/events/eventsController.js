// src/modules/events/eventsController.js

import db from '../../config/db.js'            // Conexão com o MySQL
import upload from '../../config/uploadConfig.js' // Configuração do Multer (seu uploadConfig)
import { v4 as uuidv4 } from 'uuid'           // Gerar IDs únicos
import fs from 'fs'                           // Para deletar arquivos antigos
import { logAction } from '../../utils/logUtils.js' // Registrar logs

// ======================================================
// ✅ LISTAR TODOS OS EVENTOS (PÚBLICO)
// ======================================================
export const getAllEvents = async (req, res) => {
    try {
        const [rows] = await db.execute(
            "SELECT * FROM events ORDER BY event_date DESC"
        )
        res.json(rows)
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}

// ======================================================
// ✅ LISTAR EVENTO POR ID (PÚBLICO)
// ======================================================
export const getEventById = async (req, res) => {
    try {
        const { id } = req.params

        const [rows] = await db.execute(
            "SELECT * FROM events WHERE id = ?", [id]
        )

        if (!rows.length)
            return res.status(404).json({ message: "Evento não encontrado" })

        res.json(rows[0])
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}

// ======================================================
// ✅ CRIAR EVENTO (APENAS ADMIN)
// ======================================================
export const createEvent = async (req, res) => {
    try {
        const { title, description, event_date } = req.body

        if (!title || !description || !event_date)
            return res.status(400).json({ message: "Todos os campos são obrigatórios" })

        const id = uuidv4()
        const imagePath = req.file ? req.file.path : null

        await db.execute(
            "INSERT INTO events (id, title, description, image_path, event_date) VALUES (?, ?, ?, ?, ?)",
            [id, title, description, imagePath, event_date]
        )

        await logAction(req.user.id, 'create', 'events', { id, title })

        res.status(201).json({ message: "Evento criado com sucesso", id })
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}

// ======================================================
// ✅ ATUALIZAR EVENTO (APENAS ADMIN)
// ======================================================
export const updateEvent = async (req, res) => {
    try {
        const { id } = req.params
        const { title, description, event_date } = req.body

        const [old] = await db.execute("SELECT * FROM events WHERE id = ?", [id])
        if (!old.length)
            return res.status(404).json({ message: "Evento não encontrado" })

        const oldImage = old[0].image_path
        const newImage = req.file ? req.file.path : oldImage

        await db.execute(
            "UPDATE events SET title = ?, description = ?, image_path = ?, event_date = ? WHERE id = ?",
            [title || old[0].title, description || old[0].description, newImage, event_date || old[0].event_date, id]
        )

        // Se tem nova imagem, apaga a antiga
        if (req.file && oldImage) fs.unlink(oldImage, () => null)

        await logAction(req.user.id, 'update', 'events', { id })

        res.json({ message: "Evento atualizado com sucesso" })
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}

// ======================================================
// ✅ DELETAR EVENTO (APENAS ADMIN)
// ======================================================
export const deleteEvent = async (req, res) => {
    try {
        const { id } = req.params

        const [rows] = await db.execute(
            "SELECT * FROM events WHERE id = ?", [id]
        )

        if (!rows.length)
            return res.status(404).json({ message: "Evento não encontrado" })

        const image = rows[0].image_path

        await db.execute("DELETE FROM events WHERE id = ?", [id])

        if (image) fs.unlink(image, () => null)

        await logAction(req.user.id, 'delete', 'events', { id })

        res.json({ message: "Evento deletado com sucesso" })
    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}
