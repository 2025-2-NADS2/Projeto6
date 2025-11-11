// deploy-setup.js - Script de configuração pós-deploy
import fs from 'fs';
import { execSync } from 'child_process';

console.log('🚀 Executando configuração pós-deploy...');

// Verificar ambiente
console.log('📊 Ambiente:', process.env.NODE_ENV);
console.log('🔗 Database URL:', process.env.DATABASE_URL ? 'Configurada' : 'Faltando');

// Criar estrutura de diretórios necessária
const createDirectories = () => {
  const directories = [
    'src/public/uploads/images',
    'src/public/uploads/documents',
    'src/public/uploads/videos',
    'src/public/uploads/others',
    'logs'
  ];

  directories.forEach(dir => {
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
      console.log(`✅ Criado: ${dir}`);
    }
  });
};

// Verificar variáveis críticas
const checkEnvironment = () => {
  const required = ['DATABASE_URL', 'JWT_SECRET', 'JWT_EMAIL_SECRET'];
  const missing = required.filter(key => !process.env[key]);
  
  if (missing.length > 0) {
    console.warn('⚠️  Variáveis faltando:', missing);
    return false;
  }
  
  console.log('✅ Variáveis de ambiente OK');
  return true;
};

// Executar setup do banco via API
const setupDatabase = async () => {
  try {
    console.log('🗃️  Iniciando setup do banco...');
    
    const response = await fetch(`http://localhost:${process.env.PORT || 10000}/api/setup/database`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    
    const result = await response.json();
    console.log('📊 Resultado do setup:', result.message);
    return result.success;
  } catch (error) {
    console.log('ℹ️  Setup via API falhou, continuando...');
    return true;
  }
};

// Main execution
const main = async () => {
  console.log('🔧 Iniciando configuração pós-deploy...\n');
  
  // 1. Criar diretórios
  createDirectories();
  
  // 2. Verificar ambiente
  if (!checkEnvironment()) {
    console.log('💡 Configure as variáveis no painel do Render');
  }
  
  // 3. Setup do banco (se possível)
  await setupDatabase();
  
  console.log('\n✅ Configuração pós-deploy concluída!');
  console.log('🌐 API deve estar disponível em:', `http://localhost:${process.env.PORT || 10000}`);
};

main().catch(console.error);