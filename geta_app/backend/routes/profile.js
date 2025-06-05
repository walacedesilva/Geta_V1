const express = require("express");
const profileController = require("../controllers/profileController");
const verifyToken = require("../middleware/verifyToken"); // Precisamos criar este middleware

const router = express.Router();

// Rota para buscar o perfil do usuário logado
router.get("/me", verifyToken, profileController.getMyProfile);

// Rota para atualizar o perfil do usuário logado
router.put("/me", verifyToken, profileController.updateMyProfile);

// Rota para buscar o perfil de um usuário específico por ID (pública ou protegida, dependendo dos requisitos)
// Por enquanto, vamos deixar pública, mas pode ser protegida com verifyToken se necessário
router.get("/:userId", profileController.getUserProfile);

module.exports = router;
