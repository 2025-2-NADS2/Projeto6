import { v4 as uuidv4 } from 'uuid'   // gera UUID para transparency_files.id
import db from '../../config/db.js'  // pool de conexões MySQL/config DB
import fs from 'fs'                  // manipulação de arquivos no disco
import { logAction } from '../../utils/logUtils.js' // função para gravar audit logs

// ---------------------------
// UPLOAD DE TRANSPARÊNCIA (POST)
// ---------------------------
export const uploadTransparencyFile = async (req, res) => {
    try {
      // lê campos enviados no form (title é obrigatório)
      const { title, description } = req.body
      // tratar is_public que vem como '0' ou '1' em form-data
      const isPublic = req.body.is_public === '0' ? 0 : 1

      // valida: título obrigatório — se faltar, removemos o arquivo salvo e retornamos erro
      if (!title) {
        if (req.file && req.file.path) fs.unlink(req.file.path, () => {}) // limpa arquivo órfão
        return res.status(400).json({ message: 'Título é obrigatório.' })
      }

    // valida: arquivo deve existir (uploadPDFConfig já restringiu por tipo)
    if (!req.file) {
      return res.status(400).json({ message: 'Arquivo PDF é obrigatório.' })
    }

    // apesar do fileFilter do multer, reforçamos checagem de segurança
    const mimetype = req.file.mimetype || ''
    const originalName = req.file.originalname || ''
    if (!mimetype.includes('pdf') && !originalName.toLowerCase().endsWith('.pdf')) {
      // remove arquivo inválido
      fs.unlink(req.file.path, () => {})
      return res.status(400).json({ message: 'Apenas arquivos PDF são permitidos.' })
    }

    // prepara dados do arquivo para gravar na tabela uploads
    const filepath = req.file.path                      // caminho absoluto salvo no disco
    const originalname = req.file.originalname          // nome original enviado
    const mimetypeValue = req.file.mimetype             // tipo MIME detectado
    const size = req.file.size                          // tamanho em bytes

    // insere registro na tabela uploads e guarda o insertId (id do registro)
    const [uploadResult] = await db.execute(
      'INSERT INTO uploads (path, type, original_name, mime_type, size) VALUES ($1, $2, $3, $4, $5) RETURNING *',
    [filepath, 'document', originalname, mimetypeValue, size]
    );
    const uploadId = uploadResult.rows[0].id;


    // cria id único para transparency_files
    const id = uuidv4()
    const createdBy = req.user?.id || null

    // insere metadados em transparency_files apontando para upload registrado
    await db.execute(
      'INSERT INTO transparency_files (id, upload_id, title, description, is_public, created_by) VALUES ($1, $2, $3, $4, $5, $6)',
      [id, uploadId, title, description || null, isPublic, createdBy]
    );

    // registra ação no audit_logs (quem fez, qual ação, detalhes, req para user-agent/ip)
    await logAction(createdBy, 'upload_transparency', 'transparency', { id, uploadId, title }, req)

    // retorna sucesso com id do documento e uploadId
    return res.status(201).json({ message: 'Arquivo de transparência enviado com sucesso', id, uploadId })
  } catch (err) {
    // em caso de erro, tenta remover arquivo caso tenha sido salvo no disco
    if (req.file && req.file.path) fs.unlink(req.file.path, () => {})
    return res.status(500).json({ error: err.message })
  }
}

