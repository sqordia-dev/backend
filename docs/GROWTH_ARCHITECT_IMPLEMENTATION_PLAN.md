# Growth Architect Intelligence Layer - Implementation Plan

## Overview
Transform Sqordia from a static business plan generator to a dynamic "Growth Engine" with persona-based questionnaires, Socratic Coach AI auditing, strategy mapping, and bank-readiness scoring.

---

## Phase 1: Domain Layer (Foundation)

### 1.1 New Enums
| File | Description |
|------|-------------|
| `src/Core/Sqordia.Domain/Enums/PersonaType.cs` | `Entrepreneur`, `Consultant`, `OBNL` |
| `src/Core/Sqordia.Domain/Enums/AuditCategory.cs` | `Financial`, `Strategic`, `Legal`, `Compliance` |

### 1.2 New Value Object
| File | Description |
|------|-------------|
| `src/Core/Sqordia.Domain/ValueObjects/FinancialHealthMetrics.cs` | Record with `PivotPointMonth`, `RunwayMonths`, `MonthlyBurnRate`, `TargetCAC` |

### 1.3 BusinessPlan Entity Updates
**File:** `src/Core/Sqordia.Domain/Entities/BusinessPlan/BusinessPlan.cs`

Add properties:
- `PersonaType? Persona`
- `string? StrategyMapJson` (React Flow node/edge data)
- `decimal? ReadinessScore`
- `FinancialHealthMetrics? HealthMetrics`

Add methods:
- `SetPersona(PersonaType)`
- `UpdateStrategyMap(string?)`
- `UpdateReadinessScore(decimal)`
- `UpdateHealthMetrics(FinancialHealthMetrics)`

### 1.4 New Entity: QuestionTemplateV2
**File:** `src/Core/Sqordia.Domain/Entities/BusinessPlan/QuestionTemplateV2.cs`

Properties: `PersonaType`, `StepNumber`, `QuestionText`, `QuestionTextEN`, `HelpText`, `HelpTextEN`, `QuestionType`, `Order`, `IsRequired`, `Section`, `Options`, `ValidationRules`, `ConditionalLogic`, `IsActive`

---

## Phase 2: Contracts Layer (DTOs)

### 2.1 Request DTOs
| File | Purpose |
|------|---------|
| `Contracts/Requests/StrategyMap/SaveStrategyMapRequest.cs` | Strategy map JSON payload |
| `Contracts/Requests/Questionnaire/PolishTextRequest.cs` | Text, Language, Context, Tone |
| `Contracts/Requests/Share/CreateVaultShareRequest.cs` | ExpiresAt, EnableWatermark, WatermarkText, AllowDownload, TrackViews |

### 2.2 Response DTOs
| File | Purpose |
|------|---------|
| `Contracts/Responses/Audit/AuditSectionResponse.cs` | CategoryBadge, Nudge, Triad (3 SmartSuggestions) |
| `Contracts/Responses/Readiness/ReadinessScoreResponse.cs` | OverallScore, ConsistencyScore, RiskMitigationScore, CompletenessScore |
| `Contracts/Responses/StrategyMap/StrategyMapResponse.cs` | StrategyMapJson, ReadinessScore |
| `Contracts/Responses/Questionnaire/PolishedTextResponse.cs` | PolishedText, Improvements, Confidence |
| `Contracts/Responses/Questionnaire/PersonaQuestionResponse.cs` | PersonaType, StepNumber, QuestionText, etc. |
| `Contracts/Responses/Share/VaultShareResponse.cs` | ShareUrl, Token, Watermark settings |

---

## Phase 3: Application Layer (Services)

### 3.1 New Service Interfaces
| File | Methods |
|------|---------|
| `Services/IAuditService.cs` | `AuditSectionAsync()`, `GetAuditSummaryAsync()` |
| `Services/IReadinessScoreService.cs` | `CalculateReadinessScoreAsync()`, `GetReadinessBreakdownAsync()` |
| `Services/IQuestionPolishService.cs` | `PolishTextAsync()` |
| `Services/IFinancialBenchmarkService.cs` | `CompareToBenchmarksAsync()`, `GetAvailableBenchmarksAsync()` |
| `Services/IStrategyMapService.cs` | `SaveStrategyMapAsync()`, `GetStrategyMapAsync()` |

### 3.2 Service Implementations
| File | Description |
|------|-------------|
| `Services/Implementations/AuditService.cs` | Socratic Coach - calls AI with Nudge+Triad prompts |
| `Services/Implementations/ReadinessScoreService.cs` | Calculates weighted score (50% Consistency, 30% Risk, 20% Completeness) |
| `Services/Implementations/QuestionPolishService.cs` | AI text enhancement for professional prose |
| `Services/Implementations/StrategyMapService.cs` | Save/retrieve strategy map, trigger recalculation |

### 3.3 Update Existing Service
**File:** `Services/Implementations/QuestionnaireService.cs`
- Add `GetPersonaQuestionsAsync(string persona)` method

---

## Phase 4: Infrastructure Layer

### 4.1 Financial Benchmark Service
| File | Description |
|------|-------------|
| `Infrastructure/Services/FinancialBenchmarkService.cs` | Compare conversion rates against industry standards |
| `Infrastructure/Data/IndustryBenchmarks.json` | Static benchmark data (SaaS, Consulting, Retail, etc.) |

### 4.2 AI Service Updates (Socratic Coach Prompts)
**Files to modify:**
- `Infrastructure/Services/OpenAIService.cs`
- `Infrastructure/Services/ClaudeService.cs`
- `Infrastructure/Services/GeminiService.cs`

