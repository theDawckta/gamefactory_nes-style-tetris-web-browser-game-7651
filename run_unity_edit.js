const { execFile } = require('child_process');
const fs = require('fs');

// Remove Unity lock file if present
const lockPath = 'C:\\Users\\Zeke\\Git\\ai-game-factory\\Tools\\workspace\\gamefactory_nes-style-tetris-web-browser-game-7651\\Temp\\UnityLockfile';
try { fs.unlinkSync(lockPath); console.log('Removed UnityLockfile'); } catch(e) { console.log('No lock file to remove'); }

const unity = 'C:\\Program Files\\Unity\\Hub\\Editor\\6000.4.4f1\\Editor\\Unity.exe';
const projectRoot = 'C:\\Users\\Zeke\\Git\\ai-game-factory\\Tools\\workspace\\gamefactory_nes-style-tetris-web-browser-game-7651';

const args = [
    '-batchmode',
    '-nographics',
    '-projectPath', projectRoot,
    '-runTests',
    '-testPlatform', 'EditMode',
    '-testResults', projectRoot + '/tmp/edit-results.xml',
    '-logFile', projectRoot + '/tmp/edit.log',
    '-testFilter', 'PlayfieldModelTests'
];

console.log('Starting Unity tests...');
const start = Date.now();
const proc = execFile(unity, args, { maxBuffer: 50*1024*1024 }, (err, stdout, stderr) => {
    const elapsed = ((Date.now() - start) / 1000).toFixed(1);
    console.log('\n=== Unity test process finished with code ' + err?.code + ' after ' + elapsed + 's ===');
    
    // Read the log file
    try {
        const logPath = projectRoot + '/tmp/edit.log';
        const logContent = fs.readFileSync(logPath, 'utf8');
        console.log('\n=== LAST 5000 CHARS OF UNITY LOG ===\n');
        console.log(logContent.slice(-5000));
    } catch(e) {
        console.log('Could not read log file: ' + e.message);
    }
    
    // Read results XML
    try {
        const resultsPath = projectRoot + '/tmp/edit-results.xml';
        const resultsContent = fs.readFileSync(resultsPath, 'utf8');
        console.log('\n=== TEST RESULTS XML ===\n');
        console.log(resultsContent);
    } catch(e) {
        console.log('No test results file found.');
    }
});

proc.stdout?.on('data', (d) => process.stdout.write(d));
proc.stderr?.on('data', (d) => process.stderr.write(d));

// Backup kill after 300s
setTimeout(() => { 
    try { proc.kill(); } catch(e) {}
    console.error('\nKILLED Unity after 300s timeout');
}, 310000);
