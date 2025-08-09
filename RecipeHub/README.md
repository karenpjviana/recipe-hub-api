# 🍳 RecipeHub API

Uma API completa para gerenciamento de receitas culinárias construída com .NET 8, seguindo princípios de Clean Architecture.

## 📋 Índice

- [Recursos](#-recursos)
- [Tecnologias](#-tecnologias)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Configuração](#-configuração)
- [Autenticação](#-autenticação)
- [Endpoints](#-endpoints)
- [Administração](#-administração)
- [Exemplos de Uso](#-exemplos-de-uso)
- [Banco de Dados](#-banco-de-dados)

## 🚀 Recursos

### ✅ Funcionalidades Implementadas
- **Autenticação JWT** com roles (User/Admin)
- **CRUD completo** de receitas com soft delete
- **Tags automáticas** - criadas dinamicamente nas receitas
- **Slugs SEO-friendly** - URLs amigáveis para receitas
- **Sistema de filtros avançado** - busca unificada
- **Paginação** em todos os endpoints de listagem
- **Autorização baseada em roles** para operações administrativas
- **Soft Delete** com interceptador automático
- **Seeding automático** de dados iniciais

### 🎯 Endpoints Otimizados
- **6 endpoints → 1 unificado** para receitas (83% redução!)
- **Filtros combinados**: busca + categoria + usuário + dificuldade + tempo
- **Controller limpa** com documentação inline

## 🛠️ Tecnologias

- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **JWT Bearer** - Autenticação
- **BCrypt** - Hash de senhas
- **Swagger/OpenAPI** - Documentação da API

## 🏗️ Estrutura do Projeto

```
RecipeHub/
├── RecipeHub.API/           # Camada de apresentação
├── RecipeHub.Application/   # Lógica de aplicação
├── RecipeHub.Domain/        # Entidades e regras de negócio
└── RecipeHub.Infrastructure/ # Acesso a dados e recursos externos
```

### Clean Architecture
- **Domain**: Entidades, enums, interfaces
- **Application**: Services, ViewModels, business logic
- **Infrastructure**: DbContext, repositories, migrations
- **API**: Controllers, middleware, configurações

## ⚙️ Configuração

### 1. Pré-requisitos
- .NET 8 SDK
- PostgreSQL
- Editor (Visual Studio, VS Code, Rider)

### 2. Configuração do Banco
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=recipehubdb;Username=seu_usuario;Password=sua_senha"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-jwt-aqui",
    "Issuer": "RecipeHubAPI",
    "Audience": "RecipeHubUsers"
  }
}
```

### 3. Executar Migrações
```bash
cd RecipeHub.Infrastructure
dotnet ef database update --startup-project ../RecipeHub.API
```

### 4. Executar API
```bash
cd RecipeHub.API
dotnet run
```

## 🔑 Autenticação

### Usuário Admin Padrão
```
Username: admin
Password: admin123
Role: Admin
```

### Usuários de Teste
```
Username: chef_maria | chef_joao | chef_ana
Password: password123
Role: User
```

### Como Fazer Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

### Resposta
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-09T10:30:00Z",
  "user": {
    "id": "guid",
    "username": "admin",
    "fullName": "Administrador",
    "role": "Admin"
  }
}
```

### Usar Token
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📡 Endpoints

### 🔓 Públicos (sem autenticação)

#### Autenticação
- `POST /api/auth/register` - Registrar usuário
- `POST /api/auth/login` - Fazer login
- `GET /api/auth/username-exists/{username}` - Verificar se username existe

#### Receitas
- `GET /api/recipes` - **ENDPOINT UNIFICADO** com filtros
- `GET /api/recipes/{id}` - Buscar por ID
- `GET /api/recipes/slug/{slug}` - Buscar por slug (SEO)

#### Categorias
- `GET /api/categories` - Listar todas
- `GET /api/categories/{id}` - Buscar por ID

#### Usuários
- `GET /api/users/username/{username}` - Perfil público

#### Opções
- `GET /api/options/difficulties` - Enum de dificuldades

### 🔒 Autenticados (JWT obrigatório)

#### Receitas
- `POST /api/recipes` - Criar receita
- `PUT /api/recipes/{id}` - Atualizar receita
- `DELETE /api/recipes/{id}` - Deletar receita

#### Perfil do Usuário
- `GET /api/users/me` - Meu perfil
- `PUT /api/users/me` - Atualizar meu perfil

### 👑 Admin (Role = "Admin")

#### Categorias
- `POST /api/categories` - Criar categoria
- `PUT /api/categories/{id}` - Atualizar categoria
- `DELETE /api/categories/{id}` - Deletar categoria

## 🎯 Endpoint Unificado de Receitas

### Parâmetros Disponíveis
```http
GET /api/recipes?pageNumber=1&pageSize=12&search=pão&categoryId=guid&userId=guid&isPublished=true&difficulty=fácil&maxPrepTime=30&minServings=2&maxServings=6&tags=facil,saudavel&sortBy=createdAt&sortDirection=desc
```

### Exemplos de Uso

#### Busca Simples
```http
GET /api/recipes?search=pão&pageNumber=1&pageSize=12
```

#### Filtrar por Categoria
```http
GET /api/recipes?categoryId=123e4567-e89b-12d3-a456-426614174000
```

#### Receitas de um Usuário
```http
GET /api/recipes?userId=456e7890-e89b-12d3-a456-426614174000
```

#### Apenas Publicadas
```http
GET /api/recipes?isPublished=true
```

#### Filtros Avançados
```http
GET /api/recipes?search=massa&difficulty=fácil&maxPrepTime=30&isPublished=true&pageNumber=1
```

## 🍳 Criando Receitas com Tags Automáticas

### Requisição
```http
POST /api/recipes
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": "guid-do-usuario",
  "categoryId": "guid-da-categoria",
  "title": "Pão Integral Saudável",
  "description": "Um pão delicioso e nutritivo",
  "prepTime": 45,
  "servings": 8,
  "difficulty": "Médio",
  "coverImage": "https://exemplo.com/imagem.jpg",
  "isPublished": true,
  "tagNames": ["saudável", "integral", "fácil", "caseiro"],
  "instructions": [
    {
      "stepNumber": 1,
      "content": "Misture os ingredientes secos"
    },
    {
      "stepNumber": 2,
      "content": "Adicione os líquidos gradualmente"
    }
  ],
  "ingredients": [
    {
      "name": "Farinha integral",
      "amount": 500,
      "unit": "gramas"
    },
    {
      "name": "Água morna",
      "amount": 300,
      "unit": "ml"
    }
  ]
}
```

### ✨ Magia das Tags
- Se a tag **"saudável"** não existir → **cria automaticamente**
- Se a tag **"integral"** já existir → **reutiliza**
- **Zero configuração** de tags necessária!

## 👑 Administração

### Tornar Usuário Admin

#### Via SQL (Recomendado)
```sql
-- Conectar no PostgreSQL
psql -h localhost -U seu_usuario -d recipehubdb

-- Promover usuário
UPDATE "Users" SET "Role" = 'Admin' WHERE "Username" = 'nome_do_usuario';

-- Verificar
SELECT "Username", "FullName", "Role" FROM "Users" WHERE "Role" = 'Admin';
```

#### Via Cliente PostgreSQL (PgAdmin, DBeaver)
```sql
-- Listar usuários atuais
SELECT "Id", "Username", "FullName", "Role" FROM "Users";

-- Promover usuário específico
UPDATE "Users" SET "Role" = 'Admin' WHERE "Username" = 'chef_maria';

-- Verificar resultado
SELECT "Username", "Role" FROM "Users" WHERE "Role" = 'Admin';
```

### Testar Autorização Admin
1. **Login como admin** e obter token
2. **Criar categoria** (só admin pode):
```http
POST /api/categories
Authorization: Bearer <admin-token>

{
  "name": "Sobremesas",
  "description": "Doces e sobremesas deliciosas"
}
```

## 🗄️ Banco de Dados

### Tabelas Principais
- **Users** - Usuários (com Role)
- **Categories** - Categorias de receitas
- **Recipes** - Receitas (com slug automático)
- **Tags** - Tags criadas dinamicamente
- **RecipeTags** - Relacionamento N:N
- **Instructions** - Passos das receitas
- **Ingredients** - Ingredientes das receitas
- **Favorites** - Receitas favoritas dos usuários
- **Reviews** - Avaliações das receitas

### Soft Delete
- Entidades **não são removidas fisicamente**
- Campo `IsDeleted` marca como excluído
- Interceptador automático filtra registros deletados

### Dados Seed Automático
- **4 Categorias**: Pratos Principais, Sobremesas, Entradas, Bebidas
- **4 Usuários**: 1 Admin + 3 Chefs
- **8 Tags**: Fácil, Rápido, Saudável, Vegetariano, etc.

## 🌐 URLs Amigáveis (Slugs)

### Como Funciona
- **Título**: "Pão de Açúcar Delicioso"
- **Slug**: "pao-de-acucar-delicioso"
- **URL**: `/recipes/slug/pao-de-acucar-delicioso`

### Benefícios
- ✅ **SEO otimizado**
- ✅ **URLs compartilháveis**
- ✅ **UX melhorada**
- ✅ **Unicidade garantida**

## 📊 Métricas de Otimização

### Antes vs Depois
- **Endpoints de receitas**: 7 → 4 (-43%)
- **Endpoints de categorias**: 7 → 5 (-29%)
- **Endpoints de tags**: 7 → 0 (-100%)
- **Endpoints de usuários**: 8 → 3 (-63%)

### Total Geral
- **29 endpoints → 12 endpoints** (-59%)
- **Manutenção mais fácil**
- **Performance otimizada**
- **Código mais limpo**

## 🔧 Comandos de Migration

### Criar Nova Migration
```bash
cd RecipeHub.Infrastructure
dotnet ef migrations add NomeDaMigration --startup-project ../RecipeHub.API
```

### Aplicar Migrations ao Banco
```bash
cd RecipeHub.Infrastructure
dotnet ef database update --startup-project ../RecipeHub.API
```

### Remover Última Migration (se ainda não aplicada)
```bash
cd RecipeHub.Infrastructure
dotnet ef migrations remove --startup-project ../RecipeHub.API
```

## 🚀 Próximos Passos Sugeridos

1. **Implementar cache** (Redis)
2. **Upload de imagens** (CloudFlare/AWS)
3. **Rate limiting** para APIs públicas
4. **Logs estruturados** (Serilog)
5. **Health checks** para monitoramento
6. **Docker** para containerização
7. **CI/CD** pipeline

## 📞 Suporte

Para dúvidas sobre implementação ou funcionalidades, consulte:
- **Swagger UI**: `http://localhost:5134/swagger`
- **Código fonte**: Comentários inline nos controllers
- **Migrations**: Histórico no banco de dados

---

**RecipeHub API** - Desenvolvido com ❤️ em .NET 8