# Azure Services Required for Sqordia Application

## Overview

This document lists all Azure services required to run the Sqordia application, their purpose, configuration, and costs.

**Budget:** $50 CAD/month (~$37 USD/month)

---

## Core Services

### 1. Azure Container Apps ✅
**Purpose:** Host the main API application (ASP.NET Core 8 Web API)

**Configuration:**
- 0.5 vCPU
- 1GB RAM
- Pay-per-use (only charges when handling requests)
- Auto-scaling enabled
- Min instances: 0
- Max instances: 10

**What it replaces:** GCP Cloud Run

**Cost:**
- ~$10-15 USD/month (~$14-20 CAD/month)

**Features:**
- Container-based deployment (Docker)
- Serverless architecture
- Built-in load balancing
- HTTPS endpoint included

---

### 2. Azure Database for PostgreSQL ✅
**Purpose:** Primary database for application data

**Configuration:**
- Basic tier (B1ms)
- 1 vCore, 2GB RAM
- 20GB storage
- PostgreSQL 15+
- Automatic backups (7-day retention)

**What it replaces:** GCP Cloud SQL PostgreSQL

**Cost:**
- **First 12 months:** $0 (free tier) ✅
- **After 12 months:** ~$15 USD/month (~$20 CAD/month)

**Features:**
- Fully managed PostgreSQL
- Automatic backups
- High availability options
- Connection pooling

---

### 3. Azure Blob Storage ✅
**Purpose:** Store documents, files, and user-uploaded content

**Configuration:**
- Standard tier
- Hot access tier
- ~20GB storage
- Versioning enabled

**What it replaces:** GCP Cloud Storage / AWS S3

**Cost:**
- ~$0.50 USD/month (~$0.68 CAD/month)
- First 5GB free

**Features:**
- Object storage
- REST API access
- CDN integration available
- Lifecycle management

---

### 4. Azure Service Bus ✅
**Purpose:** Message queue for asynchronous processing

**Use Cases:**
- Email sending (queued)
- AI generation requests
- Document export jobs

**Configuration:**
- Basic tier
- 3 topics/queues:
  - Email queue
  - AI generation queue
  - Export queue

**What it replaces:** GCP Cloud Pub/Sub / AWS SQS

**Cost:**
- **$0** (free tier: 13M operations/month) ✅

**Features:**
- Reliable message delivery
- At-least-once delivery
- Dead-letter queues
- Message ordering

---

### 5. Azure Functions ✅
**Purpose:** Serverless functions for background job processing

**Functions:**
1. **Email Handler** - Processes email queue messages
2. **AI Generation Handler** - Processes AI generation requests
3. **Export Handler** - Processes document export requests

**Configuration:**
- Consumption plan
- .NET 8 runtime
- 512MB memory per function
- Triggered by Service Bus messages

**What it replaces:** GCP Cloud Functions / AWS Lambda

**Cost:**
- **$0** (free tier: 1M requests/month) ✅

**Features:**
- Serverless execution
- Auto-scaling
- Pay-per-execution
- Integrated with Service Bus

---

### 6. Azure Container Registry (ACR) ✅
**Purpose:** Store Docker container images for deployment

**Configuration:**
- Basic tier
- 10GB storage included
- Private registry

**What it replaces:** GCP Artifact Registry / AWS ECR

**Cost:**
- $5 USD/month (~$6.76 CAD/month)
- Always paid (no free tier)

**Features:**
- Private Docker registry
- Image scanning
- Geo-replication available
- CI/CD integration

---

## Supporting Services

### 7. Azure Key Vault ✅
**Purpose:** Securely store secrets and connection strings

**What it stores:**
- Database connection strings
- API keys
- JWT secrets
- Third-party service credentials

**Configuration:**
- Standard tier
- 1 secret (database connection string)

**What it replaces:** GCP Secret Manager / AWS Secrets Manager

**Cost:**
- $0.03 USD/month (~$0.04 CAD/month)
- $0.03 per secret per month

**Features:**
- Secure secret storage
- Access control
- Audit logging
- Automatic rotation support

---

### 8. Azure Monitor Logs ✅
**Purpose:** Centralized logging and monitoring

**Configuration:**
- 7-day log retention
- Application logs
- Container logs
- Function logs

**What it replaces:** GCP Cloud Logging / AWS CloudWatch Logs

**Cost:**
- ~$0.50-1 USD/month (~$0.68-1.35 CAD/month)
- First 5GB ingestion free

**Features:**
- Centralized logging
- Log querying
- Alerting
- Integration with Application Insights

---

### 9. Azure Virtual Network (VNet) ✅
**Purpose:** Network isolation and security

**Configuration:**
- Private networking
- Subnet configuration
- Network security groups

**What it replaces:** GCP VPC / AWS VPC

**Cost:**
- **$0** (always free) ✅

**Features:**
- Network isolation
- Private endpoints
- VPN connectivity
- Firewall rules

---

### 10. Data Transfer ✅
**Purpose:** Outbound data transfer (API responses, file downloads)

**Configuration:**
- Outbound traffic
- First 5GB free per month

**Cost:**
- ~$1-2 USD/month (~$1.35-2.70 CAD/month)
- First 5GB free

