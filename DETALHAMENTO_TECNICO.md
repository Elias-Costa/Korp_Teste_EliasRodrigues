# Detalhamento Tecnico

Este documento resume as principais decisoes tecnicas do projeto e responde aos itens solicitados no teste.

## 1. Quais ciclos de vida do Angular foram utilizados

Foi utilizado o ciclo de vida `OnInit` nos componentes de pagina:

- `ProductsPage`
- `InvoicesPage`

Finalidade:

- carregar os dados iniciais da tela assim que o componente e inicializado
- evitar que a busca de dados fique no construtor
- deixar a responsabilidade de inicializacao mais clara

No projeto, o `ngOnInit()` foi usado para:

- carregar a lista de produtos
- carregar a lista de notas fiscais
- carregar o status da simulacao de falha

Observacao:

- o componente raiz `App` nao precisou de ciclo de vida adicional, porque ele funciona apenas como shell de layout e roteamento

## 2. Se foi feito uso da biblioteca RxJS e, em caso afirmativo, como

Sim. O projeto utiliza `RxJS`, principalmente em conjunto com o `HttpClient` do Angular.

Principais usos:

- `switchMap`
  - usado para encadear operacoes HTTP
  - exemplo: depois de cadastrar ou editar um produto, a aplicacao busca novamente a lista atualizada
  - exemplo: depois de criar uma nota, a aplicacao recarrega a lista de notas

- `finalize`
  - usado para controlar estados visuais de carregamento
  - exemplo: ativar e desativar flags como `isLoading`, `isSaving` e `isFailureModeUpdating`

- `forkJoin`
  - usado para executar chamadas em paralelo e aguardar todas ao mesmo tempo
  - exemplo: carregar produtos, notas e status da simulacao de falha na tela de notas fiscais
  - exemplo: apos imprimir uma nota, atualizar notas e produtos em paralelo

- `takeUntilDestroyed`
  - usado para encerrar inscricoes automaticamente quando o componente e destruido
  - isso ajuda a evitar vazamento de memoria e mantem o codigo mais seguro

Em resumo, o RxJS foi usado para composicao de chamadas HTTP, tratamento de fluxo assíncrono e controle de estado das telas.

## 3. Quais outras bibliotecas foram utilizadas e para qual finalidade

### Frontend

- `@angular/common`
  - diretivas e recursos basicos do Angular

- `@angular/forms`
  - implementacao dos `Reactive Forms`
  - usado para validacao dos formularios de produtos e notas fiscais

- `@angular/router`
  - configuracao das rotas do frontend
  - usado para navegar entre as telas de produtos e notas fiscais

- `@angular/platform-browser`
  - bootstrap da aplicacao no navegador

- `rxjs`
  - fluxo assincrono das chamadas HTTP e controle de estado

### Backend

- `Npgsql.EntityFrameworkCore.PostgreSQL`
  - provider do Entity Framework Core para PostgreSQL
  - usado para persistencia dos dados no banco real

- `Microsoft.AspNetCore.OpenApi`
  - suporte a OpenAPI

- `Swashbuckle.AspNetCore`
  - geracao do Swagger para documentacao e teste dos endpoints

### Ferramentas de desenvolvimento

- `Vitest`
  - testes do frontend

- `Prettier`
  - formatacao de codigo no frontend

- `Docker Compose`
  - orquestracao local do PostgreSQL e dos dois microsservicos

## 4. Para componentes visuais, quais bibliotecas foram utilizadas

Nao foi utilizada biblioteca de componentes visuais como Angular Material, PrimeNG, Bootstrap ou similares.

Os componentes visuais foram construidos com:

- HTML nativo
- SCSS customizado
- componentes standalone do Angular

Ou seja, a interface foi feita de forma manual, com foco em manter o projeto leve, simples de explicar e suficiente para o escopo do teste.

Como apoio visual, foi usado:

- import de fontes do Google Fonts no arquivo global de estilos

Mas isso foi apenas para tipografia. Nao houve uso de biblioteca de componentes pronta.

## 5. Como foi realizado o gerenciamento de dependencias no Golang (se aplicavel)

Nao se aplica, porque o backend foi desenvolvido em `C#`, e nao em `Golang`.

Mesmo assim, vale registrar como as dependencias foram gerenciadas no projeto:

- no backend C#, por meio de `NuGet` e `PackageReference` nos arquivos `.csproj`
- no frontend Angular, por meio de `npm` e do arquivo `package.json`

## 6. Quais frameworks foram utilizados no Golang ou C#

Como a implementacao foi feita em `C#`, os frameworks principais utilizados foram:

- `ASP.NET Core 8`
  - usado para construir as APIs REST dos microsservicos

- `Entity Framework Core 8`
  - usado para acesso a dados e persistencia no PostgreSQL

No frontend, tambem foi utilizado:

- `Angular 21`
  - usado para a interface web e integracao com os microsservicos

## 7. Como foram tratados os erros e excecoes no backend

O tratamento de erros foi centralizado e padronizado.

### Estrategia adotada

- foram criadas excecoes de dominio especificas, como:
  - `ValidationException`
  - `NotFoundException`
  - `ConflictException`
  - `ServiceUnavailableException`
  - `ExternalServiceException`

- essas excecoes sao capturadas por um middleware global de tratamento:
  - `ExceptionHandlingMiddleware` em cada microsservico

### Como funciona na pratica

- quando ocorre um erro esperado de negocio, a aplicacao lanca uma excecao especifica
- o middleware intercepta essa excecao
- a resposta HTTP e devolvida em formato `ProblemDetails`
- com isso, o frontend recebe:
  - status HTTP coerente
  - titulo do erro
  - descricao do erro
  - rota que gerou o problema

### Exemplos de cenarios tratados

- produto nao encontrado
- nota fiscal nao encontrada
- produto com codigo duplicado
- tentativa de imprimir nota que ja esta fechada
- tentativa de consumir estoque sem saldo suficiente
- simulacao de falha no microsservico de estoque

### Beneficios dessa abordagem

- padronizacao das respostas de erro
- melhor integracao com o frontend
- codigo mais organizado
- separacao clara entre regra de negocio e tratamento HTTP

## 8. Caso a implementacao utilize C#, indicar se foi utilizado LINQ e de que forma

Sim, foi utilizado `LINQ` em varios pontos do backend.

Principais usos do LINQ no projeto:

- `OrderBy` e `OrderByDescending`
  - ordenar produtos e notas fiscais

- `Select`
  - transformar entidades em objetos de resposta
  - transformar itens de nota em DTOs

- `Where`
  - filtrar produtos por ids

- `Any`
  - validar existencia de codigo duplicado
  - validar itens invalidos

- `Distinct`
  - remover ids duplicados na consulta de produtos

- `GroupBy` e `Sum`
  - agrupar itens repetidos da nota
  - somar quantidades do mesmo produto

- `ToDictionary`
  - mapear produtos por id para acesso rapido durante validacoes

- `ToList`
  - materializar consultas e colecoes transformadas

### Exemplos práticos dentro do projeto

- no `inventory-service`
  - ordenacao da lista de produtos
  - validacao de codigo duplicado
  - agrupamento dos itens para consumo de estoque
  - identificacao de produtos faltantes

- no `billing-service`
  - agrupamento dos itens da nota fiscal
  - ordenacao dos itens da resposta
  - mapeamento dos produtos retornados pelo microsservico de estoque

Em resumo, o LINQ foi usado para deixar as consultas e transformacoes mais legiveis, expressivas e alinhadas com a logica de negocio.
