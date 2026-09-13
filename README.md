# AcademiaDoZe

Projeto acadêmico de desenvolvimento em C# com persistência em SQL Server.

## SGBD da entrega

Esta versão está preparada para **SQL Server executado em Docker**, conforme a orientação atual da atividade.

## Executar

```bash
docker compose up -d
dotnet restore
dotnet build
dotnet test
```

Connection string padrão dos testes:

```text
Server=localhost,1433;Initial Catalog=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=False;
```

É possível substituir a connection string pela variável de ambiente `ACADEMIA_DO_ZE_CONNECTION`.

## Atividade de Matrícula

A implementação de `MatriculaRepository` e `MatriculaInfrastructureTests` contempla todos os métodos definidos em `IMatriculaRepository`. Os testes utilizam SQL parametrizado, `await using`, mapeamento via `DbDataReader` e funções específicas do SQL Server.

Nos dados de teste:
- `objetivo` = `Matheus Kautnick Domeneghini`
- `obs_restricao` contém `SQLServer`

## Verificação no banco

No SQL Server Management Studio, após executar os testes:

```sql
USE db_academia_do_ze;
SELECT * FROM dbo.tb_matricula ORDER BY id_matricula DESC;
```
