Diagrams — PoRemoveBad

This folder contains Mermaid source files and generated SVGs for the application architecture and workflows.

Files:
- component.mmd — Component-level architecture (frontend, backend, core, storage, infra).
- domain-model.mmd — Domain class diagram for TextDocument, Token, ReplacementRule, AnalysisResult, etc.
- user-workflow.mmd — High-level user workflow from paste -> analyze -> replace -> export.
- feature-sequence.mmd — Sequence diagram showing analyze -> replace -> export.
- deployment-artifacts.mmd — Shows Azure resources used and how they connect.

Generated assets:
- component.svg
- domain-model.svg
- user-workflow.svg
- feature-sequence.svg
- deployment-artifacts.svg

Generating SVGs locally (requires Node.js + npm):
1. Install mermaid CLI as a local dev dependency:
   npm install -D @mermaid-js/mermaid-cli

2. Convert a single file:
   npx mmdc -i Diagrams/component.mmd -o Diagrams/component.svg

3. Convert all .mmd files (example script):
   npx mmdc -i Diagrams/component.mmd -o Diagrams/component.svg
   npx mmdc -i Diagrams/domain-model.mmd -o Diagrams/domain-model.svg
   npx mmdc -i Diagrams/user-workflow.mmd -o Diagrams/user-workflow.svg
   npx mmdc -i Diagrams/feature-sequence.mmd -o Diagrams/feature-sequence.svg
   npx mmdc -i Diagrams/deployment-artifacts.mmd -o Diagrams/deployment-artifacts.svg

Notes:
- The .mmd files are language-agnostic and intended to be used by other assistants or tooling to recreate the diagrams.
- This repository will include both .mmd and .svg assets after conversion to make diagrams available in PRs and documentation.
