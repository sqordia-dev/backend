# Azure Cost Estimate for Sqordia Infrastructure

## 📊 AWS to Azure Service Mapping

| AWS Service | Azure Equivalent | Notes |
|-------------|------------------|-------|
| **ECS Fargate** | Azure Container Instances (ACI) or Azure Container Apps | Container orchestration |
| **RDS PostgreSQL** | Azure Database for PostgreSQL (Flexible Server) | Managed PostgreSQL |
| **ALB** | Azure Application Gateway or Azure Load Balancer | Load balancing |
| **Route 53** | Azure DNS | DNS management |
| **S3** | Azure Blob Storage | Object storage |
| **SQS** | Azure Service Bus or Azure Queue Storage | Message queuing |
| **Lambda** | Azure Functions | Serverless compute |
| **Secrets Manager** | Azure Key Vault | Secrets management |
| **CloudWatch Logs** | Azure Monitor Logs | Logging |
| **ECR** | Azure Container Registry (ACR) | Container registry |

## 💰 Monthly Cost Breakdown

### Core Infrastructure Services

| Service | Configuration | Monthly Cost | Free Tier | Notes |
|---------|--------------|--------------|-----------|-------|
| **Azure Container Instances** | 0.5 vCPU, 1GB RAM | ~$12-15 | ❌ | Running 24/7 (similar to ECS Fargate) |
| **Azure Database for PostgreSQL** | Basic tier, 20GB | $0 → $15 | ✅ 12 months | Free tier for first year |
| **Application Gateway** | Basic tier | $0 → ~$25 | ❌ | No free tier, always paid |
| **Azure DNS** | Hosted zone | $0.50 | ❌ | Always $0.50/month |
| **Azure Blob Storage** | Standard, 20GB | ~$0.50 | ✅ Partial | First 5GB free |
| **Azure Service Bus** | Basic tier | $0 | ✅ | 13M operations/month free |
| **Azure Functions** | Consumption plan | $0 | ✅ | 1M requests/month free |
| **Azure Key Vault** | Standard tier | $0.03 | ❌ | $0.03 per secret/month |
| **Azure Monitor Logs** | 7-day retention | ~$0.50-1 | ✅ Partial | First 5GB ingestion free |
| **Data Transfer** | Outbound | $1-2 | ✅ Partial | First 5GB free |
| **Azure Container Registry** | Basic tier | $5 | ❌ | Always $5/month |
| **VNet/Networking** | VNet, subnets | $0 | ✅ | Always free |

## 💰 Monthly Cost Summary

### First 12 Months (Azure Free Tier) 🎉

| Category | Cost | Details |
|----------|------|---------|
| **Azure Container Instances** | $12-15 | 0.5 vCPU + 1GB RAM, 24/7 |
| **Azure Database for PostgreSQL** | **$0** ✅ | Free tier (Basic tier, 20GB) |
| **Application Gateway** | $25 | No free tier (always paid) |
| **Azure DNS** | $0.50 | Hosted zone (no free tier) |
| **Azure Blob Storage** | $0.50 | 20GB (first 5GB free) |
| **Azure Service Bus** | **$0** ✅ | 13M operations/month free |
| **Azure Functions** | **$0** ✅ | 1M requests/month free |
| **Azure Key Vault** | $0.03 | 1 secret |
| **Azure Monitor Logs** | $0.50-1 | 7-day retention |
| **Data Transfer** | $1-2 | Outbound (first 5GB free) |
| **Azure Container Registry** | $5 | Basic tier (always paid) |
| **VNet/Networking** | **$0** ✅ | Always free |
| | | |
| **TOTAL** | **~$44-49/month** | |

### After 12 Months (Free Tier Expires)