**Features:**
- Pay-per-GB after free tier
- Varies by usage

---

## Optional Services (Not Required)

### Azure Application Gateway
**Purpose:** Advanced load balancing and WAF

**Status:** ❌ Not needed (Container Apps has built-in load balancing)

**Cost if used:** ~$25 USD/month

**When to use:** Only if you need WAF features or advanced routing

---

### Azure DNS
**Purpose:** Custom domain name management

**Status:** ❌ Optional (can use Container Apps default domain)

**Cost if used:** $0.50 USD/month per hosted zone

**When to use:** If you need custom domain (e.g., api.sqordia.com)

---

## Service Dependencies

```
┌─────────────────┐
│ Container Apps  │───┐
│  (Main API)     │   │
└─────────────────┘   │
                       │
┌─────────────────┐   │    ┌──────────────────┐
│  PostgreSQL     │◄──┼────┤  Key Vault       │
│   Database      │   │    │  (Secrets)        │
└─────────────────┘   │    └──────────────────┘
                       │
┌─────────────────┐   │    ┌──────────────────┐
│  Blob Storage   │◄──┼────┤  Service Bus     │
│  (Documents)    │   │    │  (Messages)      │
└─────────────────┘   │    └──────────────────┘
                       │              │
┌─────────────────┐   │              │
│  Functions      │◄──┼──────────────┘
│  (Background)   │   │
└─────────────────┘   │
                       │
┌─────────────────┐   │
│ Container Reg.  │───┘
│  (Docker Images)│
└─────────────────┘
```

---

## Cost Summary

### First 12 Months (Free Tier Benefits)

| Service | Monthly Cost (USD) | Monthly Cost (CAD) | Free Tier |
|---------|-------------------|-------------------|-----------|
| Container Apps | $10-15 | ~$14-20 | ❌ |
| PostgreSQL | **$0** | **$0** | ✅ 12 months |
| Blob Storage | $0.50 | ~$0.68 | ✅ Partial |
| Service Bus | **$0** | **$0** | ✅ |
| Functions | **$0** | **$0** | ✅ |
| Container Registry | $5 | ~$6.76 | ❌ |
| Key Vault | $0.03 | ~$0.04 | ❌ |
| Monitor Logs | $0.50-1 | ~$0.68-1.35 | ✅ Partial |
| Data Transfer | $1-2 | ~$1.35-2.70 | ✅ Partial |
| VNet | **$0** | **$0** | ✅ |
| | | | |
| **TOTAL** | **~$17-20** | **~$23-27** | ✅ |

### After 12 Months (Free Tier Expires)

| Service | Monthly Cost (USD) | Monthly Cost (CAD) | Change |
|---------|-------------------|-------------------|--------|
| Container Apps | $10-15 | ~$14-20 | Same |
| PostgreSQL | $15 | ~$20.27 | +$15 |
| Blob Storage | $0.50 | ~$0.68 | Same |
| Service Bus | **$0** | **$0** | Still free |
| Functions | **$0** | **$0** | Still free |
| Container Registry | $5 | ~$6.76 | Same |
| Key Vault | $0.03 | ~$0.04 | Same |
| Monitor Logs | $0.50-1 | ~$0.68-1.35 | Same |
| Data Transfer | $1-2 | ~$1.35-2.70 | Same |
| VNet | **$0** | **$0** | Same |
| | | | |
| **TOTAL** | **~$32-38** | **~$43-51** | ✅ |

**Budget Status:** ✅ All costs within $50 CAD/month budget

---

## Migration Mapping

| Current (GCP) | Azure Equivalent | Status |
|---------------|------------------|--------|
| Cloud Run | Container Apps | ✅ Selected |
| Cloud SQL PostgreSQL | Azure Database for PostgreSQL | ✅ Selected |
| Cloud Storage | Azure Blob Storage | ✅ Selected |
| Cloud Pub/Sub | Azure Service Bus | ✅ Selected |
| Cloud Functions | Azure Functions | ✅ Selected |
| Artifact Registry | Azure Container Registry | ✅ Selected |
| Secret Manager | Azure Key Vault | ✅ Selected |
| Cloud Logging | Azure Monitor Logs | ✅ Selected |
| VPC | Azure Virtual Network | ✅ Selected |

---

## Next Steps

1. **Create Azure Account** and subscription
2. **Set up Resource Group** for organizing resources
3. **Deploy Infrastructure** using Terraform or Azure CLI
4. **Configure Services** with appropriate settings
5. **Deploy Application** to Container Apps
6. **Set up CI/CD** pipeline
7. **Monitor Costs** using Azure Cost Management

---

## Notes

- All costs are approximate and may vary by:
  - Azure region
  - Actual usage patterns
  - Exchange rates (USD to CAD)
  - Pricing changes

- Free tier benefits:
  - PostgreSQL: First 12 months free
  - Service Bus: Always free (within limits)
  - Functions: Always free (within limits)
  - Blob Storage: First 5GB free
  - Data Transfer: First 5GB free

- Cost optimization:
  - Container Apps: Pay-per-use (only when handling requests)
  - Can scale down to 0 instances when idle
  - Monitor usage to optimize costs

