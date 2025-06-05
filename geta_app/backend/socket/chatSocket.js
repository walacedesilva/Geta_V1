const chatController = require('../controllers/chat/chatController');

// Configuração e gerenciamento dos eventos de Socket.IO para o chat
const setupSocketIO = (io) => {
  // Armazenar mapeamento de usuários para sockets
  const userSocketMap = new Map();
  
  // Middleware de autenticação já aplicado no server.js
  
  io.on('connection', (socket) => {
    console.log(`Usuário conectado: ${socket.user.id} (${socket.user.nome})`);
    
    // Armazenar socket do usuário para envio de mensagens
    userSocketMap.set(socket.user.id, socket.id);
    
    // Evento para buscar conversas do usuário
    socket.on('get_conversations', async () => {
      try {
        const conversations = await chatController.getUserConversations(socket.user.id);
        socket.emit('conversations_list', conversations);
      } catch (error) {
        console.error('Erro ao buscar conversas:', error);
        socket.emit('error', { message: 'Erro ao buscar conversas' });
      }
    });
    
    // Evento para buscar mensagens de uma conversa
    socket.on('get_messages', async ({ conversationId, limit = 50, offset = 0 }) => {
      try {
        // Verificar se o usuário é participante da conversa
        const isParticipant = await chatController.isConversationParticipant(socket.user.id, conversationId);
        
        if (!isParticipant) {
          return socket.emit('error', { message: 'Acesso negado a esta conversa' });
        }
        
        const messages = await chatController.getConversationMessages(conversationId, limit, offset);
        socket.emit('messages_list', { conversationId, messages });
        
        // Entrar na sala da conversa para receber mensagens em tempo real
        socket.join(`conversation:${conversationId}`);
      } catch (error) {
        console.error('Erro ao buscar mensagens:', error);
        socket.emit('error', { message: 'Erro ao buscar mensagens' });
      }
    });
    
    // Evento para enviar mensagem
    socket.on('send_message', async ({ conversationId, content }) => {
      try {
        // Verificar se o usuário é participante da conversa
        const isParticipant = await chatController.isConversationParticipant(socket.user.id, conversationId);
        
        if (!isParticipant) {
          return socket.emit('error', { message: 'Acesso negado a esta conversa' });
        }
        
        // Salvar mensagem no banco
        const message = await chatController.saveMessage(conversationId, socket.user.id, content);
        
        // Adicionar informações do remetente
        const messageWithSender = {
          ...message,
          sender: {
            id: socket.user.id,
            nome: socket.user.nome
          }
        };
        
        // Emitir mensagem para todos os participantes da conversa
        io.to(`conversation:${conversationId}`).emit('new_message', messageWithSender);
      } catch (error) {
        console.error('Erro ao enviar mensagem:', error);
        socket.emit('error', { message: 'Erro ao enviar mensagem' });
      }
    });
    
    // Evento para criar nova conversa
    socket.on('create_conversation', async ({ participantIds }) => {
      try {
        // Garantir que o criador esteja incluído nos participantes
        if (!participantIds.includes(socket.user.id)) {
          participantIds.push(socket.user.id);
        }
        
        // Verificar se já existe uma conversa com esses participantes (para evitar duplicatas)
        let conversationId = null;
        
        if (participantIds.length === 2) {
          conversationId = await chatController.findExistingConversation(participantIds);
        }
        
        // Se não existir, criar nova conversa
        if (!conversationId) {
          const conversation = await chatController.createConversation(participantIds);
          conversationId = conversation.id;
        }
        
        // Buscar detalhes da conversa
        const conversationDetails = await chatController.getConversationDetails(conversationId);
        
        // Notificar o criador sobre a nova conversa
        socket.emit('conversation_created', conversationDetails);
        
        // Notificar outros participantes que estão online
        participantIds.forEach(userId => {
          if (userId !== socket.user.id && userSocketMap.has(userId)) {
            const participantSocketId = userSocketMap.get(userId);
            io.to(participantSocketId).emit('new_conversation', conversationDetails);
          }
        });
      } catch (error) {
        console.error('Erro ao criar conversa:', error);
        socket.emit('error', { message: 'Erro ao criar conversa' });
      }
    });
    
    // Evento para sair de uma sala de conversa (quando o usuário muda de tela)
    socket.on('leave_conversation', ({ conversationId }) => {
      socket.leave(`conversation:${conversationId}`);
    });
    
    // Evento de desconexão
    socket.on('disconnect', () => {
      console.log(`Usuário desconectado: ${socket.user.id}`);
      userSocketMap.delete(socket.user.id);
    });
  });
};

module.exports = setupSocketIO;
