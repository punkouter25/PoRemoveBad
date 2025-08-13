Product Requirements Document (PRD) — PoRemoveBad

1. Overview
PoRemoveBad is a small, focused text-processing application that identifies and removes unwanted words and "buzzwords" from text, produces statistics, and allows export of processed text. The application is implemented as a single-page UI and a backend API. This PRD is intentionally language-agnostic so it can be used to regenerate the project in any stack.

2. Goals
- Provide fast, accurate identification of words to remove or replace.
- Offer a clean, minimal UI for pasting text, seeing analysis, and exporting results.
- Provide a diagnostics page for dependency & health checks.
- Allow simple deployment to a cloud provider (e.g., Azure) with IaC.

3. Users & Personas
- Writer/Editor: Cleans copy to remove filler and buzzwords.
- Developer: Integrates the API into automation pipelines or CLI tools.
- Operator: Monitors system health and deploys the app.

4. Core Features (high-level)
- Text Input: Paste text or upload text files.
- Analysis Engine: Tokenizes text, computes part-of-speech, frequency, and identifies matches against replacement lists and buzzword lists.
- Replace / Remove: Preview and apply automatic replacements or removals.
- Export: Download processed text (plain text, optional CSV with statistics).
- Diagnostics: Health/status page showing connectivity to dependent services (e.g., storage).
- API: Endpoints for analysis, replacement suggestions, health checks, and exports.

5. System Components (language-agnostic)
- Frontend (Web UI)
  - Text Editor Component: Large input area, supports paste and basic formatting.
  - Results Panel: Shows suggested replacements, frequency and statistics, and word categories.
  - Action Bar: Buttons for Analyze, Apply Replacements, Export, Clear.
  - Diagnostics Page (/diag): Displays health check results from API (dependent services).
  - Navigation: Minimal nav that switches between Editor and Diagnostics.

- Backend (API)
  - Text Analysis Service: Exposes analyze endpoint that returns tokenization, part-of-speech, categories, and replacement suggestions.
  - Replacement Service: Applies replacements to provided text and returns processed output.
  - Export Service: Provides file generation and download endpoints.
  - Health & Diagnostics: Health endpoints that report connectivity to external resources (storage, configuration).
  - Static Assets: Serves frontend assets when hosting as a single deployable unit.

- Data & Storage
  - Replacement Lists: JSON or table storage containing word replacement rules and categories (buzzwords vs. replacements).
  - Metrics / Logs: Application logs and optional usage metrics stored in a file or telemetry backend.
  - Exports: Temporary storage for generated export files (if needed) — can be transient.

6. External Dependencies
- Object storage or table storage for word lists (e.g., Azure Table Storage).
- Optional: blob storage for exports.
- Logging framework (structured logs).
- Infrastructure tooling (IaC) for cloud deployment (e.g., Bicep, Terraform, ARM).

7. API Specification (language-agnostic)
- GET /healthz
  - Returns overall health and per-dependency status.
- POST /api/analyze
  - Request: { text: "<raw text>" }
  - Response: {
      tokens: [{ text, index, partOfSpeech, category }],
      statistics: { wordCount, sentenceCount, topWords: [] },
      suggestedReplacements: [{ original, replacement, reason }]
    }
- POST /api/replace
  - Request: { text: "<raw text>", apply: true|false, rules: [ruleId] }
  - Response: { processedText: "<processed text>", replacementsApplied: [...] }
- POST /api/export
  - Request: { text: "<raw text>", format: "txt"|"csv" }
  - Response: { downloadUrl: "https://..." } or binary stream

8. UI Pages & Components (language-agnostic)
- Home / Editor Page ("/")
  - Components:
    - TextInput (primary): large input area with paste support, file drop optional.
    - AnalyzeButton: sends content to /api/analyze.
    - ResultsPanel: lists tokens, categories, and replacement suggestions with toggles to accept/reject each suggestion.
    - StatisticsCard: quick stats (word count, buzzword count, top offenders).
    - ExportButton: calls /api/export after applying replacements.
    - Toast notifications: feedback for success/error.

- Diagnostics Page ("/diag")
  - Components:
    - HealthList: shows status of each dependency (Table Storage, Blob Storage, etc).
    - LogsPreview: tail of recent logs (developer mode).
    - ConfigSummary: non-sensitive configuration visible for diagnostics.

- Shared Components
  - NavBar: links to Editor and Diagnostics.
  - Modal: confirm before applying many replacements.
  - Loading / Spinner component.

9. Non-functional Requirements
- Performance: Analysis returns results for typical inputs (<50K characters) in under 1 second on moderate hardware.
- Security: Do not store uploaded text permanently by default. Sanitize all inputs.
- Observability: Structured logs, a simple log file retained per run, and a health endpoint.
- Portability: Can be rebuilt with any backend language; API contract is primary integration surface.
- Testability: Provide unit tests for analysis logic and integration tests for API endpoints.

10. Deployment & DevOps
- Local dev:
  - Run backend on localhost at well-known ports (e.g., 5000/5001).
  - Frontend can be served independently in dev mode or served by the backend in production.
- IaC:
  - Structure templates to create an App Service, Storage accounts (Table and Blob), Resource Group matching the solution name. Use parameters for naming.
- CI/CD:
  - Build, run tests, and deploy on merge to main.
  - Deploy process should allow "code only" deploys after initial infra exists.
- Secrets:
  - Local development: appsettings.Development.json
  - Production: environment variables on the App Service (or Key Vault if desired; this PRD assumes environment variables for simplicity).

11. Deliverables for Regeneration
- PRD.md (this document).
- README.md (runnable instructions and dependencies).
- Diagrams/ (machine-readable mermaid files and generated SVGs):
  - component.mmd
  - domain-model.mmd
  - user-workflow.mmd
  - feature-sequence.mmd
  - deployment-artifacts.mmd

12. Acceptance Criteria
- README.md contains:
  - Summary of the app.
  - How to build and run locally.
  - Major data / API connections (e.g., Azure Table Storage, health endpoints).
- PRD.md and Diagrams/ exist in repo root.
- Mermaid diagrams are accurate to the architecture and available as both .mmd and .svg files.

Appendix: Notes about current implementation (example mapping you can adapt)
- Current storage: Word replacement lists are shipped as JSON resources.
- HealthChecks: Backend includes TableStorage health check to validate connectivity.
- Diagnostics endpoint: available at /diag and reads health endpoints from server.
