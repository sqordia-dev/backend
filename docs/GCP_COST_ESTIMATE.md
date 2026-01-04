# Google Cloud Platform (GCP) Cost Estimate for Sqordia Infrastructure

## 📊 AWS to GCP Service Mapping

| AWS Service | GCP Equivalent | Notes |
|-------------|----------------|-------|
| **ECS Fargate** | Cloud Run or GKE Autopilot | Container orchestration |
| **RDS PostgreSQL** | Cloud SQL for PostgreSQL | Managed PostgreSQL |
| **ALB** | Cloud Load Balancing | Load balancing |
| **Route 53** | Cloud DNS | DNS management |
| **S3** | Cloud Storage | Object storage |
| **SQS** | Cloud Pub/Sub or Cloud Tasks | Message queuing |
| **Lambda** | Cloud Functions | Serverless compute |
| **Secrets Manager** | Secret Manager | Secrets management |
| **CloudWatch Logs** | Cloud Logging | Logging |
| **ECR** | Artifact Registry | Container registry |

## 💰 Monthly Cost Breakdown

### Core Infrastructure Services

| Service | Configuration | Monthly Cost | Free Tier | Notes |
|---------|--------------|--------------|-----------|-------|
| **Cloud Run** | 0.5 vCPU, 1GB RAM | ~$10-12 | ✅ Always | Pay per request + compute time |
| **Cloud SQL PostgreSQL** | db-f1-micro, 20GB | $0 → $7-10 | ✅ 12 months | Free tier for first year |
| **Cloud Load Balancing** | HTTP(S) Load Balancer | $0 → ~$18 | ✅ Partial | First 5 forwarding rules free |
| **Cloud DNS** | Managed zone | $0.20 | ❌ | Always $0.20/month per zone |
| **Cloud Storage** | Standard, 20GB | ~$0.50 | ✅ Partial | First 5GB free |
| **Cloud Pub/Sub** | 3 topics | $0 | ✅ | 10GB/month free |
| **Cloud Functions** | 3 functions, 512MB | $0 | ✅ | 2M invocations/month free |
| **Secret Manager** | 1 secret | $0.06 | ❌ | $0.06 per secret/month |
| **Cloud Logging** | 7-day retention | ~$0.50-1 | ✅ Partial | First 50GB ingestion free |
| **Data Transfer** | Outbound | $1-2 | ✅ Partial | First 1GB free |
| **Artifact Registry** | Standard tier | $0 | ✅ | Always free |
| **VPC/Networking** | VPC, subnets | $0 | ✅ | Always free |

## 💰 Monthly Cost Summary

### First 12 Months (GCP Free Tier) 🎉

| Category | Cost | Details |
|----------|------|---------|
| **Cloud Run** | $10-12 | 0.5 vCPU + 1GB RAM, pay-per-use |
| **Cloud SQL PostgreSQL** | **$0** ✅ | Free tier (db-f1-micro, 20GB) |
| **Cloud Load Balancing** | **$0** ✅ | First 5 forwarding rules free |
| **Cloud DNS** | $0.20 | Managed zone (no free tier) |
| **Cloud Storage** | $0.50 | 20GB (first 5GB free) |
| **Cloud Pub/Sub** | **$0** ✅ | 10GB/month free |
| **Cloud Functions** | **$0** ✅ | 2M invocations/month free |
| **Secret Manager** | $0.06 | 1 secret |
| **Cloud Logging** | $0.50-1 | 7-day retention (first 50GB free) |
| **Data Transfer** | $1-2 | Outbound (first 1GB free) |
| **Artifact Registry** | **$0** ✅ | Always free |
| **VPC/Networking** | **$0** ✅ | Always free |
| | | |
| **TOTAL** | **~$12.26-15.26/month** 🎉 | |

### After 12 Months (Free Tier Expires)

| Category | Cost | Details |
|----------|------|---------|
| **Cloud Run** | $10-12 | Same as before |
| **Cloud SQL PostgreSQL** | $7-10 | Free tier expired |
| **Cloud Load Balancing** | $18 | Free tier expired |
| **Cloud DNS** | $0.20 | Same as before |
| **Cloud Storage** | $0.50 | Same as before |
| **Cloud Pub/Sub** | **$0** ✅ | Still within free tier |
| **Cloud Functions** | **$0** ✅ | Still within free tier |
| **Secret Manager** | $0.06 | Same as before |
| **Cloud Logging** | $0.50-1 | Same as before |
| **Data Transfer** | $1-2 | Same as before |
| **Artifact Registry** | **$0** ✅ | Always free |
| **VPC/Networking** | **$0** ✅ | Always free |
| | | |
| **TOTAL** | **~$37.26-43.26/month** | |

## 📈 Detailed Cost Breakdown

