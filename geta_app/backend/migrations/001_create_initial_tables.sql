-- Migração inicial para criar tabelas de usuários e perfis

-- Tabela de Usuários
CREATE TABLE usuarios (
    id SERIAL PRIMARY KEY,
    nome_completo VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    tipo_perfil VARCHAR(50) NOT NULL CHECK (tipo_perfil IN (
'Empreendedor', 'Fornecedor', 'Colaborador'
)),
    data_criacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Perfis (base)
-- Esta tabela pode ser expandida ou relacionada a tabelas específicas por tipo de perfil
CREATE TABLE perfis (
    id SERIAL PRIMARY KEY,
    usuario_id INTEGER UNIQUE NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    -- Adicionar campos comuns a todos os perfis se houver, ou deixar para tabelas específicas
    data_criacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Gatilho para atualizar data_atualizacao na tabela usuarios
CREATE OR REPLACE FUNCTION atualizar_data_atualizacao_usuarios()
RETURNS TRIGGER AS $$
BEGIN
    NEW.data_atualizacao = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_atualizar_data_atualizacao_usuarios
BEFORE UPDATE ON usuarios
FOR EACH ROW
EXECUTE FUNCTION atualizar_data_atualizacao_usuarios();

-- Gatilho para atualizar data_atualizacao na tabela perfis
CREATE OR REPLACE FUNCTION atualizar_data_atualizacao_perfis()
RETURNS TRIGGER AS $$
BEGIN
    NEW.data_atualizacao = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_atualizar_data_atualizacao_perfis
BEFORE UPDATE ON perfis
FOR EACH ROW
EXECUTE FUNCTION atualizar_data_atualizacao_perfis();

-- Índices para otimização
CREATE INDEX idx_usuarios_email ON usuarios(email);
CREATE INDEX idx_perfis_usuario_id ON perfis(usuario_id);

