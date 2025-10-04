# Deploy Scripts for TorneSe.PagamentosPedidos.App

Este diretório contém scripts para build e deploy da aplicação Lambda.

## 📋 Pré-requisitos

### Ambiente de Desenvolvimento
- ✅ .NET 8.0 SDK
- ✅ AWS CLI instalado e configurado
- ✅ Credenciais AWS válidas

### Configuração AWS CLI
```bash
aws configure
```

Você será solicitado a fornecer:
- AWS Access Key ID
- AWS Secret Access Key
- Default region name (recomendado: us-east-1)
- Default output format (recomendado: json)

## 🚀 Scripts Disponíveis

### 1. Build e Deploy Completo

#### Linux/macOS - Shell Script
```bash
# Tornar o script executável
chmod +x deploy/build-and-deploy.sh

# Executar o script
./deploy/build-and-deploy.sh
```

#### Windows - PowerShell Script
```powershell
# Executar com parâmetros padrão
.\deploy\build-and-deploy.ps1

# Executar com região e bucket customizados
.\deploy\build-and-deploy.ps1 -Region "us-west-2" -BucketName "meu-bucket-personalizado"
```

### 2. Build Local (Sem Deploy)

Para desenvolvimento local, quando você só quer gerar o pacote sem fazer deploy:

#### Linux/macOS
```bash
chmod +x deploy/build-local.sh
./deploy/build-local.sh
```

#### Windows
```powershell
.\deploy\build-local.ps1
```

## 🔧 Configurações

### Bucket S3 Padrão
- **Nome**: `torne-se-pagamentos-pedidos-app-cloudformation`
- **Região**: `us-east-1`

### Personalização
Você pode customizar as configurações editando as variáveis no início dos scripts:

**Shell Script (build-and-deploy.sh):**
```bash
S3_BUCKET="seu-bucket-personalizado"
AWS_REGION="sua-regiao"
```

**PowerShell Script (build-and-deploy.ps1):**
```powershell
.\build-and-deploy.ps1 -Region "us-west-2" -BucketName "seu-bucket"
```

## 📦 O que os scripts fazem

1. **🧹 Limpeza**: Remove builds anteriores
2. **🔨 Build**: Compila a aplicação em modo Release para Linux x64
3. **📦 Empacotamento**: Cria arquivo ZIP otimizado para Lambda
4. **🪣 S3 Bucket**: Cria bucket S3 se não existir
5. **⬆️ Upload**: Envia o pacote para o S3
6. **✅ Verificação**: Confirma o upload e exibe informações

## 📁 Estrutura de Arquivos Gerados

```
/
├── publish/                    # Diretório temporário de build
├── TorneSe.PagamentosPedidos.App.zip  # Pacote final para Lambda
└── deploy/
    ├── build-and-deploy.sh    # Script Linux/macOS
    ├── build-and-deploy.ps1   # Script Windows
    └── README.md              # Este arquivo
```

## 🏗️ Integração com CloudFormation

Após executar o script, use estas informações no seu template CloudFormation:

```yaml
Resources:
  MyLambdaFunction:
    Type: AWS::Lambda::Function
    Properties:
      Code:
        S3Bucket: torne-se-pagamentos-pedidos-app-cloudformation
        S3Key: TorneSe.PagamentosPedidos.App.zip
      # ... outras configurações
```

## 🐛 Solução de Problemas

### Erro: "AWS CLI não encontrado"
```bash
# Linux/macOS
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install

# Windows
# Baixe e instale do site oficial: https://aws.amazon.com/cli/
```

### Erro: "Credenciais AWS não configuradas"
```bash
aws configure
# ou
aws configure --profile meu-perfil
```

### Erro: "Permissões S3 insuficientes"
Certifique-se de que sua conta AWS tem as seguintes permissões:
- `s3:CreateBucket`
- `s3:PutObject`
- `s3:PutBucketVersioning`
- `s3:ListBucket`

### Erro de Build: "dotnet não encontrado"
```bash
# Instale o .NET 8.0 SDK
# Linux: https://docs.microsoft.com/dotnet/core/install/linux
# Windows: https://dotnet.microsoft.com/download
# macOS: https://docs.microsoft.com/dotnet/core/install/macos
```

## 📊 Otimizações Implementadas

- ✅ **Exclusão de arquivos desnecessários**: Remove .pdb e .xml para reduzir tamanho
- ✅ **Compressão otimizada**: Usa nível máximo de compressão
- ✅ **PublishReadyToRun**: Melhora cold start do Lambda
- ✅ **Self-contained false**: Usa runtime compartilhado do Lambda
- ✅ **Versionamento S3**: Habilita versionamento automático

## 📈 Monitoramento

Após o deploy, monitore sua função Lambda através de:
- **CloudWatch Logs**: Logs da aplicação
- **CloudWatch Metrics**: Métricas de performance
- **X-Ray**: Tracing distribuído (se habilitado)

## 🔄 CI/CD

Para integração contínua, você pode usar estes scripts em:
- **GitHub Actions**
- **AWS CodeBuild**
- **Azure DevOps**
- **Jenkins**

Exemplo para GitHub Actions:
```yaml
- name: Build and Deploy Lambda
  run: |
    chmod +x deploy/build-and-deploy.sh
    ./deploy/build-and-deploy.sh
```