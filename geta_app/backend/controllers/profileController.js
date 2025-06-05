const db = require("../config/db");

// Buscar o perfil do usuário logado
const getMyProfile = async (req, res) => {
  try {
    const userId = req.user.id; // Obtido do middleware de autenticação
    
    const result = await db.query(
      `SELECT u.id as user_id, u.nome_completo, u.email, u.tipo_perfil, 
              p.id as profile_id, p.foto_url, p.bio, p.localizacao, p.detalhes
       FROM usuarios u
       LEFT JOIN perfis p ON u.id = p.usuario_id
       WHERE u.id = $1`,
      [userId]
    );
    
    if (result.rows.length === 0) {
      return res.status(404).json({ message: "Usuário não encontrado" });
    }
    
    // Se o perfil ainda não existe (pode acontecer logo após o cadastro)
    if (!result.rows[0].profile_id) {
      // Retorna os dados básicos do usuário e indica que o perfil precisa ser completado
      return res.status(200).json({
        user_id: result.rows[0].user_id,
        nome_completo: result.rows[0].nome_completo,
        email: result.rows[0].email,
        tipo_perfil: result.rows[0].tipo_perfil,
        profile_incomplete: true
      });
    }
    
    res.status(200).json(result.rows[0]);
  } catch (error) {
    console.error("Erro ao buscar meu perfil:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

// Buscar o perfil de um usuário por ID
const getUserProfile = async (req, res) => {
  try {
    const userId = parseInt(req.params.userId, 10);
    
    if (isNaN(userId)) {
      return res.status(400).json({ message: "ID de usuário inválido" });
    }
    
    const result = await db.query(
      `SELECT u.id as user_id, u.nome_completo, u.tipo_perfil, 
              p.id as profile_id, p.foto_url, p.bio, p.localizacao, p.detalhes
       FROM usuarios u
       LEFT JOIN perfis p ON u.id = p.usuario_id
       WHERE u.id = $1`,
      [userId]
    );
    
    if (result.rows.length === 0) {
      return res.status(404).json({ message: "Usuário não encontrado" });
    }
    
    // Não retornar email ou detalhes sensíveis em perfis públicos
    const profileData = {
      user_id: result.rows[0].user_id,
      nome_completo: result.rows[0].nome_completo,
      tipo_perfil: result.rows[0].tipo_perfil,
      profile_id: result.rows[0].profile_id,
      foto_url: result.rows[0].foto_url,
      bio: result.rows[0].bio,
      localizacao: result.rows[0].localizacao,
      // Retornar detalhes apenas se necessário e se não forem sensíveis
      detalhes: result.rows[0].detalhes 
    };
    
    res.status(200).json(profileData);
  } catch (error) {
    console.error("Erro ao buscar perfil do usuário:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

// Atualizar o perfil do usuário logado
const updateMyProfile = async (req, res) => {
  try {
    const userId = req.user.id;
    const { foto_url, bio, localizacao, detalhes } = req.body;
    
    // Verificar se o perfil existe
    let profileResult = await db.query(
      "SELECT id FROM perfis WHERE usuario_id = $1",
      [userId]
    );
    
    let profileId;
    
    if (profileResult.rows.length === 0) {
      // Se o perfil não existe, criar um novo
      const insertResult = await db.query(
        "INSERT INTO perfis (usuario_id, foto_url, bio, localizacao, detalhes) VALUES ($1, $2, $3, $4, $5) RETURNING id",
        [userId, foto_url, bio, localizacao, detalhes || {}]
      );
      profileId = insertResult.rows[0].id;
    } else {
      // Se o perfil existe, atualizar
      profileId = profileResult.rows[0].id;
      await db.query(
        "UPDATE perfis SET foto_url = $1, bio = $2, localizacao = $3, detalhes = $4, data_atualizacao = NOW() WHERE id = $5",
        [foto_url, bio, localizacao, detalhes || {}, profileId]
      );
    }
    
    // Buscar o perfil atualizado para retornar
    const updatedProfile = await db.query(
      `SELECT u.id as user_id, u.nome_completo, u.email, u.tipo_perfil, 
              p.id as profile_id, p.foto_url, p.bio, p.localizacao, p.detalhes
       FROM usuarios u
       JOIN perfis p ON u.id = p.usuario_id
       WHERE u.id = $1`,
      [userId]
    );
    
    res.status(200).json(updatedProfile.rows[0]);
  } catch (error) {
    console.error("Erro ao atualizar meu perfil:", error);
    res.status(500).json({ message: "Erro interno do servidor" });
  }
};

module.exports = {
  getMyProfile,
  getUserProfile,
  updateMyProfile,
};