Add method: `PerformSocraticAuditAsync()` with structured JSON response for Nudge + Triad

### 4.3 Service Registration
**File:** `Infrastructure/ConfigureServices.cs`

Add:
```csharp
services.AddTransient<IAuditService, AuditService>();
services.AddTransient<IReadinessScoreService, ReadinessScoreService>();
services.AddTransient<IQuestionPolishService, QuestionPolishService>();
services.AddTransient<IFinancialBenchmarkService, FinancialBenchmarkService>();
services.AddTransient<IStrategyMapService, StrategyMapService>();
```

---

## Phase 5: Persistence Layer

### 5.1 EF Core Configurations
| File | Action |
|------|--------|
| `Persistence/Configurations/BusinessPlan/BusinessPlanConfiguration.cs` | Add Persona, StrategyMapJson (jsonb), ReadinessScore, HealthMetrics (owned) |
| `Persistence/Configurations/BusinessPlan/QuestionTemplateV2Configuration.cs` | NEW - Configure QuestionTemplatesV2 table |

### 5.2 DbContext Update
**File:** `Persistence/Contexts/ApplicationDbContext.cs`
- Add `DbSet<QuestionTemplateV2> QuestionTemplatesV2`

### 5.3 Migration
**File:** `Persistence/Migrations/[Timestamp]_AddGrowthArchitectFeatures.cs`

Creates:
- 7 new columns on BusinessPlans table (Persona, StrategyMapJson, ReadinessScore, PivotPointMonth, RunwayMonths, MonthlyBurnRate, TargetCAC)
- QuestionTemplatesV2 table with indexes

### 5.4 Seed Data
**File:** `scripts/seed-persona-questions.sql`
- 60 questions total (20 per persona: Entrepreneur, Consultant, OBNL)
- 5 steps per persona with 4 questions each

---

## Phase 6: Web API Layer

### 6.1 New Controllers
| File | Endpoints |
|------|-----------|
| `Controllers/StrategyMapController.cs` | `POST /api/v1/business-plans/{id}/strategy-map`, `GET /api/v1/business-plans/{id}/strategy-map` |
| `Controllers/AuditController.cs` | `GET /api/v1/business-plans/{id}/audit?section=`, `GET /api/v1/business-plans/{id}/audit/summary` |

### 6.2 Controller Updates
| File | New Endpoints |
|------|---------------|
| `Controllers/BusinessPlanShareController.cs` | `POST /api/v1/business-plans/{id}/shares/vault` |
| `Controllers/QuestionnaireController.cs` | `GET /api/v1/questionnaire/templates/{persona}`, `POST /api/v1/questionnaire/polish-text` |

---

## Implementation Order

### Sprint 1: Foundation (Days 1-2)
1. Create enums: `PersonaType.cs`, `AuditCategory.cs`
2. Create value object: `FinancialHealthMetrics.cs`
3. Update `BusinessPlan.cs` entity
4. Create `QuestionTemplateV2.cs` entity
5. Create EF configurations
6. Update `ApplicationDbContext.cs`
7. Create and run migration
8. Create seed script for 60 questions

### Sprint 2: Contracts (Day 3)
1. Create all Request DTOs
2. Create all Response DTOs
3. Build and verify

### Sprint 3: Application Services (Days 4-6)
1. Create service interfaces
2. Implement `AuditService`
3. Implement `ReadinessScoreService`
4. Implement `QuestionPolishService`
5. Implement `StrategyMapService`
6. Update `QuestionnaireService`

### Sprint 4: Infrastructure (Days 7-8)
1. Create `FinancialBenchmarkService`
2. Create `IndustryBenchmarks.json`
3. Add Socratic Coach prompts to AI services
4. Register all services in DI

### Sprint 5: API & Testing (Days 9-10)
1. Create `StrategyMapController`
2. Create `AuditController`
3. Update `BusinessPlanShareController`
4. Update `QuestionnaireController`
5. Write unit tests
6. Integration testing

---

## Critical Files Summary

| Layer | Files to Create | Files to Modify |
|-------|-----------------|-----------------|
| Domain | 4 new files | `BusinessPlan.cs` |
| Contracts | 9 new files | - |
| Application | 9 new files | `QuestionnaireService.cs` |
| Infrastructure | 2 new files | `OpenAIService.cs`, `ClaudeService.cs`, `GeminiService.cs`, `ConfigureServices.cs` |
| Persistence | 2 new files | `BusinessPlanConfiguration.cs`, `ApplicationDbContext.cs` |
| WebAPI | 2 new files | `BusinessPlanShareController.cs`, `QuestionnaireController.cs` |

**Total: ~28 new files, ~8 files to modify**

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/business-plans/{id}/strategy-map` | Save strategy map |
| GET | `/api/v1/business-plans/{id}/strategy-map` | Get strategy map |
| GET | `/api/v1/business-plans/{id}/audit` | Socratic Coach audit |
| GET | `/api/v1/business-plans/{id}/audit/summary` | Audit summary |
| POST | `/api/v1/business-plans/{id}/shares/vault` | Create vault share |
| GET | `/api/v1/questionnaire/templates/{persona}` | Get persona questions |
| POST | `/api/v1/questionnaire/polish-text` | AI text enhancement |

---

## Technical Decisions

| Decision | Choice | Notes |
|----------|--------|-------|
| Strategy Map sync | REST only | SignalR real-time collaboration planned for future sprint |
| Seed questions | Generate 60 bilingual questions | Based on BDC standards, 20 per persona |
| Implementation scope | All features | Implement complete Growth Architect layer |
