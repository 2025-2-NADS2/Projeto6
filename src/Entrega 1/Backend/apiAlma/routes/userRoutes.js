import express from 'express' //IMPORTA O EXPRESS
import { //IMPORTA AS FUNÇÕES DO CONTROLLER
    createUser, //FUNÇÃO PARA CRIAR UM NOVO USUÁRIO
    readUser, //FUNÇÃO PARA LER UM USUÁRIO ESPECÍFICO
    updateUser, //FUNÇÃO PARA ATUALIZAR UM USUÁRIO
    deleteUser, //FUNÇÃO PARA DELETAR UM USUÁRIO
    readUsers //FUNÇÃO PARA LER TODOS OS USUÁRIOS
} from '../controllers/userController.js' //IMPORTA O ARQUIVO CONTROLLER

const router = express.Router() //CRIA UMA INSTÂNCIA DO ROUTER

//DEFINE AS ROTAS E OS MÉTODOS HTTP

router.post('/doador', createUser)//Rota para criar um novo usuário
router.get('/doador/:id', readUser)//Rota para ler um usuário específico
router.get('/doadores', readUsers)//Rota para ler todos os usuários
router.put('/doador/:id', updateUser)//Rota para atualizar um usuário
router.delete('/doador/:id', deleteUser)//Rota para deletar um usuário

export default router //EXPORTA O ROUTER
