// app-clean.js - VERSÃO LIMPA
import express from 'express'
const app = express()

app.use(express.json())

// ROTA 100% PÚBLICA
app.post('/api/clean-feedback', async (req, res) => {
  console.log('✅ CLEAN - Recebido:', req.body);
  res.json({ 
    success: true, 
    message: 'Funciona perfeitamente!',
    data: req.body 
  })
})

app.listen(3000, () => {
  console.log('🚀 App limpo rodando na porta 3000');
})