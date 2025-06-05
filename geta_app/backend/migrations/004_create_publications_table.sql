-- Migração para criar a tabela de publicações (necessidades/projetos)

CREATE TYPE publicacao_status AS ENUM ('Aberto', 'Em Andamento', 'Concluído', 'Cancelado');
CREATE TYPE publicacao_tipo AS ENUM ('Necessidade', 'Projeto');

CREATE TABLE publicacoes (
    id SERIAL PRIMARY KEY,
    usuario_id INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE, -- Quem publicou
    titulo VARCHAR(255) NOT NULL,
    descricao TEXT NOT NULL,
    tipo publicacao_tipo NOT NULL DEFAULT 'Necessidade',
    status publicacao_status NOT NULL DEFAULT 'Aberto',
    orcamento DECIMAL(10, 2), -- Opcional: orçamento estimado
    prazo DATE, -- Opcional: prazo desejado
    tags TEXT[], -- Opcional: palavras-chave para busca
    data_criacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Gatilho para atualizar data_atualizacao na tabela publicacoes
CREATE OR REPLACE FUNCTION atualizar_data_atualizacao_publicacoes()
RETURNS TRIGGER AS $$
BEGIN
    NEW.data_atualizacao = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_atualizar_data_atualizacao_publicacoes
BEFORE UPDATE ON publicacoes
FOR EACH ROW
EXECUTE FUNCTION atualizar_data_atualizacao_publicacoes();

-- Índices para otimização
CREATE INDEX idx_publicacoes_usuario_id ON publicacoes(usuario_id);
CREATE INDEX idx_publicacoes_status ON publicacoes(status);
CREATE INDEX idx_publicacoes_tipo ON publicacoes(tipo);
CREATE INDEX idx_publicacoes_tags_gin ON publicacoes USING GIN (tags); -- Índice GIN para busca em tags

