import express from 'express' //IMPORTA O EXPRESS
import cors from 'cors' //IMPORTA O CORS
import dotenv from 'dotenv' //IMPORTA O DOTENV
import userRoutes from './routes/userRoutes.js' //IMPORTA AS ROTAS

const app = express() //CRIA UMA INSTÂNCIA DO EXPRESS
dotenv.config() //HABILITA O DOTENV

app.use(express.json()) //HABILITA O EXPRESS PARA LER JSON
app.use(cors()) //HABILITA O CORS
app.use('/api', userRoutes) //DEFINE O PREFIXO DAS ROTAS

//INICIA O SERVIDOR

const PORT = process.env.PORT || 3000 //DEFINE A PORTA DO SERVIDOR
app.listen(PORT, () => { //INICIA O SERVIDOR
    console.log(`Servidor rodando tranquilo na porta ${PORT}`) //MENSAGEM DE SUCESSO
})
