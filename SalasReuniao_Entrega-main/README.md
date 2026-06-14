# Gestão de Salas de Reunião — ASP.NET Core (Minimal API) + JWT + React

Projeto desenvolvido para a Avaliação Prática (CRUD com Autenticação JWT) — Tópicos
Especiais, Prof. Escobar.

**Credenciais de acesso (usuário fixo):**
- Email: `teste@teste.com`
- Senha: `123`

---

## Estrutura do projeto

```
entrega/
├── SalasReuniaoApi/      -> Backend (.NET 7, Minimal API, EF Core, JWT)
│   ├── Program.cs
│   ├── Models/SalaReuniao.cs
│   ├── Data/AppDbContext.cs
│   ├── Auth/LoginRequest.cs
│   └── Migrations/        -> Migrations do EF Core (SQLite)
└── frontend/
    └── index.html          -> Frontend em React (sem build, via CDN)
```

---

## Pré-requisitos

- .NET 7 SDK instalado (https://dotnet.microsoft.com/download)
- Um navegador (Chrome, Edge, Firefox...) para abrir o `index.html`
- (Opcional) `dotnet-ef` para gerenciar migrations manualmente

Não é necessário instalar MySQL, SQL Server ou qualquer outro banco. O projeto usa
**SQLite**, que cria automaticamente um arquivo de banco de dados local
(`salas.db`) na primeira execução.

---

## 1. Banco de dados (SQLite)

Não há etapa manual de configuração de banco. O `Program.cs` já chama
`db.Database.Migrate()` na inicialização, então, ao rodar `dotnet run` pela primeira
vez, o arquivo `salas.db` (com a tabela `SalasReuniao`) é criado automaticamente
dentro da pasta `SalasReuniaoApi`.

Se quiser recriar o banco do zero, basta apagar o arquivo `salas.db` e rodar o
projeto novamente — ele será recriado.

(Opcional) Se preferir gerar/aplicar as migrations manualmente:
```bash
cd SalasReuniaoApi
dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet ef database update
```

---

## 2. Backend (SalasReuniaoApi)

```bash
cd SalasReuniaoApi
dotnet restore
dotnet run
```

A API ficará disponível em:
- API: `http://localhost:5000`
- Swagger (para testar as rotas): `http://localhost:5000/swagger`

### Rotas disponíveis

| Método | Rota          | Autenticação | Corpo (JSON)                                                 |
|--------|---------------|--------------|---------------------------------------------------------------|
| POST   | /login        | Não          | `{"email":"teste@teste.com","senha":"123"}` → retorna `{ "token": "..." }` |
| GET    | /salas        | Sim (Bearer) | -                                                               |
| GET    | /salas/{id}   | Sim (Bearer) | -                                                               |
| POST   | /salas        | Sim (Bearer) | `{"nome":"Sala A","capacidade":10,"possuiProjetor":true}`     |
| PUT    | /salas/{id}   | Sim (Bearer) | `{"nome":"Sala A","capacidade":12,"possuiProjetor":false}`    |
| DELETE | /salas/{id}   | Sim (Bearer) | -                                                               |

### Testando com o Swagger/Postman

1. Faça `POST /login` com `{"email":"teste@teste.com","senha":"123"}`.
2. Copie o `token` retornado.
3. No Swagger, clique em **Authorize** e digite `Bearer SEU_TOKEN`.
4. Agora as rotas `/salas` podem ser testadas normalmente.

---

## 3. Frontend (index.html)

O frontend é o arquivo `frontend/index.html`, em React puro via CDN (sem necessidade
de `npm install` ou build).

Com a API rodando em `http://localhost:5000`, basta abrir o arquivo `index.html`
diretamente no navegador (duplo clique ou "Abrir com" o navegador).

Se preferir servir por um servidor local (opcional, evita eventuais bloqueios do
navegador para `file://`):
```bash
cd frontend
npx serve .
```
e acesse o endereço indicado (ex.: `http://localhost:3000`).

### Usando a aplicação

1. Faça login com `teste@teste.com` / `123`.
2. A tela de "Gerenciar Salas de Reunião" será exibida.
3. Use o formulário para adicionar uma sala (Nome, Capacidade, Possui Projetor).
4. Use os botões **Editar** e **Excluir** na tabela para atualizar/remover salas.
5. Clique em **Sair** para encerrar a sessão (remove o token do `localStorage`).

---

## Resumo da ordem de execução

1. `cd SalasReuniaoApi && dotnet restore && dotnet run` (cria o banco automaticamente)
2. Abrir `frontend/index.html` no navegador
3. Login: `teste@teste.com` / `123`
4. Usar o CRUD de salas normalmente
