require("dotenv").config();
const express = require("express");
const cors = require("cors");
const http = require("http");
const { Server } = require("socket.io");
const authRoutes = require("./routes/auth");
const profileRoutes = require("./routes/profile");
const publicationRoutes = require("./routes/publication");
const searchRoutes = require("./routes/search"); // Importar rotas de busca
const socketAuth = require("./middleware/socketAuth");
const setupSocketIO = require("./socket/chatSocket");

const app = express();
const PORT = process.env.PORT || 3000;

// Criar servidor HTTP
const server = http.createServer(app);

// Configurar Socket.IO
const io = new Server(server, {
  cors: {
    origin: "*", // Em produção, limitar às origens permitidas
    methods: ["GET", "POST"],
    credentials: true
  }
});

// Aplicar middleware de autenticação ao Socket.IO
io.use(socketAuth);

// Configurar handlers de eventos do chat
setupSocketIO(io);

// Middleware Express
app.use(cors()); // Enable Cross-Origin Resource Sharing
app.use(express.json()); // Parse JSON request bodies
app.use(express.urlencoded({ extended: true })); // Parse URL-encoded request bodies

// Routes
app.get("/", (req, res) => {
  res.send("API do Geta está rodando!");
});

app.use("/api/auth", authRoutes);
app.use("/api/profiles", profileRoutes);
app.use("/api/publications", publicationRoutes);
app.use("/api/search", searchRoutes); // Montar rotas de busca

// Basic Error Handling (can be expanded)
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).send("Algo deu errado!");
});

// Start Server
server.listen(PORT, () => {
  console.log(`Servidor rodando na porta ${PORT}`);
});

// Handle graceful shutdown (optional but good practice)
process.on("SIGINT", async () => {
  console.log("Recebido SIGINT. Fechando servidor e pool do banco de dados...");
  try {
    const db = require("./config/db");
    await db.pool.end();
    console.log("Pool de conexões do banco de dados fechado.");
    process.exit(0);
  } catch (error) {
    console.error("Erro ao fechar pool do banco de dados:", error);
    process.exit(1);
  }
});