### Cloud Run (~$10-12/month)
- **CPU:** 0.5 vCPU × $0.00002400/vCPU-second × 2,592,000 seconds = **~$31.10**
- **Memory:** 1 GB × $0.00000250/GB-second × 2,592,000 seconds = **~$6.48**
- **Requests:** 1M requests × $0.40 per million = **$0.40**
- **Total:** **~$38/month** (if running 24/7)
- **Note:** Cloud Run charges only when handling requests, so actual cost is **~$10-12/month** for typical usage (not 24/7)

### Cloud SQL for PostgreSQL
- **First 12 months:** **FREE** (Free Tier)
  - db-f1-micro (shared-core, 0.6GB RAM)
  - 20GB storage
  - 20GB backup storage
- **After 12 months:** **~$7-10/month**
  - db-f1-micro: ~$7.67/month
  - Storage: ~$0.17/month (20GB × $0.17/GB)
  - Backup: ~$0.17/month (20GB × $0.17/GB)
  - **Total:** **~$8-10/month**

### Cloud Load Balancing (~$18/month after free tier)
- **First 12 months:** **FREE** (First 5 forwarding rules)
- **After 12 months:** **~$18/month**
  - Base cost: ~$18/month
  - Data processing: $0.008/GB (if over free tier)
  - **Note:** More expensive than AWS ALB, but includes global load balancing

### Cloud DNS ($0.20/month)
- **Managed Zone:** $0.20/month per zone (always applies, no free tier)
- **DNS Queries:** $0.20 per million (but first 1B queries free)
- **Total:** **$0.20/month** (for typical usage)

### Cloud Storage (~$0.50/month)
- **First 5GB:** FREE
- **Next 15GB:** 15GB × $0.020/GB = **$0.30**
- **Operations:** Minimal (mostly free tier)
- **Total:** **~$0.50/month**

### Cloud Pub/Sub (FREE)
- **Free Tier:** 10GB/month
- **Your Usage:** 3 topics (email, AI generation, export)
- **Cost:** **$0/month** (stays within free tier for small to medium usage)

### Cloud Functions (FREE)
- **Free Tier:**
  - 2M invocations/month
  - 400,000 GB-seconds compute time
  - 200,000 GHz-seconds compute time
- **Your Usage:** 3 functions (email-handler, ai-generation-handler, export-handler)
- **Cost:** **$0/month** (stays within free tier for small to medium usage)

### Secret Manager ($0.06/month)
- **Cost:** $0.06 per secret per month
- **Your Usage:** 1 secret (database connection string)
- **Total:** **$0.06/month** (cheaper than AWS Secrets Manager)

### Cloud Logging (~$0.50-1/month)
- **Ingestion:** First 50GB free, then $0.50/GB
- **Storage:** $0.50/GB/month (7-day retention)
- **Your Usage:** 
  - Container logs (7-day retention)
  - Function logs (7-day retention)
- **Total:** **~$0.50-1/month** (with optimized log levels)

### Data Transfer (~$1-2/month)
- **First 1GB outbound:** FREE
- **Next 9GB:** 9GB × $0.12/GB = **$1.08**
- **Varies based on:** API usage, storage downloads, etc.
- **Total:** **~$1-2/month**

### Artifact Registry (FREE)
- **Standard tier:** Always free
- **Storage:** Included (unlimited)
- **Total:** **$0/month** (much better than AWS ECR or Azure ACR)

## 🎯 Cost Comparison: AWS vs GCP

### First 12 Months

| Provider | Monthly Cost | Key Differences |
|----------|--------------|-----------------|
| **AWS** | ~$17-22.50/month | ALB free tier, ECR minimal cost |
| **GCP** | ~$12.26-15.26/month | Cloud Run pay-per-use, Artifact Registry free |

**GCP is ~$4.74-7.24/month cheaper** in the first year! 🎉

### After 12 Months

| Provider | Monthly Cost | Key Differences |
|----------|--------------|-----------------|
| **AWS** | ~$48-54/month | ALB ~$16/month |
| **GCP** | ~$37.26-43.26/month | Cloud Load Balancing ~$18/month |

**GCP is ~$10.74-10.74/month cheaper** after free tier expires! 🎉

## 💡 Key Cost Differences

### Cheaper on GCP
1. **Cloud Run** ($10-12/month) vs **ECS Fargate** (~$18/month)
   - GCP: Pay-per-use (only when handling requests)
   - AWS: Pay for 24/7 running
   - **Savings:** ~$6-8/month

2. **Artifact Registry** ($0/month) vs **ECR** (~$0.10/month) vs **Azure ACR** ($5/month)
   - GCP: Always free
   - AWS: Pay per GB
   - Azure: Always $5/month
   - **Savings:** Minimal vs AWS, but significant vs Azure

