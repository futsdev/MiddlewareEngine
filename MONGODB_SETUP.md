# MongoDB Setup Guide

## Problem
Test cases are not saving because **MongoDB is not running**.

## Quick Solution: MongoDB Atlas (Cloud - FREE)

### Step 1: Create Free Account
1. Go to [MongoDB Atlas](https://www.mongodb.com/cloud/atlas/register)
2. Sign up for free account
3. Create a **FREE M0 cluster** (512MB storage)

### Step 2: Get Connection String
1. Click **"Connect"** on your cluster
2. Choose **"Connect your application"**
3. Copy the connection string (looks like):
   ```
   mongodb+srv://username:<password>@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority
   ```
4. Replace `<password>` with your actual password

### Step 3: Update appsettings.json
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://YOUR_USERNAME:YOUR_PASSWORD@YOUR_CLUSTER.mongodb.net/?retryWrites=true&w=majority",
    "DatabaseName": "MiddlewareEngineDB",
    "FunctionDefinitionsCollectionName": "FunctionDefinitions"
  }
}
```

### Step 4: Whitelist Your IP
1. In Atlas, go to **Network Access**
2. Click **"Add IP Address"**
3. Click **"Allow Access from Anywhere"** (for development)

### Step 5: Test
1. Restart your app: `dotnet run`
2. Call seed endpoint: Open `seed-testcases.http` and click "Send Request"
3. Refresh Test Cases page

---

## Alternative: Local MongoDB Installation

### Windows (using Chocolatey)
```powershell
choco install mongodb
```

### Windows (Manual)
1. Download from: https://www.mongodb.com/try/download/community
2. Run installer
3. Choose "Complete" installation
4. Check "Install MongoDB as a Service"

### Start MongoDB
```powershell
net start MongoDB
```

### Verify Running
```powershell
Get-Service MongoDB
# Should show Status: Running
```

---

## Current Status
- ✅ Application code is correct
- ✅ Seed controller has dummy data
- ❌ MongoDB is not connected
- ❌ Test cases cannot be saved

Once MongoDB is connected, the seed endpoint will work immediately!
