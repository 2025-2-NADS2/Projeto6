// src/server.js - VERSÃO PRODUÇÃO
import 'dotenv/config';
import app from './app.js';
import { logger } from './utils/logger.js';

const port = process.env.PORT || 10000;

// Graceful shutdown handling
const gracefulShutdown = (signal) => {
  logger.info(`Received ${signal}, shutting down gracefully`);
  
  setTimeout(() => {
    logger.info('Forcefully shutting down');
    process.exit(1);
  }, 10000).unref();

  server.close(() => {
    logger.info('HTTP server closed');
    process.exit(0);
  });
};

// Listen for termination signals
process.on('SIGTERM', () => gracefulShutdown('SIGTERM'));
process.on('SIGINT', () => gracefulShutdown('SIGINT'));

// Uncaught exception handler
process.on('uncaughtException', (error) => {
  logger.error('Uncaught Exception', error);
  process.exit(1);
});

// Unhandled promise rejection handler
process.on('unhandledRejection', (reason, promise) => {
  logger.error('Unhandled Rejection', { reason, promise });
  process.exit(1);
});

const server = app.listen(port, '0.0.0.0', () => {
  logger.info(`🚀 Servidor rodando na porta ${port}`);
  logger.info(`🌐 Ambiente: ${process.env.NODE_ENV || 'development'}`);
  logger.info(`📊 Health check: http://localhost:${port}/health`);
});

export default server;