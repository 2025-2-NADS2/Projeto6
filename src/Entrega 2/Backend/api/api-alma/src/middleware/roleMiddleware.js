// exporta a função authorizeRoles que recebe uma lista de cargos permitidos
export const authorizeRoles = (...allowedRoles) => {

    // retorna o middleware que será usado nas rotas
    return (req, res, next) => {

        // se não houver objeto req.user, significa que o usuário não foi autenticado antes
        // (o authMiddleware deveria ter preenchido req.user com os dados do token)
        if (!req.user)
            return res.status(401).json({ error: 'Não autenticado '}) // 401 Unauthorized

        // verifica se o cargo do usuário (req.user.role) está dentro da lista de cargos permitidos
        // OBS: usamos "role" (singular) porque o token/payload contém normalmente { role: 'admin' }
        if (!allowedRoles.includes(req.user.role)) {
            // se o cargo não estiver na lista, retorna 403 Forbidden
            return res.status(403).json({ error: 'Acesso negado! '})
        }

        // se passou nas checagens, chama next() para continuar o fluxo (permitir acesso)
        next();
    }
}
