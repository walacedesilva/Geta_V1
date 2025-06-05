-- Migração para adicionar detalhes aos perfis

-- Adicionar colunas comuns à tabela de perfis
ALTER TABLE perfis
ADD COLUMN foto_url VARCHAR(255),
ADD COLUMN bio TEXT,
ADD COLUMN localizacao VARCHAR(255);

-- Adicionar coluna JSONB para detalhes específicos do tipo de perfil
ALTER TABLE perfis
ADD COLUMN detalhes JSONB;

-- Atualizar o gatilho para incluir as novas colunas (se necessário, mas a função atual já atualiza data_atualizacao)
-- Não é estritamente necessário recriar o gatilho se ele só atualiza a data.

-- Adicionar índices se necessário para campos comuns ou JSONB (opcional por agora)
-- Exemplo: CREATE INDEX idx_perfis_detalhes_gin ON perfis USING GIN (detalhes);

-- Atualizar a função de gatilho para garantir que data_atualizacao seja atualizada
-- (A função existente já faz isso, mas vamos garantir que ela esteja aplicada)
DROP TRIGGER IF EXISTS trigger_atualizar_data_atualizacao_perfis ON perfis;
CREATE TRIGGER trigger_atualizar_data_atualizacao_perfis
BEFORE UPDATE ON perfis
FOR EACH ROW
EXECUTE FUNCTION atualizar_data_atualizacao_perfis();

