#!/usr/bin/env node
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Read JSON from stdin for model information
let stdinData = '';
process.stdin.on('data', chunk => {
    stdinData += chunk;
});

process.stdin.on('end', () => {
    if (stdinData.trim()) {
        try {
            const inputData = JSON.parse(stdinData);
            // Extract model information
            const modelName = inputData.model?.display_name || inputData.model?.id || 'Unknown';
            const sessionId = inputData.session_id ? inputData.session_id.substring(0, 8) : 'no-session';
            
            processStatusLine(modelName, sessionId);
        } catch (e) {
            processStatusLine('JSON-Error', 'parse-error');
        }
    } else {
        processStatusLine('Unknown', 'no-stdin');
    }
});

function processStatusLine(modelName = 'Unknown', sessionId = 'no-session') {
try {
    // Get current time
    const currentTime = new Date().toLocaleTimeString('en-US', { 
        hour12: false, 
        hour: '2-digit', 
        minute: '2-digit' 
    });

    // Get git info
    let gitBranch = 'no-git';
    let prCount = 0;
    let behindCommits = 0;
    let unpushedCommits = 0;
    let stagedChanges = 0;
    let unstagedChanges = 0;
    
    try {
        gitBranch = execSync('git branch --show-current', { encoding: 'utf8' }).trim() || 'detached';
        
        // Get detailed status
        const statusOutput = execSync('git status --porcelain', { encoding: 'utf8' });
        const statusLines = statusOutput.split('\n').filter(line => line.trim());
        
        // Count staged vs unstaged changes
        statusLines.forEach(line => {
            const firstChar = line[0];
            const secondChar = line[1];
            if (firstChar !== ' ' && firstChar !== '?') stagedChanges++;
            if (secondChar !== ' ') unstagedChanges++;
        });
        
        // Get unpushed commits count
        try {
            const unpushedResult = execSync(`git rev-list --count origin/${gitBranch}..HEAD`, { encoding: 'utf8' });
            unpushedCommits = parseInt(unpushedResult.trim()) || 0;
        } catch (e) {
            // Unable to check unpushed commits
        }
        
        // Get PR count (assuming GitHub CLI is available)
        try {
            const prResult = execSync('gh pr list --state=open --json number', { encoding: 'utf8' });
            const prs = JSON.parse(prResult);
            prCount = prs.length;
        } catch (e) {
            // GitHub CLI not available or no PRs
        }
        
        // Get commits behind origin (pull needed)
        try {
            execSync('git fetch origin', { stdio: ['ignore', 'ignore', 'ignore'] });
            const behindResult = execSync(`git rev-list --count HEAD..origin/${gitBranch}`, { encoding: 'utf8' });
            behindCommits = parseInt(behindResult.trim()) || 0;
        } catch (e) {
            // Unable to check remote status
        }
    } catch (e) {
        // Git not available or not in repo
    }

    // Enhanced format with detailed git status - Model name first, time after git info
    let gitStatus = '';
    
    // Pending changes (staged + unstaged)
    if (stagedChanges > 0) gitStatus += `📦${stagedChanges}`;
    if (unstagedChanges > 0) gitStatus += `📝${unstagedChanges}`;
    
    // Unpushed commits 
    if (unpushedCommits > 0) gitStatus += ` 📤${unpushedCommits}`;
    
    // Pull requests
    if (prCount > 0) gitStatus += ` 🔄${prCount}`;
    
    // Behind commits (needs pull)
    if (behindCommits > 0) gitStatus += ` 📥${behindCommits}`;
    
    // If no git activity, show clean
    if (!gitStatus && gitBranch !== 'no-git') gitStatus = '✅';
    
    console.log(`🌿 ${gitBranch} ${gitStatus} | ⏰ ${currentTime} | 🤖 ${modelName}`);
} catch (error) {
    console.log(`🌿 error | ⏰ ${new Date().toLocaleTimeString('en-US', { hour12: false, hour: '2-digit', minute: '2-digit' })} | 🤖 ${modelName}`);
}
}