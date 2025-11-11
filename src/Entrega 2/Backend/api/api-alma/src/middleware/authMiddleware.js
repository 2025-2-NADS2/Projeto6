import { verifyToken } from '../modules/auth/tokenUtils.js' // Importa a função verifyToken, usada para validar o token JWT
import { denyToken } from '../modules/auth/tokenUtils.js' // Importa a função denyToken, usada para revogar tokens
import jwt from 'jsonwebtoken' // Importa jwt para decodificar tokens no logout

// ======================================================================
// ✅ Middleware de autenticação
// Verifica se o usuário enviou um token válido antes de acessar rotas protegidas
// ======================================================================
export const authMiddleware = async (req, res, next) => {

    const authHeader = req.headers.authorization // Obtém o cabeçalho Authorization da requisição

    if (!authHeader || !authHeader.startsWith('Bearer ')) { 
        // Verifica se o cabeçalho existe e segue o formato correto
        return res.status(401).json({ message: 'Token ausente ou inválido' }) 
    }

    const token = authHeader.split(' ')[1] // Extrai o token após "Bearer "

    try {
        const decoded = await verifyToken(token) // Valida o token e retorna os dados decodificados

        req.user = decoded // Armazena os dados do usuário na requisição para uso futuro (como req.user.id, req.user.role)

        next() // Avança para o próximo middleware ou controlador
    } catch (error) {

        // Se o token estiver revogado, verifyToken lança o erro "Token denylisted"
        const status = error.message === 'Token denylisted' ? 401 : 403 

        return res.status(status).json({ message: 'Token inválido ou expirado' })
    }
}



// ======================================================================
// ✅ Função de Logout
// Revoga o token atual do usuário, impedindo seu uso futuro
// ======================================================================
export const logout = async (req, res) => {

    const authHeader = req.headers.authorization // Obtém o cabeçalho Authorization

    if (!authHeader) 
        return res.status(400).json({ message: 'Token ausente' }) // Se não houver token, retorna erro

    const token = authHeader.split(' ')[1] // Extrai o token
    const decoded = jwt.decode(token) // Decodifica o token (não valida, apenas lê o conteúdo)

    if (!decoded || !decoded.jti) {
        // Proteção extra caso o token esteja malformado
        return res.status(400).json({ message: 'Token inválido' })
    }

    denyToken(decoded.jti) // Revoga o token adicionando-o à denylist

    return res.status(200).json({ message: 'Logout realizado com sucesso' }) // Resposta final ao cliente
}
