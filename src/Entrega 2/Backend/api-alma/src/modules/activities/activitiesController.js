// src/modules/activities/activitiesController.js - AJUSTES POSTGRESQL
import { v4 as uuidv4 } from 'uuid';
import db from '../../config/db.js';
import { DBCompat } from '../../utils/dbCompat.js';
import { logAction } from '../../utils/logUtils.js';

// ✅ LISTAR TODAS AS ATIVIDADES (Ajustado para PostgreSQL)
export const getAllActivities = async (req, res) => {
  try {
    console.log('🔍 Buscando atividades...');
    
    const result = await db.execute(
      'SELECT * FROM activities ORDER BY created_at DESC'
    );
    
    const activities = DBCompat.getRows(result);
    
    console.log(`✅ Encontradas ${activities.length} atividades`);
    res.json(activities);
    
  } catch (error) {
    console.error('❌ Erro em getAllActivities:', error);
    res.status(500).json({ 
      error: 'Erro interno do servidor',
      message: error.message 
    });
  }
};

// ✅ BUSCAR ATIVIDADE POR ID (Ajustado para PostgreSQL)
export const getActivityById = async (req, res) => {
  try {
    const { id } = req.params;
    console.log('🔍 Buscando atividade:', id);

    const result = await db.execute(
      'SELECT * FROM activities WHERE id = $1',
      [id]
    );

    const activity = DBCompat.getFirstRow(result);

    if (!activity) {
      return res.status(404).json({ message: 'Atividade não encontrada' });
    }

    res.json(activity);
    
  } catch (error) {
    console.error('❌ Erro em getActivityById:', error);
    res.status(500).json({ 
      error: 'Erro ao buscar atividade',
      message: error.message 
    });
  }
};

// ✅ CRIAR ATIVIDADE (Admin) - Ajustado para PostgreSQL
export const createActivity = async (req, res) => {
  try {
    const { title, description } = req.body;

    if (!title || !description) {
      return res.status(400).json({ 
        message: 'Título e descrição são obrigatórios' 
      });
    }

    if (!req.file) {
      return res.status(400).json({ 
        message: 'Imagem é obrigatória' 
      });
    }

    const id = uuidv4();
    const imagePath = req.file.path;

    await db.execute(
      'INSERT INTO activities (id, title, description, image_path) VALUES ($1, $2, $3, $4)',
      [id, title, description, imagePath]
    );

    if (req.user) {
      await logAction(req.user.id, 'create_activity', 'activities', { id, title });
    }

    res.status(201).json({ 
      message: 'Atividade criada com sucesso!',
      id: id
    });
    
  } catch (error) {
    console.error('❌ Erro em createActivity:', error);
    res.status(500).json({ 
      error: 'Erro ao criar atividade',
      message: error.message 
    });
  }
};

// ✅ ATUALIZAR ATIVIDADE (Admin) - Ajustado para PostgreSQL
export const updateActivity = async (req, res) => {
  try {
    const { id } = req.params;
    const { title, description } = req.body;

    const result = await db.execute(
      'SELECT * FROM activities WHERE id = $1',
      [id]
    );

    const existingActivity = DBCompat.getFirstRow(result);
    
    if (!existingActivity) {
      return res.status(404).json({ message: 'Atividade não encontrada' });
    }

    let newImagePath = existingActivity.image_path;

    if (req.file) {
      // Remover imagem antiga se existir
      if (existingActivity.image_path) {
        const fs = await import('fs');
        fs.unlink(existingActivity.image_path, () => {});
      }
      newImagePath = req.file.path;
    }

    await db.execute(
      'UPDATE activities SET title = $1, description = $2, image_path = $3 WHERE id = $4',
      [title, description, newImagePath, id]
    );

    if (req.user) {
      await logAction(req.user.id, 'update_activity', 'activities', { id });
    }

    res.json({ message: 'Atividade atualizada com sucesso!' });
    
  } catch (error) {
    console.error('❌ Erro em updateActivity:', error);
    res.status(500).json({ 
      error: 'Erro ao atualizar atividade',
      message: error.message 
    });
  }
};

// ✅ DELETAR ATIVIDADE (Admin) - Ajustado para PostgreSQL
export const deleteActivity = async (req, res) => {
  try {
    const { id } = req.params;

    const result = await db.execute(
      'SELECT * FROM activities WHERE id = $1',
      [id]
    );

    const activity = DBCompat.getFirstRow(result);
    
    if (!activity) {
      return res.status(404).json({ message: 'Atividade não encontrada' });
    }

    if (activity.image_path) {
      const fs = await import('fs');
      fs.unlink(activity.image_path, () => {});
    }

    await db.execute('DELETE FROM activities WHERE id = $1', [id]);

    if (req.user) {
      await logAction(req.user.id, 'delete_activity', 'activities', { id });
    }

    res.json({ message: 'Atividade deletada com sucesso!' });
    
  } catch (error) {
    console.error('❌ Erro em deleteActivity:', error);
    res.status(500).json({ 
      error: 'Erro ao deletar atividade',
      message: error.message 
    });
  }
};