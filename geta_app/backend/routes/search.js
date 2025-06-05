const express = require("express");
const searchController = require("../controllers/searchController");
// const verifyToken = require("../middleware/verifyToken"); // A busca pode ser pública ou protegida

const router = express.Router();

// Rota principal de busca (GET /api/search?q=termo&type=users|publications)
// Por enquanto, deixaremos pública. Adicionar verifyToken se necessário.
router.get("/", searchController.performSearch);

module.exports = router;
