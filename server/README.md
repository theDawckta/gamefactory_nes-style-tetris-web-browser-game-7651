# Leaderboard Server

A lightweight Node.js Express server for managing global leaderboard scores, shared by all clients playing the NES-style Tetris web browser game.

## Features

- **GET /scores** - Returns the top 5 entries sorted by score (descending)
- **POST /scores** - Submits a new score entry with automatic validation and top-5 enforcement
- **OPTIONS /scores** - CORS preflight support for WebGL browser clients
- **JSON file persistence** - Scores are stored in `scores.json` on disk
- **CORS enabled** - All responses include `Access-Control-Allow-Origin: *`

## Prerequisites

- Node.js 18+ installed

## Setup

```bash
# Install dependencies
npm install

# Start the server
node server.js
```

By default the server listens on port `3000`. Override with the `PORT` environment variable:

```bash
# Linux / macOS
PORT=8080 node server.js

# Windows (cmd)
set PORT=8080 && node server.js

# Windows (PowerShell)
$env:PORT=8080; node server.js
```

## API Reference

### GET /scores

Returns a JSON array of the top 5 scores, sorted by score descending.

**Response:**
```json
[
  { "name": "AAA", "score": 15234 },
  { "name": "BBB", "score": 10500 }
]
```

Returns an empty array `[]` when no scores have been submitted.

### POST /scores

Submit a new score entry.

**Request body:**
```json
{ "name": "ABC", "score": 12345 }
```

**Validation rules:**
- `name`: String of 1-3 alphanumeric characters (letters and digits only). Automatically uppercased on storage.
- `score`: Non-negative integer (0 or greater).

**Response on success (HTTP 200):**
Returns the updated top 5 scores as a JSON array.

**Response on validation failure (HTTP 400):**
```json
{ "error": "Invalid name. Must be 1-3 alphanumeric characters." }
```

## Storage

Scores are persisted in `server/scores.json`. This file is created automatically on first server start if it does not exist, initialized to an empty array. Only the top 5 scores are stored after each POST - lower scores are discarded.
