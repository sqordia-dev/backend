# GCP Cost Monitoring Setup Guide

This guide explains how to set up cost monitoring and billing alerts for the Sqordia GCP deployment.

## Overview

Monitoring costs is crucial for managing cloud expenses. GCP provides several tools to track and control spending:

1. **Billing Alerts** - Get notified when spending exceeds thresholds
2. **Budget Alerts** - Set budgets and receive notifications
3. **Cost Reports** - View detailed cost breakdowns
4. **Cost Recommendations** - Get suggestions for optimization

## Prerequisites

- GCP Project with billing enabled
- Billing Account Administrator or Billing Account Costs Manager role
- Access to GCP Console

## Setup Steps

### 1. Enable Billing Alerts

#### Option A: Using GCP Console

1. Go to [GCP Console → Billing](https://console.cloud.google.com/billing)
2. Select your billing account
3. Click **Budgets & alerts** in the left menu
4. Click **CREATE BUDGET**
5. Configure the budget:
   - **Budget name**: `Sqordia Production Budget`
   - **Budget amount**: Set your monthly budget (e.g., $500)
   - **Budget scope**: Select your project
   - **Alert threshold**: 
     - 50% of budget
     - 90% of budget
     - 100% of budget
   - **Alert recipients**: Add email addresses
6. Click **CREATE BUDGET**

#### Option B: Using gcloud CLI

```bash
# Create a budget
gcloud billing budgets create \
  --billing-account=YOUR_BILLING_ACCOUNT_ID \
  --display-name="Sqordia Production Budget" \
  --budget-amount=500USD \
  --threshold-rule=percent=50 \
  --threshold-rule=percent=90 \
  --threshold-rule=percent=100 \
  --projects=YOUR_PROJECT_ID \
  --notification-rule=pubsub-topic=projects/YOUR_PROJECT_ID/topics/billing-alerts \
  --notification-rule=email-addresses=your-email@example.com
```

### 2. Set Up Cost Reports

1. Go to [GCP Console → Billing → Reports](https://console.cloud.google.com/billing/reports)
2. Select your billing account
3. View cost breakdown by:
   - **Service** (Cloud Run, Cloud SQL, Cloud Storage, etc.)
   - **Project**
   - **Time period** (daily, monthly, custom)
4. Export reports as CSV for analysis

### 3. Configure Cost Anomaly Detection

1. Go to [GCP Console → Billing → Cost anomaly detection](https://console.cloud.google.com/billing/cost-anomaly-detection)
2. Enable anomaly detection for your project
3. Set sensitivity level (Low, Medium, High)
4. Configure notification channels

### 4. Set Up Monitoring Dashboards

#### Create Custom Dashboard

1. Go to [GCP Console → Monitoring → Dashboards](https://console.cloud.google.com/monitoring/dashboards)
2. Click **CREATE DASHBOARD**
3. Add widgets for:
   - **Cloud Run costs**
   - **Cloud SQL costs**
   - **Cloud Storage costs**
   - **Pub/Sub costs**
   - **Total project costs**

#### Use Pre-built Cost Dashboard

1. Go to [GCP Console → Monitoring → Dashboards](https://console.cloud.google.com/monitoring/dashboards)
2. Import the "Cost Management" dashboard template
3. Customize for your project

### 5. Set Up Programmatic Cost Monitoring

#### Using Cloud Monitoring API

Create a script to monitor costs programmatically:

```powershell
# scripts/monitor-gcp-costs.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectId,
    
    [Parameter(Mandatory=$false)]
    [string]$BillingAccountId = ""
)

# Get current month costs
$costs = gcloud billing projects describe $ProjectId --format="json" | ConvertFrom-Json

# Query cost data
gcloud billing accounts list --format="json" | ConvertFrom-Json
```

## Cost Optimization Recommendations

### 1. Cloud Run

- **Right-size instances**: Use appropriate CPU and memory
- **Enable CPU throttling**: Use `cpu_idle = true` for lower costs
- **Set min instances to 0**: Scale to zero when not in use
- **Use request-based pricing**: Pay only for requests processed

### 2. Cloud SQL

- **Use smaller instance sizes**: Start small and scale up as needed
- **Enable automatic backups**: But set retention period appropriately
- **Use committed use discounts**: For predictable workloads
- **Consider Cloud SQL for PostgreSQL**: More cost-effective than MySQL for some workloads

### 3. Cloud Storage

- **Use appropriate storage classes**:
  - Standard: Frequently accessed data
  - Nearline: Data accessed < once/month
  - Coldline: Data accessed < once/quarter
  - Archive: Long-term archival
- **Set lifecycle policies**: Automatically move old data to cheaper storage
- **Enable object versioning only when needed**

### 4. Cloud Functions

- **Optimize function execution time**: Reduce cold starts
- **Use appropriate memory allocation**: More memory = higher cost
- **Set concurrency limits**: Control parallel executions
- **Use Cloud Functions (2nd gen)**: Better performance and cost efficiency

### 5. Pub/Sub

- **Batch messages**: Reduce API calls
- **Use appropriate retention periods**: Don't keep messages longer than needed
- **Monitor message volume**: Set up alerts for unexpected spikes

## Expected Monthly Costs

Based on typical usage patterns:

| Service | Estimated Cost (USD/month) |
|---------|---------------------------|
| Cloud Run | $20-50 |
| Cloud SQL (db-f1-micro) | $10-15 |
| Cloud Storage | $5-10 |
| Cloud Functions | $5-15 |
| Pub/Sub | $1-5 |
| **Total** | **$41-95** |

*Note: Actual costs vary based on usage. Monitor your specific usage patterns.*

## Cost Alerts Setup Script

Create a PowerShell script to automate budget creation:

```powershell
# scripts/setup-cost-alerts.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectId,
    
    [Parameter(Mandatory=$true)]
    [string]$BillingAccountId,
    
    [Parameter(Mandatory=$false)]
    [double]$BudgetAmount = 100,
    
    [Parameter(Mandatory=$false)]
    [string[]]$AlertEmails = @()
)

# Create budget with alerts
gcloud billing budgets create `
  --billing-account=$BillingAccountId `
  --display-name="Sqordia Production Budget" `
  --budget-amount=$BudgetAmount USD `
  --threshold-rule=percent=50 `
  --threshold-rule=percent=90 `
  --threshold-rule=percent=100 `
  --projects=$ProjectId `
  --notification-rule=email-addresses=$($AlertEmails -join ',')
```

## Monitoring Best Practices

1. **Set realistic budgets**: Based on actual usage patterns
2. **Review costs weekly**: Identify unexpected charges early
3. **Use cost labels**: Tag resources for better cost tracking
4. **Set up multiple alert thresholds**: 50%, 90%, 100%
5. **Monitor cost trends**: Use reports to identify patterns
6. **Review recommendations**: GCP provides cost optimization suggestions
7. **Use committed use discounts**: For predictable workloads
8. **Right-size resources**: Don't over-provision

## Troubleshooting

### Budget Alerts Not Working

1. Verify billing account is active
2. Check notification email addresses
3. Verify IAM permissions (Billing Account Administrator)
4. Check Pub/Sub topic permissions (if using Pub/Sub notifications)

### Costs Higher Than Expected

1. Check Cloud Run instance counts
2. Review Cloud SQL instance size
3. Check Cloud Storage data volume
4. Review Pub/Sub message volume
5. Check for orphaned resources

### Access Denied Errors

Ensure you have one of these roles:
- Billing Account Administrator
- Billing Account Costs Manager
- Billing Account Viewer

## Related Documentation

- [GCP Billing Documentation](https://cloud.google.com/billing/docs)
- [GCP Cost Management](https://cloud.google.com/cost-management)
- [GCP Budgets and Alerts](https://cloud.google.com/billing/docs/how-to/budgets)
- [GCP Cost Optimization](https://cloud.google.com/cost-optimization)

## Quick Reference

### View Current Costs
```bash
gcloud billing accounts list
gcloud billing projects describe PROJECT_ID
```

### Create Budget
```bash
gcloud billing budgets create --billing-account=ACCOUNT_ID --display-name="Budget Name" --budget-amount=100USD
```

### List Budgets
```bash
gcloud billing budgets list --billing-account=ACCOUNT_ID
```

### Export Cost Report
```bash
# Via Console: Billing → Reports → Export
# Or use BigQuery Export (requires setup)
```

