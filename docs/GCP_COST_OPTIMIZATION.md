# GCP Cost Optimization Guide

## Current GCP Cost Breakdown

| Service | First 12 Months | After 12 Months | Can Optimize? |
|---------|----------------|------------------|---------------|
| Cloud Run | $10-12/month | $10-12/month | ✅ Yes |
| Cloud SQL | $0 (free tier) | $7-10/month | ✅ Yes |
| Cloud Load Balancing | $0 (free tier) | $18/month | ✅ **YES - Skip it!** |
| Cloud DNS | $0.20/month | $0.20/month | ⚠️ Minimal |
| Cloud Storage | $0.50/month | $0.50/month | ⚠️ Minimal |
| Cloud Pub/Sub | $0 (free tier) | $0 (free tier) | ❌ Already free |
| Cloud Functions | $0 (free tier) | $0 (free tier) | ❌ Already free |
| Secret Manager | $0.06/month | $0.06/month | ⚠️ Minimal |
| Cloud Logging | $0.50-1/month | $0.50-1/month | ✅ Yes |
| Data Transfer | $1-2/month | $1-2/month | ⚠️ Minimal |
| Artifact Registry | $0 (free) | $0 (free) | ❌ Already free |
| **TOTAL** | **~$12-15/month** | **~$37-43/month** | |

## Cost Optimization Strategies

### 1. Skip Cloud Load Balancing (SAVINGS: $18/month) ⭐ HIGHEST IMPACT

**Current Cost:** $18/month after free tier expires

**Alternative:** Use Cloud Run URL directly
- Cloud Run provides a default HTTPS URL: `https://sqordia-api-production-xxxxx.run.app`
- **Cost:** $0 (included with Cloud Run)
- **Savings:** $18/month

