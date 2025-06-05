const db = require("../config/db");

// Função principal de busca
const performSearch = async (req, res) => {
  try {
    const { q, type } = req.query; // q = termo de busca, type = 'users' ou 'publications'

    if (!q || q.trim() === ") {
      return res.status(400).json({ message: "Termo de busca é obrigatório." });
    }

    const searchTerm = `%${q.trim()}%`;
    let results = {
      users: [],
      publications: [],
    };

    // Buscar Usuários (se type não for 'publications' ou não especificado)
    if (!type || type === 'users') {
      const userQuery = `
        SELECT 
          u.id, 
          u.nome_completo, 
          u.tipo_perfil, 
          p.foto_url, 
          p.bio, 
          p.localizacao
        FROM usuarios u
        LEFT JOIN perfis p ON u.id = p.usuario_id
        WHERE 
          u.nome_completo ILIKE $1 OR 
          p.bio ILIKE $1 OR 
          p.localizacao ILIKE $1 OR
          -- Buscar em detalhes JSONB (exemplo: habilidades, area_atuacao)
          (p.detalhes ->> 'habilidades') ILIKE $1 OR 
          (p.detalhes ->> 'area_atuacao') ILIKE $1 OR
          (p.detalhes ->> 'segmento') ILIKE $1
        LIMIT 20; -- Limitar resultados
      `;
      const userResults = await db.query(userQuery, [searchTerm]);
      results.users = userResults.rows;
    }

    // Buscar Publicações (se type não for 'users' ou não especificado)
    if (!type || type === 'publications') {
      const publicationQuery = `
        SELECT 
          p.id, 
          p.titulo, 
          p.tipo, 
          p.status, 
          p.data_criacao, 
          u.id as usuario_id, 
          u.nome_completo as usuario_nome,
          LEFT(p.descricao, 150) as descricao_curta -- Pega os primeiros 150 caracteres da descrição
        FROM publicacoes p
        JOIN usuarios u ON p.usuario_id = u.id
        WHERE 
          p.titulo ILIKE $1 OR 
          p.descricao ILIKE $1 OR 
          -- Buscar em array de tags
          EXISTS (
            SELECT 1 FROM unnest(p.tags) tag WHERE tag ILIKE $1
          )
        ORDER BY p.data_criacao DESC
        LIMIT 20; -- Limitar resultados
      `;
      const publicationResults = await db.query(publicationQuery, [searchTerm]);
      results.publications = publicationResults.rows;
    }

    res.status(200).json(results);

  } catch (error) {
    console.error("Erro ao realizar busca:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

module.exports = {
  performSearch,
};
