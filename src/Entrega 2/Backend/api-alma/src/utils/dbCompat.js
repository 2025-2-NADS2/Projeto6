// src/utils/dbCompat.js - VERSÃO OTIMIZADA PARA POSTGRESQL
/**
 * Compatibilidade completa entre MySQL e PostgreSQL
 * Foco em PostgreSQL para Render
 */

export class DBCompat {
  static getRows(result) {
    // PostgreSQL: result.rows, MySQL: result[0]
    if (result.rows !== undefined) return result.rows;
    if (Array.isArray(result) && result.length >= 1) return result[0];
    return [];
  }

  static getFirstRow(result) {
    const rows = this.getRows(result);
    return rows && rows.length > 0 ? rows[0] : null;
  }

  static getInsertId(result, tableName = 'id') {
    // PostgreSQL - retorna a linha inserida com RETURNING
    if (result.rows && result.rows[0] && result.rows[0][tableName]) {
      return result.rows[0][tableName];
    }
    if (result.rows && result.rows[0] && result.rows[0].id) {
      return result.rows[0].id;
    }
    // MySQL - retorna o insertId
    if (result[0] && result[0].insertId) {
      return result[0].insertId;
    }
    return null;
  }

  static getAffectedRows(result) {
    // PostgreSQL
    if (result.rowCount !== undefined) {
      return result.rowCount;
    }
    // MySQL
    if (result[0] && result[0].affectedRows !== undefined) {
      return result[0].affectedRows;
    }
    return 0;
  }

  // Gerar placeholder baseado no banco
  static getPlaceholder(index) {
    // PostgreSQL usa $1, $2, $3...
    return process.env.DATABASE_URL ? `$${index + 1}` : '?';
  }

  // Preparar query com placeholders corretos
  static prepareQuery(sql, values = []) {
    if (!process.env.DATABASE_URL) return { sql, values };
    
    // Converter ? para $1, $2, etc.
    let paramIndex = 0;
    const convertedSQL = sql.replace(/\?/g, () => {
      paramIndex++;
      return `$${paramIndex}`;
    });
    
    return { sql: convertedSQL, values };
  }

  // Executar query de forma compatível
  static async executeQuery(db, sql, values = []) {
    const { sql: preparedSQL, values: preparedValues } = this.prepareQuery(sql, values);
    return await db.execute(preparedSQL, preparedValues);
  }
}