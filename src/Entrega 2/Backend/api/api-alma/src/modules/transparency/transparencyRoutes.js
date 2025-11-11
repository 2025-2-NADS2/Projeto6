import express from 'express'                                 // framework web
import uploadPDF from '../../config/uploadPDFConfig.js'       // nosso multer só-PDF
import { authMiddleware } from '../../middleware/authMiddleware.js' // valida JWT
import { authorizeRoles } from '../../middleware/roleMiddleware.js' // valida role=admin
import { uploadTransparencyFile, listPublicFiles, getFileById, listAllFilesForAdmin, updateTransparencyFile, deleteTransparencyFile } from './transparencyController.js'                         // controller que implementamos

const router = express.Router()                              // cria roteador isolado

// Rota pública — lista apenas arquivos marcados como "is_public = 1"
router.get('/', listPublicFiles)

// Rota pública — metadados + url de um arquivo (se público). Se privado, só admin tem acesso via token.
router.get('/:id', getFileById)

// Rota admin — lista todos (privados e públicos) para dashboard
router.get('/admin/all', authMiddleware, authorizeRoles('admin'), listAllFilesForAdmin)

// Rota admin — upload de PDF (usa uploadPDF para garantir somente PDF)
router.post('/', authMiddleware, authorizeRoles('admin'), uploadPDF.single('file'), uploadTransparencyFile)

// Rota admin — atualizar (metadados e/ou substituir arquivo)
router.put('/:id', authMiddleware, authorizeRoles('admin'), uploadPDF.single('file'), updateTransparencyFile)

// Rota admin — deletar
router.delete('/:id', authMiddleware, authorizeRoles('admin'), deleteTransparencyFile)

export default router