| Category | Cost | Details |
|----------|------|---------|
| **Azure Container Instances** | $12-15 | Same as before |
| **Azure Database for PostgreSQL** | $15 | Free tier expired |
| **Application Gateway** | $25 | Same as before |
| **Azure DNS** | $0.50 | Same as before |
| **Azure Blob Storage** | $0.50 | Same as before |
| **Azure Service Bus** | **$0** ✅ | Still within free tier |
| **Azure Functions** | **$0** ✅ | Still within free tier |
| **Azure Key Vault** | $0.03 | Same as before |
| **Azure Monitor Logs** | $0.50-1 | Same as before |
| **Data Transfer** | $1-2 | Same as before |
| **Azure Container Registry** | $5 | Same as before |
| **VNet/Networking** | **$0** ✅ | Always free |
| | | |
| **TOTAL** | **~$59-64/month** | |

## 📈 Detailed Cost Breakdown

### Azure Container Instances (~$12-15/month)
- **CPU:** 0.5 vCPU × $0.000012/vCPU-second × 2,592,000 seconds = **~$15.55**
- **Memory:** 1 GB × $0.0000015/GB-second × 2,592,000 seconds = **~$3.89**
- **Total:** **~$19/month** (varies by region)
- **Alternative:** Azure Container Apps (~$12-15/month) or Azure App Service (~$13-15/month)

### Azure Database for PostgreSQL
- **First 12 months:** **FREE** (Free Tier)
  - Basic tier (B1ms - 1 vCore, 2GB RAM)
  - 20GB storage
  - 20GB backup storage
- **After 12 months:** **~$15/month**
  - Basic tier: ~$13/month
  - Storage: ~$2/month (20GB × $0.10/GB)

### Application Gateway (~$25/month)
- **Base cost:** ~$25/month (Basic tier)
- **Data processing:** $0.008/GB (if over free tier)
- **Note:** More expensive than AWS ALB, but includes WAF features

### Azure DNS ($0.50/month)
- **Hosted Zone:** $0.50/month (always applies, no free tier)
- **DNS Queries:** $0.40 per million (but alias records are free)
- **Total:** **$0.50/month** (for typical usage)

### Azure Blob Storage (~$0.50/month)
- **First 5GB:** FREE
- **Next 15GB:** 15GB × $0.018/GB = **$0.27**
- **Requests:** Minimal (mostly free tier)
- **Total:** **~$0.50/month**

### Azure Service Bus (FREE)
- **Free Tier:** 13M operations/month
- **Your Usage:** 3 queues (email, AI generation, export)
- **Cost:** **$0/month** (stays within free tier for small to medium usage)

### Azure Functions (FREE)
- **Free Tier:**
  - 1M requests/month
  - 400,000 GB-seconds compute time
- **Your Usage:** 3 functions (email-handler, ai-generation-handler, export-handler)
- **Cost:** **$0/month** (stays within free tier for small to medium usage)

### Azure Key Vault ($0.03/month)
- **Cost:** $0.03 per secret per month
- **Your Usage:** 1 secret (database connection string)
- **Total:** **$0.03/month** (much cheaper than AWS Secrets Manager)

### Azure Monitor Logs (~$0.50-1/month)
- **Ingestion:** First 5GB free, then $0.50/GB
- **Storage:** $0.03/GB/month (7-day retention)
- **Your Usage:** 
  - Container logs (7-day retention)
  - Function logs (7-day retention)
- **Total:** **~$0.50-1/month** (with optimized log levels)

### Data Transfer (~$1-2/month)
- **First 5GB outbound:** FREE (Azure gives more free data transfer)
- **Next 5GB:** 5GB × $0.05/GB = **$0.25**
- **Varies based on:** API usage, storage downloads, etc.
- **Total:** **~$1-2/month**

### Azure Container Registry ($5/month)
- **Basic tier:** $5/month (always paid, no free tier)
- **Storage:** Included (10GB)
- **Total:** **$5/month**

## 🎯 Cost Comparison: AWS vs Azure

### First 12 Months

| Provider | Monthly Cost | Key Differences |
|----------|--------------|-----------------|
| **AWS** | ~$17-22.50/month | ALB free tier, ECR minimal cost |
| **Azure** | ~$44-49/month | Application Gateway always paid, ACR $5/month |

