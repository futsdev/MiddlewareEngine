# Operation Types Guide

## Overview
The MiddlewareEngine now supports **Operation Types** for advanced functionality including file upload/download operations across REST, SSH, SCPI, and SDK execution types.

## Operation Types

### 1. **READ**
Query or retrieve data from instruments/systems.

**Use Cases:**
- Query instrument status
- Read measurements
- Get configuration values
- Retrieve test results

**Example Function Definition:**
```json
{
  "function_id": "get_instrument_status",
  "name": "Get Instrument Status",
  "execution_type": "SCPI",
  "operation_type": "READ",
  "execution_config": {
    "scpi_command": "*IDN?",
    "connection_string": "TCPIP0::192.168.1.100::inst0::INSTR"
  }
}
```

### 2. **WRITE**
Send commands or set values on instruments/systems.

**Use Cases:**
- Configure instrument settings
- Execute control commands
- Set parameters
- Start/stop operations

**Example Function Definition:**
```json
{
  "function_id": "set_voltage",
  "name": "Set Output Voltage",
  "execution_type": "SCPI",
  "operation_type": "WRITE",
  "execution_config": {
    "scpi_command": "VOLT {voltage}",
    "connection_string": "TCPIP0::192.168.1.100::inst0::INSTR"
  },
  "parameters": [
    {
      "name": "voltage",
      "type": "number",
      "required": true
    }
  ]
}
```

### 3. **FILE_UPLOAD**
Upload files to instruments or remote systems.

**Use Cases:**
- Upload firmware updates
- Send configuration files
- Deploy test scripts
- Transfer calibration data

#### SSH File Upload Example:
```json
{
  "function_id": "upload_firmware",
  "name": "Upload Firmware to Instrument",
  "execution_type": "Ssh",
  "operation_type": "FILE_UPLOAD",
  "execution_config": {
    "ssh_host": "192.168.1.100",
    "ssh_port": 22,
    "ssh_username": "admin",
    "ssh_password": "password123",
    "remote_path": "/tmp/firmware.bin",
    "max_file_size_mb": 500
  }
}
```

**Execution Call:**
```bash
POST /api/execute
{
  "function_id": "upload_firmware",
  "parameters": {
    "localPath": "C:\\firmware\\latest.bin",
    "remotePath": "/tmp/firmware.bin"
  }
}
```

#### REST API File Upload Example:
```json
{
  "function_id": "upload_test_data",
  "name": "Upload Test Data via REST",
  "execution_type": "RestApi",
  "operation_type": "FILE_UPLOAD",
  "execution_config": {
    "url": "https://api.example.com/upload",
    "http_method": "POST",
    "headers": {
      "Authorization": "Bearer TOKEN123"
    },
    "max_file_size_mb": 100
  }
}
```

**Execution Call:**
```bash
POST /api/execute
{
  "function_id": "upload_test_data",
  "parameters": {
    "localPath": "C:\\data\\test_results.csv",
    "fileName": "test_results.csv"
  }
}
```

### 4. **FILE_DOWNLOAD**
Download files from instruments or remote systems.

**Use Cases:**
- Retrieve test logs
- Download measurement data
- Backup configurations
- Export reports

#### SSH File Download Example:
```json
{
  "function_id": "download_logs",
  "name": "Download Instrument Logs",
  "execution_type": "Ssh",
  "operation_type": "FILE_DOWNLOAD",
  "execution_config": {
    "ssh_host": "192.168.1.100",
    "ssh_port": 22,
    "ssh_username": "admin",
    "ssh_password": "password123",
    "remote_path": "/var/log/instrument.log",
    "max_file_size_mb": 50
  }
}
```

**Execution Call:**
```bash
POST /api/execute
{
  "function_id": "download_logs",
  "parameters": {
    "remotePath": "/var/log/instrument.log",
    "localPath": "C:\\logs\\instrument_20260130.log"
  }
}
```

#### REST API File Download Example:
```json
{
  "function_id": "download_report",
  "name": "Download Test Report",
  "execution_type": "RestApi",
  "operation_type": "FILE_DOWNLOAD",
  "execution_config": {
    "url": "https://api.example.com/reports/download",
    "http_method": "GET",
    "headers": {
      "Authorization": "Bearer TOKEN123"
    },
    "timeout": 300
  }
}
```

**Execution Call:**
```bash
POST /api/execute
{
  "function_id": "download_report",
  "parameters": {
    "reportId": "12345",
    "localPath": "C:\\reports\\report_12345.pdf"
  }
}
```

## Execution Type + Operation Type Matrix

| Execution Type | READ | WRITE | FILE_UPLOAD | FILE_DOWNLOAD |
|---------------|------|-------|-------------|---------------|
| **RestApi**   | ✅ GET | ✅ POST/PUT | ✅ Multipart | ✅ Binary Stream |
| **SCPI**      | ✅ Query | ✅ Command | ❌ | ❌ |
| **SSH**       | ✅ Command | ✅ Command | ✅ SFTP | ✅ SFTP |
| **SDK**       | ✅ Method | ✅ Method | ❌ | ❌ |

