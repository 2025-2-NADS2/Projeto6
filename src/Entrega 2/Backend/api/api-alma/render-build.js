// build.js - Script de build para produção
import fs from 'fs';
import { execSync } from 'child_process';

console.log('🚀 Iniciando build para produção...');

// Verificar se estamos em produção
if (process.env.NODE_ENV !== 'production') {
  console.log('⚠️  Aviso: Este script é destinado ao ambiente de produção.');
}

// Criar diretórios necessários para uploads
const directories = [
  'src/public/uploads/images',
  'src/public/uploads/documents',
  'src/public/uploads/videos',
  'src/public/uploads/others'
];

directories.forEach(dir => {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
    console.log(`✅ Criado diretório: ${dir}`);
  }
});

// Verificar variáveis de ambiente críticas
const criticalEnvVars = ['DATABASE_URL', 'JWT_SECRET', 'JWT_EMAIL_SECRET'];
const missingVars = criticalEnvVars.filter(varName => !process.env[varName]);

if (missingVars.length > 0) {
  console.error('❌ Variáveis de ambiente críticas faltando:');
  missingVars.forEach(varName => {
    console.error(`   - ${varName}`);
  });
  console.log('💡 Configure-as no painel do Render.');
  process.exit(1);
} else {
  console.log('✅ Todas as variáveis críticas estão definidas.');
}

// Verificar se o banco está acessível
try {
  console.log('🔍 Testando conexão com o banco...');
  // Importar o db dinamicamente para testar
  const db = await import('./src/config/db.js');
  const result = await db.default.execute('SELECT 1 as test_value');
  console.log('✅ Conexão com o banco estabelecida.');
} catch (error) {
  console.error('❌ Erro na conexão com o banco:', error.message);
  console.log('💡 Verifique a DATABASE_URL no Render.');
  process.exit(1);
}

console.log('✅ Build concluído com sucesso!');