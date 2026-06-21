# Leaderboard Server

Node.js/Express HTTP server for the NES-style Tetris leaderboard.

## Setup

```
cd server
npm install
node server.js
```

Listens on port 3000 by default. Override with the `PORT` environment variable.

## Endpoints

### GET /scores

Returns the top 5 scores sorted by score descending.

Response: `[{ "name": "AAA", "score": 12345 }, ...]`

Returns `[]` if no scores have been posted yet.

### POST /scores

Submit a new score entry.

Request body: `{ "name": "AAA", "score": 12345 }`

- `name`: 1-3 alphanumeric characters
- `score`: non-negative integer

Returns the updated top 5 list. Returns HTTP 400 if validation fails.

### OPTIONS /scores

CORS preflight. Returns 200 with CORS headers.

## Notes

- `scores.json` is created automatically on first start if missing.
- All responses include `Access-Control-Allow-Origin: *` for WebGL browser client compatibility.
