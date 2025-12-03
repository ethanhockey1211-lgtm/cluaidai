# Norta - Social Media Platform

Norta is a modern, mobile-first social media application built with .NET MAUI and ASP.NET Core, featuring real-time messaging, a feed system, and an iOS-inspired glass UI aesthetic.

## Features

- 🔐 **Authentication**: JWT-based auth with refresh tokens
- 📱 **Feed**: Create posts with images and captions, infinite scroll
- ❤️ **Interactions**: Like and comment on posts
- 👥 **Social**: Follow/unfollow users, view profiles
- 💬 **Real-time Messaging**: 1:1 DMs with delivery and read receipts via SignalR
- 🔔 **Notifications**: Real-time notifications for likes, comments, follows, and messages
- 🔍 **Search**: Find users by name
- 🎨 **Modern UI**: iOS-style glass/translucency design

## Tech Stack

### Backend (Norta.Api)
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL (production), SQLite (development)
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Identity + JWT
- **Real-time**: SignalR
- **Storage**: Azure Blob Storage / AWS S3 / Local (configurable)
- **API Documentation**: Swagger/OpenAPI

### Frontend (cluaidai - MAUI App)
- **Framework**: .NET MAUI 9.0
- **Platforms**: Android, iOS, macOS Catalyst
- **MVVM**: CommunityToolkit.Mvvm
- **UI**: CommunityToolkit.Maui
- **Real-time**: SignalR Client

## Project Structure

```
/norta
├── Norta.Api/                  # Backend API
│   ├── Controllers/            # API endpoints
│   ├── Data/                   # EF Core DbContext
│   ├── DTOs/                   # Data transfer objects
│   ├── Hubs/                   # SignalR hubs
│   ├── Models/                 # Domain models
│   ├── Services/               # Business logic
│   ├── Dockerfile              # API container config
│   └── appsettings.json        # Configuration
├── cluaidai/                   # MAUI Mobile App
│   ├── Models/                 # Client-side models
│   ├── Services/               # API & SignalR services
│   ├── ViewModels/             # MVVM ViewModels
│   ├── Views/                  # XAML pages
│   └── Resources/Styles/       # Glass UI styles
├── Norta.Tests/                # Unit & integration tests
├── docker-compose.yml          # Docker orchestration
└── .github/workflows/          # CI/CD pipelines
```

## Getting Started

### Prerequisites

- .NET 8 SDK (for API)
- .NET 9 SDK (for MAUI app)
- Docker & Docker Compose (optional)
- PostgreSQL (or use SQLite for dev)
- Visual Studio 2022 or VS Code

### Running the Backend API

#### Option 1: Docker Compose (Recommended)

```bash
docker-compose up --build
```

API will be available at `http://localhost:5000`

#### Option 2: Local Development

1. Navigate to the API directory:
   ```bash
   cd Norta.Api
   ```

2. Update `appsettings.Development.json` if needed

3. Run the API:
   ```bash
   dotnet run
   ```

4. Access Swagger UI: `https://localhost:5001/swagger`

#### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add InitialCreate --project Norta.Api

# Apply migrations
dotnet ef database update --project Norta.Api
```

### Running the MAUI App

1. Navigate to the MAUI project:
   ```bash
   cd cluaidai
   ```

2. Update `Services/ApiService.cs` and `Services/SignalRService.cs` to point to your API URL

3. Run the app:
   ```bash
   # Android
   dotnet build -t:Run -f net9.0-android

   # iOS
   dotnet build -t:Run -f net9.0-ios

   # macOS
   dotnet build -t:Run -f net9.0-maccatalyst
   ```

Or open in Visual Studio and run from there.

### Running Tests

```bash
dotnet test Norta.Tests/Norta.Tests.csproj
```

## Configuration

### Backend Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `UsePostgreSQL` | Use PostgreSQL instead of SQLite | `false` |
| `ConnectionStrings__Postgres` | PostgreSQL connection string | See appsettings.json |
| `Jwt__Key` | JWT signing key | **Change in production!** |
| `Jwt__Issuer` | JWT issuer | `norta.app` |
| `Jwt__Audience` | JWT audience | `norta.app` |
| `Storage__Provider` | Storage provider: `Local`, `AzureBlob`, `S3` | `Local` |
| `Storage__AzureBlob__ConnectionString` | Azure Blob connection string | - |
| `Storage__S3__AccessKey` | AWS S3 access key | - |
| `Storage__S3__SecretKey` | AWS S3 secret key | - |

### MAUI App Configuration

Update these in `Services/ApiService.cs` and `Services/SignalRService.cs`:
- `_baseUrl`: Your API base URL (default: `https://localhost:5001`)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh access token

