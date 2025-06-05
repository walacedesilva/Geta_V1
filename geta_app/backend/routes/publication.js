const express = require("express");
const publicationController = require("../controllers/publicationController");
const verifyToken = require("../middleware/verifyToken");

const router = express.Router();

// Rota para criar uma nova publicação (protegida)
router.post("/", verifyToken, publicationController.createPublication);

// Rota para listar todas as publicações (pública, pode ser protegida se necessário)
router.get("/", publicationController.listPublications);

// Rota para obter detalhes de uma publicação específica (pública)
router.get("/:publicationId", publicationController.getPublicationDetails);

// Rota para atualizar uma publicação (protegida e apenas proprietário)
router.put("/:publicationId", verifyToken, publicationController.updatePublication);

// Rota para deletar uma publicação (protegida e apenas proprietário)
router.delete("/:publicationId", verifyToken, publicationController.deletePublication);

module.exports = router;
