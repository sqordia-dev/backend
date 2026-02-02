# Google OAuth 2.0 Production Setup Guide for https://sqordia.app/

This guide will help you complete the Google OAuth 2.0 setup for your production domain `https://sqordia.app/`.

## Step 1: Configure Google Cloud Console

### 1.1 Create or Update OAuth 2.0 Client ID

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select your project (or create a new one)
3. Navigate to **APIs & Services** > **Credentials**
4. Click **+ Create Credentials** > **OAuth client ID**

### 1.2 Configure OAuth Client Settings

**Application type:** Web application

**Name:** Sqordia Production

**Authorized JavaScript origins:**
Add the **exact** origin where your frontend is served (no trailing slash; use HTTPS in production). If the production origin is missing, Google Sign-In will show "missing verified parent origin" and COOP/postMessage errors.
```
https://sqordia.app
http://localhost:5173
```
If you use a different production URL (e.g. Azure Static Web Apps: `https://<name>.azurestaticapps.net`), add that origin here as well.

**Authorized redirect URIs:**
Add the following URLs:
```
https://sqordia.app/auth/google/callback
https://sqordia.app/api/v1/auth/google/callback
http://localhost:5173/auth/google/callback
http://localhost:5241/api/v1/auth/google/callback
```

**Important Notes:**
- The redirect URIs must match exactly (including trailing slashes or lack thereof)
- Changes may take 5-10 minutes to propagate
- You can have multiple redirect URIs for different environments

### 1.3 Copy Your Credentials

After creating the OAuth client:
- **Client ID**: Copy this (looks like: `xxxxx.apps.googleusercontent.com`)
- **Client Secret**: Copy this (keep it secure!)

## Step 2: Backend Configuration

### 2.1 Environment Variables

Set the following environment variables in your production environment:

```bash
# Google OAuth Configuration
GOOGLE_OAUTH_CLIENT_ID=your-client-id.apps.googleusercontent.com
GOOGLE_OAUTH_CLIENT_SECRET=your-client-secret

# Frontend Base URL (for email links and redirects)
FRONTEND_BASE_URL=https://sqordia.app
```

### 2.2 Azure Container Apps / Production Environment

If using Azure Container Apps, set these in your container app settings:

**Environment Variables:**
- `GOOGLE_OAUTH_CLIENT_ID` = Your Google OAuth Client ID
- `GOOGLE_OAUTH_CLIENT_SECRET` = Your Google OAuth Client Secret
- `FRONTEND_BASE_URL` = `https://sqordia.app`

**Or use Azure Key Vault (Recommended for Secrets):**
```bash
GOOGLE_OAUTH_CLIENT_SECRET=@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/google-oauth-secret/)
```

### 2.3 appsettings.Production.json

The production configuration file already includes:
```json
{
  "Frontend": {
    "BaseUrl": "https://sqordia.app"
  },
  "GoogleOAuth": {
    "ClientId": "${GOOGLE_OAUTH_CLIENT_ID}",
    "ClientSecret": "${GOOGLE_OAUTH_CLIENT_SECRET}",
    "RedirectUri": "${GOOGLE_OAUTH_REDIRECT_URI}"
  }
}
```

## Step 3: Frontend Configuration

### 3.1 Production Environment File

Create `frontend/.env.production`:

```env
VITE_GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
VITE_API_URL=https://your-production-api-url.com
```

**Important:**
- The Client ID must match the one in Google Cloud Console
- The Client ID must match the backend configuration
- Never commit `.env.production` with real secrets (add to `.gitignore`)

### 3.2 Build for Production

When building for production:
```bash
cd frontend
npm run build
```

Vite will automatically:
- Load `.env.production`
- Embed `VITE_GOOGLE_CLIENT_ID` into the built JavaScript
- Use production API URL from `VITE_API_URL`

## Step 4: Verify Configuration

### 4.1 Test Google OAuth Flow

1. **Production:**
   - Go to `https://sqordia.app/login`
   - Click "Sign in with Google"
   - Should redirect to Google sign-in
   - After authentication, should redirect back to `https://sqordia.app/dashboard`

2. **Development:**
   - Go to `http://localhost:5173/login`
   - Click "Sign in with Google"
   - Should work with localhost configuration

### 4.2 Check Email Links

All email links (verification, password reset, invitations) should now point to:
- `https://sqordia.app/verify-email?token=...`
- `https://sqordia.app/reset-password?token=...`
- `https://sqordia.app/accept-invitation?token=...`

## Step 5: Common Issues and Solutions

### Issue: "redirect_uri_mismatch" Error

**Solution:**
- Verify the redirect URI in Google Cloud Console matches exactly
- Check for trailing slashes
- Ensure both `https://sqordia.app/auth/google/callback` and `https://sqordia.app/api/v1/auth/google/callback` are added
- Wait 5-10 minutes after making changes

### Issue: "403 Forbidden" Error

**Solution:**
- Verify the Client ID matches in both frontend and backend
- Check that the JavaScript origin `https://sqordia.app` is added
- Clear browser cache and cookies
- Check browser console for specific error messages

### Issue: "Missing verified parent origin" / COOP blocking postMessage (Google Sign-In)

If the browser console shows **"Resize command was not sent due to missing verified parent origin"** or **"Cross-Origin-Opener-Policy policy would block the window.postMessage call"**, the production frontend origin is not correctly configured for Google Sign-In.

**Solution:**

1. **Authorized JavaScript origins (required)**  
   In **Google Cloud Console** → **APIs & Services** → **Credentials** → your **OAuth 2.0 Client ID**:
   - Open **Authorized JavaScript origins**.
   - Add the **exact** production frontend origin (e.g. `https://sqordia.app` or your Azure Static Web Apps URL like `https://<app>.azurestaticapps.net`).
   - Use **no trailing slash**; use **HTTPS** if the site is served over HTTPS.
   - Save; changes can take 5–10 minutes to propagate.

2. **Do not set Cross-Origin-Opener-Policy (COOP)**  
   Google Sign-In uses an iframe that communicates via `postMessage`. If your site sends a COOP header, the browser can block that and you will see "Cross-Origin-Opener-Policy policy would block the window.postMessage call".  
   This project does **not** set COOP. If you add security headers on another host, do not include `Cross-Origin-Opener-Policy`.

### Issue: Email Links Point to localhost

**Solution:**
- Verify `FRONTEND_BASE_URL` environment variable is set to `https://sqordia.app`
- Check `appsettings.Production.json` has correct `Frontend:BaseUrl`
- Restart the application after changing environment variables

## Step 6: Security Best Practices

1. **Never commit secrets to git:**
   - Add `.env.production` to `.gitignore`
   - Use environment variables or Azure Key Vault for secrets

2. **Use different OAuth clients for dev/prod:**
   - Create separate OAuth clients for development and production
   - This allows different redirect URIs and better security isolation

3. **Rotate secrets regularly:**
   - Update Client Secret periodically
   - Revoke old credentials when rotating

4. **Monitor OAuth usage:**
   - Check Google Cloud Console for OAuth usage metrics
   - Set up alerts for unusual activity

## Summary

✅ **Completed:**
- Email links now use configurable frontend URL (defaults to https://sqordia.app)
- Google OAuth callback redirects to production domain
- All email templates updated to use production URLs
- Configuration supports both development and production

**Next Steps:**
1. Create OAuth 2.0 Client ID in Google Cloud Console with production domain
2. Set environment variables in production environment
3. Update frontend `.env.production` with Client ID
4. Test the OAuth flow in production
5. Verify email links work correctly
