---

# Trybe Hotel (API .NET 6)

Projeto desenvolvido a partir de um _boilerplate_ fornecido pela Trybe e posteriormente estendido/adaptado por mim para uso em portfólio pessoal.

É uma API REST em .NET 6 para gestão de **cidades**, **hotéis** e **quartos**, usando **Entity Framework Core** com **SQL Server** em container Docker. A API expõe endpoints para listar, criar e remover recursos, e conta com testes de integração cobrindo os fluxos principais.

---

## O que foi desenvolvido por mim

Sobre o código base disponibilizado pela Trybe, foram implementados e/ou ajustados por mim:

- **Modelagem e contexto de dados**
  - Implementação das models `City`, `Hotel` e `Room` com chaves primárias, estrangeiras e propriedades de navegação.
  - Configuração do `TrybeHotelContext`:
    - Mapeamento de relacionamentos entre `City`–`Hotel` e `Hotel`–`Room`.
    - Configuração da connection string com suporte a variável de ambiente `TRYBEHOTEL_CONNECTION` e valor padrão para ambiente local (SQL Server em Docker).

- **Camada de API (Controllers)**
  - `CityController`:
    - `GET /city`
    - `POST /city`
  - `HotelController`:
    - `GET /hotel`
    - `POST /hotel`
  - `RoomController`:
    - `GET /room/{hotelId}`
    - `POST /room`
    - `DELETE /room/{roomId}` com tratamento de sucesso (`204`) e não encontrado (`404`).

- **Camada de repositórios**
  - Implementação da lógica de acesso a dados nos repositórios:
    - `CityRepository` (`GetCities`, `AddCity`);
    - `HotelRepository` (`GetHotels`, `AddHotel`);
    - `RoomRepository` (`GetRooms`, `AddRoom`, `DeleteRoom`);
  - Projeções para DTOs (`CityDto`, `HotelDto`, `RoomDto`) utilizando LINQ.

- **Testes de integração**
  - Ampliação dos testes de integração em `src/TrybeHotel.Test/IntegrationTest.cs`, usando `WebApplicationFactory<Program>` e um contexto em memória:
    - Testes para `GET /city`, `POST /city`;
    - Testes para `GET /hotel`, `POST /hotel`;
    - Testes para `GET /room/{hotelId}`, `POST /room`;
    - Testes para `DELETE /room/{roomId}` cobrindo tanto o fluxo de exclusão bem-sucedida (`204`) quanto o caso de quarto inexistente (`404`);
    - Testes validando não só status code, mas também conteúdo de respostas e persistência de dados (POST seguido de GET).

---

## Tecnologias utilizadas

- C# / .NET 6  
- ASP.NET Core Web API  
- Entity Framework Core  
- SQL Server (Azure SQL Edge em Docker)  
- xUnit  
- Docker + Docker Compose v2  

---

## Arquitetura do projeto

O projeto segue uma estrutura em camadas:

- `Controllers/`  
  Contém os controllers da API:
  - `CityController` – endpoints para cidades (`/city`);
  - `HotelController` – endpoints para hotéis (`/hotel`);
  - `RoomController` – endpoints para quartos (`/room`).

- `Models/`  
  Models mapeadas para o banco de dados via EF Core:
  - `City` – tabela `Cities`;
  - `Hotel` – tabela `Hotels`;
  - `Room` – tabela `Rooms`.

- `Dto/`  
  Objetos de transferência de dados usados como resposta da API:
  - `CityDto`, `HotelDto`, `RoomDto`.

- `Repository/`  
  Contém:
  - Interfaces: `ICityRepository`, `IHotelRepository`, `IRoomRepository`, `ITrybeHotelContext`;
  - Implementações concretas: `CityRepository`, `HotelRepository`, `RoomRepository`;
  - Contexto de banco: `TrybeHotelContext`.

- `TrybeHotel.Test/`  
  Projeto de testes (xUnit) com testes de **integração**:
  - `IntegrationTest.cs` – testa os endpoints reais da API usando um contexto em memória.

---

## Banco de dados

O banco é um SQL Server rodando em container Docker (Azure SQL Edge).  
O modelo relacional é:

- **Cities**
  - `CityId` (PK, int, identity)
  - `Name` (nvarchar)

- **Hotels**
  - `HotelId` (PK, int, identity)
  - `Name` (nvarchar)
  - `Address` (nvarchar)
  - `CityId` (FK para `Cities.CityId`)

- **Rooms**
  - `RoomId` (PK, int, identity)
  - `Name` (nvarchar)
  - `Capacity` (int)
  - `Image` (nvarchar)
  - `HotelId` (FK para `Hotels.HotelId`)

Relacionamentos:

