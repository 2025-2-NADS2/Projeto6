// src/middleware/requestLogger.js - Log de requisições
import { logger } from '../utils/logger.js';

export const requestLogger = (req, res, next) => {
  const start = Date.now();
  
  // Log da requisição recebida
  logger.info('Request received', {
    method: req.method,
    url: req.url,
    ip: req.ip,
    userAgent: req.get('User-Agent')
  });

  // Capturar resposta
  res.on('finish', () => {
    const duration = Date.now() - start;
    
    logger.info('Response sent', {
      method: req.method,
      url: req.url,
      statusCode: res.statusCode,
      duration: `${duration}ms`,
      contentLength: res.get('Content-Length')
    });
  });

  next();
};

// Middleware de erro com logging
export const errorLogger = (error, req, res, next) => {
  logger.error('Unhandled error', error);
  
  // Em produção, não enviar stack trace
  const response = {
    error: 'Internal server error',
    message: process.env.NODE_ENV === 'production' 
      ? 'Something went wrong' 
      : error.message
  };

  res.status(500).json(response);
};