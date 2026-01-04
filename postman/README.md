# Postman API Collections

This directory contains Postman collections for testing the Sqordia API in different environments.

## Collections

### 1. Local Development (`Sqordia_API_Local.json`)

**Purpose:** Testing against local Docker development environment

**Base URL:** `http://localhost:5241`

**Usage:**
- Import into Postman
- Use when testing locally with Docker Compose
- Default admin credentials are pre-configured

**Default Variables:**
- `base_url`: `http://localhost:5241`
- `admin_email`: `admin@sqordia.com`
- `admin_password`: `Sqordia2025!`

### 2. Production (`Sqordia_API_Production.json`)

**Purpose:** Testing against production deployment

**Base URL:** `http://34.19.252.60:8080`

**Usage:**
- Import into Postman
- Use for testing production API
- Update admin credentials if different from local

**Default Variables:**
- `base_url`: `http://34.19.252.60:8080`
- `admin_email`: `admin@sqordia.com` (update if different)
- `admin_password`: `Sqordia2025!` (update if different)

### 3. Complete Collection (`Sqordia_API_Collection.json`)

**Purpose:** Master collection (use Local or Production instead)

**Note:** This is the original collection. Use `Sqordia_API_Local.json` or `Sqordia_API_Production.json` for environment-specific testing.

## Import Instructions

### Option 1: Import via Postman UI

1. Open Postman
2. Click **Import** button (top left)
3. Click **Upload Files**
4. Select the collection file (`Sqordia_API_Local.json` or `Sqordia_API_Production.json`)
5. Click **Import**

### Option 2: Import via File Menu

1. Open Postman
2. Go to **File** → **Import**
3. Select the collection file
4. Click **Import**

## Collection Features

### Auto-Save Tokens

Both collections include scripts that automatically:
- Save JWT token after successful login
- Save refresh token
- Save user ID
- Automatically use saved token for authenticated requests

### Pre-configured Variables

Collections include these variables:
- `base_url` - API base URL (environment-specific)
- `jwt_token` - JWT access token (auto-populated)
- `refresh_token` - Refresh token (auto-populated)
- `user_id` - Current user ID (auto-populated)
- `admin_email` - Admin email (pre-configured)
- `admin_password` - Admin password (pre-configured)
- `organization_id` - Organization ID (set manually)
- `business_plan_id` - Business plan ID (set manually)
- `template_id` - Template ID (set manually)
- And more...

### Collection Structure

Both collections are organized into folders:

1. **0. Health & System**
   - Health Check

2. **1. Database Seeding (Admin Only)**
   - Seed Database
   - Get Seed Status
   - Get Configuration

3. **2. Authentication**
   - Register User
   - Login (Admin)
   - Login (Custom)
   - Get Current User
   - Refresh Token
   - Logout
   - Revoke Token
   - Google OAuth endpoints

4. **3. User Profile**
   - Get Profile
   - Update Profile
   - Change Password
   - Delete Account

5. **4. Two-Factor Authentication**
   - Setup 2FA
   - Enable/Disable 2FA
   - Get 2FA Status
   - Regenerate Backup Codes
   - Verify 2FA

6. **5. Security & Sessions**
   - Get Active Sessions
   - Revoke Sessions
   - Get Login History

7. **6. Admin Dashboard**
   - System Overview
   - User Management
   - Organization Management
   - Business Plan Management
   - Activity Logs
   - System Health

8. **7. Role Management (Admin Only)**
   - CRUD operations for roles
   - Assign roles to users
   - Permission management

9. **8. Organizations**
   - Create/Read/Update/Delete organizations
   - Member management
   - Organization settings

10. **9. Business Plans**
    - CRUD operations
    - Section management
    - Versioning
    - Sharing

11. **10. Templates**
    - Template CRUD
    - Public templates
    - Template search
    - Template analytics

12. **11. Questionnaires**
    - Get questionnaire
    - Submit responses
    - Get responses
    - AI question suggestions
    - Progress tracking

13. **12. OBNL Plans**
    - OBNL plan management
    - Compliance analysis
    - Grant applications
    - Impact measurements

14. **13. Financial**
    - Financial projections
    - Currency management
    - Tax calculations
    - KPI calculations
    - Investment analysis
    - Reports

15. **14. Export**
    - PDF export
    - Word export
    - HTML export
    - Export templates

16. **15. Business Plan Generation**
    - Generate business plan
    - Regenerate sections
    - Get generation status
    - Get available sections

17. **16. Admin AI Prompts**
    - AI prompt management
    - Prompt testing
    - Usage statistics
    - Version management

## Quick Start Guide

### Local Development

1. **Start Services** (see LOCAL_DEVELOPMENT_SETUP.md)
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

2. **Import Collection**
   - Import `Sqordia_API_Local.json`

3. **Test Health**
   - Run "Health Check" request
   - Should return: `{"status":"Healthy",...}`

4. **Login**
   - Run "Login (Admin)" request
   - Token will be auto-saved

5. **Test Endpoints**
   - All authenticated endpoints will use the saved token

### Production

1. **Import Collection**
   - Import `Sqordia_API_Production.json`

2. **Update Credentials** (if different)
   - Update `admin_email` and `admin_password` variables

3. **Test Health**
   - Run "Health Check" request

4. **Login**
   - Run "Login (Admin)" request
   - Token will be auto-saved

5. **Test Endpoints**
   - Use any endpoint in the collection

## Environment Variables

### Local Development

```json
{
  "base_url": "http://localhost:5241",
  "admin_email": "admin@sqordia.com",
  "admin_password": "Sqordia2025!"
}
```

### Production

```json
{
  "base_url": "http://34.19.252.60:8080",
  "admin_email": "admin@sqordia.com",
  "admin_password": "Sqordia2025!"
}
```

## Tips

1. **Use Collection Runner**: Run entire folders or collections in sequence
2. **Save Responses**: Use Postman's "Save Response" feature for reference
3. **Environment Variables**: Create Postman environments for easy switching
4. **Pre-request Scripts**: Collections include scripts to auto-populate variables
5. **Tests**: Some requests include tests to validate responses

## Troubleshooting

### Token Not Saving

- Check that "Login" requests have test scripts enabled
- Verify response format matches expected structure
- Check Postman console for script errors

### 401 Unauthorized

- Verify token is saved: Check `jwt_token` variable
- Token may have expired: Run "Refresh Token" request
- Re-login if refresh fails

### Connection Refused

- **Local**: Ensure Docker containers are running
- **Production**: Verify production URL is correct
- Check firewall/network settings

### CORS Errors

- CORS is configured on the backend
- If issues persist, check backend CORS settings
- Ensure correct `Origin` header is sent

## Updating Collections

When API endpoints change:

1. Export updated collection from Postman
2. Update base URLs if needed
3. Update variable defaults
4. Commit changes to repository

## Support

For API documentation:
- Swagger UI: `http://localhost:5241/swagger` (local)
- Swagger UI: `http://34.19.252.60:8080/swagger` (production)

For setup help:
- See `docs/LOCAL_DEVELOPMENT_SETUP.md`
