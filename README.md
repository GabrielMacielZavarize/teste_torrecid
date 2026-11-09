# Gerenciamento de Estoque

Aplicação para cadastro de produtos e controle de entradas, saídas, reservas, ajustes e histórico de estoque, com login e permissões por papel.

Tecnologias: .NET 10 (Web API), Angular 22, PostgreSQL e Docker.

## Como rodar

Com o Docker em execução, na raiz do repositório:

```bash
docker compose up --build
```

A primeira execução leva alguns minutos. As tabelas e os usuários de teste são criados automaticamente.

- Aplicação: http://localhost:4200
- Swagger: http://localhost:5047/swagger

A API roda em modo `Development` para facilitar a avaliação. Credenciais e segredos do repositório são apenas para uso local.

## Usuários de teste

| Usuário | Senha | Papel |
|---|---|---|
| admin | Admin@12345 | Administrador |
| operador | Operador@12345 | Operador |
| consultor | Consultor@12345 | Consultor |

## Testes

Requer o .NET SDK 10:

```bash
dotnet test backend/InventoryManagement.slnx
```
