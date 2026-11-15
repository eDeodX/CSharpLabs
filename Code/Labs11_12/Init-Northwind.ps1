$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (-not $env:MSSQL_SA_PASSWORD) {
        throw 'Set the MSSQL_SA_PASSWORD environment variable first.'
    }

    docker compose up -d northwind
    if ($LASTEXITCODE -ne 0) { throw 'Could not start SQL Server.' }

    $sqlcmd = '/opt/mssql-tools18/bin/sqlcmd'
    $ready = $false
    for ($i = 0; $i -lt 45; $i++) {
        docker compose exec -T northwind $sqlcmd -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -l 2 -b -Q 'SET NOCOUNT ON; SELECT 1' *> $null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Start-Sleep -Seconds 2
    }
    if (-not $ready) { throw 'SQL Server did not become ready.' }

    $query = "IF DB_ID('Northwind') IS NULL CREATE DATABASE Northwind;"
    docker compose exec -T northwind $sqlcmd -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -b -Q $query
    if ($LASTEXITCODE -ne 0) { throw 'Could not create Northwind.' }

    $check = "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID('dbo.Categories','U') IS NULL THEN 0 ELSE 1 END;"
    $installed = docker compose exec -T northwind $sqlcmd -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -b -d Northwind -h -1 -W -Q $check
    if ($LASTEXITCODE -ne 0) { throw 'Could not check the Northwind schema.' }
    if (($installed -join '').Trim() -eq '1') {
        Write-Host 'Northwind already contains Categories. Existing data was kept.'
        return
    }

    $source = 'https://raw.githubusercontent.com/microsoft/sql-server-samples/master/samples/databases/northwind-pubs/instnwnd.sql'
    Invoke-WebRequest -Uri $source -OutFile 'instnwnd.sql'
    docker compose cp instnwnd.sql northwind:/tmp/instnwnd.sql
    if ($LASTEXITCODE -ne 0) { throw 'Could not copy the Northwind script.' }
    docker compose exec -T northwind $sqlcmd -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -b -d Northwind -i /tmp/instnwnd.sql
    if ($LASTEXITCODE -ne 0) { throw 'Northwind import failed.' }
    Write-Host 'Northwind is ready.'
}
finally {
    Pop-Location
}