// ---------------------------
// LISTAR ARQUIVOS PÚBLICOS (GET /)
// ---------------------------
export const listPublicFiles = async (req, res) => {
  try {
    // seleciona registros públicos juntando uploads para pegar o caminho físico
    const [rows] = await db.execute(
    'SELECT t.id, t.title, t.description, t.is_public, t.created_at, u.path AS file_path, u.original_name FROM transparency_files t JOIN uploads u ON u.id = t.upload_id WHERE t.is_public = true ORDER BY t.created_at DESC'
    );

    // mapeia resultados para formato de resposta convertendo caminho físico em url pública
    const files = rows.map(r => ({
      id: r.id,
      title: r.title,
      description: r.description,
      created_at: r.created_at,
      // gera URL pública assumindo que app.js serve /uploads -> src/public/uploads
      url: `/uploads/${r.file_path.split('src/public/uploads/').pop()}`
    }))

    return res.json(files)
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}

// ---------------------------
// OBTER ARQUIVO POR ID (GET /:id)
// ---------------------------
export const getFileById = async (req, res) => {
  try {
    const { id } = req.params

    // busca metadados + caminho físico via join com uploads
    const [rows] = await db.execute(
    'SELECT t.id, t.title, t.description, t.is_public, t.created_at, u.path AS file_path, u.original_name FROM transparency_files t JOIN uploads u ON u.id = t.upload_id WHERE t.id = $1',
    [id]
    );

    if (!rows.length) return res.status(404).json({ message: 'Arquivo não encontrado' })

    const file = rows[0]

    // se arquivo for privado, só admin (ou usuário com permissão) pode acessar
    if (!file.is_public) {
      // verifica req.user (se authMiddleware rodou, req.user existe)
      if (!req.user || req.user.role !== 'admin') {
        return res.status(403).json({ message: 'Arquivo privado. Acesso negado.' })
      }
    }

    // retorna metadados + url pública (ou privada caso admin)
    const url = `/uploads/${file.file_path.split('src/public/uploads/').pop()}`
    return res.json({
      id: file.id,
      title: file.title,
      description: file.description,
      created_at: file.created_at,
      url
    })
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}

// ---------------------------
// LISTAR TODOS (ADMIN) (GET /admin/all)
// ---------------------------
export const listAllFilesForAdmin = async (req, res) => {
  try {
    const [rows] = await db.execute(
      'SELECT t.id, t.title, t.description, t.is_public, t.created_at, u.path AS file_path, u.original_name, t.created_by FROM transparency_files t JOIN uploads u ON u.id = t.upload_id ORDER BY t.created_at DESC'
    );

    const files = rows.map(r => ({
      id: r.id,
      title: r.title,
      description: r.description,
      is_public: r.is_public,
      created_by: r.created_by,
      created_at: r.created_at,
      url: `/uploads/${r.file_path.split('src/public/uploads/').pop()}`
    }))

    return res.json(files)
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}

// ---------------------------
// ATUALIZAR (PUT /:id) (ADMIN)
// ---------------------------
export const updateTransparencyFile = async (req, res) => {
  try {
    const { id } = req.params
    const { title, description } = req.body
    const isPublic = req.body.is_public === '0' ? 0 : 1

    // busca registro atual para pegar upload_id e caminho físico
    const [rows] = await db.execute(
      'SELECT t.id, t.upload_id, u.path AS file_path FROM transparency_files t JOIN uploads u ON u.id = t.upload_id WHERE t.id = ?',
      [id]
    )
    if (!rows.length) return res.status(404).json({ message: 'Registro não encontrado' })

    const rec = rows[0]
    let newUploadId = rec.upload_id
    let newFilePath = rec.file_path

    // se vier novo arquivo, validamos e inserimos novo registro em uploads
    if (req.file) {
      const mimetype = req.file.mimetype || ''
      const originalName = req.file.originalname || ''
      if (!mimetype.includes('pdf') && !originalName.toLowerCase().endsWith('.pdf')) {
        fs.unlink(req.file.path, () => {}) // limpa arquivo inválido
        return res.status(400).json({ message: 'Apenas PDF permitido.' })
      }

      // insere novo upload e guarda insertId
      const [uploadResult] = await db.execute(
        'INSERT INTO uploads (path, type, original_name, mime_type, size) VALUES (?, ?, ?, ?, ?)',
        [req.file.path, 'document', req.file.originalname, req.file.mimetype, req.file.size]
      )
      newUploadId = uploadResult.insertId
      newFilePath = req.file.path

      // remove arquivo antigo do disco (não remove registro de uploads aqui automaticamente)
      fs.unlink(rec.file_path, () => {})
    }

    // atualiza metadados no transparency_files
    await db.execute(
      'UPDATE transparency_files SET upload_id = $1, title = $2, description = $3, is_public = $4 WHERE id = $5',
      [newUploadId, title || null, description || null, isPublic, id]
  );

    // log de auditoria
    await logAction(req.user.id, 'update_transparency', 'transparency', { id }, req)

    return res.json({ message: 'Arquivo de transparência atualizado com sucesso' })
  } catch (err) {
    // se falhar e houver novo arquivo, limpa
    if (req.file && req.file.path) fs.unlink(req.file.path, () => {})
    return res.status(500).json({ error: err.message })
  }
}

// ---------------------------
// DELETAR (DELETE /:id) (ADMIN)
// ---------------------------
export const deleteTransparencyFile = async (req, res) => {
  try {
    const { id } = req.params

    // busca caminho físico via join
    const [rows] = await db.execute(
      'SELECT t.upload_id, u.path AS file_path FROM transparency_files t JOIN uploads u ON u.id = t.upload_id WHERE t.id = ?',
      [id]
    )
    if (!rows.length) return res.status(404).json({ message: 'Registro não encontrado' })

    const rec = rows[0]

    // deleta registro de transparency_files
    await db.execute('DELETE FROM transparency_files WHERE id = $1', [id]);

    // deleta registro em uploads (opcional, remove histórico)
    await db.execute('DELETE FROM uploads WHERE id = $1', [rec.upload_id]);

    // remove arquivo físico do disco
    fs.unlink(rec.file_path, () => {})

    // registra log de auditoria
    await logAction(req.user.id, 'delete_transparency', 'transparency', { id }, req)

    return res.json({ message: 'Arquivo de transparência removido com sucesso' })
  } catch (err) {
    return res.status(500).json({ error: err.message })
  }
}
