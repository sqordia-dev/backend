# Terraform Organization

This directory has been organized by cloud platform for better maintainability.

## Directory Structure

```
infrastructure/terraform/
├── aws/                    # AWS infrastructure
│   ├── *.tf               # Terraform configuration files
│   ├── terraform.tfvars   # AWS-specific variables
│   ├── terraform.tfstate  # AWS state file
│   └── README.md          # AWS deployment documentation
│
├── gcp/                    # GCP infrastructure
│   ├── *.tf               # Terraform configuration files
│   ├── terraform.tfvars.example  # GCP example variables
│   └── README.md          # GCP deployment documentation
│
├── README.md              # Main documentation
└── ORGANIZATION.md        # This file
```

## Migration Notes

**Date**: 2026-01-03

All AWS-related Terraform files have been moved from `infrastructure/terraform/` to `infrastructure/terraform/aws/`.

### Files Moved to `aws/`:
- All `*.tf` files (main.tf, variables.tf, outputs.tf, etc.)
- `terraform.tfvars` and `terraform.tfvars.example`
- `README.md` (AWS-specific)
- `build-lambda.ps1` and `build-lambda.sh`
- Lambda zip files (`email-handler.zip`, `ai-generation-handler.zip`, `export-handler.zip`)
- State files (`terraform.tfstate`, `terraform.tfstate.backup`, `tfplan`)

### Files in `gcp/`:
- All GCP Terraform configuration files
- GCP-specific variables and outputs
- GCP deployment documentation

## Usage

### AWS Deployment
```powershell
cd infrastructure/terraform/aws
terraform init
terraform plan
terraform apply
```

### GCP Deployment
```powershell
cd infrastructure/terraform/gcp
terraform init
terraform plan
terraform apply
```

Or use the deployment scripts:
```powershell
# AWS (if scripts exist)
.\scripts\deploy-aws.ps1

# GCP
.\scripts\deploy-gcp.ps1 -Action plan
```

## Benefits

1. **Clear Separation**: Easy to see which files belong to which platform
2. **Independent State**: Each platform has its own state files
3. **Better Organization**: Easier to maintain and understand
4. **Scalability**: Easy to add more platforms (Azure, etc.) in the future

