const db = require("./config/db");

async function testConnection() {
  try {
    console.log("Tentando conectar ao banco de dados...");
    const client = await db.pool.connect();
    console.log("Conexão bem-sucedida!");

    console.log("Testando consulta simples (SELECT NOW())...");
    const timeResult = await db.query("SELECT NOW()");
    console.log("Hora atual do banco de dados:", timeResult.rows[0].now);

    console.log("Testando consulta à tabela usuarios (pode estar vazia)...");
    const userResult = await db.query("SELECT COUNT(*) FROM usuarios");
    console.log("Contagem de usuários:", userResult.rows[0].count);

    client.release();
    console.log("Conexão liberada.");
    console.log("\nValidação da conexão com o banco de dados concluída com sucesso!");
    // Fechar o pool para que o script termine
    await db.pool.end();
    console.log("Pool de conexões fechado.");

  } catch (error) {
    console.error("Erro ao conectar ou consultar o banco de dados:", error);
    // Fechar o pool em caso de erro também
    try {
      await db.pool.end();
      console.log("Pool de conexões fechado após erro.");
    } catch (endError) {
      console.error("Erro ao fechar o pool de conexões:", endError);
    }
    process.exit(1); // Sair com código de erro
  }
}

testConnection();
