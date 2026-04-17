# Korp_Teste_EliasRodrigues

Projeto do teste tecnico pratico da Korp com:

- frontend em `Angular 21`
- `inventory-service` em `ASP.NET Core 8`
- `billing-service` em `ASP.NET Core 8`
- `PostgreSQL` como banco real

## O que ja esta pronto

- cadastro de produtos com codigo, descricao e saldo
- cadastro de notas fiscais com varios itens
- impressao da nota com mudanca de status para `Closed`
- baixa automatica do estoque ao imprimir
- simulacao de falha no microsservico de estoque
- feedback claro quando a impressao falha
- separacao em dois microsservicos

## Estrutura

```text
/frontend
/services/inventory-service
/services/billing-service
/database/init
/docker-compose.yml
```

## Como rodar o backend

### Com Docker Compose

```bash
docker compose up --build
```

Servicos:

- inventory swagger: `http://localhost:5001/swagger`
- billing swagger: `http://localhost:5002/swagger`
- postgres: `localhost:5432`

Credenciais do banco:

- usuario: `korp`
- senha: `korp`

### Com dotnet local

Suba um PostgreSQL local e depois rode:

```bash
dotnet run --project services/inventory-service
dotnet run --project services/billing-service
```

## Como rodar o frontend

Entre na pasta `frontend` e rode:

```bash
npm install
npm start
```

Aplicacao Angular:

- `http://localhost:4200`

Observacao:

- o backend ja esta configurado com CORS para `http://localhost:4200`

## Fluxo para demonstrar no video

1. Cadastrar um produto na tela de produtos.
2. Ir para a tela de notas fiscais.
3. Criar uma nota com um ou mais itens.
4. Imprimir a nota.
5. Mostrar que a nota foi fechada e o saldo do produto diminuiu.
6. Ativar a simulacao de falha.
7. Tentar imprimir outra nota e mostrar que ela continua aberta com mensagem de erro.

## Validacoes feitas

- `dotnet build Korp.Backend.sln`
- `npm run build` dentro de `frontend`
- fluxo completo validado com `docker compose`

## Ideia da arquitetura

- `inventory-service` cuida de produtos e saldo
- `billing-service` cuida de notas fiscais
- o frontend Angular conversa diretamente com os dois servicos
- na impressao, o billing chama o inventory para consumir estoque
- se o inventory falhar, o billing nao fecha a nota
