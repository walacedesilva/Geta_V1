const db = require("../config/db");
const bcrypt = require("bcrypt");
const jwt = require("jsonwebtoken");
require("dotenv").config();

const SALT_ROUNDS = 10;

// Função de Registro
exports.register = async (req, res) => {
  const { nome_completo, email, senha, tipo_perfil } = req.body;

  // Validação básica (pode ser expandida com bibliotecas como Joi)
  if (!nome_completo || !email || !senha || !tipo_perfil) {
    return res.status(400).json({ message: "Todos os campos são obrigatórios." });
  }

  if (!["Empreendedor", "Fornecedor", "Colaborador"].includes(tipo_perfil)) {
    return res.status(400).json({ message: "Tipo de perfil inválido." });
  }

  const client = await db.pool.connect();

  try {
    // Verificar se o email já existe
    const emailCheck = await client.query("SELECT id FROM usuarios WHERE email = $1", [email]);
    if (emailCheck.rows.length > 0) {
      return res.status(409).json({ message: "Email já cadastrado." });
    }

    // Hash da senha
    const senha_hash = await bcrypt.hash(senha, SALT_ROUNDS);

    // Iniciar transação
    await client.query("BEGIN");

    // Inserir usuário
    const newUserQuery = `
      INSERT INTO usuarios (nome_completo, email, senha_hash, tipo_perfil)
      VALUES ($1, $2, $3, $4)
      RETURNING id;
    `;
    const newUserResult = await client.query(newUserQuery, [nome_completo, email, senha_hash, tipo_perfil]);
    const userId = newUserResult.rows[0].id;

    // Inserir perfil base (pode ser expandido depois)
    const newProfileQuery = `
      INSERT INTO perfis (usuario_id)
      VALUES ($1);
    `;
    await client.query(newProfileQuery, [userId]);

    // Commit da transação
    await client.query("COMMIT");

    // Gerar token JWT
    const token = jwt.sign({ id: userId, email: email, tipo_perfil: tipo_perfil }, process.env.JWT_SECRET, { expiresIn: "1h" }); // Token expira em 1 hora

    res.status(201).json({ message: "Usuário cadastrado com sucesso!", token });

  } catch (error) {
    // Rollback em caso de erro
    await client.query("ROLLBACK");
    console.error("Erro no registro:", error);
    res.status(500).json({ message: "Erro interno do servidor ao registrar usuário." });
  } finally {
    client.release();
  }
};

// Função de Login
exports.login = async (req, res) => {
  const { email, senha } = req.body;

  if (!email || !senha) {
    return res.status(400).json({ message: "Email e senha são obrigatórios." });
  }

  try {
    // Buscar usuário pelo email
    const userResult = await db.query("SELECT id, email, senha_hash, tipo_perfil FROM usuarios WHERE email = $1", [email]);

    if (userResult.rows.length === 0) {
      return res.status(401).json({ message: "Credenciais inválidas." }); // Email não encontrado
    }

    const user = userResult.rows[0];

    // Comparar senha
    const match = await bcrypt.compare(senha, user.senha_hash);

    if (!match) {
      return res.status(401).json({ message: "Credenciais inválidas." }); // Senha incorreta
    }

    // Gerar token JWT
    const token = jwt.sign({ id: user.id, email: user.email, tipo_perfil: user.tipo_perfil }, process.env.JWT_SECRET, { expiresIn: "1h" });

    res.status(200).json({ message: "Login bem-sucedido!", token });

  } catch (error) {
    console.error("Erro no login:", error);
    res.status(500).json({ message: "Erro interno do servidor ao fazer login." });
  }
};
