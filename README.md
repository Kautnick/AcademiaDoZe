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

## Aplicação .NET MAUI

O projeto `AcademiaDoZe.Presentation.AppMaui` contém a Dashboard e as telas de CRUD de Logradouros, Alunos e Colaboradores. A lista de Alunos e Colaboradores permite filtrar por CPF; os formulários cadastram, editam e excluem dados por meio dos services da camada Application. O cadastro exige foto e senha. Na edição, uma foto já armazenada é mantida quando nenhuma imagem nova é selecionada.

As imagens são capturadas/selecionadas com `MediaPicker` e armazenadas como bytes no `ArquivoDto`, persistidos na coluna `foto` do SQL Server. A câmera requer permissão e suporte do dispositivo; cancelamentos são ignorados e arquivos são limitados a 15 MB. Neste checkout, a Presentation está configurada apenas para `net10.0-windows10.0.22621.0`; captura ou seleção dependem do suporte do dispositivo Windows/runtime e não foram verificadas em hardware. O código informa indisponibilidade/permissão negada e oferece a seleção da galeria como alternativa à câmera.

Para executar a interface no Windows, use Visual Studio com workload .NET MAUI instalado e configure `ACADEMIA_DO_ZE_CONNECTION` para a instância SQL Server disponível. A conexão padrão do projeto Presentation pode ser ajustada em `MauiProgram.cs`.

Os formulários usam os logradouros já cadastrados; cadastre primeiro o endereço na aba Logradouros caso a lista de endereços esteja vazia.

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
