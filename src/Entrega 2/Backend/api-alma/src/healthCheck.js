// src/healthCheck.js - VERSÃO AVANÇADA
import db from './config/db.js';
import { DBCompat } from './utils/dbCompat.js';
import { logger } from './utils/logger.js';

export const healthCheck = async (req, res) => {
  const healthCheck = {
    status: 'OK',
    timestamp: new Date().toISOString(),
    service: 'Instituto Alma API',
    version: '1.0.0',
    checks: {}
  };

  try {
    // Check de banco de dados
    const dbStart = Date.now();
    const dbResult = await db.execute('SELECT 1 as test_value, NOW() as db_time');
    const dbTest = DBCompat.getFirstRow(dbResult);
    healthCheck.checks.database = {
      status: dbTest ? 'OK' : 'ERROR',
      response_time: `${Date.now() - dbStart}ms`,
      timestamp: dbTest?.db_time
    };

    // Check de memória
    healthCheck.checks.memory = {
      used: `${(process.memoryUsage().heapUsed / 1024 / 1024).toFixed(2)}MB`,
      total: `${(process.memoryUsage().heapTotal / 1024 / 1024).toFixed(2)}MB`,
      rss: `${(process.memoryUsage().rss / 1024 / 1024).toFixed(2)}MB`
    };

    // Check de uptime
    healthCheck.checks.system = {
      uptime: `${process.uptime().toFixed(2)}s`,
      node_version: process.version,
      environment: process.env.NODE_ENV || 'development'
    };

    // Determinar status geral
    const allChecksOk = Object.values(healthCheck.checks).every(
      check => check.status === 'OK' || !check.status
    );

    if (!allChecksOk) {
      healthCheck.status = 'DEGRADED';
      return res.status(503).json(healthCheck);
    }

    logger.info('Health check passed', healthCheck);
    res.status(200).json(healthCheck);

  } catch (error) {
    healthCheck.status = 'ERROR';
    healthCheck.checks.database = {
      status: 'ERROR',
      error: error.message
    };
    
    logger.error('Health check failed', error);
    res.status(503).json(healthCheck);
  }
};