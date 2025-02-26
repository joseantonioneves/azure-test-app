# AzureTestApp - Teste: Projeto .NET 8.0 com Azure Entra ID e React

Este projeto é uma aplicação web que utiliza uma API backend em .NET 8.0 com autenticação Azure Entra ID e um frontend em React. O objetivo é exibir informações do usuário autenticado e do tenant, como nome, ID do tenant, tentativas de login recentes, usuários e grupos do tenant.

## Estrutura do Projeto

O projeto está organizado da seguinte forma:

```
AzureTestApp/
├── Backend/               # Pasta do projeto backend (API .NET)
│   ├── AzureTestApp.Api/  # Projeto Web API
│   │   ├── Controllers/   # Controllers da API
│   │   ├── Program.cs     # Ponto de entrada da aplicação
│   │   ├── appsettings.json # Configurações da aplicação
│   │   ├── AzureTestApp.Api.csproj # Arquivo do projeto
├── Frontend/              # Pasta do projeto frontend (React)
│   ├── azure-test-app/    # Projeto React
│   │   ├── src/           # Código-fonte do frontend
│   │   │   ├── components/ # Componentes React
│   │   │   ├── App.tsx    # Componente principal
│   │   │   ├── index.tsx  # Ponto de entrada do React
│   │   ├── package.json   # Dependências do frontend
│   │   ├── tsconfig.json  # Configuração do TypeScript
```

## Pré-requisitos

Antes de começar, certifique-se de ter os seguintes softwares instalados:

- **.NET SDK 8.0**: [Download .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js**: [Download Node.js](https://nodejs.org/)
- **Visual Studio Code**: [Download VSCode](https://code.visualstudio.com/)
- **Azure CLI** (opcional, para configuração do Azure Entra ID): [Install Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)

## A receita do bolo ...

### 1. Configuração do Backend (.NET 8.0)

#### 1.1. Criar o Projeto Backend

1. Abra o terminal e navegue até a pasta `Backend`:

   ```bash
   cd AzureTestApp/Backend
   ```

2. Crie um novo projeto Web API:

   ```bash
   dotnet new webapi -n AzureTestApp.Api
   ```

3. Navegue até a pasta do projeto:

   ```bash
   cd AzureTestApp.Api
   ```

#### 1.2. Adicionar Pacotes Necessários

1. Adicione os pacotes para autenticação com Azure Entra ID:

   ```bash
   dotnet add package Microsoft.Identity.Web
   dotnet add package Microsoft.Identity.Web.UI
   ```

#### 1.3. Configurar o Azure Entra ID

1. No arquivo `appsettings.json`, adicione as configurações do Azure Entra ID:

   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "seu-dominio.onmicrosoft.com",
       "TenantId": "seu-tenant-id",
       "ClientId": "seu-client-id",
       "CallbackPath": "/signin-oidc"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     },
     "AllowedHosts": "*"
   }
   ```

   - Substitua `seu-dominio.onmicrosoft.com`, `seu-tenant-id` e `seu-client-id` pelas informações do seu Azure Entra ID.

2. No arquivo `Program.cs`, configure a autenticação:

   ```csharp
   using Microsoft.Identity.Web;

   var builder = WebApplication.CreateBuilder(args);

   // Add services to the container.
   builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");

   builder.Services.AddControllers();
   builder.Services.AddEndpointsApiExplorer();
   builder.Services.AddSwaggerGen();

   var app = builder.Build();

   // Configure the HTTP request pipeline.
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();
   }

   app.UseHttpsRedirection();

   app.UseAuthentication();
   app.UseAuthorization();

   app.MapControllers();

   app.Run();
   ```

#### 1.4. Criar o Controller para Exibir Informações do Usuário

1. Crie um novo controller chamado `UserController.cs`:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Mvc;
   using System.Security.Claims;

   namespace AzureTestApp.Api.Controllers
   {
       [Authorize]
       [ApiController]
       [Route("api/[controller]")]
       public class UserController : ControllerBase
       {
           [HttpGet]
           public IActionResult GetUserInfo()
           {
               var userName = User.Identity.Name;
               var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

               var userInfo = new
               {
                   Name = userName,
                   TenantId = tenantId
               };

               return Ok(userInfo);
           }

           [HttpGet("tenant-info")]
           public IActionResult GetTenantInfo()
           {
               var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

               var tenantInfo = new
               {
                   TenantId = tenantId,
                   RecentLoginAttempts = new[] { "2023-10-01", "2023-10-02" },
                   Users = new[] { "user1", "user2" },
                   Groups = new[] { "group1", "group2" }
               };

               return Ok(tenantInfo);
           }
       }
   }
   ```