- Uma `City` possui vários `Hotels`;
- Um `Hotel` possui vários `Rooms`.

---

## Configuração da conexão com o banco

O `TrybeHotelContext` configura o `UseSqlServer` em `OnConfiguring`, permitindo sobrescrever a connection string via variável de ambiente:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("TRYBEHOTEL_CONNECTION")
            ?? "Server=localhost;Database=TrybeHotel;User=SA;Password=TrybeHotel12!;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);
    }
}

## Como rodar o projeto localmente

### Pré-requisitos

- .NET 6 SDK  
- Docker + Docker Compose v2 (`docker compose`)  

### 1. Clonar o repositório

    git clone https://github.com/d4n13ln13ls3n/Trybe-Hotel.git
    cd Trybe-Hotel

### 2. Subir o banco de dados com Docker

Na raiz do repositório:

    docker compose up -d --build

Isso sobe um container SQL Server (Azure SQL Edge) exposto na porta `1433`.

### 3. Criar o banco e as tabelas

Conecte no container e no SQL Server:

    docker exec -it trybe_hotel_db /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "TrybeHotel12!"

No prompt do `sqlcmd`, crie o banco e as tabelas:

    CREATE DATABASE TrybeHotel;
    GO
    USE TrybeHotel;
    GO

    CREATE TABLE Cities (CityId INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(255) NULL);
    GO

    CREATE TABLE Hotels (HotelId INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(255) NULL, Address NVARCHAR(255) NULL, CityId INT NOT NULL, CONSTRAINT FK_Hotels_Cities FOREIGN KEY (CityId) REFERENCES Cities (CityId));
    GO

    CREATE TABLE Rooms (RoomId INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(255) NULL, Capacity INT NOT NULL, Image NVARCHAR(255) NULL, HotelId INT NOT NULL, CONSTRAINT FK_Rooms_Hotels FOREIGN KEY (HotelId) REFERENCES Hotels (HotelId));
    GO

    EXIT

### 4. Restaurar dependências e rodar a API

    cd src/TrybeHotel
    dotnet restore
    dotnet run

A aplicação deve subir em algo como:

- `http://localhost:5107`
- `https://localhost:7018`

---

## Endpoints principais

### Cidades

- `GET /city`  
  Lista todas as cidades.

- `POST /city`  
  Cria uma nova cidade.

  **Request body:**

      {
        "name": "Manaus"
      }

  **Response (201 Created):**

      {
        "cityId": 1,
        "name": "Manaus"
      }

---

### Hotéis

- `GET /hotel`  
  Lista todos os hotéis.

- `POST /hotel`  
  Cria um novo hotel vinculado a uma cidade.

  **Request body:**

      {
        "name": "Trybe Hotel Manaus",
        "address": "Endereço 1",
        "cityId": 1
      }

  **Response (201 Created):**

      {
        "hotelId": 1,
        "name": "Trybe Hotel Manaus",
        "address": "Endereço 1",
        "cityId": 1,
        "cityName": "Manaus"
      }

---

### Quartos

- `GET /room/{hotelId}`  
  Lista todos os quartos de um hotel específico.

- `POST /room`  
  Cria um novo quarto vinculado a um hotel.

  **Request body:**

      {
        "name": "Quarto 1",
        "capacity": 2,
        "image": "img",
        "hotelId": 1
      }

  **Response (201 Created):**

      {
        "roomId": 1,
        "name": "Quarto 1",
        "capacity": 2,
        "image": "img",
        "hotel": {
          "hotelId": 1,
          "name": "Trybe Hotel Manaus",
          "address": "Endereço 1",
          "cityId": 1,
          "cityName": "Manaus"
        }
      }

- `DELETE /room/{roomId}`  
  Remove um quarto específico.

  - **204 No Content** se o quarto existir e for deletado;
  - **404 Not Found** se o quarto não existir (tratado no `RoomController` com `try/catch`).

---

## Testes

Os testes foram implementados em `src/TrybeHotel.Test/IntegrationTest.cs`, usando xUnit e `WebApplicationFactory<Program>`.

Os testes de integração cobrem:

- `GET /city`, `POST /city`;
- `GET /hotel`, `POST /hotel`;
- `GET /room/{hotelId}`;
- `POST /room`;
- `DELETE /room/{roomId}` (fluxo de sucesso e de erro / 404);
- Verificações de status code e, em vários casos, do conteúdo retornado (por exemplo, garantir que um recurso criado aparece posteriormente no `GET` correspondente).

### Como executar os testes

Na pasta do projeto de testes:

    cd src/TrybeHotel.Test
    dotnet test

Para rodar apenas os testes marcados com um `Trait` específico, por exemplo:

    dotnet test --filter "Category=Meus testes"
