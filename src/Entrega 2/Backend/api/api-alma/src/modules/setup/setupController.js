// src/modules/setup/setupController.js - VERSÃO MELHORADA
import db from '../../config/db.js'
import { v4 as uuidv4 } from 'uuid'
import { DBCompat } from '../../utils/dbCompat.js'
import bcrypt from 'bcrypt'

export const setupDatabase = async (req, res) => {
  try {
    console.log('🔄 Iniciando configuração do banco PostgreSQL...')

    // 1. Verificar se estamos no PostgreSQL
    const dbCheck = await db.execute("SELECT version() AS db_version")
    const dbInfo = DBCompat.getFirstRow(dbCheck)
    console.log('📊 Banco:', dbInfo.db_version)

    // 2. Criar extensão UUID se for PostgreSQL
    try {
      await db.execute('CREATE EXTENSION IF NOT EXISTS "uuid-ossp"')
      console.log('✅ Extensão UUID habilitada')
    } catch (extError) {
      console.log('ℹ️  Extensão UUID já existe ou não é necessário')
    }

    // 3. SQL para PostgreSQL
    const tablesSQL = [
      // Tabela de usuários
      `CREATE TABLE IF NOT EXISTS users (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        name VARCHAR(100) NOT NULL,
        email VARCHAR(100) UNIQUE NOT NULL,
        password VARCHAR(255) NOT NULL,
        role VARCHAR(10) DEFAULT 'user' CHECK (role IN ('admin', 'user')),
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        is_verified BOOLEAN DEFAULT false
      )`,

      // Tabela de uploads
      `CREATE TABLE IF NOT EXISTS uploads (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        path VARCHAR(500) NOT NULL,
        type VARCHAR(50),
        original_name VARCHAR(255),
        mime_type VARCHAR(100),
        size BIGINT,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`,

      // Tabela de sessões
      `CREATE TABLE IF NOT EXISTS sessions (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        user_id UUID REFERENCES users(id) ON DELETE CASCADE,
        jti UUID NOT NULL,
        ip VARCHAR(45),
        user_agent TEXT,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        revoked_at TIMESTAMP NULL
      )`,

      // Tabela de auditoria
      `CREATE TABLE IF NOT EXISTS audit_logs (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        user_id UUID,
        action VARCHAR(100),
        resource VARCHAR(255),
        details JSONB,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`,

      // Tabela de atividades
      `CREATE TABLE IF NOT EXISTS activities (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        title VARCHAR(255) NOT NULL,
        description TEXT NOT NULL,
        image_path VARCHAR(255),
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`,

      // Tabela de transparência
      `CREATE TABLE IF NOT EXISTS transparency_files (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        upload_id UUID NOT NULL REFERENCES uploads(id) ON DELETE CASCADE,
        title VARCHAR(255) NOT NULL,
        description TEXT,
        is_public BOOLEAN DEFAULT true,
        created_by UUID,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`,

      // Tabela de doações
      `CREATE TABLE IF NOT EXISTS donations (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        user_id UUID REFERENCES users(id),
        amount DECIMAL(10,2) NOT NULL,
        method VARCHAR(10) CHECK (method IN ('pix','card','transfer','qr')),
        status VARCHAR(10) DEFAULT 'pending' CHECK (status IN ('pending','paid','failed')),
        transaction_token VARCHAR(200),
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`,

      // Tabela de feedback
      `CREATE TABLE IF NOT EXISTS feedback_messages (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        name VARCHAR(255) NOT NULL,
        email VARCHAR(255) NOT NULL,
        subject VARCHAR(255),
        message TEXT NOT NULL,
        status VARCHAR(10) DEFAULT 'new' CHECK (status IN ('new','read','archived')),
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`,

      // Tabela de eventos
      `CREATE TABLE IF NOT EXISTS events (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        title VARCHAR(255) NOT NULL,
        description TEXT NOT NULL,
        image_path VARCHAR(255),
        event_date DATE NOT NULL,
        location VARCHAR(255),
        social_link VARCHAR(500),
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NULL
      )`,

      // Tabela de reset de senha
      `CREATE TABLE IF NOT EXISTS password_resets (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        token VARCHAR(255) NOT NULL,
        expires_at TIMESTAMP NOT NULL,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`,

      // Tabela de tentativas de auth
      `CREATE TABLE IF NOT EXISTS auth_attempts (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        email VARCHAR(255),
        ip VARCHAR(50),
        user_agent VARCHAR(255),
        action VARCHAR(50),
        success BOOLEAN DEFAULT false,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      )`
    ]

    // Executar criação de tabelas
    for (const sql of tablesSQL) {
      try {
        await db.execute(sql)
        console.log(`✅ Tabela criada: ${sql.split(' ')[5]}`)
      } catch (tableError) {
        console.log(`ℹ️  Tabela já existe: ${sql.split(' ')[5]}`)
      }
    }

    // 4. Criar usuário admin padrão
    const adminPassword = await bcrypt.hash('Admin123!', 10)
    const adminId = uuidv4()
    
    try {
      await db.execute(
        `INSERT INTO users (id, name, email, password, role, is_verified) 
         VALUES ($1, $2, $3, $4, $5, $6) 
         ON CONFLICT (email) DO NOTHING`,
        [adminId, 'Administrador', 'admin@institutoalma.org', adminPassword, 'admin', true]
      )
      console.log('✅ Usuário admin criado')
    } catch (adminError) {
      console.log('ℹ️  Usuário admin já existe')
    }

    // 5. Inserir dados de exemplo
    const sampleDataSQL = [
      `INSERT INTO activities (id, title, description) VALUES
       ($1, 'Oficina de Artes', 'Oficina de pintura e desenho para crianças'),
       ($2, 'Aula de Música', 'Aulas de violão e canto para iniciantes')
       ON CONFLICT (id) DO NOTHING`,
      [uuidv4(), uuidv4()]
    ]

    await db.execute(...sampleDataSQL)
    console.log('✅ Dados de exemplo inseridos')

    res.json({ 
      success: true, 
      message: '🎉 Banco PostgreSQL configurado com sucesso!',
      database: 'PostgreSQL (Render)',
      tables: ['users', 'uploads', 'sessions', 'audit_logs', 'activities', 'transparency_files', 'donations', 'feedback_messages', 'events'],
      admin_user: 'admin@institutoalma.org',
      admin_password: 'Admin123!'
    })

  } catch (error) {
    console.error('❌ Erro no setup do banco:', error)
    res.status(500).json({ 
      success: false, 
      error: error.message,
      hint: 'Verifique a conexão com o PostgreSQL no Render'
    })
  }
}

// Teste de conexão melhorado
export const testDatabase = async (req, res) => {
  try {
    const result = await db.execute('SELECT NOW() as current_time, version() as db_version')
    const data = DBCompat.getFirstRow(result)
    
    res.json({
      success: true,
      database: process.env.DATABASE_URL ? 'PostgreSQL (Render)' : 'MySQL (Local)',
      current_time: data.current_time,
      db_version: data.db_version,
      status: '✅ Conexão com banco funcionando!'
    })
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
      status: '❌ Erro na conexão com o banco'
    })
  }
}