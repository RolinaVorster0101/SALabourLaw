# SA Labour Law & Contract Compliance Assistant

![Status](https://img.shields.io/badge/Status-Active-green) ![Stack](https://img.shields.io/badge/Stack-ASP.NET%20Core%208%20%7C%20Azure%20OpenAI%20%7C%20Azure%20AI%20Search-blue)

## Problem Statement
South African employment law is dense, frequently misunderstood, and rarely consulted until there's already a dispute. The Basic Conditions of Employment Act (BCEA), the Labour Relations Act (LRA), and CCMA guidance govern everything from notice periods to leave entitlements to dismissal procedures, but most employers and employees never read the legislation directly — they rely on guesswork, outdated templates, or expensive legal consultations for questions that often have a clear, documented answer.

This project applies Retrieval-Augmented Generation (RAG) to that problem. RAG is the architectural pattern behind every serious enterprise AI application in 2026 — "chat with your documents," internal knowledge bases, document Q&A systems — and it directly addresses the core limitation of LLMs: they only know what they were trained on. RAG feeds them fresh, specific, authoritative information at query time before they generate an answer, rather than letting the model guess from general training knowledge.

The tool operates in two modes. A general Q&A mode lets any user ask questions directly against the BCEA, LRA, and CCMA guidance — "how much notice is required to terminate an employee," "what are the rules around Sunday pay." A contract review mode lets a user upload their own employment contract and ask whether specific clauses comply with the legislation, with the tool retrieving relevant sections from both the uploaded contract and the legislation simultaneously and flagging discrepancies with citations back to both sources.

This project is both a practical tool and a direct lab for the AI-103 certification (Azure AI Apps and Agents Developer Associate), covering exactly the concepts that exam tests.

## Architecture
Legislation ingestion pipeline: the BCEA, LRA, and CCMA guidance documents are chunked into overlapping segments, each chunk is converted into a vector embedding using Azure OpenAI embedding models, and the embeddings are stored in Azure AI Search with the original text as metadata. This collection is indexed once and persists as the permanent compliance baseline.

Contract ingestion pipeline: when a user uploads their own employment contract, the same chunking and embedding process runs against that document, but the resulting vectors are stored in a separate, session-scoped collection rather than the permanent legislation index — keeping uploaded personal documents isolated from the shared knowledge base.

Query pipeline — Q&A mode: a user question is embedded using the same model, a similarity search retrieves the most relevant chunks from the legislation index, and Azure OpenAI generates an answer grounded in that retrieved context with citations back to the specific act and section.

Query pipeline — contract review mode: a user question is embedded and matched against both the uploaded contract's vectors and the legislation index simultaneously. Retrieved chunks from both sources are assembled into a single context window, and the generation prompt explicitly instructs the model to treat the uploaded contract as the subject under review and the legislation as the compliance reference — comparing the two rather than conflating them.

This architecture ensures the model never answers from general training knowledge alone — every response is explicitly grounded in either the legislation, the uploaded contract, or both, which is the fundamental RAG guarantee.

## Features
- Legislation knowledge base — BCEA, LRA, and CCMA guidance pre-indexed and queryable from launch
- Contract upload — PDF and text employment contract ingestion via a simple upload interface
- Dual-mode querying — general legislation Q&A and contract-specific compliance review in one interface
- Chunking pipeline — documents split into overlapping segments with configurable chunk size and overlap
- Embedding generation — Azure OpenAI text-embedding-3-small converts each chunk to a vector representation
- Vector storage — embeddings and source text stored in Azure AI Search with vector search capability
- Similarity retrieval — user query embedded and matched against stored vectors to find most relevant chunks
- Grounded generation — Azure OpenAI gpt-5-mini generates answers using only the retrieved chunks as context
- Cross-source comparison — contract review answers cite both the uploaded clause and the relevant legislative section side by side
- Source citations — every answer includes which act and section the answer was drawn from
- Chat-style frontend — conversational interface for querying legislation or an uploaded contract
- Not-legal-advice disclaimer — clearly surfaced in the UI given the employment law subject matter
- Session isolation — uploaded contracts are scoped to a session and not persisted permanently
- PDF ingestion — both legislation seeding and contract upload support PDF files via PdfPig

## Data Sources
- Basic Conditions of Employment Act (BCEA) — consolidated act, South African government legislation, public domain
- Labour Relations Act (LRA) — South African government legislation, public domain
- CCMA Guidelines: Misconduct Arbitrations — public guidance from the Commission for Conciliation, Mediation and Arbitration
- User-uploaded employment contracts — PDF and plain text files uploaded directly to the application, session-scoped and not persisted to the permanent index

## Tech Stack
- ASP.NET Core 8 (Razor Pages)
- Azure OpenAI (text-embedding-3-small for embeddings, gpt-5-mini for generation)
- Azure AI Search (vector index storage and similarity retrieval)
- PdfPig (PDF text extraction)
- Custom CSS (no Bootstrap or jQuery)

## Skills Demonstrated
- RAG architecture end-to-end (chunking, embedding, retrieval, grounded generation)
- Azure OpenAI integration
- Azure AI Search vector indexing and querying
- ASP.NET Core 8 Razor Pages
- Session-scoped data isolation
- PDF ingestion pipeline
- Custom UI without CSS frameworks

## Project Structure
```
/Pages
  /Ask/Index.cshtml(.cs)          — Legislation Q&A chat interface
  /Contracts/Upload.cshtml(.cs)   — Contract upload and ingestion
  /Contracts/Review.cshtml(.cs)   — Contract review chat interface
/Services
  EmbeddingService.cs             — Azure OpenAI embedding calls
  VectorSearchService.cs          — Azure AI Search queries and upserts
  ChunkingService.cs              — Overlapping chunk splitting
  RagQueryService.cs              — Retrieval, prompt assembly, generation
  LegislationSeederService.cs     — One-time ingestion of BCEA/LRA/CCMA
/Models
  ChatMessage.cs
  RetrievedChunk.cs
  LegislationChunk.cs
  ContractChunk.cs
  SessionExtensions.cs
  SeedDocument.cs
/SeedData
  basic-conditions-of-employment-act-75-of-1997.pdf
  labour-relations-act-66-of-1995.pdf
  ccma-guidelines-misconduct-arbitrations.pdf
```

## Current Status
Core RAG pipeline fully functional end-to-end. Legislation indexed and queryable. Contract upload and dual-source compliance review working. See Pending / Next Steps below for outstanding items.

## Pending / Next Steps
- **Admin re-seeding page** (`/Admin/Legislation`) — UI to re-run the legislation ingestion pipeline without code changes or app restart
- **Azure App Service deployment** — app currently runs locally only
- **Session cleanup job** — purge contract vectors from the index after session expiry (`uploadedAt` field is in place, cleanup logic not yet implemented)
- **Retrieval confidence indicators** — surface similarity scores in the UI so users understand how well retrieved chunks matched their query
- **Streaming responses** — stream tokens as they generate rather than waiting for the full response

---
*Part of [RolinaVorster0101](https://github.com/RolinaVorster0101)'s portfolio*
