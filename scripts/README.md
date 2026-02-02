# Scripts Directory

## Overview

This directory contains PowerShell scripts for deployment, monitoring, and maintenance tasks for the GCP-based Sqordia backend.

## GCP Deployment Scripts

- `deploy-gcp.ps1` - Deploy to GCP Cloud Run
- `build-and-push-gcp.ps1` - Build and push Docker image to GCP Artifact Registry
- `build-cloud-functions.ps1` - Build GCP Cloud Functions
- `complete-gcp-deployment.ps1` - Complete GCP deployment workflow
- `quick-deploy-gcp.ps1` - Quick GCP deployment
- `test-gcp-deployment.ps1` - Test GCP deployment

## GCP Setup Scripts

- `setup-gcp-cli.ps1` - Setup GCP CLI
- `setup-gcp-apis.ps1` - Enable required GCP APIs
- `verify-gcp-project.ps1` - Verify GCP project configuration
- `verify-gcp-setup.ps1` - Verify GCP setup
- `check-gcp-readiness.ps1` - Check GCP readiness
- `create-github-actions-sa.ps1` - Create GitHub Actions service account
- `phase1-quick-start.ps1` - Phase 1 quick start guide
- `phase1-gcp-setup-checklist.md` - Phase 1 setup checklist

## Database Scripts

- `CreateAdminUser.sql` - Create admin user (PostgreSQL)
- `run-migrations.ps1` - Run EF Core migrations using the WebAPI connection string (from appsettings or env). Run from `backend`: `.\scripts\run-migrations.ps1`. After it succeeds, retry login to verify schema (e.g. `MicrosoftId` column).
- `run-db-migrations.ps1` - Run database migrations
- `run-gcp-migrations.ps1` - Run GCP database migrations
- `run-seed-scripts.ps1` - Run seed scripts
- `run-seed-database.ps1` - Run database seeding
- `run-seed-database-dotnet.ps1` - Run database seeding via .NET
- `run-seed-questionnaire.ps1` - Run questionnaire seed script
- `execute-seed-sql.ps1` - Execute seed SQL scripts
- `test-db-connection.ps1` - Test database connection
- `check-migration-status.ps1` - Check migration status
- `seed-database.sql` - Database seed script (roles, permissions, admin user)
- `seed-questionnaire.sql` - Questionnaire seed script (BusinessPlan templates and questions)
- `seed-questionnaire-strategic-plan.sql` - Questionnaire seed for StrategicPlan (OBNL). Run in production after initial deploy if StrategicPlan plans are used.

## Database Admin Scripts

- `assign-admin-role.sql` - Assign admin role
- `check-admin-role.sql` - Check admin role
- `check-admin-status.sql` - Check admin status
- `check-admin.sql` - Check admin
- `check-roles.sql` - Check roles
- `setup-admin-user.ps1` - Setup admin user
- `update-admin-password.ps1` - Update admin password
- `drop-all-tables.sql` - Drop all tables
- `ResetQuestionnaireSeeding.sql` - Reset questionnaire seeding
- `reset-admin-subscription.sql` - Reset Admin user subscription to Free plan
- `run-reset-admin-subscription.ps1` - Run reset admin subscription script

## Utility Scripts

- `copy-backend-code.ps1` - Copy backend code

