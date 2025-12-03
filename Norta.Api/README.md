# Norta API

ASP.NET Core Web API for the Norta social media platform.

## Quick Start

### Development (SQLite)
```bash
cd Norta.Api
dotnet run
```

API will be available at `https://localhost:5001` with Swagger UI at `https://localhost:5001/swagger`

### Production (PostgreSQL)
```bash
docker-compose up --build
```

## Database Migrations

### Create Migration
```bash
dotnet ef migrations add MigrationName --project Norta.Api
```

### Apply Migration
```bash
dotnet ef database update --project Norta.Api
```

### Remove Last Migration
```bash
dotnet ef migrations remove --project Norta.Api
```

## Environment Variables

Create a `.env` file or set environment variables:

```env
UsePostgreSQL=true
ConnectionStrings__Postgres=Host=localhost;Database=norta;Username=norta;Password=yourpassword
Jwt__Key=your-secret-key-min-256-bits
Jwt__Issuer=norta.app
Jwt__Audience=norta.app
Storage__Provider=Local
```

## Storage Providers

### Local Storage (Development)
No additional configuration needed. Files stored in `wwwroot/uploads`.

### Azure Blob Storage
```json
{
  "Storage": {
    "Provider": "AzureBlob",
    "AzureBlob": {
      "ConnectionString": "your-azure-connection-string",
      "ContainerName": "norta-uploads"
    }
  }
}
```

### AWS S3
```json
{
  "Storage": {
    "Provider": "S3",
    "S3": {
      "AccessKey": "your-access-key",
      "SecretKey": "your-secret-key",
      "BucketName": "norta-uploads",
      "Region": "us-east-1"
    }
  }
}
```

## Testing

Run unit tests:
```bash
dotnet test ../Norta.Tests/Norta.Tests.csproj
```

## API Documentation

Once running, visit `https://localhost:5001/swagger` for interactive API documentation.

## Production Deployment

### Docker
```bash
docker build -t norta-api .
docker run -p 5000:80 -e UsePostgreSQL=true norta-api
```

### Azure App Service
1. Publish: `dotnet publish -c Release`
2. Deploy `bin/Release/net8.0/publish` folder to App Service
3. Configure App Settings with environment variables
4. Set connection strings in Azure Portal

### AWS Elastic Beanstalk
1. Publish: `dotnet publish -c Release`
2. Create deployment package from `bin/Release/net8.0/publish`
3. Deploy via EB CLI or AWS Console
4. Configure environment variables in EB environment settings

## Security Considerations

- Always use HTTPS in production
- Change JWT key to a strong secret (min 256 bits)
- Enable CORS only for trusted origins
- Implement rate limiting
- Set up proper logging and monitoring
- Use managed database service with automated backups
- Store secrets in Azure Key Vault or AWS Secrets Manager
