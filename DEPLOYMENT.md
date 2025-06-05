# Azure Developer CLI (azd) Deployment Guide for PoRemoveBad

## Prerequisites
1. Ensure you have Azure CLI and azd CLI installed
2. Ensure you're logged into Azure

## Deployment Steps

### Step 1: Login to Azure (if not already logged in)
```bash
azd auth login --tenant-id punkouter25outlook.onmicrosoft.com
```

### Step 2: Initialize azd environment (if not already done)
```bash
azd init --environment poremovebad-dev
```

### Step 3: Set the subscription (replace with your subscription ID)
```bash
azd env set AZURE_SUBSCRIPTION_ID <your-subscription-id>
```

### Step 4: Set the location
```bash
azd env set AZURE_LOCATION eastus
```

### Step 5: Deploy the application (azd will create infrastructure automatically)
```bash
azd up
```

## Alternative: Deploy infrastructure and app separately

### Deploy infrastructure only:
```bash
azd provision
```

### Deploy application only (after infrastructure is deployed):
```bash
azd deploy
```

## Post-deployment verification

### Check deployment status:
```bash
azd show
```

### View logs:
```bash
azd logs
```

### Get the application URL:
```bash
azd show --output table
```

## Troubleshooting

If you encounter issues:
1. Check `azd logs` for deployment errors
2. Verify your Azure subscription has sufficient permissions
3. Ensure your Azure CLI is authenticated: `az account show`

## What gets deployed:
- App Service (hosting the Blazor WebAssembly app)
- Default Azure resources created by azd for .NET applications
- Basic configuration for production deployment

Note: This deployment uses azd's default infrastructure provisioning without custom Bicep templates.
