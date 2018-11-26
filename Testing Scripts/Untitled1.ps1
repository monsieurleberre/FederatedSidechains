$params = @{
  "walletName"= "string";
  "password"= "string"
}

Invoke-WebRequest -Uri http://localhost:38202/api/Wallet/account -Method post -Body ($params|ConvertTo-Json) -ContentType "application/json"
