import db from '../config/db.js' //IMPORTA A CONEXÃO COM O BANCO DE DADOS

//CRIAR USUARIO
export const createUser = async (req, res) => {//INICIO DA FUNÇÃO PARA CRIAR UM NOVO USUÁRIO
    const {nome, email} = req.body //FUNÇÃO PARA PEGAR O NOME E EMAIL DO CORPO DA REQUISIÇÃO
    if(!nome || !email){ //VERIFICA SE O CAMPO POSSUI NOME E EMAIL
        return res.status(400).json({error: 'Faltam campos obrigatórios: nome e email'})//RETORNA O ERRO 400 (REQUISIÇÃO INVÁLIDA)
    }
    try { //INICIO DA TENTATIVA DE CRIAÇÃO DO USUÁRIO
        const [result] = await db.execute( //EXECUTA A QUERY PARA INSERIR UM NOVO USUÁRIO
            'INSERT INTO users (nome, email) VALUES (?,?)', //COMANDO SQL
            [nome, email] //VALORES A SEREM INSERIDOS (PROTEGIDO CONTRA SQL INJECTION
        )
        res.status(201).json({id: result.insertId, nome, email}) //RETORNA O RESULTADO DA REQUISIÇÃO
    } catch (error) { //INICIO DA TRATATIVA DE ERROS
        if (error.code === 'ER_DUP_ENTRY') { //VERIFICA SE O ERRO É DE ENTRADA DUPLICADA (EMAIL JÁ CADASTRADO)
            return res.status(409).json({error: 'Email já cadastrado'}) //RETORNA O ERRO 409 (CONFLITO)
        }
        res.status(500).json({error: 'Erro ao criar usuario'}) //RETORNA O ERRO 500 (ERRO INTERNO DO SERVIDOR
    }
}

//LER TODOS USUARIOS
export const readUsers = async (req, res) => { //INICIO DA FUNÇÃO PARA LER TODOS OS USUÁRIOS
    try { //INICIO DA TENTATIVA DE LEITURA DOS USUÁRIOS
        const [users] = await db.execute('SELECT * FROM users')//FUNÇÃO PARA EXECUTAR A QUERY DE SELEÇÃO DE TODOS OS USUÁRIOS
        res.status(200).json(users)//RETORNA O RESULTADO DA REQUISIÇÃO
    } catch (error) {//INICIO DA TRATATIVA DE ERROS
        res.status(500).json({error: 'Erro ao buscar usuários'}) //RETORNA O ERRO 500 (ERRO INTERNO DO SERVIDOR)
    }
}

//LER USUARIO ESPECIFICO
export const readUser = async (req, res) => {//INICIO DA FUNÇÃO PARA LER UM USUÁRIO ESPECÍFICO
    const {id} =req.params//FUNÇÃO PARA PEGAR O ID DO USUÁRIO DOS PARÂMETROS DA REQUISIÇÃO
    try{//INICIO DA TENTATIVA DE LEITURA DO USUÁRIO
        const [rows] = await db.execute(//FUNÇÃO PARA EXECUTAR A QUERY DE SELEÇÃO DO USUÁRIO PELO ID
            'SELECT id, nome, email, created_at FROM users WHERE id = ?',//COMANDO SQL
            [id]//VALOR DO ID A SER PESQUISADO (PROTEGIDO CONTRA SQL INJECTION
        )
        if(rows.length === 0){//VERIFICA SE O USUÁRIO FOI ENCONTRADO
            return res.status(404).json({error: 'Usuario não encontrado'})//RETORNA O ERRO 404 (NÃO ENCONTRADO)
        }
        res.json(rows[0])//RETORNA O RESULTADO DA REQUISIÇÃO
    } catch(error){//INICIO DA TRATATIVA DE ERROS
        res.status(500).json({error: 'Erro ao buscar usuário'})//RETORNA O ERRO 500 (ERRO INTERNO DO SERVIDOR)
    }
}


export const updateUser = async (req, res) => {//INICIO DA FUNÇÃO PARA ATUALIZAR UM USUÁRIO
    const {id} = req.params//FUNÇÃO PARA PEGAR O ID DO USUÁRIO DOS PARÂMETROS DA REQUISIÇÃO
    const {nome, email} = req.body//FUNÇÃO PARA PEGAR O NOME E EMAIL DO CORPO DA REQUISIÇÃO
    if(!nome || !email){//VERIFICA SE O CAMPO POSSUI NOME E EMAIL
        return res.status(400).json({error: 'Faltam campos obrigatórios: nome e email'})//RETORNA O ERRO 400 (REQUISIÇÃO INVÁLIDA)
    }
    try {//INICIO DA TENTATIVA DE ATUALIZAÇÃO DO USUÁRIO
        const [result] = await db.execute(//EXECUTA A QUERY PARA ATUALIZAR O USUÁRIO
            'UPDATE users SET nome = ?, email = ? WHERE id = ?',//COMANDO SQL
            [nome, email, id]//VALORES A SEREM ATUALIZADOS (PROTEGIDO CONTRA SQL INJECTION
        )
        if (result.affectedRows === 0) {//VERIFICA SE O USUÁRIO FOI ENCONTRADO
            return res.status(404).json({error: 'Usuario não encontrado'})//RETORNA O ERRO 404 (NÃO ENCONTRADO
        }
        const [rows] = await db.execute(//FUNÇÃO PARA EXECUTAR A QUERY DE SELEÇÃO DO USUÁRIO PELO ID
            'SELECT id, nome, email, created_at FROM users WHERE id = ?',//COMANDO SQL
            [id]//VALOR DO ID A SER PESQUISADO (PROTEGIDO CONTRA SQL INJECTION
        )
        res.status(200).json({mensagem: 'Usuario atualizado com sucesso'})//RETORNA O RESULTADO DA REQUISIÇÃO
    } catch (error) {//INICIO DA TRATATIVA DE ERROS
        if(error.code === 'ER_DUP_ENTRY'){//VERIFICA SE O ERRO É DE ENTRADA DUPLICADA (EMAIL JÁ CADASTRADO)
            return res.status(409).json({error: 'Email já cadastrado'})//RETORNA O ERRO 409 (CONFLITO)
        }
        res.status(500).json({error: 'Erro ao atualizar usuario'})//RETORNA O ERRO 500 (ERRO INTERNO DO SERVIDOR)
    }
}

export const deleteUser = async (req, res) => {//INICIO DA FUNÇÃO PARA DELETAR UM USUÁRIO
    const {id} = req.params//FUNÇÃO PARA PEGAR O ID DO USUÁRIO DOS PARÂMETROS DA REQUISIÇÃO
    try {//INICIO DA TENTATIVA DE DELEÇÃO DO USUÁRIO
        const [result] = await db.execute(//EXECUTA A QUERY PARA DELETAR O USUÁRIO
            'DELETE FROM users WHERE id = ?', [Number(id)]//COMANDO SQL E VALOR DO ID A SER DELETADO (PROTEGIDO CONTRA SQL INJECTION
        )
        if (result.affectedRows === 0) {//VERIFICA SE O USUÁRIO FOI ENCONTRADO
            return res.status(404).json({error: 'Usuario não encontrado'})//RETORNA O ERRO 404 (NÃO ENCONTRADO
        }
        res.status(200).json({mensagem: 'Usuario deletado com sucesso'})//RETORNA O RESULTADO DA REQUISIÇÃO
    } catch (error) {//INICIO DA TRATATIVA DE ERROS
        res.status(500).json({error: 'Erro ao deletar usuario'})//RETORNA O ERRO 500 (ERRO INTERNO DO SERVIDOR
    }
}