**Trade-offs:**
- ✅ No additional cost
- ✅ HTTPS included automatically
- ✅ Simpler setup
- ⚠️ URL is not custom domain (but you're skipping DNS anyway)
- ⚠️ Can add custom domain later if needed

**Recommendation:** **SKIP IT** - You're already skipping DNS cutover, so this makes perfect sense.

### 2. Use Cloud SQL Shared-Core Instance (SAVINGS: $2-5/month)

**Current:** `db-f1-micro` (dedicated core) = $7-10/month after free tier

**Alternative:** Use `db-shared-core` instance
- **Cost:** ~$5-7/month (slightly cheaper)
- **Savings:** $2-5/month

**Trade-offs:**
- ✅ Lower cost
- ⚠️ Slightly less performance (shared CPU)
- ⚠️ Still sufficient for small to medium workloads

**Alternative 2:** Use Cloud SQL for PostgreSQL with minimal configuration
- Keep `db-f1-micro` but optimize storage
- Use 10GB storage instead of 20GB: **Savings: ~$1/month**

**Recommendation:** Keep `db-f1-micro` but reduce storage if not needed.

### 3. Replace Cloud Run with Cloud Functions for API (SAVINGS: $10-12/month) ⚠️ ARCHITECTURAL CHANGE

**Current:** Cloud Run = $10-12/month

**Alternative:** Use Cloud Functions (2nd gen) for API
- **Cost:** $0 (within free tier: 2M invocations/month)
- **Savings:** $10-12/month

**Trade-offs:**
- ✅ Significant cost savings
- ✅ Pay-per-request model
- ⚠️ **Major architectural change** - requires refactoring
- ⚠️ Cold starts on first request
- ⚠️ Different deployment model
- ⚠️ May not support all ASP.NET Core features

**Recommendation:** **NOT RECOMMENDED** - Too much refactoring, keep Cloud Run.

### 4. Optimize Cloud Logging (SAVINGS: $0.25-0.50/month)

**Current:** $0.50-1/month

**Optimizations:**
- Reduce log retention from 7 days to 3 days: **Savings: ~$0.25/month**
- Filter out verbose logs: **Savings: ~$0.25/month**
- Use log sampling for high-volume logs

**Recommendation:** Reduce retention to 3 days if acceptable.

### 5. Use Cloud Storage Nearline/Archive (SAVINGS: $0.10-0.20/month)

**Current:** Standard storage = $0.50/month

**Alternative:** Use Nearline storage class
- **Cost:** ~$0.30/month (for infrequently accessed files)
- **Savings:** $0.20/month

**Trade-offs:**
- ✅ Lower storage cost
- ⚠️ Slightly higher retrieval cost (but minimal for small usage)
- ⚠️ Only for files accessed < 1 time per month

**Recommendation:** Keep Standard storage - savings are minimal.

### 6. Skip Cloud DNS (SAVINGS: $0.20/month)

**Current:** $0.20/month

**Alternative:** Use external DNS provider (e.g., Cloudflare - free)
- **Cost:** $0
- **Savings:** $0.20/month

**Trade-offs:**
- ✅ Free
- ⚠️ Requires external DNS management
- ⚠️ Minimal savings

**Recommendation:** Keep Cloud DNS - minimal cost, better integration.

## Recommended Cost Optimization Plan

### Option 1: Maximum Savings (After Free Tier)

| Optimization | Savings | Impact |
|--------------|---------|--------|
| Skip Cloud Load Balancing | $18/month | ⭐⭐⭐ High |
| Reduce Cloud SQL storage to 10GB | $1/month | ⭐ Low |
| Reduce log retention to 3 days | $0.25/month | ⭐ Low |
| **TOTAL SAVINGS** | **~$19.25/month** | |

**New Total Cost:** ~$37-43/month → **~$18-24/month** (after free tier)

### Option 2: Balanced Approach (Recommended)

| Optimization | Savings | Impact |
|--------------|---------|--------|
| Skip Cloud Load Balancing | $18/month | ⭐⭐⭐ High |
| **TOTAL SAVINGS** | **$18/month** | |

**New Total Cost:** ~$37-43/month → **~$19-25/month** (after free tier)

**Why this is best:**
- ✅ Maximum impact with minimal changes
- ✅ You're already skipping DNS, so skipping Load Balancer makes sense
- ✅ Cloud Run URL works perfectly for your use case
- ✅ Can add Load Balancer + custom domain later if needed

## Updated Cost Estimates

### First 12 Months (With Optimizations)

| Service | Cost | Notes |
|---------|------|-------|
| Cloud Run | $10-12 | Pay-per-use |
| Cloud SQL | $0 | Free tier |
| Cloud Load Balancing | **$0** ✅ | **SKIPPED** |
| Cloud DNS | $0.20 | Keep for future |
| Cloud Storage | $0.50 | Standard |
| Cloud Pub/Sub | $0 | Free tier |
| Cloud Functions | $0 | Free tier |
| Secret Manager | $0.06 | Minimal |
| Cloud Logging | $0.50-1 | Optimized |
| Data Transfer | $1-2 | Minimal |
| **TOTAL** | **~$12.26-15.26/month** | Same as before |

### After 12 Months (With Optimizations)

| Service | Cost | Notes |
|---------|------|-------|
| Cloud Run | $10-12 | Pay-per-use |
| Cloud SQL | $7-10 | db-f1-micro |
| Cloud Load Balancing | **$0** ✅ | **SKIPPED** |
| Cloud DNS | $0.20 | Keep for future |
| Cloud Storage | $0.50 | Standard |
| Cloud Pub/Sub | $0 | Still free tier |
| Cloud Functions | $0 | Still free tier |
| Secret Manager | $0.06 | Minimal |
| Cloud Logging | $0.50-1 | Optimized |
| Data Transfer | $1-2 | Minimal |
| **TOTAL** | **~$19.26-25.26/month** | **$18/month savings!** |

## Cost Comparison: Optimized vs Original

| Period | Original | Optimized | Savings |
|--------|----------|-----------|---------|
| **First 12 Months** | ~$12-15/month | ~$12-15/month | $0 (already optimized) |
| **After 12 Months** | ~$37-43/month | **~$19-25/month** | **~$18/month** 🎉 |

## Implementation Steps

### 1. Skip Cloud Load Balancing

**In Terraform:**
- Remove `load_balancer.tf` file (or mark as optional)
- Remove Cloud DNS A record pointing to Load Balancer
- Use Cloud Run service URL directly

**In Code:**
- No changes needed - Cloud Run URL works the same way
- Update documentation to use Cloud Run URL

### 2. Optimize Cloud SQL (Optional)

**In Terraform (`cloud_sql.tf`):**
```hcl
# Reduce storage if not needed
allocated_storage = 10  # Instead of 20GB
```

**Savings:** ~$1/month

### 3. Optimize Cloud Logging (Optional)

**In Terraform (`cloud_logging.tf`):**
```hcl
# Reduce retention
retention_in_days = 3  # Instead of 7
```

**Savings:** ~$0.25/month

## Summary

### Best Cost Optimization: Skip Cloud Load Balancing

**Impact:**
- **Savings:** $18/month (after free tier expires)
- **New Total:** ~$19-25/month (down from ~$37-43/month)
- **Effort:** Minimal (just don't create the resource)
- **Trade-off:** Using Cloud Run URL instead of custom domain (which you're skipping anyway)

### Final Optimized Cost

| Period | Cost | Status |
|--------|------|--------|
| **First 12 Months** | **~$12-15/month** | ✅ Within $20/month budget |
| **After 12 Months** | **~$19-25/month** | ⚠️ Slightly over $20/month budget |

**Note:** After free tier expires, you'll be slightly over the $20/month budget, but still much better than the original $37-43/month estimate.

## Additional Cost-Saving Tips

1. **Monitor Usage:** Set up billing alerts to track costs
2. **Use Committed Use Discounts:** If usage is predictable, commit to 1-year terms (saves 20-30%)
3. **Right-size Resources:** Monitor Cloud Run and Cloud SQL usage, adjust as needed
4. **Use Preemptible/Spot Instances:** Not applicable for Cloud Run/SQL, but good to know
5. **Optimize Data Transfer:** Minimize outbound data transfer
6. **Review Logs Regularly:** Delete old logs, reduce verbosity

## Next Steps

1. ✅ **Skip Cloud Load Balancing** in Terraform configuration
2. ✅ Update plan to reflect this optimization
3. ✅ Use Cloud Run URL for API access
4. ⚠️ Monitor costs after deployment
5. ⚠️ Consider additional optimizations if needed

