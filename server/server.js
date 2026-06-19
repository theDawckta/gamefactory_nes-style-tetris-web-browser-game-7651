const express = require('express');
const fs = require('fs');
const path = require('path');

const app = express();
app.use(express.json());

const SCORES_FILE = path.join(__dirname, 'scores.json');
const PORT = process.env.PORT || 3000;

// Middleware: CORS headers on every response
app.use((req, res, next) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
  next();
});

// Helper: load scores or return empty array
function loadScores() {
  if (!fs.existsSync(SCORES_FILE)) {
    fs.writeFileSync(SCORES_FILE, JSON.stringify([]));
    return [];
  }
  try {
    const data = fs.readFileSync(SCORES_FILE, 'utf8');
    return JSON.parse(data);
  } catch (e) {
    return [];
  }
}

// Helper: save scores to disk
function saveScores(scores) {
  fs.writeFileSync(SCORES_FILE, JSON.stringify(scores));
}

// Helper: trim to top 5 sorted descending by score
function getTop5(scores) {
  return scores
    .sort((a, b) => b.score - a.score)
    .slice(0, 5);
}

// OPTIONS /scores - CORS preflight
app.options('/scores', (req, res) => {
  res.sendStatus(200);
});

// GET /scores - return top 5 entries sorted by score descending
app.get('/scores', (req, res) => {
  const scores = loadScores();
  const top5 = getTop5(scores);
  res.json(top5);
});

// POST /scores - accept a new score entry
app.post('/scores', (req, res) => {
  const { name, score } = req.body;

  // Validate name: must be 1-3 alphanumeric characters
  if (typeof name !== 'string' || !/^[a-zA-Z0-9]{1,3}$/.test(name)) {
    return res.status(400).json({ error: 'Invalid name. Must be 1-3 alphanumeric characters.' });
  }

  // Validate score: must be a non-negative integer
  if (!Number.isInteger(score) || score < 0) {
    return res.status(400).json({ error: 'Invalid score. Must be a non-negative integer.' });
  }

  // Load existing scores, append new entry, sort, keep top 5
  const scores = loadScores();
  scores.push({ name: name.toUpperCase(), score });
  const top5 = getTop5(scores);
  saveScores(top5);

  res.json(top5);
});

// Initialize scores.json on startup if it doesn't exist
if (!fs.existsSync(SCORES_FILE)) {
  fs.writeFileSync(SCORES_FILE, JSON.stringify([]));
}

// Start the server
app.listen(PORT, () => {
  console.log(`Leaderboard server listening on port ${PORT}`);
});
