# CustomAssemblies Folder

This folder stores custom .NET assemblies (DLLs) uploaded via the AssemblyManager service.

## Purpose

When you upload a DLL through the API endpoint `/api/assemblies/upload`, the file is stored here and becomes available for execution through SDK Method functions.

## Usage

### Upload DLL via API

**PowerShell:**
```powershell
$dllPath = "C:\Path\To\Your\Custom.dll"
$uri = "https://localhost:5001/api/assemblies/upload"

$form = @{
    file = Get-Item -Path $dllPath
}

Invoke-RestMethod -Uri $uri -Method Post -Form $form -SkipCertificateCheck
```

**Curl:**
```bash
curl -X POST https://localhost:5001/api/assemblies/upload \
  -F "file=@Custom.dll" \
  -k
```

### List Available Assemblies

```bash
curl https://localhost:5001/api/assemblies -k
```

### Use in Function Definition

Once uploaded, reference the assembly name (without .dll extension) in your SDK Method functions:

```json
{
  "execution_type": "SdkMethod",
  "execution_config": {
    "assembly_name": "MyCustomLibrary",
    "class_name": "MyNamespace.MyClass",
    "method_name": "MyMethod"
  }
}
```

## How It Works

The `AssemblyManager` service:
1. Stores uploaded DLLs in this folder
2. Scans this folder when listing available assemblies
3. Loads assemblies from here when executing SDK Method functions

The `SdkMethodExecutor`:
1. Uses `AssemblyManager.LoadAssembly()` to find the assembly
2. First tries GAC (for system assemblies like `System.Math`)
3. Then tries this CustomAssemblies folder
4. Finally tries the application base directory

## Security Note

⚠️ **Important**: This folder allows execution of arbitrary .NET code. In production:
- Implement authentication/authorization for upload endpoint
- Validate uploaded DLLs (digital signatures, allowed assemblies list)
- Run in isolated environment or sandbox
- Implement malware scanning
- Use role-based access control

## Folder Structure

```
CustomAssemblies/
├── MyCustomCalculator.dll      ← Your uploaded assemblies
├── BusinessLogicLibrary.dll
├── ThirdPartyIntegration.dll
└── README.md                    ← This file
```

## Supported .NET Versions

- .NET 9.0 (primary)
- .NET 8.0 (compatible)
- .NET 7.0 (compatible)
- .NET Standard 2.0+ libraries

## File Naming

- Assembly files must have `.dll` extension
- Assembly name should match the file name (without extension)
- Example: `MyLibrary.dll` → Assembly name: `MyLibrary`

## Troubleshooting

### DLL Not Found
- Check file exists in this folder
- Verify filename matches assembly name
- Check file permissions

### Type Not Found
- Ensure class is `public`
- Use fully qualified name: `Namespace.ClassName`
- Verify DLL is compiled correctly

### Dependency Issues
- Upload all dependent DLLs
- Check for version conflicts
- Use `dotnet publish` to gather all dependencies

## Clean Up

To remove unused assemblies, simply delete the `.dll` files from this folder. The application will automatically stop listing them.

For more details, see: [DYNAMIC_DLL_GUIDE.md](../DYNAMIC_DLL_GUIDE.md)
