const express = require('express');
const fs = require('fs');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;
const SCORES_FILE = path.join(__dirname, 'scores.json');
const TOP_N = 5;

app.use(express.json());

function corsHeaders(res) {
    res.set('Access-Control-Allow-Origin', '*');
    res.set('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    res.set('Access-Control-Allow-Headers', 'Content-Type');
}

function loadScores() {
    if (!fs.existsSync(SCORES_FILE)) {
        fs.writeFileSync(SCORES_FILE, '[]', 'utf8');
    }
    try {
        return JSON.parse(fs.readFileSync(SCORES_FILE, 'utf8'));
    } catch {
        return [];
    }
}

function saveScores(scores) {
    fs.writeFileSync(SCORES_FILE, JSON.stringify(scores, null, 2), 'utf8');
}

app.options('/scores', (req, res) => {
    corsHeaders(res);
    res.sendStatus(200);
});

app.get('/scores', (req, res) => {
    corsHeaders(res);
    const scores = loadScores();
    res.json(scores);
});

app.post('/scores', (req, res) => {
    corsHeaders(res);
    const { name, score } = req.body || {};

    if (typeof name !== 'string' || !/^[a-zA-Z0-9]{1,3}$/.test(name)) {
        return res.status(400).json({ error: 'name must be 1-3 alphanumeric characters' });
    }
    if (!Number.isInteger(score) || score < 0) {
        return res.status(400).json({ error: 'score must be a non-negative integer' });
    }

    const scores = loadScores();
    scores.push({ name, score });
    scores.sort((a, b) => b.score - a.score);
    const top = scores.slice(0, TOP_N);
    saveScores(top);

    res.json(top);
});

app.listen(PORT, () => {
    console.log('Leaderboard server listening on port ' + PORT);
});