**Azure is ~$27-27.50/month more expensive** in the first year.

### After 12 Months

| Provider | Monthly Cost | Key Differences |
|----------|--------------|-----------------|
| **AWS** | ~$48-54/month | ALB ~$16/month |
| **Azure** | ~$59-64/month | Application Gateway ~$25/month |

**Azure is ~$11-10/month more expensive** after free tier expires.

## 💡 Key Cost Differences

### More Expensive on Azure
1. **Application Gateway** ($25/month) vs **ALB** ($0 → $16/month)
   - Azure: No free tier
   - AWS: Free tier for 12 months
   - **Difference:** +$9/month after AWS free tier expires

2. **Azure Container Registry** ($5/month) vs **ECR** (~$0.10/month)
   - Azure: Always $5/month
   - AWS: Pay per GB (~$0.10/month for minimal usage)
   - **Difference:** +$4.90/month

3. **Container Service**
   - Azure Container Instances: ~$19/month
   - AWS ECS Fargate: ~$18/month
   - **Difference:** +$1/month

### Cheaper on Azure
1. **Key Vault** ($0.03/month) vs **Secrets Manager** ($0.40/month)
   - **Savings:** -$0.37/month

2. **Data Transfer**
   - Azure: First 5GB free
   - AWS: First 1GB free
   - **Savings:** Minimal (~$0.20/month)

## 📊 Total Cost Summary

### Azure Monthly Costs

| Period | Total Cost | Your Budget | Status |
|--------|------------|-------------|--------|
| **First 12 Months** | **~$44-49/month** | $20/month | ⚠️ **OVER BUDGET** |
| **After 12 Months** | **~$59-64/month** | $20/month | ⚠️ **OVER BUDGET** |

### AWS Monthly Costs (for comparison)

| Period | Total Cost | Your Budget | Status |
|--------|------------|-------------|--------|
| **First 12 Months** | **~$17-22.50/month** | $20/month | ✅ **WITHIN BUDGET** |
| **After 12 Months** | **~$48-54/month** | $20/month | ⚠️ **OVER BUDGET** |

## 💡 Cost Optimization Options for Azure

### Option 1: Use Azure Container Apps (Recommended)
- **Savings:** ~$4-7/month vs Container Instances
- **New Total:** ~$40-45/month (first year)
- **Benefits:** Better scaling, easier management

### Option 2: Use Basic Load Balancer
- **Savings:** ~$20/month vs Application Gateway
- **New Total:** ~$24-29/month (first year)
- **Trade-off:** No WAF features, less advanced routing

### Option 3: Use Azure Static Web Apps (if applicable)
- **Savings:** Significant if API can be serverless
- **Trade-off:** Requires architecture changes

### Option 4: Use Azure App Service (instead of containers)
- **Cost:** ~$13-15/month (Basic tier)
- **Savings:** ~$4-6/month vs Container Instances
- **New Total:** ~$40-45/month (first year)

## 🎯 Recommendation

**Stick with AWS** for cost reasons:
- ✅ **First year:** AWS is ~$27/month cheaper
- ✅ **After free tier:** AWS is ~$11/month cheaper
- ✅ **Within budget:** AWS fits your $20/month budget (first year)
- ✅ **Better free tier:** AWS offers more free tier benefits

**Consider Azure if:**
- You need Azure-specific features (Azure AD integration, etc.)
- You have Azure credits or enterprise agreements
- You prefer Azure's developer experience
- Cost is not the primary concern

## 📝 Notes

- Azure pricing varies by region (same as AWS)
- All prices are approximate and in USD
- Prices may change - check Azure pricing calculator for exact costs
- Some services have different pricing models (e.g., pay-as-you-go vs reserved)

## 🔗 Useful Links

- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)
- [Azure Free Account](https://azure.microsoft.com/free/)
- [Azure Container Instances Pricing](https://azure.microsoft.com/pricing/details/container-instances/)
- [Azure Database for PostgreSQL Pricing](https://azure.microsoft.com/pricing/details/postgresql/flexible-server/)

