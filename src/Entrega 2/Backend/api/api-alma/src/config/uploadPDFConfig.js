// src/config/uploadPDFConfig.js - VERSÃO PRODUÇÃO
import multer from 'multer'
import path from 'path'
import fs from 'fs'

const baseDir = process.env.NODE_ENV === 'production'
  ? '/opt/render/project/src/public/uploads/documents'
  : path.resolve('src/public/uploads/documents')

console.log('📁 Diretório de PDFs:', baseDir)

const storage = multer.diskStorage({
    destination: (req, file, cb) => {
        if (!fs.existsSync(baseDir)) {
            fs.mkdirSync(baseDir, { recursive: true })
        }
        cb(null, baseDir)
    },
    filename: (req, file, cb) => {
        const name = `${Date.now()}-${file.originalname}`
        cb(null, name)
    }
})

const fileFilter = (req, file, cb) => {
    const mimetype = file.mimetype || ''
    const original = (file.originalname || '').toLowerCase()
    if (mimetype.includes('pdf') || original.endsWith('.pdf')) {
        cb(null, true)
    } else {
        cb(new Error('Somente arquivos PDF são permitidos.'), false)
    }
}

export default multer({
    storage,
    fileFilter,
    limits: { fileSize: 10 * 1024 * 1024 }
})