# 🔍 Why Data Is Not Showing - ISSUE DIAGNOSIS

## THE PROBLEM

You're experiencing these issues:
1. ❌ Steps, actions, setup, teardown not showing in Test Cases list
2. ❌ Edit page not showing saved data
3. ❌ Execution history list is empty
4. ❌ Detailed reports not appearing

## THE ROOT CAUSE

**MongoDB is NOT connected!** 🔴

Without MongoDB:
- Data cannot be saved to database
- When you click "Save" in Test Case Builder → Goes nowhere
- When you click "Execute" → Results not saved
- When you click "History" → Empty (no data in database)
- Test Cases list shows 0 items (nothing in database)

## HOW TO VERIFY THE ISSUE

### Option 1: Check MongoDB Status Page
1. Open browser to: `http://localhost:5277/mongodb-status.html`
2. You will see:
   - ✓ Green = MongoDB Connected (Good!)
   - ✗ Red = MongoDB Disconnected (This is your problem!)

### Option 2: Check Health API
```bash
# Open this in browser or curl:
http://localhost:5277/api/health/mongodb
```

### Option 3: Check Browser Console
1. Open Test Case Builder
2. Add steps, actions, setup operations
3. Click "Save Test Case"
4. Open browser console (F12)
5. You'll see error like:
   ```
   POST http://localhost:5277/api/testcases 500 (Internal Server Error)
   Error: A timeout occurred after 30000ms selecting a server...
   ```

## THE SOLUTION

You have 2 options:

### Option A: MongoDB Atlas (Cloud - FREE & EASY) ✅ **RECOMMENDED**

1. **Sign up**: Go to https://www.mongodb.com/cloud/atlas/register
2. **Create FREE cluster**: M0 tier (512MB - Free forever)
3. **Get connection string**: Click "Connect" → "Connect your application"
   - Example: `mongodb+srv://user:pass@cluster0.xxxxx.mongodb.net/`
4. **Whitelist IP**: Network Access → Add IP Address → "Allow from Anywhere" (for dev)
5. **Update appsettings.json**:
   ```json
   {
     "MongoDbSettings": {
       "ConnectionString": "mongodb+srv://YOUR_USER:YOUR_PASS@YOUR_CLUSTER.mongodb.net/",
       "DatabaseName": "MiddlewareEngineDB"
     }
   }
   ```
6. **Restart app**: `dotnet run`
7. **Verify**: Open `http://localhost:5277/mongodb-status.html`

### Option B: Local MongoDB

#### Windows (using Chocolatey):
```powershell
choco install mongodb
net start MongoDB
```

#### Windows (Manual):
1. Download: https://www.mongodb.com/try/download/community
2. Run installer → Choose "Complete" installation
3. Check "Install MongoDB as a Service"
4. Finish installation
5. Verify: `Get-Service MongoDB` (should show "Running")

#### Update appsettings.json:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MiddlewareEngineDB"
  }
}
```

## AFTER CONNECTING MONGODB

Once MongoDB is connected, everything will work:

### 1. Test Case Builder
- Add steps, actions, operations
- Click "Save Test Case"
- Data is saved to MongoDB
- Browser console shows: `"Saved Steps: 2, Saved Setup: 1"`

### 2. Test Cases List
- Shows all saved test cases
- Each card displays:
  - Number of steps
  - Number of actions
  - Setup operations count
  - Teardown operations count

### 3. Edit Test Case
- Click "Edit" on any test case
- Loads all steps, actions, operations
- You can modify and save again

### 4. Execute Test Case
- Click "Execute" button
- Execution runs and results saved to MongoDB
- Success/fail status recorded

### 5. Execution History
- Click "History" button on any test case
- Shows list of all past executions
- Click any execution → Opens detailed report

### 6. Detailed Report
- Shows complete hierarchy:
  - Setup operations (✓/✗ status)
  - Steps → Actions → Operations (all with status)
  - Teardown operations (✓/✗ status)
- Execution times at every level
- Error messages if failures occurred

## QUICK DEBUG CHECKLIST

```
☐ 1. Open http://localhost:5277/mongodb-status.html
☐ 2. Is it GREEN (Connected)?
   ✓ YES → Your issue is somewhere else (check console logs)
   ✗ NO → Follow "THE SOLUTION" above
☐ 3. After connecting MongoDB, restart app: dotnet run
☐ 4. Verify connection again: http://localhost:5277/mongodb-status.html
☐ 5. Should now see GREEN with collection names
☐ 6. Test: Create a simple test case with 1 step
☐ 7. Save → Check browser console for "Saved Steps: 1"
☐ 8. Go to Test Cases page → Should see your test case
☐ 9. Click History → Should show "No executions yet"
☐ 10. Click Execute → Wait for completion
☐ 11. Click History again → Should show 1 execution
☐ 12. Click execution → Opens detailed report
```

## STILL NOT WORKING?

Check browser console (F12) for errors and share the exact error message.

### Common Errors:

**"Timeout selecting a server"**
→ MongoDB not running or connection string wrong

**"Authentication failed"**
→ Wrong username/password in connection string

**"Network is unreachable"**
→ IP not whitelisted in MongoDB Atlas

**"Cannot read properties of null"**
→ Fields missing in HTML (already fixed)

**"Failed to fetch"**
→ API endpoint issue (check if app is running)
