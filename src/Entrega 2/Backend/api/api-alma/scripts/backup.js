// scripts/backup.js - Backup automático do PostgreSQL
import { exec } from 'child_process';
import fs from 'fs';
import { logger } from '../src/utils/logger.js';

class DatabaseBackup {
  constructor() {
    this.backupDir = 'backups';
    this.ensureBackupDirectory();
  }

  ensureBackupDirectory() {
    if (!fs.existsSync(this.backupDir)) {
      fs.mkdirSync(this.backupDir, { recursive: true });
    }
  }

  createBackup() {
    return new Promise((resolve, reject) => {
      const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
      const backupFile = `${this.backupDir}/backup-${timestamp}.sql`;
      
      // Comando pg_dump para PostgreSQL
      const command = `pg_dump ${process.env.DATABASE_URL} > ${backupFile}`;
      
      exec(command, (error, stdout, stderr) => {
        if (error) {
          logger.error('Backup failed', error);
          reject(error);
        } else {
          logger.info('Backup created successfully', { file: backupFile });
          resolve(backupFile);
        }
      });
    });
  }

  // Limpar backups antigos (manter apenas últimos 7 dias)
  cleanupOldBackups() {
    const files = fs.readdirSync(this.backupDir);
    const now = Date.now();
    const sevenDaysAgo = now - (7 * 24 * 60 * 60 * 1000);

    files.forEach(file => {
      const filePath = `${this.backupDir}/${file}`;
      const stats = fs.statSync(filePath);
      
      if (stats.mtimeMs < sevenDaysAgo) {
        fs.unlinkSync(filePath);
        logger.info('Deleted old backup', { file });
      }
    });
  }
}

export const backupManager = new DatabaseBackup();

// Executar backup se chamado diretamente
if (import.meta.url === `file://${process.argv[1]}`) {
  backupManager.createBackup()
    .then(() => backupManager.cleanupOldBackups())
    .catch(console.error);
}