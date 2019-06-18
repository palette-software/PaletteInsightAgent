@echo off
dotnet tool install --global AzureSignTool
azuresigntool sign --description-url "https://starschema.com" ^
    --file-digest sha256 --azure-key-vault-url https://billiant-keyvault.vault.azure.net ^
    --azure-key-vault-client-id %AZURE_KEY_VAULT_CLIENT_ID% ^
    --azure-key-vault-client-secret %AZURE_KEY_VAULT_CLIENT_SECRET% ^
    --azure-key-vault-certificate StarschemaCodeSigning ^
    --timestamp-rfc3161 http://timestamp.digicert.com --timestamp-digest sha256 ^
    --no-page-hashing ^
    %*
