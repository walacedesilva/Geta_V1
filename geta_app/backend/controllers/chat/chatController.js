const db = require('../../config/db');

// Buscar todas as conversas de um usuário
const getUserConversations = async (userId) => {
  try {
    const result = await db.query(
      `SELECT c.id, c.created_at, 
        (SELECT json_agg(json_build_object('id', u.id, 'nome', u.nome, 'email', u.email))
         FROM conversation_participants cp
         JOIN usuarios u ON cp.user_id = u.id
         WHERE cp.conversation_id = c.id AND cp.user_id != $1) as participants,
        (SELECT content FROM messages 
         WHERE conversation_id = c.id 
         ORDER BY created_at DESC LIMIT 1) as last_message,
        (SELECT created_at FROM messages 
         WHERE conversation_id = c.id 
         ORDER BY created_at DESC LIMIT 1) as last_message_time
      FROM conversations c
      JOIN conversation_participants cp ON c.id = cp.conversation_id
      WHERE cp.user_id = $1
      ORDER BY last_message_time DESC NULLS LAST`,
      [userId]
    );
    
    return result.rows;
  } catch (error) {
    console.error('Erro ao buscar conversas do usuário:', error);
    throw error;
  }
};

// Buscar mensagens de uma conversa específica
const getConversationMessages = async (conversationId, limit = 50, offset = 0) => {
  try {
    const result = await db.query(
      `SELECT m.id, m.content, m.created_at, m.sender_id,
        json_build_object('id', u.id, 'nome', u.nome) as sender
      FROM messages m
      JOIN usuarios u ON m.sender_id = u.id
      WHERE m.conversation_id = $1
      ORDER BY m.created_at DESC
      LIMIT $2 OFFSET $3`,
      [conversationId, limit, offset]
    );
    
    return result.rows;
  } catch (error) {
    console.error('Erro ao buscar mensagens da conversa:', error);
    throw error;
  }
};

// Verificar se o usuário é participante da conversa
const isConversationParticipant = async (userId, conversationId) => {
  try {
    const result = await db.query(
      'SELECT 1 FROM conversation_participants WHERE user_id = $1 AND conversation_id = $2',
      [userId, conversationId]
    );
    
    return result.rowCount > 0;
  } catch (error) {
    console.error('Erro ao verificar participação na conversa:', error);
    throw error;
  }
};

// Criar uma nova conversa
const createConversation = async (participantIds) => {
  // Inicia uma transação para garantir que todas as operações sejam concluídas ou nenhuma
  const client = await db.pool.connect();
  
  try {
    await client.query('BEGIN');
    
    // Cria a conversa
    const conversationResult = await client.query(
      'INSERT INTO conversations DEFAULT VALUES RETURNING id, created_at'
    );
    
    const conversationId = conversationResult.rows[0].id;
    
    // Adiciona todos os participantes
    for (const userId of participantIds) {
      await client.query(
        'INSERT INTO conversation_participants (conversation_id, user_id) VALUES ($1, $2)',
        [conversationId, userId]
      );
    }
    
    await client.query('COMMIT');
    return conversationResult.rows[0];
  } catch (error) {
    await client.query('ROLLBACK');
    console.error('Erro ao criar conversa:', error);
    throw error;
  } finally {
    client.release();
  }
};

// Verificar se já existe uma conversa entre os usuários
const findExistingConversation = async (participantIds) => {
  try {
    // Para conversas com 2 participantes
    if (participantIds.length === 2) {
      const result = await db.query(
        `SELECT cp1.conversation_id
         FROM conversation_participants cp1
         JOIN conversation_participants cp2 ON cp1.conversation_id = cp2.conversation_id
         WHERE cp1.user_id = $1 AND cp2.user_id = $2
         GROUP BY cp1.conversation_id
         HAVING COUNT(DISTINCT cp1.user_id) = 1 AND COUNT(DISTINCT cp2.user_id) = 1`,
        [participantIds[0], participantIds[1]]
      );
      
      if (result.rows.length > 0) {
        return result.rows[0].conversation_id;
      }
    } else {
      // Para conversas com mais de 2 participantes (grupos)
      // Lógica mais complexa seria necessária para grupos exatos
      return null;
    }
    
    return null;
  } catch (error) {
    console.error('Erro ao buscar conversa existente:', error);
    throw error;
  }
};

// Salvar uma nova mensagem
const saveMessage = async (conversationId, senderId, content) => {
  try {
    const result = await db.query(
      `INSERT INTO messages (conversation_id, sender_id, content)
       VALUES ($1, $2, $3)
       RETURNING id, conversation_id, sender_id, content, created_at`,
      [conversationId, senderId, content]
    );
    
    return result.rows[0];
  } catch (error) {
    console.error('Erro ao salvar mensagem:', error);
    throw error;
  }
};

// Buscar detalhes de uma conversa
const getConversationDetails = async (conversationId) => {
  try {
    const result = await db.query(
      `SELECT c.id, c.created_at,
        (SELECT json_agg(json_build_object('id', u.id, 'nome', u.nome, 'email', u.email))
         FROM conversation_participants cp
         JOIN usuarios u ON cp.user_id = u.id
         WHERE cp.conversation_id = c.id) as participants
       FROM conversations c
       WHERE c.id = $1`,
      [conversationId]
    );
    
    return result.rows[0];
  } catch (error) {
    console.error('Erro ao buscar detalhes da conversa:', error);
    throw error;
  }
};

module.exports = {
  getUserConversations,
  getConversationMessages,
  isConversationParticipant,
  createConversation,
  findExistingConversation,
  saveMessage,
  getConversationDetails
};
