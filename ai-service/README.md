# AI Microservice (Node.js + TypeScript + Google Gemini)

A Node.js TypeScript microservice that provides AI-powered text analysis using Google Gemini API. Designed to integrate with the ShapeGlobalTask .NET Windows Service.

## Features

- **Sentiment Analysis**: Analyze text sentiment with score (-1 to 1) and confidence
- **Tag Extraction**: Extract relevant tags/keywords from text
- **User Insights**: Generate comprehensive AI insights for user profiles
- **Structured Logging**: Winston-based logging with correlation ID support
- **Request Validation**: Express-validator for input validation
- **Health Checks**: Endpoint for monitoring service health
- **Docker Support**: Multi-stage Dockerfile for production deployment

## Prerequisites

- Node.js 18+ or Docker
- Google Gemini API Key ([Get one here](https://makersuite.google.com/app/apikey))

## Quick Start

### 1. Install Dependencies

```bash
cd ai-service
npm install
```

### 2. Configure Environment

Copy the example environment file:

```bash
cp .env.example .env
```

Edit `.env` and add your Gemini API key:

```env
GEMINI_API_KEY=your_actual_api_key_here
PORT=3000
NODE_ENV=development
```

### 3. Run in Development Mode

```bash
npm run dev
```

The service will start on `http://localhost:3000`.

### 4. Run Tests

```bash
npm test
```

## API Endpoints

### Health Check

```http
GET /health
```

**Response:**
```json
{
  "success": true,
  "data": {
    "status": "healthy",
    "service": "AI Service",
    "timestamp": "2024-01-15T10:30:00.000Z"
  },
  "correlationId": "abc123"
}
```

### Analyze Sentiment

```http
POST /api/ai/sentiment
Content-Type: application/json
X-Correlation-ID: optional-correlation-id

{
  "text": "I absolutely love this product! It's amazing."
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "sentiment": "positive",
    "score": 0.92,
    "confidence": 0.95
  },
  "correlationId": "abc123"
}
```

### Extract Tags

```http
POST /api/ai/tags
Content-Type: application/json

{
  "text": "John is a software engineer who loves Python and machine learning"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "tags": ["software engineer", "Python", "machine learning"]
  },
  "correlationId": "abc123"
}
```

### Generate User Insights

```http
POST /api/ai/insights
Content-Type: application/json

{
  "text": "User bio: Software developer passionate about AI. Email: john@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "sentiment": "positive",
    "sentimentScore": 0.75,
    "confidence": 0.88,
    "tags": ["software developer", "AI", "technology"],
    "engagementLevel": "high",
    "summary": "Tech-savvy professional with strong interest in AI technologies"
  },
  "correlationId": "abc123"
}
```

## Error Responses

All errors follow a consistent format:

```json
{
  "success": false,
  "error": {
    "message": "Error description",
    "code": "ERROR_CODE"
  },
  "correlationId": "abc123"
}
```

### Common Error Codes

| Code | Description |
|------|-------------|
| `VALIDATION_ERROR` | Input validation failed |
| `GEMINI_API_ERROR` | Gemini API call failed |
| `RATE_LIMITED` | Too many requests |
| `INTERNAL_ERROR` | Internal server error |
| `NOT_FOUND` | Endpoint not found |

## Project Structure

```
ai-service/
├── src/
│   ├── config/
│   │   └── index.ts         # Environment configuration
│   ├── controllers/
│   │   └── aiController.ts  # Request handlers
│   ├── middleware/
│   │   ├── correlationId.ts # Correlation ID middleware
│   │   └── errorHandler.ts  # Global error handler
│   ├── routes/
│   │   └── aiRoutes.ts      # API route definitions
│   ├── services/
│   │   └── geminiService.ts # Gemini AI integration
│   ├── types/
│   │   └── index.ts         # TypeScript type definitions
│   ├── utils/
│   │   └── logger.ts        # Winston logger setup
│   └── index.ts             # Application entry point
├── tests/
│   └── ai.test.ts           # Jest test suite
├── .env.example             # Environment template
├── Dockerfile               # Docker configuration
├── jest.config.js           # Jest configuration
├── package.json             # NPM dependencies
├── tsconfig.json            # TypeScript configuration
└── README.md                # This file
```

## Docker Deployment

### Build Image

```bash
docker build -t ai-service .
```

### Run Container

```bash
docker run -d \
  --name ai-service \
  -p 3000:3000 \
  -e GEMINI_API_KEY=your_api_key_here \
  -e NODE_ENV=production \
  ai-service
```

### Docker Compose (with .NET Service)

See the root `docker-compose.yml` for running both services together.

## Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `GEMINI_API_KEY` | Yes | - | Google Gemini API key |
| `PORT` | No | 3000 | Server port |
| `NODE_ENV` | No | development | Environment mode |
| `GEMINI_MODEL` | No | gemini-1.5-flash | Gemini model to use |
| `LOG_LEVEL` | No | info | Logging level |

## Integration with .NET Service

The .NET ShapeGlobalTask service calls this AI service via HTTP. Configure the AI service URL in `appsettings.json`:

```json
{
  "AIService": {
    "BaseUrl": "http://localhost:3000",
    "TimeoutSeconds": 30
  }
}
```

The .NET service will call `POST /api/ai/insights` when creating users with AI analysis.

## Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start development server with hot reload |
| `npm run build` | Compile TypeScript to JavaScript |
| `npm start` | Run production build |
| `npm test` | Run Jest test suite |
| `npm run lint` | Run ESLint (if configured) |

## Logging

Logs are written to:
- **Console**: All environments
- **File**: `logs/combined.log` (all logs)
- **File**: `logs/error.log` (errors only)

Each log entry includes:
- Timestamp
- Log level
- Correlation ID (if available)
- Message and metadata

## License

MIT
