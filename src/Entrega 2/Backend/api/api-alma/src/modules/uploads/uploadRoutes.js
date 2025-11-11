import express from 'express' // Importa o framework Express
import db from '../../config/db.js' // Importa a configuração do banco de dados
import upload from '../../config/uploadConfig.js' // Importa a configuração do multer para upload de arquivos
import fs from 'fs' // Importa o módulo de sistema de arquivos do Node.js utilizado para manipular arquivos no servidor

const router = express.Router() // Cria uma instância do roteador do Express

//GET -> http://localhost:3000/api/:files
//Retorna Lista com todos os arquivos do tipo especificado
router.get('/files', async (req, res) => { //Uma rota GET que atenderá por /api/files e retornará uma lista de todos os arquivos armazenados no banco de dados
    try {
        const [rows] = await db.execute( // atribui a rows o resultado da consulta ao banco de dados, que seleciona todos os registros da tabela uploads
            "SELECT * FROM uploads ORDER BY created_at DESC" // consulta SQL para selecionar todos os registros da tabela uploads, ordenados pela coluna created_at em ordem decrescente
        )
        res.status(200).json(rows) //responde com um status 200 (OK) e envia os dados dos arquivos em formato JSON
    } catch (error) {
        res.status(500).json({ error: error.message }) //Em caso de erro, responde com um status 500 (Erro Interno do Servidor) e envia uma mensagem de erro em formato JSON
    }
})

//POST -> http://localhost:3000/api/files
//BODY -> form-data -> key: file (file)
//Inserir a imagem
router.post('/files', upload.single('file'), async (req, res) => { //Rota POST que atenderá por /api/files e permitirá o upload de um único arquivo utilizando o middleware multer configurado em uploadConfig.js
    try {
        const { originalname, mimetype, size, path: filepath } = req.file //Extrai informações do arquivo enviado, como nome original, tipo MIME, tamanho e caminho do arquivo no servidor

        let type = 'other' //Determina o tipo do arquivo com base no seu MIME type
        if (mimetype.startsWith('image/')) type = 'image' // Se o tipo MIME começar com "image/", define o tipo como "image"
        else if (mimetype === 'application/pdf') type = 'document' // Se o tipo MIME for "application/pdf", define o tipo como "document"
        else if (mimetype.startsWith('video/')) type = 'video' // Se o tipo MIME começar com "video/", define o tipo como "video"

    await db.execute( //Aguarda a execução da consulta SQL para inserir um novo registro na tabela uploads com as informações do arquivo enviado
        "INSERT INTO uploads (path, type, original_name, mime_type, size) VALUES (?, ?, ?, ?, ?)", // Consulta SQL para inserir um novo registro na tabela uploads
        [filepath, type, originalname, mimetype, size] // Valores a serem inseridos na consulta SQL (size é em bytes)
    )

    res.status(201).json({ //Retorna uma resposta com status 201, indicando que deu certo
        message: `Arquivo ${type} enviado com sucesso!`, file: { filepath, originalname, mimetype, size }, //Envia uma mensagem com detalhes do arquivo enviado
    })
    } catch (error) {
        res.status(500).json({ error: error.message }) //Em caso de erro, retorna uma resposta com status 500 e a mensagem de erro
    }
})

//PUT -> http://localhost:3000/api/files/:id
//BODY -> form-data -> key: file (file)
//Atualiza a imagem pelo ID
router.put('/files/:id', upload.single('file'), async (req, res) => { //Rota PUT que atenderá por /api/files/:id e permitirá a atualização de um arquivo existente identificado pelo seu ID
    try {
        const { id } = req.params //Extrai o ID do arquivo a ser atualizado a partir dos parâmetros da URL
        const newFile = req.file // Obtém o novo arquivo enviado no corpo da requisição

    const [old] = await db.execute("SELECT * FROM uploads WHERE id = ?", [id]) //Consulta o banco de dados para obter o registro do arquivo antigo com base no ID fornecido
        if (old.length === 0) //Se nenhum registro for encontrado, retorna um status 404 (Não Encontrado) com uma mensagem apropriada
            return res.status(404).json({ message: 'Arquivo não encontrado' }) //Mensagem de arquivo não encontrado

    const oldPath = old[0].path //Obtém o caminho do arquivo antigo a partir do registro retornado pelo banco de dados

    await db.execute( //Aguarda a execução da consulta SQL para prosseguir
        "UPDATE uploads SET path = ?, mime_type = ?, original_name = ?, size = ? WHERE id = ?", //Consulta SQL para atualizar o registro do arquivo no banco de dados com as informações do novo arquivo
        [newFile.path, newFile.mimetype, newFile.originalname, newFile.size, id] //Valores a serem atualizados na consulta SQL
    )

    fs.unlink(oldPath, (err) => { // Comando que irá deletar automaticamente o arquivo antigo assim que atualizar o novo no banco de dados
        if (err) console.warn('Erro ao deletar arquivo antigo:', err) //Se ocorrer um erro ao deletar o arquivo antigo, registra um aviso no console
    })

    res.status(200).json({ message: 'Arquivo atualizado com sucesso!', file: newFile.path }) //Tudo ocorrendo bem, retorna uma resposta com status 200 e uma mensagem de sucesso junto com o caminho do novo arquivo
    } catch (error) {
        res.status(500).json({ error: error.message }) //Em caso de erro, retorna uma resposta com status 500 e a mensagem de erro
    }
})

//DELETE -> http://localhost:3000/api/files/:id
//Deleta a imagem pelo ID
router.delete('/files/:id', async (req, res) => { //Rota DELETE que atenderá por /api/files/:id e permitirá a exclusão de um arquivo existente identificado pelo seu ID
    try {
        const { id } = req.params //Extrai o ID do arquivo a ser excluído a partir dos parâmetros da URL
        const [rows] = await db.execute("SELECT * FROM uploads WHERE id = ?", [id]) //Consulta o banco de dados para obter o registro do arquivo com base no ID fornecido

        if (rows.length === 0) return res.status(404).json({ message: 'Arquivo não encontrado' }) //Se nenhum registro for encontrado, retorna um status 404 (Não Encontrado) com uma mensagem apropriada

    const filePath = rows[0].path //Obtém o caminho do arquivo a partir do registro retornado pelo banco de dados

    await db.execute("DELETE FROM uploads WHERE id = ?", [id]) //Aguarda a execução da consulta SQL para excluir o registro do arquivo no banco de dados com base no ID fornecido

    fs.unlink(filePath, (err) => { //Comando que irá deletar o arquivo do sistema de arquivos do servidor
    if (err) console.warn('Erro ao deletar arquivo:', err) //Se ocorrer um erro ao deletar o arquivo, registra um aviso no console
    })

    res.status(200).json({ message: 'Arquivo deletado com sucesso!' }) //Tudo ocorrendo bem, retorna uma resposta com status 200 e uma mensagem de sucesso
    } catch (error) {
    res.status(500).json({ error: error.message }) //Em caso de erro, retorna uma resposta com status 500 e a mensagem de erro
    }
})  

export default router //Exporta o roteador para ser utilizado em outros arquivos
