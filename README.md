<div align="center">

# 📋 Kanban App

Um quadro Kanban colaborativo, do zero: backend em **ASP.NET Core**, banco **PostgreSQL**, containerizado com **Docker** e rodando na nuvem via **Azure Container Apps**, com **CI/CD** automatizado e uma **API REST própria** que também alimenta um app mobile em Flutter.

`ASP.NET Core MVC` · `PostgreSQL` · `Docker` · `Azure` · `GitHub Actions`

</div>

---

## Sobre o projeto

Este é um projeto pessoal de estudo, construído para aprender — na prática — todo o ciclo de vida de uma aplicação web moderna: modelagem de banco de dados, backend com ASP.NET Core MVC, autenticação, containerização, deploy em nuvem, integração contínua e, por fim, exposição de uma API REST para consumo externo (um app mobile = https://github.com/kishinbr/KanbanAppMobile).

A ideia central é simples: um **Kanban colaborativo**. Qualquer pessoa pode criar um quadro, organizar colunas e cartões, e convidar outras pessoas para acompanhar (ou não) seu progresso — sem precisar de conta compartilhada ou permissões complicadas.

LINK DE ACESSO : https://kanban-app.bluewave-06d366a7.chilecentral.azurecontainerapps.io

## ✨ Funcionalidades

**Contas**
- Cadastro e login com senha criptografada (BCrypt)
- Sessão via cookie (site) e via token JWT (API)

**Quadros**
- Criação de quadros, com um código único gerado automaticamente para compartilhar
- Entrar em um quadro de outra pessoa digitando o código — vira **espectador** (somente visualização)
- Quem cria o quadro é automaticamente o **dono**, com controle total
- Sair de um quadro (espectador) ou excluí-lo por completo (dono)
- Lista de participantes visível no cabeçalho do quadro

**Colunas e cartões**
- Criar, renomear e excluir colunas livremente
- Criar, editar (título e descrição) e excluir cartões
- Mover cartões entre colunas com **drag-and-drop**, via endpoint de API dedicado

**Permissões**
- Toda ação sensível é validada tanto na interface quanto no backend — um espectador nunca consegue editar nada, mesmo tentando "forçar" uma requisição

## 🏗️ Arquitetura

```
                     ┌────────────────────────────┐
                     │   Azure Container Apps      │
                     │                              │
   Navegador ───────▶│  kanban-app  ───▶ kanban-postgres │
   (Cookie)           │  ASP.NET Core     PostgreSQL 16  │
                     │                              │
   App Mobile ───────▶│  (mesma API, via JWT)        │
   (Flutter)          └────────────────────────────┘
```

O mesmo backend serve dois tipos de cliente:
- **Views Razor + Cookie**, para quem acessa pelo navegador
- **API REST + JWT**, para o [app mobile em Flutter](#-app-mobile) e qualquer outro cliente futuro

Os dois modelos de autenticação convivem no mesmo projeto, sem conflito — inclusive no mesmo Controller, quando necessário (o endpoint de mover cartões aceita tanto cookie quanto token).

## 🛠️ Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core MVC + Web API (.NET 10) |
| Acesso a dados | Dapper + Npgsql (SQL escrito à mão) |
| Banco de dados | PostgreSQL 16 |
| Autenticação | Cookie (site) + JWT (API) |
| Frontend | Razor Views, Bootstrap, JavaScript puro |
| Containers | Docker + Docker Compose |
| Nuvem | Azure Container Apps + Azure Container Registry |
| CI/CD | GitHub Actions |

## 🗄️ Modelo de dados

Cinco tabelas, propositalmente simples:

```
usuarios ──┬──< membros >──┬── quadros ──< colunas ──< cartoes
           │                │
     (dono/espectador)  (código de compartilhamento)
```

- `usuarios` — conta e senha (hash)
- `quadros` — cada kanban, com um código único de 6 caracteres
- `membros` — a relação entre usuário e quadro, com o papel (`dono` ou `espectador`)
- `colunas` — pertencem a um quadro
- `cartoes` — pertencem a uma coluna, com campo de ordem para o drag-and-drop

Exclusões em cascata garantem que apagar um quadro remove tudo relacionado a ele automaticamente.

## 🚀 Rodando localmente

Pré-requisito: [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
git clone https://github.com/kishinbr/Kanban.git
cd Kanban/KanbanApp/KanbanApp
docker-compose up --build
```

A aplicação sobe em `http://localhost:8080`, junto com um container PostgreSQL.

Na primeira vez, crie as tabelas:

```bash
docker exec -i kanbanapp-banco-1 psql -U postgres -d kanban_db < script.sql
```

## ☁️ Deploy e CI/CD

- A cada `push` na branch `master`, o **GitHub Actions** builda a imagem Docker e envia automaticamente para o **Azure Container Registry**.
- A aplicação roda em dois **Container Apps** independentes (backend + banco), no mesmo ambiente, se comunicando internamente.
- A atualização final (apontar o Container App para a imagem mais recente) ainda é um passo manual — os detalhes estão comentados no próprio workflow.

## 📱 App mobile

Existe também um cliente mobile construído em **Flutter**, consumindo essa mesma API via JWT — com as mesmas funcionalidades do site, incluindo drag-and-drop. Repositório separado: [Kanban App Mobile](#).

## 📚 Documentação da API

Todos os endpoints REST (autenticação, quadros, colunas, cartões — com exemplos de request/response) estão documentados em [`API-Kanban-Documentacao.md`](./API-Kanban-Documentacao.md).

## 🗂️ Estrutura do projeto

```
Controllers/
├── ContaController.cs        # Login, cadastro, logout (site)
├── PainelController.cs       # Dashboard do usuário (site)
├── QuadroController.cs       # CRUD de quadros/colunas/cartões (site)
└── Api/
    ├── AuthController.cs     # Login/cadastro com JWT
    ├── QuadrosController.cs
    ├── ColunasController.cs
    └── CartoesController.cs

Data/
├── Repositorios/             # Queries SQL via Dapper
└── Servicos/TokenService.cs  # Geração de tokens JWT

Models/          # Entidades (Usuario, Quadro, Coluna, Cartao)
ViewModels/      # Modelos compostos para as Views
Views/           # Razor
```

## 📌 Roadmap / pendências

- [ ] Persistência (volume) do PostgreSQL em produção
- [ ] Automatizar por completo o deploy final na Azure
- [ ] Domínio personalizado

---

<div align="center">
Feito como projeto de estudo — do banco de dados ao deploy em nuvem.
</div>
