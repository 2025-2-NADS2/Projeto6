// src/modules/auth/authService.js - VERSÃO POSTGRESQL
import db from '../../config/db.js';
import bcrypt from 'bcrypt';
import { v4 as uuidv4 } from 'uuid';
import { DBCompat } from '../../utils/dbCompat.js';

export const createUser = async ({ name, email, password, role = 'user', is_verified = false }) => {
  const hashedPassword = await bcrypt.hash(password, 10);
  const id = uuidv4();

  // Inserir no PostgreSQL
  const result = await db.execute(
    `INSERT INTO users (id, name, email, password, role, is_verified)
     VALUES ($1, $2, $3, $4, $5, $6)
     RETURNING *`,
    [id, name, email, hashedPassword, role, is_verified]
  );

  // Retorna o usuário criado (usando DBCompat para compatibilidade)
  const user = DBCompat.getFirstRow(result);
  return { 
    id: user.id, 
    name: user.name, 
    email: user.email, 
    role: user.role, 
    is_verified: user.is_verified 
  };
};

export const findUserByEmail = async (email) => {
  const result = await db.execute(
    'SELECT * FROM users WHERE email = $1', 
    [email]
  );
  
  return DBCompat.getFirstRow(result);
};