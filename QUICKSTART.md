# Norta - Quick Start Guide

Get Norta up and running in 5 minutes.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for MAUI)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional but recommended)

## Option 1: Docker Compose (Easiest)

### 1. Start everything with Docker
```bash
docker-compose up --build
```

This will start:
- PostgreSQL database on port 5432
- Norta API on port 5000

### 2. Verify API is running
Open browser to: http://localhost:5000/swagger

### 3. Create a test account
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "displayName": "Test User"
  }'
```

Save the `accessToken` from the response.

### 4. Run the mobile app

Update `cluaidai/Services/ApiService.cs` and `SignalRService.cs`:
```csharp
_baseUrl = "http://localhost:5000/api"; // or your computer's IP for mobile device
```

Then run:
```bash
cd cluaidai
dotnet build -t:Run -f net9.0-android  # or net9.0-ios
```

## Option 2: Local Development (No Docker)

### 1. Start the API
```bash
cd Norta.Api
dotnet run
```

API will use SQLite by default at `https://localhost:5001`

### 2. Test the API
Open: https://localhost:5001/swagger

### 3. Run the mobile app
```bash
cd cluaidai
dotnet build -t:Run -f net9.0-android
```

## Next Steps

### Create sample data

1. Register a few users via API
2. Create posts
3. Follow users
4. Like and comment on posts
5. Send direct messages

### Explore the API

All endpoints are documented at the Swagger UI: https://localhost:5001/swagger

Key endpoints:
- `POST /api/auth/register` - Register
- `POST /api/auth/login` - Login
- `GET /api/posts/feed` - Get feed
- `POST /api/posts` - Create post
- `POST /api/posts/{id}/like` - Like post
- `POST /api/users/{id}/follow` - Follow user

### Connect SignalR for real-time features

The mobile app automatically connects to SignalR hubs for:
- Real-time direct messages
- Live notifications
- Typing indicators

### Upload images

#### Method 1: Get signed URL (recommended)
```bash
# 1. Get signed URL
curl -X POST https://localhost:5001/api/uploads/signed-url \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"fileName":"photo.jpg","contentType":"image/jpeg"}'

# 2. Upload to the returned uploadUrl
curl -X PUT "UPLOAD_URL" \
  -H "Content-Type: image/jpeg" \
  --data-binary @photo.jpg

# 3. Create post with publicUrl
curl -X POST https://localhost:5001/api/posts \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"caption":"My photo","imageUrl":"PUBLIC_URL"}'
```

#### Method 2: Direct upload
```bash
curl -X POST https://localhost:5001/api/uploads/direct \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@photo.jpg"
```

## Troubleshooting

### API won't start
- Check if port 5000/5001 is already in use
- Verify .NET 8 SDK is installed: `dotnet --version`

### Mobile app can't connect to API
- Update API URL to your computer's IP address (not localhost)
- Ensure API is accessible from your device/emulator
- Check firewall settings

### Database errors
- Delete `norta.db` and restart (SQLite dev mode)
- For Docker: `docker-compose down -v` to reset everything

### MAUI build fails
- Verify .NET 9 SDK is installed
- Install MAUI workload: `dotnet workload install maui`
- Clean and rebuild: `dotnet clean && dotnet build`

## Development Workflow

1. Make changes to API code
2. API hot-reloads automatically (with `dotnet watch run`)
3. Make changes to mobile app
4. Rebuild and run mobile app
5. Test in emulator or device

## Sample Test Script

```bash
# Register user
TOKEN=$(curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@test.com","password":"Pass123!","displayName":"Alice"}' \
  | jq -r '.accessToken')

# Create post
curl -X POST http://localhost:5000/api/posts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"caption":"Hello from Norta!","imageUrl":null}'

# Get feed
curl -X GET "http://localhost:5000/api/posts/feed?page=0&size=10" \
  -H "Authorization: Bearer $TOKEN"
```

## Production Deployment

See [README.md](README.md) for full production deployment guide.

Quick overview:
1. Set strong JWT secret
2. Use PostgreSQL database
3. Configure Azure Blob or S3 for storage
4. Set up HTTPS/SSL
5. Configure CORS for your domains
6. Enable rate limiting
7. Set up monitoring and logging

## Need Help?

- Check [README.md](README.md) for detailed documentation
- Review API docs at `/swagger`
- Open an issue on GitHub
