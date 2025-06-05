-- Migração para criar tabelas do chat

-- Tabela de Conversas
CREATE TABLE conversations (
    id SERIAL PRIMARY KEY,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Participantes da Conversa (Tabela de Junção)
-- Garante que um usuário só possa participar uma vez de cada conversa
CREATE TABLE conversation_participants (
    id SERIAL PRIMARY KEY,
    conversation_id INTEGER NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (conversation_id, user_id) -- Chave única para evitar duplicatas
);

-- Tabela de Mensagens
CREATE TABLE messages (
    id SERIAL PRIMARY KEY,
    conversation_id INTEGER NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    sender_id INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE, -- Quem enviou
    content TEXT NOT NULL, -- Conteúdo da mensagem
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para otimizar buscas comuns
CREATE INDEX idx_conversation_participants_user_id ON conversation_participants(user_id);
CREATE INDEX idx_messages_conversation_id_created_at ON messages(conversation_id, created_at DESC); -- Para buscar mensagens recentes de uma conversa

