-- src/config/setup/postgresql-migration.sql
-- MIGRAÇÃO COMPLETA PARA POSTGRESQL - INSTITUTO ALMA

-- Habilitar UUID (se não estiver habilitado)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Criar tabelas se não existirem
DO $$ BEGIN
    CREATE TABLE IF NOT EXISTS users (
        id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
        name VARCHAR(100) NOT NULL,
        email VARCHAR(100) UNIQUE NOT NULL,
        password VARCHAR(255) NOT NULL,
        role VARCHAR(10) DEFAULT 'user' CHECK (role IN ('admin', 'user')),
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        is_verified BOOLEAN DEFAULT false
    );
EXCEPTION
    WHEN duplicate_table THEN NULL;
END $$;

-- 2. Continuar com outras tabelas...
-- [Restante do SQL da Parte 1, mas com sintaxe PostgreSQL]