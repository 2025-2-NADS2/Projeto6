import { v4 as uuidv4 } from 'uuid'
import db from '../../config/db.js'
import { logAction } from '../../utils/logUtils.js'


// ===========================================================
// ✅ CRIAR DOAÇÃO
// ===========================================================
export const createDonation = async (req, res) => {
    try {
        // Extrai valores enviados pelo corpo da requisição
        const { amount, method } = req.body

        // Verifica se o valor é válido e maior que zero
        if (!amount || amount <= 0)
            return res.status(400).json({ message: 'Valor da doação inválido.' })

        // Métodos permitidos
        const validMethods = ['pix', 'card', 'transfer', 'qr']

        // Verifica se o método de pagamento existe
        if (!validMethods.includes(method))
            return res.status(400).json({ message: 'Método de pagamento inválido.' })

        // Usa ID do usuário caso esteja logado (opcional)
        const userId = req.user?.id || null

        // Cria identificador único da doação
        const donationId = uuidv4()

        // Token usado para rastrear transações externas de pagamento
        const transactionToken = uuidv4()

        // Insere doação no banco
        await db.execute(
            `INSERT INTO donations (id, user_id, amount, method, transaction_token, status)
            VALUES (?, ?, ?, ?, ?, 'pending')`,
            [donationId, userId, amount, method, transactionToken]
        )

        // Registra log (para auditoria)
        await logAction(userId, 'create_donation', 'donations', { donationId, amount, method })

        // Retorna resposta ao front-end
        res.status(201).json({
            message: 'Doação criada com sucesso.',
            donationId,
            payment_token: transactionToken,
            status: 'pending',
            next_step: 'Finalize o pagamento usando o token enviado.'
        })

    } catch (err) {
        return res.status(500).json({ error: err.message })
    }
}



// ===========================================================
// ✅ ADMIN – APROVAR PAGAMENTO (Simulação)
// ===========================================================
export const approveDonation = async (req, res) => {
    try {
        const { id } = req.params

        // Procura a doação no banco
        const [rows] = await db.execute(
            "SELECT * FROM donations WHERE id = ?",
            [id]
        )

        // Se não existir
        if (!rows.length)
            return res.status(404).json({ message: 'Doação não encontrada.' })

        // Atualiza status para "pago"
        await db.execute(
            "UPDATE donations SET status = 'paid' WHERE id = ?",
            [id]
        )

        // Registra log
        await logAction(req.user.id, 'approve_donation', 'donations', { id })

        res.json({ message: 'Pagamento aprovado com sucesso!' })

    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}


// ===========================================================
// ✅ LISTAR MINHAS DOAÇÕES
// ===========================================================
export const listMyDonations = async (req, res) => {
    try {
        const userId = req.user.id

        // Lista somente as doações do próprio usuário
        const [rows] = await db.execute(
            "SELECT id, amount, method, status, created_at FROM donations WHERE user_id = ? ORDER BY created_at DESC",
            [userId]
        )

        res.json(rows)

    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}


// ===========================================================
// ✅ ADMIN – LISTAR TODAS AS DOAÇÕES
// ===========================================================
export const listAllDonations = async (req, res) => {
    try {
        const [rows] = await db.execute(`
            SELECT d.*, u.email AS user_email
            FROM donations d
            LEFT JOIN users u ON u.id = d.user_id
            ORDER BY d.created_at DESC
        `)

        res.json(rows)

    } catch (err) {
        res.status(500).json({ error: err.message })
    }
}


// ===========================================================
// ✅ BUSCAR DOAÇÃO PELO ID
// ===========================================================
export const getDonationById = async (req, res) => {
    try {
        const { id } = req.params

        // Busca no banco
        const [rows] = await db.execute(
            "SELECT id, user_id, amount, method, status, created_at FROM donations WHERE id = ?",
            [id]
        )

        if (!rows.length)
            return res.status(404).json({ message: "Doação não encontrada." })

        res.json(rows[0])

    } catch (error) {
        res.status(500).json({ error: error.message })
    }
}