#### 1.5. Executar o Backend

1. No terminal, execute o projeto:

   ```bash
   dotnet run
   ```

   O backend estará disponível em `http://localhost:5000`.

---

### 2. Configuração do Frontend (React)

#### 2.1. Criar o Projeto Frontend

1. Navegue até a pasta `Frontend`:

   ```bash
   cd AzureTestApp/Frontend
   ```

2. Crie um novo projeto React com TypeScript:

   ```bash
   npx create-react-app azure-test-app --template typescript
   ```

3. Navegue até a pasta do projeto:

   ```bash
   cd azure-test-app
   ```

#### 2.2. Instalar Axios

1. Instale o Axios para fazer chamadas à API:

   ```bash
   npm install axios
   ```

#### 2.3. Criar o Componente para Exibir Informações do Usuário

1. Crie um novo componente chamado `UserInfo.tsx`:

   ```tsx
   import React, { useEffect, useState } from 'react';
   import axios from 'axios';

   const UserInfo = () => {
       const [userInfo, setUserInfo] = useState<{ Name: string, TenantId: string } | null>(null);
       const [tenantInfo, setTenantInfo] = useState<{ TenantId: string, RecentLoginAttempts: string[], Users: string[], Groups: string[] } | null>(null);

       useEffect(() => {
           const fetchUserInfo = async () => {
               try {
                   const userResponse = await axios.get('/api/user');
                   setUserInfo(userResponse.data);

                   const tenantResponse = await axios.get('/api/user/tenant-info');
                   setTenantInfo(tenantResponse.data);
               } catch (error) {
                   console.error('Erro ao buscar informações:', error);
               }
           };

           fetchUserInfo();
       }, []);

       return (
           <div>
               {userInfo ? (
                   <div>
                       <h2>Informações do Usuário</h2>
                       <p>Nome: {userInfo.Name}</p>
                       <p>Tenant ID: {userInfo.TenantId}</p>
                   </div>
               ) : (
                   <p>Carregando informações do usuário...</p>
               )}

               {tenantInfo ? (
                   <div>
                       <h2>Informações do Tenant</h2>
                       <p>Tenant ID: {tenantInfo.TenantId}</p>
                       <p>Tentativas de Login Recentes: {tenantInfo.RecentLoginAttempts.join(', ')}</p>
                       <p>Usuários: {tenantInfo.Users.join(', ')}</p>
                       <p>Grupos: {tenantInfo.Groups.join(', ')}</p>
                   </div>
               ) : (
                   <p>Carregando informações do tenant...</p>
               )}
           </div>
       );
   };

   export default UserInfo;
   ```

#### 2.4. Configurar o Proxy para a API

1. No arquivo `package.json`, adicione a configuração do proxy:

   ```json
   "proxy": "http://localhost:5000"
   ```

#### 2.5. Usar o Componente no App

1. Abra o arquivo `App.tsx` e use o componente `UserInfo`:

   ```tsx
   import React from 'react';
   import UserInfo from './components/UserInfo';

   const App = () => {
       return (
           <div>
               <h1>Azure Test App</h1>
               <UserInfo />
           </div>
       );
   };

   export default App;
   ```

#### 2.6. Executar o Frontend

1. No terminal, execute o projeto:

   ```bash
   npm start
   ```

   O frontend estará disponível em `http://localhost:3000`.

---

### 3. Testar a Aplicação

1. Acesse `http://localhost:3000` no navegador.
2. Faça login com uma conta do Azure Entra ID.
3. Verifique se as informações do usuário e do tenant são exibidas corretamente.

---

### 4. Instalação no Windows e Ubuntu

#### Windows

- Instalar .NET SDK: Baixe e instale a partir do site oficial.
- Instalar Node.js: Baixe e instale a partir do site oficial.
- Istalar as  seguintes bibliotecas:
```bash
   dotnet add package Microsoft.Identity.Web
   dotnet add package Microsoft.Identity.Web.UI
   ```

- Executar o backend:

  ```cmd
  dotnet run
  ```

- Executar o frontend:

  ```cmd
  npm start
  ```

#### Ubuntu

- Instalar .NET SDK:

  ```bash
  wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --version 8.0.0
  ```

- Instalar Node.js:

  ```bash
  sudo apt update
  sudo apt install nodejs npm
  ```

- Executar o backend:

  ```bash
  dotnet run
  ```

- Executar o frontend:

  ```bash
  npm start
  ```

---

### 5. Finalmente ...

Este projeto demonstra como integrar uma API .NET 8.0 com Azure Entra ID e um frontend React. Ele exibe informações do usuário autenticado e do tenant. Siga os passos acima para configurar, executar e testar a aplicação.
