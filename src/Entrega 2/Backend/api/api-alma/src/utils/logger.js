// src/utils/logger.js - Sistema de logs para produção
import fs from 'fs';
import path from 'path';

class Logger {
  constructor() {
    this.logDir = 'logs';
    this.ensureLogDirectory();
  }

  ensureLogDirectory() {
    if (!fs.existsSync(this.logDir)) {
      fs.mkdirSync(this.logDir, { recursive: true });
    }
  }

  getLogFilePath() {
    const date = new Date().toISOString().split('T')[0];
    return path.join(this.logDir, `app-${date}.log`);
  }

  writeLog(level, message, data = null) {
    const timestamp = new Date().toISOString();
    const logEntry = {
      timestamp,
      level,
      message,
      data,
      pid: process.pid
    };

    const logLine = JSON.stringify(logEntry) + '\n';
    
    // Escrever no arquivo
    fs.appendFileSync(this.getLogFilePath(), logLine, 'utf8');
    
    // Também logar no console em desenvolvimento
    if (process.env.NODE_ENV !== 'production') {
      console.log(`[${level.toUpperCase()}] ${message}`, data || '');
    }
  }

  info(message, data = null) {
    this.writeLog('info', message, data);
  }

  error(message, error = null) {
    this.writeLog('error', message, {
      error: error?.message,
      stack: error?.stack
    });
  }

  warn(message, data = null) {
    this.writeLog('warn', message, data);
  }

  debug(message, data = null) {
    if (process.env.NODE_ENV !== 'production') {
      this.writeLog('debug', message, data);
    }
  }
}

export const logger = new Logger();