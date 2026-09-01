# Documentação da API — Kanban App

Base URL (local): `http://localhost:8080`
Base URL (produção): `https://kanban-app.bluewave-06d366a7.chilecentral.azurecontainerapps.io`

## Autenticação

A maioria dos endpoints exige um token JWT. Depois de fazer login ou cadastro, você recebe um `token`. Envie esse token em **todas** as requisições protegidas, no cabeçalho:

```
Authorization: Bearer SEU_TOKEN_AQUI
```

O token expira em **7 dias**. Depois disso, é preciso fazer login novamente.

---

## 1. Autenticação (`/api/auth`)

### Cadastrar usuário
Cria um novo usuário e já devolve um token (login automático).

- **Método:** `POST`
- **Rota:** `/api/auth/cadastro`
- **Autenticação:** não precisa
- **Body (JSON):**
```json
{
  "nome": "Seu Nome",
  "email": "seuemail@teste.com",
  "senha": "123456"
}
```
- **Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "usuarioId": 1,
  "nome": "Seu Nome",
  "email": "seuemail@teste.com"
}
```
- **Erros possíveis:**
  - `400 Bad Request` — email já cadastrado

---

### Login
Autentica um usuário existente e devolve um token.

- **Método:** `POST`
- **Rota:** `/api/auth/login`
- **Autenticação:** não precisa
- **Body (JSON):**
```json
{
  "email": "seuemail@teste.com",
  "senha": "123456"
}
```
- **Resposta (200 OK):** igual ao cadastro (token + dados do usuário)
- **Erros possíveis:**
  - `401 Unauthorized` — email ou senha inválidos

---

### Ver dados do usuário logado (teste de autenticação)
Endpoint simples para confirmar que o token está válido.

- **Método:** `GET`
- **Rota:** `/api/auth/eu`
- **Autenticação:** obrigatória (Bearer)
- **Resposta (200 OK):**
```json
{
  "usuarioId": "1",
  "nome": "Seu Nome",
  "email": "seuemail@teste.com"
}
```
- **Erros possíveis:**
  - `401 Unauthorized` — sem token ou token inválido/expirado

---

## 2. Quadros (`/api/quadros`)

Todos os endpoints abaixo exigem autenticação (Bearer).

### Listar meus quadros
Lista todos os kanbans que o usuário logado participa (como dono ou espectador).

- **Método:** `GET`
- **Rota:** `/api/quadros`
- **Resposta (200 OK):**
```json
[
  {
    "id": 1,
    "nome": "Meu Kanban",
    "usuarioDonoId": 2,
    "codigoCompartilhamento": "AB3F9K",
    "papel": "dono"
  }
]
```

---

### Criar quadro
Cria um novo kanban. O usuário logado vira automaticamente o dono.

- **Método:** `POST`
- **Rota:** `/api/quadros`
- **Body (JSON):**
```json
{
  "nome": "Nome do Kanban"
}
```
- **Resposta (200 OK):**
```json
{
  "quadroId": 3,
  "codigo": "SYYPF9"
}
```

---

### Entrar com código
Entra em um kanban de outra pessoa usando o código de compartilhamento. O usuário vira espectador (somente leitura).

- **Método:** `POST`
- **Rota:** `/api/quadros/entrar`
- **Body (JSON):**
```json
{
  "codigo": "SYYPF9"
}
```
- **Resposta (200 OK):**
```json
{
  "quadroId": 3,
  "nome": "Nome do Kanban"
}
```
- **Erros possíveis:**
  - `404 Not Found` — código não encontrado
  - `400 Bad Request` — já é membro deste kanban (inclui o dono tentando entrar no próprio)

---

### Ver detalhes de um quadro
Devolve o quadro completo: colunas, cartões de cada coluna, e lista de membros.

- **Método:** `GET`
- **Rota:** `/api/quadros/{id}`
- **Resposta (200 OK):**
```json
{
  "quadro": {
    "id": 3,
    "nome": "Nome do Kanban",
    "usuarioDonoId": 2,
    "codigoCompartilhamento": "SYYPF9",
    "papel": ""
  },
  "papel": "dono",
  "colunas": [
    {
      "coluna": { "id": 5, "quadroId": 3, "nome": "A Fazer" },
      "cartoes": [
        { "id": 10, "colunaId": 5, "titulo": "Tarefa 1", "descricao": null, "ordem": 0 }
      ]
    }
  ],
  "membros": [
    { "nome": "Seu Nome", "papel": "dono" }
  ]
}
```
> Nota: o campo `papel` dentro de `quadro` fica sempre vazio — o campo correto para saber a permissão do usuário logado é o `papel` no nível principal do JSON.
- **Erros possíveis:**
  - `404 Not Found` — quadro não existe
  - `403 Forbidden` — usuário não é membro deste quadro

---

### Sair do quadro
Só funciona para espectadores (donos não podem sair do próprio kanban).

- **Método:** `POST`
- **Rota:** `/api/quadros/{id}/sair`
- **Resposta (200 OK):**
```json
{ "mensagem": "Você saiu do kanban." }
```
- **Erros possíveis:**
  - `400 Bad Request` — usuário é dono (não pode sair)

---

### Excluir quadro
Só o dono pode excluir. Remove o quadro e tudo relacionado (colunas, cartões, membros).

- **Método:** `DELETE`
- **Rota:** `/api/quadros/{id}`
- **Resposta (200 OK):**
```json
{ "mensagem": "Kanban excluído." }
```
- **Erros possíveis:**
  - `403 Forbidden` — usuário não é dono

---

## 3. Colunas (`/api/colunas`)

Todos exigem autenticação e permissão de **dono** do quadro correspondente.

### Criar coluna

- **Método:** `POST`
- **Rota:** `/api/colunas`
- **Body (JSON):**
```json
{
  "quadroId": 3,
  "nome": "A Fazer"
}
```
- **Resposta (200 OK):**
```json
{ "colunaId": 5 }
```
- **Erros possíveis:**
  - `403 Forbidden` — usuário não é dono do quadro

---

### Editar (renomear) coluna

- **Método:** `PUT`
- **Rota:** `/api/colunas/{id}`
- **Body (JSON):**
```json
{
  "nome": "Em Andamento"
}
```
- **Resposta (200 OK):**
```json
{ "mensagem": "Coluna atualizada." }
```
- **Erros possíveis:**
  - `404 Not Found` — coluna não existe
  - `403 Forbidden` — usuário não é dono do quadro

---

### Excluir coluna
Remove a coluna e todos os cartões dela (em cascata).

- **Método:** `DELETE`
- **Rota:** `/api/colunas/{id}`
- **Resposta (200 OK):**
```json
{ "mensagem": "Coluna excluída." }
```
- **Erros possíveis:**
  - `404 Not Found` — coluna não existe
  - `403 Forbidden` — usuário não é dono do quadro

---

## 4. Cartões (`/api/cartoes`)

Todos exigem autenticação e permissão de **dono** do quadro correspondente.
> Este Controller aceita tanto Cookie (usado pelo site) quanto Bearer/JWT (usado pela API/mobile).

### Criar cartão

- **Método:** `POST`
- **Rota:** `/api/cartoes`
- **Body (JSON):**
```json
{
  "colunaId": 5,
  "titulo": "Nova tarefa",
  "descricao": "Detalhes opcionais (pode ser null)"
}
```
- **Resposta (200 OK):**
```json
{ "cartaoId": 10 }
```
- **Erros possíveis:**
  - `404 Not Found` — coluna não existe
  - `403 Forbidden` — usuário não é dono do quadro

---

### Editar cartão (título e descrição)

- **Método:** `PUT`
- **Rota:** `/api/cartoes/{id}`
- **Body (JSON):**
```json
{
  "titulo": "Título editado",
  "descricao": "Nova descrição"
}
```
- **Resposta (200 OK):**
```json
{ "mensagem": "Cartão atualizado." }
```
- **Erros possíveis:**
  - `404 Not Found` — cartão não existe
  - `403 Forbidden` — usuário não é dono do quadro

---

### Mover cartão (drag-and-drop)
Move um cartão para outra coluna e/ou outra posição.

- **Método:** `PUT`
- **Rota:** `/api/cartoes/{id}/mover`
- **Body (JSON):**
```json
{
  "novaColunaId": 6,
  "novaOrdem": 0
}
```
- **Resposta (200 OK):** vazio (`200 OK` sem corpo)
- **Erros possíveis:**
  - `404 Not Found` — cartão não existe
  - `403 Forbidden` — usuário não é dono do quadro

---

### Excluir cartão

- **Método:** `DELETE`
- **Rota:** `/api/cartoes/{id}`
- **Resposta (200 OK):**
```json
{ "mensagem": "Cartão excluído." }
```
- **Erros possíveis:**
  - `404 Not Found` — cartão não existe
  - `403 Forbidden` — usuário não é dono do quadro

---

## Referência rápida de todos os endpoints

| Método | Rota | Autenticação | Descrição |
|--------|------|---------------|-----------|
| POST | `/api/auth/cadastro` | Não | Cadastrar novo usuário |
| POST | `/api/auth/login` | Não | Login |
| GET | `/api/auth/eu` | Bearer | Ver dados do usuário logado |
| GET | `/api/quadros` | Bearer | Listar meus quadros |
| POST | `/api/quadros` | Bearer | Criar quadro |
| POST | `/api/quadros/entrar` | Bearer | Entrar com código |
| GET | `/api/quadros/{id}` | Bearer | Ver detalhes do quadro |
| POST | `/api/quadros/{id}/sair` | Bearer | Sair do quadro (espectador) |
| DELETE | `/api/quadros/{id}` | Bearer | Excluir quadro (dono) |
| POST | `/api/colunas` | Bearer | Criar coluna |
| PUT | `/api/colunas/{id}` | Bearer | Renomear coluna |
| DELETE | `/api/colunas/{id}` | Bearer | Excluir coluna |
| POST | `/api/cartoes` | Cookie ou Bearer | Criar cartão |
| PUT | `/api/cartoes/{id}` | Cookie ou Bearer | Editar cartão |
| PUT | `/api/cartoes/{id}/mover` | Cookie ou Bearer | Mover cartão (drag-and-drop) |
| DELETE | `/api/cartoes/{id}` | Cookie ou Bearer | Excluir cartão |

---

## Como testar no Postman (passo a passo)

1. Crie uma nova requisição, escolha o método (GET/POST/PUT/DELETE)
2. Cole a URL (ex: `http://localhost:8080/api/quadros`)
3. Se o endpoint precisar de Body: aba **Body** → **raw** → tipo **JSON** → cole o JSON de exemplo
4. Se o endpoint precisar de token: aba **Headers** → adicione `Authorization` como Key e `Bearer SEU_TOKEN` como Value
5. Clique **Send**
