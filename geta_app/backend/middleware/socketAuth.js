const jwt = require('jsonwebtoken');
const db = require('../config/db');

// Middleware para autenticar conexões de socket usando JWT
const socketAuth = async (socket, next) => {
  try {
    // Obter token do handshake
    const token = socket.handshake.auth.token || socket.handshake.headers.authorization?.split(' ')[1];
    
    if (!token) {
      return next(new Error('Autenticação necessária'));
    }
    
    // Verificar token
    const decoded = jwt.verify(token, process.env.JWT_SECRET);
    
    // Verificar se o usuário existe no banco
    const result = await db.query('SELECT id, nome, email FROM usuarios WHERE id = $1', [decoded.userId]);
    
    if (result.rows.length === 0) {
      return next(new Error('Usuário não encontrado'));
    }
    
    // Adicionar dados do usuário ao objeto socket
    socket.user = result.rows[0];
    
    next();
  } catch (error) {
    console.error('Erro na autenticação do socket:', error);
    next(new Error('Token inválido'));
  }
};

module.exports = socketAuth;