## Configuration Fields by Execution Type

### SSH Configuration
```json
{
  "ssh_host": "192.168.1.100",          // Required
  "ssh_port": 22,                        // Optional, default 22
  "ssh_username": "admin",               // Required
  "ssh_password": "password",            // Required (if no key)
  "ssh_key_path": "/path/to/key.pem",   // Required (if no password)
  "remote_path": "/path/on/remote",      // For file operations
  "local_path": "C:\\path\\on\\local",   // For file operations
  "max_file_size_mb": 100                // Optional, default 100MB
}
```

### REST API Configuration
```json
{
  "url": "https://api.example.com/endpoint",
  "http_method": "POST",
  "headers": {
    "Authorization": "Bearer TOKEN",
    "Content-Type": "application/json"
  },
  "timeout": 30,                         // Seconds
  "max_file_size_mb": 100                // For file operations
}
```

### SCPI Configuration
```json
{
  "scpi_command": "*IDN?",
  "connection_string": "TCPIP0::192.168.1.100::inst0::INSTR"
}
```

### SDK Configuration
```json
{
  "assembly_name": "MyInstrument.SDK",
  "class_name": "InstrumentController",
  "method_name": "ExecuteCommand"
}
```

## Response Format

### Successful File Upload Response:
```json
{
  "success": true,
  "result": {
    "fileName": "firmware.bin",
    "fileSizeBytes": 10485760,
    "localPath": "C:\\firmware\\latest.bin",
    "remotePath": "/tmp/firmware.bin",
    "durationMs": 1234.56
  },
  "executedAt": "2026-01-30T12:34:56Z"
}
```

### Successful File Download Response:
```json
{
  "success": true,
  "result": {
    "fileName": "instrument.log",
    "fileSizeBytes": 524288,
    "remotePath": "/var/log/instrument.log",
    "localPath": "C:\\logs\\instrument_20260130.log",
    "durationMs": 456.78
  },
  "executedAt": "2026-01-30T12:34:56Z"
}
```

### Error Response:
```json
{
  "success": false,
  "errorMessage": "File size exceeds maximum allowed size of 100MB",
  "executedAt": "2026-01-30T12:34:56Z"
}
```

## Best Practices

### Security
- ✅ Use SSH keys instead of passwords when possible
- ✅ Store credentials securely (use environment variables or secret management)
- ✅ Set appropriate file size limits
- ✅ Validate file paths to prevent directory traversal attacks

### Performance
- ✅ Set reasonable timeout values for large files
- ✅ Use chunking for very large files
- ✅ Monitor file transfer progress for long operations
- ✅ Clean up temporary files after operations

### Error Handling
- ✅ Validate file exists before upload
- ✅ Check available disk space before download
- ✅ Handle network interruptions gracefully
- ✅ Log all file operations for audit trail

## Common Use Cases

### 1. Firmware Update Workflow
```
1. FILE_DOWNLOAD: Download latest firmware from repository
2. FILE_UPLOAD: Upload firmware to instrument
3. WRITE: Execute firmware update command
4. READ: Verify firmware version
```

### 2. Configuration Backup
```
1. READ: Check instrument status
2. FILE_DOWNLOAD: Download current config
3. WRITE: Save backup metadata
```

### 3. Test Data Collection
```
1. WRITE: Start measurement
2. READ: Monitor progress
3. FILE_DOWNLOAD: Retrieve results
4. FILE_UPLOAD: Upload to analysis server
```

## API Endpoints

### Execute Function
```bash
POST /api/execute
Content-Type: application/json

{
  "function_id": "upload_firmware",
  "parameters": {
    "localPath": "C:\\firmware\\latest.bin",
    "remotePath": "/tmp/firmware.bin"
  }
}
```

### List Functions by Operation Type
```bash
GET /api/functions?operationType=FILE_UPLOAD
```

## Error Codes

| Code | Description |
|------|-------------|
| `FILE_NOT_FOUND` | Local or remote file not found |
| `FILE_SIZE_EXCEEDED` | File exceeds max size limit |
| `CONNECTION_FAILED` | Cannot connect to remote system |
| `AUTH_FAILED` | Authentication failed |
| `PERMISSION_DENIED` | Insufficient permissions |
| `TIMEOUT` | Operation timed out |
| `DISK_FULL` | Insufficient disk space |

## Troubleshooting

### SSH Connection Issues
- Verify host, port, username are correct
- Check SSH service is running
- Validate credentials or key file path
- Ensure firewall allows SSH port

### File Upload Failures
- Check file exists and is readable
- Verify remote path has write permissions
- Ensure sufficient disk space on remote
- Check file size doesn't exceed limit

### File Download Failures
- Verify remote file exists
- Check local path has write permissions
- Ensure sufficient local disk space
- Validate network connectivity

## Dependencies

The following NuGet packages are required:
- `SSH.NET` (v2024.2.0) - For SSH/SFTP operations
- `MongoDB.Driver` - For data persistence
- Built-in `HttpClient` - For REST file operations
