const jwt = require("jsonwebtoken");

// Middleware para verificar o token JWT
const verifyToken = (req, res, next) => {
  // Obter token do cabeçalho Authorization
  const authHeader = req.headers.authorization;
  const token = authHeader && authHeader.split(" ")[1]; // Bearer TOKEN
  
  if (!token) {
    return res.status(401).json({ message: "Acesso negado. Nenhum token fornecido." });
  }
  
  try {
    // Verificar token
    const decoded = jwt.verify(token, process.env.JWT_SECRET);
    
    // Adicionar dados do usuário decodificados ao objeto req
    req.user = decoded; // Geralmente contém { userId: ..., iat: ..., exp: ... }
    
    next(); // Passar para a próxima função de middleware ou rota
  } catch (error) {
    console.error("Erro na verificação do token:", error);
    if (error.name === "TokenExpiredError") {
      return res.status(401).json({ message: "Token expirado." });
    }
    return res.status(403).json({ message: "Token inválido." });
  }
};

module.exports = verifyToken;
