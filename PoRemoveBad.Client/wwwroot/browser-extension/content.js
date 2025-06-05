// Listen for messages from the popup
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === 'analyzeSelectedText') {
        const selectedText = window.getSelection().toString();
        if (selectedText) {
            analyzeText(selectedText)
                .then(analysis => sendResponse({ success: true, analysis }))
                .catch(error => sendResponse({ success: false, error: error.message }));
            return true; // Will respond asynchronously
        }
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

// Add context menu for text selection
chrome.runtime.onInstalled.addListener(() => {
    chrome.contextMenus.create({
        id: 'analyzeText',
        title: 'Analyze with PoRemoveBad',
        contexts: ['selection']
    });
});

// Handle context menu clicks
chrome.contextMenus.onClicked.addListener((info, tab) => {
    if (info.menuItemId === 'analyzeText') {
        chrome.tabs.sendMessage(tab.id, {
            action: 'analyzeSelectedText',
            text: info.selectionText
        });
    }
});

// Add keyboard shortcut for quick analysis
document.addEventListener('keydown', (event) => {
    if (event.ctrlKey && event.shiftKey && event.key === 'A') {
        const selectedText = window.getSelection().toString();
        if (selectedText) {
            analyzeText(selectedText)
                .then(analysis => {
                    // Show results in a floating panel
                    showResultsPanel(analysis);
                })
                .catch(error => {
                    console.error('Analysis error:', error);
                });
        }
    }
});

// Function to show results in a floating panel
function showResultsPanel(analysis) {
    // Remove existing panel if any
    const existingPanel = document.getElementById('poremovebad-results-panel');
    if (existingPanel) {
        existingPanel.remove();
    }

    // Create new panel
    const panel = document.createElement('div');
    panel.id = 'poremovebad-results-panel';
    panel.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        width: 300px;
        max-height: 80vh;
        overflow-y: auto;
        background: white;
        border: 1px solid #ccc;
        border-radius: 4px;
        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        padding: 15px;
        z-index: 10000;
    `;

    // Add close button
    const closeButton = document.createElement('button');
    closeButton.textContent = '×';
    closeButton.style.cssText = `
        position: absolute;
        top: 5px;
        right: 5px;
        background: none;
        border: none;
        font-size: 20px;
        cursor: pointer;
    `;
    closeButton.onclick = () => panel.remove();
    panel.appendChild(closeButton);

    // Add results content
    const content = document.createElement('div');
    content.innerHTML = `
        <h3 style="margin-top: 0;">Analysis Results</h3>
        <div>
            <h4>Sentiment</h4>
            <p>${analysis.sentiment.label} (${(analysis.sentiment.score * 100).toFixed(1)}%)</p>
        </div>
        <div>
            <h4>Grammar Issues</h4>
            <p>${analysis.grammarIssues.length} issues found</p>
        </div>
        <div>
            <h4>Vocabulary</h4>
            <p>${analysis.vocabulary.uniqueWordCount} unique words</p>
        </div>
        <div>
            <h4>Target Audience</h4>
            <p>${analysis.targetAudience.readingLevel} level</p>
        </div>
    `;
    panel.appendChild(content);

    // Add panel to document
    document.body.appendChild(panel);

    // Auto-close after 30 seconds
    setTimeout(() => panel.remove(), 30000);
} 