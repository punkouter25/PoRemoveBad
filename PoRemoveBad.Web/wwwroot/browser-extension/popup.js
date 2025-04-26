document.addEventListener('DOMContentLoaded', function() {
    const textInput = document.getElementById('textInput');
    const analyzeButton = document.getElementById('analyzeButton');
    const loadingIndicator = document.getElementById('loadingIndicator');
    const results = document.getElementById('results');
    const errorMessage = document.getElementById('errorMessage');
    const sentimentResult = document.getElementById('sentimentResult');
    const grammarResult = document.getElementById('grammarResult');
    const vocabularyResult = document.getElementById('vocabularyResult');
    const targetAudienceResult = document.getElementById('targetAudienceResult');

    // Load saved text if available
    chrome.storage.local.get(['lastAnalyzedText'], function(result) {
        if (result.lastAnalyzedText) {
            textInput.value = result.lastAnalyzedText;
        }
    });

    analyzeButton.addEventListener('click', async function() {
        const text = textInput.value.trim();
        if (!text) {
            showError('Please enter some text to analyze');
            return;
        }

        // Save the text for future use
        chrome.storage.local.set({ lastAnalyzedText: text });

        try {
            showLoading();
            clearResults();
            clearError();

            // Analyze the text with enhanced fetch features
            const analysis = await analyzeText(text);
            displayResults(analysis);
        } catch (error) {
            showError('An error occurred while analyzing the text: ' + error.message);
            console.error('Analysis error:', error);
        } finally {
            hideLoading();
        }
    });

    async function analyzeText(text) {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 30000); // 30 second timeout

        try {
            const response = await fetch('https://your-api-url/api/TextAnalysis/analyze', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({ text }),
                keepalive: true,
                signal: controller.signal
            });

            if (!response.ok) {
                throw new Error(`Analysis failed: ${response.status} ${response.statusText}`);
            }

            return await response.json();
        } finally {
            clearTimeout(timeoutId);
        }
    }

    function displayResults(analysis) {
        // Display sentiment results with enhanced formatting
        sentimentResult.innerHTML = `
            <h3>Sentiment Analysis</h3>
            <p>Overall sentiment: <strong>${analysis.sentiment.label}</strong> (${(analysis.sentiment.score * 100).toFixed(1)}%)</p>
            <p>Confidence: ${(analysis.sentiment.confidence * 100).toFixed(1)}%</p>
            <div class="sentiment-details">
                <h4>Word Analysis:</h4>
                <ul>
                    <li>Positive Words: ${analysis.sentiment.details["Positive Words"]}</li>
                    <li>Negative Words: ${analysis.sentiment.details["Negative Words"]}</li>
                    <li>Total Words: ${analysis.sentiment.details["Total Words"]}</li>
                </ul>
            </div>
        `;

        // Display grammar results with enhanced formatting
        grammarResult.innerHTML = `
            <h3>Grammar Analysis</h3>
            ${analysis.grammarIssues.length > 0 
                ? `<ul>${analysis.grammarIssues.map(issue => `
                    <li>
                        <strong>${issue.issueType}</strong>: ${issue.description}
                        <br>Suggestion: ${issue.suggestion}
                        <br>Position: ${issue.position}
                    </li>
                `).join('')}</ul>`
                : '<p>No grammar issues found</p>'}
        `;

        // Display vocabulary results with enhanced formatting
        vocabularyResult.innerHTML = `
            <h3>Vocabulary Analysis</h3>
            <p>Unique words: ${analysis.vocabulary.uniqueWordCount}</p>
            ${analysis.vocabulary.suggestions.length > 0 
                ? `<h4>Suggestions:</h4>
                   <ul>${analysis.vocabulary.suggestions.map(suggestion => `
                    <li>
                        <strong>${suggestion.word}</strong>: ${suggestion.suggestion}
                        <br>Reason: ${suggestion.reason}
                    </li>
                `).join('')}</ul>`
                : '<p>No vocabulary suggestions</p>'}
        `;

        // Display target audience results with enhanced formatting
        targetAudienceResult.innerHTML = `
            <h3>Target Audience</h3>
            <p>Reading Level: <strong>${analysis.targetAudience.readingLevel}</strong></p>
            <p>Age Group: ${analysis.targetAudience.ageGroup}</p>
            <p>Education Level: ${analysis.targetAudience.educationLevel}</p>
            <p>Confidence: ${(analysis.targetAudience.confidence * 100).toFixed(1)}%</p>
        `;
    }

    function showLoading() {
        loadingIndicator.style.display = 'block';
        analyzeButton.disabled = true;
    }

    function hideLoading() {
        loadingIndicator.style.display = 'none';
        analyzeButton.disabled = false;
    }

    function showError(message) {
        errorMessage.textContent = message;
        errorMessage.style.display = 'block';
    }

    function clearError() {
        errorMessage.textContent = '';
        errorMessage.style.display = 'none';
    }

    function clearResults() {
        sentimentResult.innerHTML = '';
        grammarResult.innerHTML = '';
        vocabularyResult.innerHTML = '';
        targetAudienceResult.innerHTML = '';
    }
}); 