### Users
- `GET /api/users/me` - Get current user
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/me` - Update profile
- `GET /api/users/search?q={query}` - Search users
- `POST /api/users/{id}/follow` - Follow user
- `DELETE /api/users/{id}/follow` - Unfollow user

### Posts
- `GET /api/posts/feed?page={page}&size={size}` - Get feed
- `POST /api/posts` - Create post
- `GET /api/posts/{id}` - Get post by ID
- `DELETE /api/posts/{id}` - Delete post
- `POST /api/posts/{id}/like` - Like post
- `DELETE /api/posts/{id}/like` - Unlike post
- `GET /api/posts/{id}/comments` - Get comments
- `POST /api/posts/{id}/comments` - Create comment

### Messages
- `GET /api/messages/conversations` - Get conversations list
- `GET /api/messages/conversation/{userId}` - Get conversation with user

### Notifications
- `GET /api/notifications` - Get notifications
- `GET /api/notifications/unread-count` - Get unread count
- `PUT /api/notifications/{id}/read` - Mark as read

### Uploads
- `POST /api/uploads/signed-url` - Get signed upload URL
- `POST /api/uploads/direct` - Direct file upload

## SignalR Hubs

### ChatHub (`/hubs/chat`)
- `SendMessage(toUserId, text)` - Send a message
- `MarkAsRead(messageId)` - Mark message as read
- `Typing(toUserId)` - Send typing indicator

**Events:**
- `ReceiveMessage` - Receive a message
- `MessageSent` - Confirmation of sent message
- `MessageRead` - Message was read
- `UserTyping` - User is typing

### NotificationsHub (`/hubs/notifications`)
**Events:**
- `ReceiveNotification` - Receive a notification

## Sample API Requests

### Register
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "Password123!",
    "displayName": "Alice"
  }'
```

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "Password123!"
  }'
```

### Create Post
```bash
curl -X POST https://localhost:5001/api/posts \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "caption": "Hello Norta!",
    "imageUrl": "https://example.com/image.jpg"
  }'
```

### Upload Image (Signed URL Flow)
```bash
# 1. Get signed URL
curl -X POST https://localhost:5001/api/uploads/signed-url \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fileName": "photo.jpg",
    "contentType": "image/jpeg"
  }'

# 2. Upload to signed URL (response contains uploadUrl and publicUrl)
curl -X PUT "SIGNED_UPLOAD_URL" \
  -H "Content-Type: image/jpeg" \
  --data-binary @photo.jpg

# 3. Create post with publicUrl
```

## Production Deployment Checklist

### Security
- [ ] Change `Jwt__Key` to a strong, randomly generated secret (min 256 bits)
- [ ] Enable refresh token rotation
- [ ] Configure CORS to whitelist only your domains
- [ ] Enable HTTPS and HSTS headers
- [ ] Set up Content Security Policy (CSP) headers
- [ ] Implement rate limiting (use AspNetCoreRateLimit or similar)
- [ ] Enable file type and size validation for uploads
- [ ] Consider adding virus scanning for uploaded files
- [ ] Review and harden password policies

### Database
- [ ] Use PostgreSQL in production
- [ ] Set up automated backups
- [ ] Configure connection pooling
- [ ] Run migrations as part of CI/CD (don't auto-migrate in production)
- [ ] Set up read replicas for scalability

### Storage
- [ ] Configure Azure Blob Storage or AWS S3
- [ ] Enable CDN for faster image delivery
- [ ] Set up lifecycle policies for old files
- [ ] Configure CORS on storage bucket

### SignalR
- [ ] Configure scale-out with Redis backplane or Azure SignalR Service
- [ ] Set connection limits and timeouts
- [ ] Monitor connection health

### Monitoring & Logging
- [ ] Set up structured logging (Serilog, NLog)
- [ ] Configure Application Insights or similar APM tool
- [ ] Set up error tracking (Sentry, Raygun)
- [ ] Monitor API performance and latency
- [ ] Set up alerts for errors and performance issues

### Infrastructure
- [ ] Use environment variables or Key Vault for secrets
- [ ] Set up health checks
- [ ] Configure auto-scaling
- [ ] Set up load balancer
- [ ] Configure SSL certificates (Let's Encrypt or purchased)
- [ ] Set up CI/CD pipelines
- [ ] Configure staging environment

### Mobile App
- [ ] Set up push notifications (APNs for iOS, FCM for Android)
- [ ] Configure proper API URLs for production
- [ ] Set up crash reporting (App Center, Firebase Crashlytics)
- [ ] Enable code signing for iOS and Android
- [ ] Submit to App Store and Google Play Store

## Architecture Notes

### Authentication Flow
1. User registers/logs in via `/api/auth/register` or `/api/auth/login`
2. API returns `accessToken` (1-hour expiry) and `refreshToken` (30-day expiry)
3. Client stores tokens securely (SecureStorage in MAUI)
4. Client includes `Authorization: Bearer {accessToken}` header in requests
5. On token expiry, client uses refresh token to get new access token

### Image Upload Flow
1. **Signed URL (Recommended)**:
   - Client requests signed URL from `/api/uploads/signed-url`
   - API generates time-limited upload URL
   - Client uploads directly to cloud storage
   - Client uses public URL when creating post

2. **Direct Upload (Fallback)**:
   - Client uploads to `/api/uploads/direct`
   - API handles upload to storage
   - Returns public URL

### Real-time Communication
- SignalR connections authenticated via JWT in query string
- Hubs grouped by user ID for targeted message delivery
- Automatic reconnection on connection loss

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please open an issue on GitHub.
