import mysql from 'mysql2' //IMPORTA O MYSQL2
import dotenv from 'dotenv' //IMPORTA O DOTENV

dotenv.config() //HABILITA O DOTENV

//CRIA A CONEXÃO COM O BANCO DE DADOS
const pool = mysql.createPool({
    host: process.env.DB_HOST,
    user: process.env.DB_USER,
    password: process.env.DB_PASSWORD,
    database: process.env.DB_NAME,
    port: process.env.DB_PORT
})

//EXPORTA A CONEXÃO COM O BANCO DE DADOS
export default pool.promise()
