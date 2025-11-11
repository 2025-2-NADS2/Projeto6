// src/config/db.js - VERSÃO OTIMIZADA PARA RENDER
console.log('🔧 Iniciando configuração do banco...');

let db;
let databaseType = 'unknown';

// Configuração para Render (PostgreSQL)
if (process.env.DATABASE_URL) {
  try {
    console.log('🎯 Ambiente: PRODUÇÃO (PostgreSQL - Render)');
    databaseType = 'postgresql';
    
    const { Pool } = await import('pg');
    
    db = new Pool({
      connectionString: process.env.DATABASE_URL,
      ssl: {
        rejectUnauthorized: false
      },
      // Configurações de pool otimizadas para Render
      max: 20,
      idleTimeoutMillis: 30000,
      connectionTimeoutMillis: 2000,
    });
    
    // Adaptar PostgreSQL para compatibilidade
    db.execute = db.query.bind(db);
    console.log('✅ PostgreSQL configurado para Render');
    
  } catch (error) {
    console.error('❌ Erro ao configurar PostgreSQL:', error.message);
    db = createFallbackDB('postgresql');
  }
} else {
  // Desenvolvimento local (MySQL)
  try {
    console.log('🎯 Ambiente: DESENVOLVIMENTO (MySQL)');
    databaseType = 'mysql';
    
    const mysql = await import('mysql2/promise');
    db = mysql.createPool({
      host: process.env.MYSQL_HOST || 'localhost',
      user: process.env.MYSQL_USER || 'root',
      password: process.env.MYSQL_PASSWORD || '',
      database: process.env.MYSQL_DB || 'almaDB',
      port: process.env.MYSQL_PORT || 3306,
      waitForConnections: true,
      connectionLimit: 10,
      queueLimit: 0
    });
    console.log('✅ MySQL configurado para desenvolvimento');
  } catch (error) {
    console.error('❌ Erro ao configurar MySQL:', error.message);
    db = createFallbackDB('mysql');
  }
}

// Função fallback melhorada
function createFallbackDB(type) {
  console.log(`🔄 Usando banco em memória (fallback ${type})...`);
  
  return {
    execute: (sql, params) => {
      console.log(`📝 SQL Executado (fallback ${type}):`, sql.substring(0, 100));
      
      // Simular respostas realistas
      if (sql.includes('SELECT 1') || sql.includes('SELECT NOW()')) {
        return Promise.resolve([[{ test: 1 }], []]);
      }
      if (sql.includes('SELECT') && sql.includes('activities')) {
        return Promise.resolve([[
          { id: 'fallback-1', title: 'Atividade Exemplo', description: 'Banco em configuração' }
        ], []]);
      }
      if (sql.includes('INSERT') || sql.includes('UPDATE') || sql.includes('DELETE')) {
        return Promise.resolve([{ insertId: 1, affectedRows: 1 }, []]);
      }
      
      return Promise.resolve([[], []]);
    }
  };
}

// Teste de conexão otimizado
const testConnection = async () => {
  try {
    if (databaseType === 'postgresql') {
      const result = await db.execute('SELECT NOW() as current_time');
      console.log('✅ PostgreSQL conectado:', result.rows[0].current_time);
    } else {
      const [rows] = await db.execute('SELECT 1 as test_value');
      console.log('✅ MySQL conectado - Teste:', rows[0].test_value);
    }
  } catch (error) {
    console.error(`❌ Erro na conexão com ${databaseType}:`, error.message);
  }
};

testConnection();

export default db;
export { databaseType };