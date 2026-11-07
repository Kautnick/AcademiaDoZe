# AcademiaDoZe

Projeto acadêmico em C#/.NET MAUI com arquitetura Domain, Application, Infrastructure e persistência exclusiva em Microsoft SQL Server.

## Banco de dados local

A aplicação acessa diretamente a instância local do SQL Server; Docker e outros SGBDs não são necessários. A connection string padrão está em `AcademiaDoZe.Presentation.AppMaui/MauiProgram.cs` e pode ser sobrescrita por `ACADEMIA_DO_ZE_CONNECTION`. Ajuste a instância e o catálogo para o SQL Server instalado neste computador, mantendo `Initial Catalog=db_academia_do_ze`.

## Executar

```bash
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

A aba Matrículas oferece cadastro, consulta, edição e exclusão com confirmação. A listagem permite filtrar por ID da matrícula, ID do aluno, CPF (parcial, com ou sem pontuação), vigência e vencimento. Antes de salvar, o formulário mostra uma prévia atualizada com aluno, plano, período, objetivo, restrições e laudos. Também permite selecionar vários laudos/documentos (PDF ou imagem) em uma seleção, adicionar mais, remover individualmente ou limpar todos. Os arquivos são persistidos em `dbo.tb_matricula_laudo` e carregados na edição; documentos legados são migrados sem exclusão.

O plano calcula automaticamente a data final; restrições médicas são flags independentes e podem ser combinadas. Alunos com idade entre 12 e 16 anos na data de início e alunos com restrições exigem pelo menos um laudo.

## Verificação no banco

No SQL Server Management Studio, após executar os testes:

```sql
USE db_academia_do_ze;
SELECT * FROM dbo.tb_matricula ORDER BY id_matricula DESC;
```
