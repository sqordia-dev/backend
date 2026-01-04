# Custom Domain Setup Guide

**Complete guide for setting up a custom domain for your API**

This guide explains how to set up a custom domain for your API. **Recommended:** `api.sqordia-dev.com` (industry standard subdomain pattern).

## 📋 Prerequisites

1. **Domain Registration**: You need to own or have access to a domain name
   - Option 1: Register a new domain (e.g., `sqordia.api` via Route 53 or another registrar)
   - Option 2: Use a subdomain of an existing domain (e.g., `api.sqordia.com`)

2. **AWS Resources**: The following will be created:
   - Application Load Balancer (ALB) - ~$16/month
   - Route 53 Hosted Zone - $0.50/month per hosted zone
   - ACM Certificate - Free
   - Data transfer costs apply

## 💰 Cost Breakdown

### First 12 Months (AWS Free Tier) 🎉

| Resource | Monthly Cost | Free Tier Details |
|----------|--------------|-------------------|
| ALB | **$0** ✅ | 750 hours/month (24/7) + 15GB data processing + 15 LCUs |
| Route 53 Hosted Zone | $0.50 | No free tier (always $0.50/month) |
| Route 53 Queries | $0 | Alias records to AWS resources are free |
| ACM Certificate | **Free** ✅ | Always free |
| **Total** | **~$0.50/month** 🎉 | Only Route 53 hosted zone cost |

### After 12 Months

| Resource | Monthly Cost | Notes |
|----------|--------------|-------|
| ALB | ~$16 | Base cost + data processing |
| Route 53 Hosted Zone | $0.50 | Per hosted zone |
| Route 53 Queries | $0.40/million | Alias records to AWS resources are free |
| ACM Certificate | Free | SSL/TLS certificate |
| **Total** | **~$16.50/month** | Plus data transfer |

## 🚀 Setup Steps

### Step 1: Configure Domain in Terraform

Edit `infrastructure/terraform/terraform.tfvars` (or create it if it doesn't exist):

```hcl
# Recommended: Use subdomain (industry standard)
domain_name = "api.sqordia.com"

# Alternative: Use .api TLD
# domain_name = "sqordia.api"
```

Or set it as an environment variable:
```powershell
# Recommended
$env:TF_VAR_domain_name = "api.sqordia.com"

# Alternative
# $env:TF_VAR_domain_name = "sqordia.api"
```

**Note:** See `docs/DOMAIN_NAMING_BEST_PRACTICES.md` for detailed recommendations.

### Step 2: Register Domain (if needed)

If you don't own the domain yet:

**Option A: Register main domain (recommended for subdomain)**
```powershell
# If using api.sqordia.com, register sqordia.com first
aws route53domains register-domain --domain-name sqordia.com --duration-in-years 1 --region us-east-1
```

**Option B: Register .api TLD (if using sqordia.api)**
```powershell
aws route53domains register-domain --domain-name sqordia.api --duration-in-years 1 --region us-east-1
```

**Option C: Register via another registrar**
- Go to your preferred domain registrar (Namecheap, GoDaddy, etc.)
- Register `sqordia.com` (for subdomain) or `sqordia.api` (for TLD)

### Step 3: Deploy Infrastructure

```powershell
cd infrastructure/terraform
terraform init
terraform plan
terraform apply
```

This will create:
- Application Load Balancer
- Route 53 Hosted Zone
- ACM Certificate
- DNS records

### Step 4: Update Domain Name Servers

After `terraform apply`, you'll get name servers in the output:

```powershell
terraform output route53_name_servers
```

**If you registered via Route 53:**
- The name servers are automatically configured
- Skip to Step 5

**If you registered via another registrar:**
1. Log into your domain registrar
2. Find the DNS/Name Server settings
3. Update the name servers to match the Route 53 name servers from the output
4. Wait for DNS propagation (can take up to 48 hours, usually 1-2 hours)

### Step 5: Wait for Certificate Validation

The ACM certificate needs to be validated via DNS. Terraform will:
1. Create the certificate
2. Create DNS validation records
3. Wait for validation to complete

This usually takes 5-10 minutes.

### Step 6: Verify Setup

```powershell
# Get your API URL
terraform output api_url

# Test the endpoint
curl https://sqordia.api/api/health
```

## 🔧 Configuration Options

### Using a Subdomain (RECOMMENDED)

The subdomain approach (`api.sqordia.com`) is the industry standard and recommended:

```hcl
domain_name = "api.sqordia.com"
```

**Important**: 
- Make sure you own `sqordia.com` and can update its DNS records
- This is more cost-effective and professional than using a `.api` TLD
- See `docs/DOMAIN_NAMING_BEST_PRACTICES.md` for detailed comparison

### Without Custom Domain

If you don't want a custom domain (to save costs), leave `domain_name` empty:

```hcl
domain_name = ""
```

The API will still be accessible via the ECS task's public IP address.

## 📝 DNS Configuration

### Route 53 Hosted Zone

When you set `domain_name`, Terraform creates:
- A Route 53 hosted zone for your domain
- An A record pointing to the ALB
- DNS validation records for the ACM certificate

### Manual DNS Setup (Alternative)

If you prefer to manage DNS manually:

1. **Create the hosted zone manually:**
   ```powershell
   aws route53 create-hosted-zone --name sqordia.api --caller-reference $(Get-Date -Format "yyyyMMddHHmmss")
   ```

2. **Get the name servers:**
   ```powershell
   aws route53 get-hosted-zone --id <zone-id> --query 'DelegationSet.NameServers'
   ```

3. **Update your domain registrar** with these name servers

4. **Create the A record manually** pointing to the ALB DNS name

## 🔒 SSL/TLS Certificate

The ACM certificate is automatically:
- Created for your domain
- Validated via DNS
- Attached to the ALB HTTPS listener

The certificate is free and auto-renewed by AWS.

## 🌐 API Endpoints

After setup, your API will be available at:

- **HTTPS**: `https://sqordia.api` (redirects from HTTP)
- **HTTP**: `http://sqordia.api` (redirects to HTTPS)
- **Health Check**: `https://sqordia.api/api/health`
- **API Root**: `https://sqordia.api/api`

## 🐛 Troubleshooting

### Certificate Validation Failed

1. Check DNS validation records:
   ```powershell
   terraform output route53_name_servers
   ```

2. Verify name servers are updated at your registrar

3. Wait for DNS propagation (can take up to 48 hours)

### ALB Not Responding

1. Check ALB status:
   ```powershell
   aws elbv2 describe-load-balancers --region ca-central-1
   ```

2. Check target group health:
   ```powershell
   aws elbv2 describe-target-health --target-group-arn <target-group-arn> --region ca-central-1
   ```

3. Check ECS service:
   ```powershell
   aws ecs describe-services --cluster sqordia-cluster-production --services sqordia-api-production --region ca-central-1
   ```

### DNS Not Resolving

1. Check name servers:
   ```powershell
   nslookup -type=NS sqordia.api
   ```

2. Verify they match Route 53 name servers

3. Wait for DNS propagation

## 📚 Additional Resources

- [Route 53 Documentation](https://docs.aws.amazon.com/route53/)
- [ACM Certificate Documentation](https://docs.aws.amazon.com/acm/)
- [ALB Documentation](https://docs.aws.amazon.com/elasticloadbalancing/latest/application/)

## ✅ Next Steps

1. Set `domain_name` in `terraform.tfvars`
2. Run `terraform apply`
3. Update name servers at your registrar
4. Wait for DNS propagation
5. Test your API at `https://sqordia.api`

