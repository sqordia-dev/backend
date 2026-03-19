# CI/CD Improvement Plan

## Current State

| Workflow | Auto Trigger | Manual | Target Env |
|---|---|---|---|
| Backend CI | push/PR to `main`, `develop` | No | -- |
| Backend Deploy Prod | CI passes on `main` | `workflow_dispatch` (no env choice) | production |
| Backend Deploy Preprod | CI passes on `develop` | No | preprod |
| Frontend Deploy Prod | Frontend CI passes on `main` | No | production |
| Frontend Deploy Preprod | Frontend CI passes on `develop` | No | preprod |
| AI Service Deploy | push to `main` | `workflow_dispatch` | production only |
| DB Migration | push to `release` (paths) | `workflow_dispatch` with env choice | any |

## Problems

1. **Branch-locked**: Prod is hardwired to `main`, preprod to `develop`. Can't deploy a feature branch to preprod or hotfix to prod.
2. **No on-demand env selection**: Only the DB migration workflow lets you pick an environment. The deploy workflows don't.
3. **AI Service has no preprod** support -- hardcoded to production resources.
4. **Preprod deploy has no `workflow_dispatch`** -- can't trigger it manually.

## Goal

Any branch can deploy to any environment on demand via `workflow_dispatch`, while keeping automatic triggers for the standard flow (`main` -> prod, `develop` -> preprod).

---

## Current Deploy Chain

```
CI (build + unit tests)
  -> Docker image build (dry-run)
  -> DB migration
  -> Docker push to ACR
  -> Container App update
  -> Health check (curl /health)
  -> Maintenance mode toggle (prod only)
```

## Proposed Deploy Chain

```
CI (build + test)
  -> Security scan (NuGet vulns + Trivy)
  -> DB backup
  -> Migration safety check -> (auto-approve safe, manual-approve destructive)
  -> DB migration
  -> Docker build + push
  -> Deploy new revision (0% traffic)
  -> Smoke tests against new revision
  -> Traffic shift (100%) or auto-rollback
  -> E2E tests (preprod only)
  -> Deploy notification
  -> Disable maintenance mode
```

---

## New Steps Detail

### Pre-deployment

#### 1. Security Scanning
- `dotnet list package --vulnerable` for NuGet packages
- `trivy image` scan on the built Docker image
- Fail on CRITICAL/HIGH vulnerabilities
- **Effort**: Low | **Risk Reduction**: High

#### 2. DB Backup Before Migration
- `az postgres flexible-server backup create` before running EF migrations
- Cheap insurance, zero effort to restore if a migration goes wrong
- **Effort**: Low | **Risk Reduction**: Very high

#### 3. EF Migration Safety Check
- Detect destructive migrations (column drops, table drops)
- Auto-approve safe migrations (add column, add table, add index)
- Require manual approval for destructive changes
- Implementation: `dotnet ef migrations script` + grep for `DROP`, `ALTER COLUMN`, `DELETE`
- **Effort**: Medium | **Risk Reduction**: High

### During Deployment

#### 4. Smoke Tests After Deploy
- Not just `/health`, but hit 2-3 real endpoints:
  - `GET /health` (detailed, check DB + AI service connectivity)
  - `GET /api/v1/subscription-plans` (public endpoint, verifies DB reads work)
  - Auth token generation + authenticated endpoint (verifies JWT signing works)
- **Effort**: Low | **Risk Reduction**: High

#### 5. Blue-Green Revision Staging
- Azure Container Apps supports traffic splitting natively
- Deploy to a new revision with 0% traffic
- Run smoke tests against the new revision's direct URL
- Shift 100% traffic only after smoke tests pass
- Currently updates in-place -- broken deploys are live instantly
- **Effort**: Medium | **Risk Reduction**: Very high

### Post-deployment

#### 6. Automatic Rollback
- If smoke tests fail, revert to the previous Container App revision
- Pairs naturally with blue-green revision staging
- `az containerapp revision list` + `az containerapp ingress traffic set` to shift back
- **Effort**: Medium | **Risk Reduction**: High

#### 7. Deployment Notification
- Slack/Teams/Discord webhook with deploy status
- Include: commit SHA, branch, environment, duration, health check result
- Success and failure notifications
- **Effort**: Low | **Risk Reduction**: Low (but high visibility)

#### 8. E2E Tests Post-Deploy
- Trigger Playwright suite against preprod after deploy
- `e2e-tests.yml` already exists but is not wired into the deploy chain
- Run against preprod only (not production) to avoid test data pollution
- **Effort**: Low (already exists) | **Risk Reduction**: Medium

---

## Summary Table

| Step | Effort | Risk Reduction | Priority |
|---|---|---|---|
| Security scan | Low | High | P1 |
| DB backup pre-migration | Low | Very high | P1 |
| Migration safety check | Medium | High | P1 |
| Smoke tests | Low | High | P1 |
| Blue-green revision | Medium | Very high | P1 |
| Auto rollback | Medium | High | P1 |
| Deploy notification | Low | Low (high visibility) | P2 |
| E2E post-deploy | Low | Medium | P2 |

## What We Skip (For Now)

- **SAST/DAST** (SonarQube, OWASP ZAP) -- overkill for current team size, security-reviewer agent covers code-level issues
- **Performance/load testing** -- useful later, premature for preprod validation phase
- **Approval gates** -- GitHub environment protection rules already provide this without workflow changes

---

## Implementation Order

1. Unified workflow with `workflow_dispatch` (any branch -> any env)
2. Security scanning (NuGet + Trivy)
3. DB backup before migration
4. Migration safety check
5. Blue-green revision staging + smoke tests + auto-rollback
6. Deploy notification
7. E2E wiring
