# Kenz Project

OCR-powered document processing application with AI extraction capabilities.

## Architecture

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Web App    │────▶│   API       │────▶│  LLM        │
│  (React)    │     │  (.NET)     │     │  (Mistral)  │
└─────────────┘     └─────────────┘     └─────────────┘
     :3000              :3001
```

## Tech Stack

- **Frontend**: React, TanStack Start, TanStack Router, Tailwind CSS, Vite+
- **Backend**: .NET 10, ASP.NET Core
- **LLM**: Mistral AI (configurable)
- **Proxy**: Caddy

## Getting Started

### Prerequisites

- Node.js (via Vite+)
- .NET 10 SDK
- Docker & Docker Compose (optional)

### Development

**Option 1: Docker Compose**

```bash
docker compose up
```

**Option 2: Manual**

```bash
# Frontend
cd web-app
pnpm install
pnpm dev

# API (separate terminal)
cd services/api
dotnet run
```

### Environment Variables

Copy `.env.example` to `.env` and configure:

| Variable | Default | Description |
|----------|---------|-------------|
| `LlmProvider` | `mistral` | LLM provider |
| `LlmApiKey` | - | API key for LLM |
| `LlmBaseUrl` | `https://api.mistral.ai` | LLM API base URL |
| `LlmChatModel` | `mistral-small-latest` | Chat model name |

## Project Structure

```
.
├── web-app/          # Frontend React application
├── services/
│   ├── api/          # .NET API service
│   └── common/       # Shared library
├── docker-compose.yml
└── Caddyfile         # Reverse proxy config
```

## Features

- PDF document upload and OCR processing
- AI-powered field extraction (SIRET, invoice numbers, service codes)
- File preview and raw OCR visualization
- Dark/light theme support