3. **Cloud SQL** ($7-10/month after free tier) vs **RDS** ($15/month)
   - GCP: db-f1-micro is cheaper
   - AWS: db.t4g.micro is more expensive
   - **Savings:** ~$5-8/month

4. **Secret Manager** ($0.06/month) vs **AWS Secrets Manager** ($0.40/month)
   - **Savings:** -$0.34/month

5. **Cloud Logging** (50GB free) vs **CloudWatch Logs** (5GB free)
   - **Savings:** Minimal (~$0.20/month)

### More Expensive on GCP
1. **Cloud Load Balancing** ($18/month after free tier) vs **ALB** ($0 → $16/month)
   - GCP: No free tier after first 5 forwarding rules
   - AWS: Free tier for 12 months
   - **Difference:** +$2/month after AWS free tier expires

2. **Data Transfer**
   - GCP: $0.12/GB after first 1GB
   - AWS: $0.09/GB after first 1GB
   - **Difference:** +$0.03/GB (minimal impact)

## 📊 Total Cost Summary

### GCP Monthly Costs

| Period | Total Cost | Your Budget | Status |
|--------|------------|-------------|--------|
| **First 12 Months** | **~$12.26-15.26/month** | $20/month | ✅ **WITHIN BUDGET** 🎉 |
| **After 12 Months** | **~$37.26-43.26/month** | $20/month | ⚠️ **OVER BUDGET** |

### AWS Monthly Costs (for comparison)

| Period | Total Cost | Your Budget | Status |
|--------|------------|-------------|--------|
| **First 12 Months** | **~$17-22.50/month** | $20/month | ✅ **WITHIN BUDGET** |
| **After 12 Months** | **~$48-54/month** | $20/month | ⚠️ **OVER BUDGET** |

## 💡 Cost Optimization Options for GCP

### Option 1: Use Cloud Run with Minimum Instances = 0 (Recommended)
- **Savings:** Already optimized (pay-per-use)
- **Benefits:** Only pay when handling requests
- **Trade-off:** Cold starts on first request

### Option 2: Use Cloud SQL with Smaller Instance
- **Current:** db-f1-micro (~$8-10/month after free tier)
- **Alternative:** Keep db-f1-micro (already smallest)
- **Note:** Already optimized

### Option 3: Use Cloud Functions for API (if applicable)
- **Cost:** ~$0-5/month (within free tier)
- **Savings:** ~$10-12/month vs Cloud Run
- **Trade-off:** Requires architecture changes, cold starts

### Option 4: Use Cloud Load Balancing with Single Region
- **Current:** Global load balancing (~$18/month)
- **Alternative:** Regional load balancing (~$18/month)
- **Note:** Same cost, but regional is simpler

## 🎯 Recommendation

**GCP is the most cost-effective option!** 🎉

### Advantages:
- ✅ **Cheapest:** ~$12-15/month (first year) vs AWS ~$17-22.50/month
- ✅ **Within budget:** Fits your $20/month budget perfectly
- ✅ **Better free tier:** More generous than AWS
- ✅ **Pay-per-use:** Cloud Run only charges when handling requests
- ✅ **Free container registry:** Artifact Registry is always free
- ✅ **Cheaper database:** Cloud SQL is ~$5-8/month cheaper than RDS

### Considerations:
- ⚠️ **After 12 months:** Still over budget (~$37-43/month)
- ⚠️ **Cold starts:** Cloud Run has cold starts (can be mitigated with minimum instances)
- ⚠️ **Learning curve:** Different from AWS (if team is AWS-focused)

## 📝 Notes

- GCP pricing varies by region (same as AWS/Azure)
- All prices are approximate and in USD
- Prices may change - check GCP pricing calculator for exact costs
- Cloud Run charges only when handling requests (not 24/7 like ECS Fargate)
- GCP offers $300 free credit for new accounts (valid for 90 days)

## 🔗 Useful Links

- [GCP Pricing Calculator](https://cloud.google.com/products/calculator)
- [GCP Free Tier](https://cloud.google.com/free)
- [Cloud Run Pricing](https://cloud.google.com/run/pricing)
- [Cloud SQL Pricing](https://cloud.google.com/sql/pricing)
- [GCP Always Free Tier](https://cloud.google.com/free/docs/free-cloud-features)

## 🏆 Final Comparison: AWS vs Azure vs GCP

| Provider | First 12 Months | After 12 Months | Best For |
|----------|-----------------|------------------|----------|
| **AWS** | ~$17-22.50/month | ~$48-54/month | AWS ecosystem, enterprise |
| **Azure** | ~$44-49/month | ~$59-64/month | Microsoft ecosystem |
| **GCP** | **~$12-15/month** 🏆 | **~$37-43/month** 🏆 | **Cost optimization** |

**Winner: GCP** - Most cost-effective for your use case! 🎉

