### **Prompt for Claude Code: Sqordia Project Context & Vision**

**Context:**
You are a Senior Full-Stack Architect helping me evolve **Sqordia Inc.**, a SaaS and OBNL strategic planning platform. The codebase consists of an **ASP.NET Core 8 API** (following Clean Architecture) and a **React/Vite/Tailwind** frontend.

**The Vision: The "Growth Architect" 🚀**
We are shifting the product from a static business plan generator to a dynamic "Growth Engine." The core persona is the **Growth Architect**: a consultant or entrepreneur who treats business planning as engineering. They need quantitative rigor, operational clarity, and bank-ready output.

**Key Frontend Concepts (To be implemented/updated):**

1. **5-Step Wizard:** A split-pane UI where the left side is for input and the right side is a live, auto-updating "Bank-Ready" document preview.
2. **The Strategy Map (Step 4):** A node-based canvas (drag-and-drop) where users map out their acquisition funnel (e.g., LinkedIn Ads -> Discovery Call -> Sale).
3. **Socratic Coach Sidebar:** An AI mentor that audits the plan in three categories: **Financial**, **Legal/Compliance**, and **Strategic**. It provides a "Nudge" (question) and a "Triad" (three clickable Smart Suggestions).

**Key Backend Architecture (Current State):**

* **Domain:** Includes entities like `BusinessPlan`, `AIPrompt`, `FinancialProjection`, and `QuestionnaireTemplate`.
* **Application:** Uses the CQRS pattern (Commands/Queries) and service interfaces like `IAIService` and `IBusinessPlanService`.
* **Infrastructure:** Contains the `OpenAIService` (handling GPT-4o calls) and `DocumentExportService`.
* **Persistence:** EF Core with PostgreSQL.

**Your Mission:**
You will be tasked with implementing the "Intelligence Layer" that connects these components. This includes:

* Updating **Domain Entities** to support growth metrics (Readiness Score, Pivot Point, Runway).
* Implementing the **Socratic Coach API** that returns structured JSON for the frontend sidebar.
* Building the **Financial Benchmark Engine** to validate user-defined conversion rates against industry standards.
* Enabling **Quebec-specific compliance logic** (Bill 96) for bilingual planning.

**Technical Guardrails:**

1. **Follow Clean Architecture:** Keep business logic in `Application`, entities in `Domain`, and external integrations in `Infrastructure`.
2. **Reactive State:** The backend must support a "Live Sync" feel. When a node in the Strategy Map changes, the financial projections must recalculate.
3. **Prompt Studio Logic:** Use and extend the existing `AIPrompt` system for managing AI personas and system instructions.


To move from vision to code, we will break down the implementation details for the **Sqordia Backend Intelligence Layer**. These details are organized by the **Clean Architecture** layers to match your existing project structure.

---

### **1. Domain Layer Updates (`Sqordia.Domain`)**

The foundation of the "Growth Architect" features requires new data structures to store the strategy map and growth metrics.

**New Enums (`Sqordia.Domain/Enums`):**

```csharp
public enum PersonaType { Entrepreneur, Consultant, OBNL }
public enum AuditCategory { Financial, Strategic, Legal, Compliance }

```

**Entity Update (`Sqordia.Domain/Entities/BusinessPlan.cs`):**
Add these properties to the existing `BusinessPlan` entity:

* `PersonaType Persona { get; set; }`
* `string StrategyMapJson { get; set; }` // Stores the React Flow node/edge data
* `decimal ReadinessScore { get; set; }`
* `FinancialHealthMetrics HealthMetrics { get; set; }` // Value Object

**Value Object (`Sqordia.Domain/ValueObjects/FinancialHealthMetrics.cs`):**

```csharp
public record FinancialHealthMetrics(
    int PivotPointMonth, 
    int RunwayMonths, 
    decimal MonthlyBurnRate, 
    decimal TargetCAC
);

```

---

### **2. Application Layer Logic (`Sqordia.Application`)**

This is where the "Brain" of the Growth Architect lives.

#### **A. The Socratic Coach Service**

This service uses your existing `IAIService` but applies the **Nudge + Triad** logic.

**Interface:**

```csharp
public interface IAuditService {
    Task<AuditResult> AuditSectionAsync(Guid planId, string sectionName);
}

```

**Implementation Detail (The "Nudge" DTO):**
We must ensure the AI returns a structured format so the frontend can render the clickable "Triad."

```csharp
public class AuditResult {
    public string CategoryBadge { get; set; } // e.g., "[FINANCIAL AUDIT]"
    public string Nudge { get; set; }         // The Socratic Question
    public List<SmartSuggestion> Triad { get; set; } 
}

public class SmartSuggestion {
    public string Label { get; set; }  // "Option A"
    public string Text { get; set; }   // The actual advice
    public string Action { get; set; } // "update_budget", "add_risk_note"
}

```

#### **B. Readiness Score Calculation**

A service to compute the "Bank-Ready" percentage based on three weights.

