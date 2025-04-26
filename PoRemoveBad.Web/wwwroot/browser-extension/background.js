// Listen for installation
chrome.runtime.onInstalled.addListener(() => {
    console.log('PoRemoveBad Text Analyzer extension installed');
});

// Handle messages from content script
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === 'analyzeText') {
        analyzeText(request.text)
            .then(analysis => sendResponse({ success: true, analysis }))
            .catch(error => sendResponse({ success: false, error: error.message }));
        return true; // Will respond asynchronously
    }
});

// Function to analyze text using the API
async function analyzeText(text) {
    const response = await fetch('https://your-api-url/api/TextAnalysis/analyze', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ text }),
    });

    if (!response.ok) {
        throw new Error('Analysis failed');
    }

    return await response.json();
}

// Handle keyboard shortcuts
chrome.commands.onCommand.addListener((command) => {
    if (command === 'analyze-selection') {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            chrome.tabs.sendMessage(tabs[0].id, { action: 'analyzeSelectedText' });
        });
    }
}); 