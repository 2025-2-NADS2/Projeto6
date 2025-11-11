// src/config/uploadConfig.js - AJUSTADO PARA RENDER
import multer from 'multer'
import path from 'path'
import fs from 'fs'

// Define base directory para Render e desenvolvimento
const baseDir = process.env.NODE_ENV === 'production' 
  ? '/opt/render/project/src/public/uploads'
  : path.resolve('src/public/uploads')

console.log('📁 Upload base directory:', baseDir)

const storage = multer.diskStorage({
    destination: (req, file, cb) => {
        let folder = 'others'

        if (file.mimetype.startsWith('image/')) folder = 'images'
        else if (file.mimetype === 'application/pdf') folder = 'documents'
        else if (file.mimetype.startsWith('video/')) folder = 'videos'

        const uploadPath = path.join(baseDir, folder)
        
        // Garantir que o diretório existe
        if (!fs.existsSync(uploadPath)) {
            fs.mkdirSync(uploadPath, { recursive: true })
        }

        cb(null, uploadPath)
    },

    filename: (req, file, cb) => {
        const timestamp = Date.now()
        const uniqueName = `${timestamp}-${file.originalname}`
        cb(null, uniqueName)
    }
})

// File filter para segurança
const fileFilter = (req, file, cb) => {
    const allowedMimes = [
        'image/jpeg',
        'image/png',
        'image/gif',
        'image/webp',
        'application/pdf',
        'video/mp4',
        'video/mpeg',
        'video/quicktime'
    ]

    if (allowedMimes.includes(file.mimetype)) {
        cb(null, true)
    } else {
        cb(new Error('Tipo de arquivo não permitido. Apenas imagens, PDFs e vídeos são aceitos.'), false)
    }
}

const upload = multer({ 
    storage,
    fileFilter,
    limits: {
        fileSize: 10 * 1024 * 1024 // 10MB
    }
})

export default upload