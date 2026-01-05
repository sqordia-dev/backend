# Terraform Infrastructure

This directory contains Terraform configurations organized by cloud platform.

## Structure

```
terraform/
├── aws/          # AWS infrastructure (ECS, RDS, Lambda, etc.)
├── gcp/          # GCP infrastructure (Cloud Run, Cloud SQL, etc.)
├── azure/        # Azure infrastructure (Container Apps, PostgreSQL, etc.)
└── README.md      # This file
```

## Platform-Specific Documentation

- **AWS**: See [aws/README.md](./aws/README.md)
- **GCP**: See [gcp/README.md](./gcp/README.md)
- **Azure**: See [azure/README.md](./azure/README.md)

## Quick Start

### AWS Deployment
```bash
cd aws
terraform init
terraform plan
terraform apply
```

### GCP Deployment
```bash
cd gcp
terraform init
terraform plan
terraform apply
```

### Azure Deployment
```bash
cd azure
terraform init
terraform plan
terraform apply
```

## Notes

- Each platform has its own `terraform.tfvars` file
- State files are stored in their respective platform directories
- Use platform-specific deployment scripts from the `scripts/` directory

