const db = require("../config/db");

// Criar uma nova publicação
const createPublication = async (req, res) => {
  try {
    const userId = req.user.id; // Obtido do middleware verifyToken
    const { titulo, descricao, tipo, status, orcamento, prazo, tags } = req.body;

    // Validações básicas
    if (!titulo || !descricao) {
      return res.status(400).json({ message: "Título e descrição são obrigatórios." });
    }

    const result = await db.query(
      `INSERT INTO publicacoes (usuario_id, titulo, descricao, tipo, status, orcamento, prazo, tags)
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
       RETURNING *`,
      [
        userId,
        titulo,
        descricao,
        tipo || 'Necessidade', // Valor padrão
        status || 'Aberto', // Valor padrão
        orcamento,
        prazo,
        tags || [] // Valor padrão
      ]
    );

    res.status(201).json(result.rows[0]);
  } catch (error) {
    console.error("Erro ao criar publicação:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

// Listar todas as publicações (com filtros básicos, pode ser expandido)
const listPublications = async (req, res) => {
  try {
    // Adicionar filtros básicos (ex: por tipo, status) e paginação no futuro
    const result = await db.query(
      `SELECT p.id, p.titulo, p.tipo, p.status, p.data_criacao, 
              u.id as usuario_id, u.nome_completo as usuario_nome
       FROM publicacoes p
       JOIN usuarios u ON p.usuario_id = u.id
       ORDER BY p.data_criacao DESC`
      // Adicionar LIMIT e OFFSET para paginação aqui
    );

    res.status(200).json(result.rows);
  } catch (error) {
    console.error("Erro ao listar publicações:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

// Obter detalhes de uma publicação específica
const getPublicationDetails = async (req, res) => {
  try {
    const publicationId = parseInt(req.params.publicationId, 10);

    if (isNaN(publicationId)) {
      return res.status(400).json({ message: "ID de publicação inválido" });
    }

    const result = await db.query(
      `SELECT p.*, u.nome_completo as usuario_nome
       FROM publicacoes p
       JOIN usuarios u ON p.usuario_id = u.id
       WHERE p.id = $1`,
      [publicationId]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({ message: "Publicação não encontrada" });
    }

    res.status(200).json(result.rows[0]);
  } catch (error) {
    console.error("Erro ao obter detalhes da publicação:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

// Atualizar uma publicação (apenas o proprietário)
const updatePublication = async (req, res) => {
  try {
    const userId = req.user.id;
    const publicationId = parseInt(req.params.publicationId, 10);
    const { titulo, descricao, tipo, status, orcamento, prazo, tags } = req.body;

    if (isNaN(publicationId)) {
      return res.status(400).json({ message: "ID de publicação inválido" });
    }

    // Verificar se a publicação existe e pertence ao usuário
    const checkResult = await db.query(
      "SELECT usuario_id FROM publicacoes WHERE id = $1",
      [publicationId]
    );

    if (checkResult.rows.length === 0) {
      return res.status(404).json({ message: "Publicação não encontrada" });
    }

    if (checkResult.rows[0].usuario_id !== userId) {
      return res.status(403).json({ message: "Acesso negado. Você não é o proprietário desta publicação." });
    }

    // Atualizar a publicação
    const result = await db.query(
      `UPDATE publicacoes 
       SET titulo = $1, descricao = $2, tipo = $3, status = $4, orcamento = $5, prazo = $6, tags = $7, data_atualizacao = NOW()
       WHERE id = $8
       RETURNING *`,
      [titulo, descricao, tipo, status, orcamento, prazo, tags, publicationId]
    );

    res.status(200).json(result.rows[0]);
  } catch (error) {
    console.error("Erro ao atualizar publicação:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

// Deletar uma publicação (apenas o proprietário)
const deletePublication = async (req, res) => {
  try {
    const userId = req.user.id;
    const publicationId = parseInt(req.params.publicationId, 10);

    if (isNaN(publicationId)) {
      return res.status(400).json({ message: "ID de publicação inválido" });
    }

    // Verificar se a publicação existe e pertence ao usuário
    const checkResult = await db.query(
      "SELECT usuario_id FROM publicacoes WHERE id = $1",
      [publicationId]
    );

    if (checkResult.rows.length === 0) {
      return res.status(404).json({ message: "Publicação não encontrada" });
    }

    if (checkResult.rows[0].usuario_id !== userId) {
      return res.status(403).json({ message: "Acesso negado. Você não é o proprietário desta publicação." });
    }

    // Deletar a publicação
    await db.query("DELETE FROM publicacoes WHERE id = $1", [publicationId]);

    res.status(204).send(); // No Content
  } catch (error) {
    console.error("Erro ao deletar publicação:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

module.exports = {
  createPublication,
  listPublications,
  getPublicationDetails,
  updatePublication,
  deletePublication,
};