* **Weights:** 50% Consistency, 30% Risk Mitigation, 20% Completeness.

```csharp
public decimal CalculateScore(BusinessPlan plan) {
    var completeness = plan.Sections.Count(s => !string.IsNullOrEmpty(s.Content)) / TotalSections;
    var riskMitigation = plan.AuditLogs.Count(a => a.IsResolved) / TotalAudits;
    // Logical Check: If Marketing Budget > 0 but Projected Leads == 0, deduct points.
    return (completeness * 0.2m) + (riskMitigation * 0.3m) + (consistency * 0.5m);
}

```

---

### **3. Infrastructure Layer (`Sqordia.Infrastructure`)**

This layer handles the heavy lifting of AI prompt engineering and data validation.

#### **A. Financial Benchmark Engine**

To keep the UI responsive, we use a local JSON file containing industry benchmarks rather than calling the AI for every slider move.

**`IndustryBenchmarks.json`:**

```json
{
  "SaaS": { "AvgCAC": 200, "ConvRate": 0.03 },
  "Consulting": { "AvgCAC": 500, "ConvRate": 0.10 }
}

```

**Implementation:**
The `FinancialBenchmarkService` compares the `StrategyMapJson` conversion rates against these values. If the user inputs a 90% conversion rate for cold emails, the service returns a `BenchmarkWarning`.

#### **B. AI Prompt Engineering (The "Technicolor" System Prompt)**

You will update the `OpenAIService` to use a specific system instruction for the coach:

> "You are the Sqordia Coach. Your output MUST be valid JSON. Analyze the business plan for contradictions. If the user has high revenue but low marketing spend, flag it as [FINANCIAL AUDIT]. Your response must include one 'Nudge' question and exactly three options (A, B, C)."

---

### **4. Web API Layer (`Sqordia.WebAPI`)**

Exposing the new intelligence features to your React frontend.

**New Endpoints:**

* `POST /api/v1/business-plans/{id}/strategy-map`: Receives the graph data and triggers a background recalculation of the financial projections.
* `GET /api/v1/business-plans/{id}/audit`: Triggers the Socratic Coach for the currently active section in the wizard.
* `POST /api/v1/business-plans/{id}/share/vault`: Creates a secure "Live-Link" with the `ExpiresAt` and `Watermark` settings.

---

### **5. Implementation Timeline (Backend)**

1. **Week 1: Schema & State**
* EF Migrations for `StrategyMapJson` and `PersonaType`.
* Update `BusinessPlanService` to handle JSON graph persistence.


2. **Week 2: Intelligence Layer**
* Build `FinancialBenchmarkService` (Static JSON validation).
* Build `AuditService` (AI integration for Nudge/Triad).


3. **Week 3: Export & Security**
* Update `DocumentExportService` to convert Strategy Maps into PDF images.
* Implement "The Vault" logic (Link expiry and access logging).


Addition information for the backend changes:
Backend Changes:

New Endpoint: GET /api/v1/questionnaire/templates/{persona} - Get persona-specific questions
New Endpoint: POST /api/v1/questionnaire/polish-text - AI text enhancement
Request: { text: string, context?: string }
Response: { polishedText: string, suggestions?: string[] }
Update: QuestionnaireController.cs - Add persona filtering
New Service: QuestionPolishService.cs - AI-powered text enhancement
Uses OpenAI/Claude to transform raw notes to professional prose
Maintains BDC-standard formatting
Database:

New Table: QuestionTemplatesV2 - New question structure
Columns: Id, PersonaType, StepNumber, Order, QuestionText, HelpText, QuestionType, IsRequired
Migration: Create new question templates for each persona
Seed Data: Insert 20 questions per persona (60 total questions)




The Sqordia 20: Business Plan Questions
Section 1: Identity & Vision
1. What is the legal name and business structure (e.g., Corporation)? ⚖️
2. Is this a new startup, an expansion, or an acquisition? 🚀
3. What is your mission statement (why does the business exist)? 🎯
4. What are your top three objectives for the first 12 months? 📍
Section 2: The Offering
5. What specific market problem or gap are you solving? 🔍
6. Describe your product or service in detail. 📦
7. What is your Unique Selling Proposition (USP)? ✨
8. What is your revenue model (how do you charge)? 💸
Section 3: Market Analysis
9. What is your primary location (City and Province)? 🗺️
10. Who is your ideal customer (demographics and interests)? 👥
11. Who are your top three competitors? 🏁
12. What major industry trend are you capitalizing on? 📈
Section 4: Operations & People
13. What relevant experience does the founding team bring? 🎓
14. What is your staffing plan for the next year? 🤝
15. What essential technology or equipment is required? 💻
16. Who are your key strategic partners or suppliers? 🔗
Section 5: Financials & Risks
17. How much total funding are you seeking? 💵
18. How will you allocate these funds (list top 3 expenses)? 📋
19. What are your sales projections for Years 1 and 2? 📉
20. What is your biggest risk and your specific pivot plan? 🛡️

