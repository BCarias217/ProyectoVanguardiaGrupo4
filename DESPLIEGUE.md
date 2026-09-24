# LogiChain — Despliegue en Azure App Service (Semana 3)

Esta guía la tienen que ejecutar ustedes con su propia cuenta/suscripción de
Azure — necesita login interactivo y no se puede correr desde acá. Sigue
exactamente la decisión del Avance 1: **Azure App Service Linux, plan B1
compartido entre los 3 servicios, y una Azure SQL Database independiente por
servicio.**

## 0. Antes de desplegar: regenerar las migraciones para SQL Server

Las migraciones que ya tienen (`InicialInventario`, `InicialTransporte`) se
generaron contra **SQLite** y no son 100% compatibles con Azure SQL (tipos de
columna e identidad distintos). El código ya soporta ambos motores (ver
`Program.cs` de cada Api — detecta el proveedor por la cadena de conexión),
pero las migraciones sí hay que regenerarlas una vez apuntando a SQL Server.

Por cada servicio (Inventario, Transporte, Facturacion), **antes de
desplegar**:

```bash
cd Inventario   # (o Transporte / Facturacion)

# Borrar las migraciones viejas (generadas para SQLite)
rm -rf Inventario.Infrastructure/Migrations

# Editar temporalmente Inventario.Api/appsettings.json y poner ahí la
# cadena de conexión real de Azure SQL (la que da el paso 2 más abajo)

dotnet ef migrations add InicialInventarioSql -p Inventario.Infrastructure -s Inventario.Api
```

`Program.cs` ya llama `db.Database.Migrate()` al iniciar, así que en el
primer arranque en Azure la migración se aplica sola contra Azure SQL.

## 1. Login y variables

```bash
az login
RG=rg-logichain
LOCATION=eastus
PLAN=plan-logichain-b1
SQLSERVER=sql-logichain-$RANDOM
SQLADMIN=logichainadmin
SQLPASSWORD='CambiaEsto123!'   # usá una contraseña real y no la subas a GitHub
```

## 2. Grupo de recursos, plan B1 (compartido) y Azure SQL

```bash
az group create --name $RG --location $LOCATION

az appservice plan create --name $PLAN --resource-group $RG \
  --sku B1 --is-linux

az sql server create --name $SQLSERVER --resource-group $RG \
  --location $LOCATION --admin-user $SQLADMIN --admin-password $SQLPASSWORD

# Permite que los App Services (y solo Azure) se conecten al servidor SQL
az sql server firewall-rule create --resource-group $RG --server $SQLSERVER \
  --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Una base de datos independiente por servicio (nivel Basic/serverless según
# lo que tengan disponible en su suscripción — revisen el nivel gratuito vigente)
az sql db create --resource-group $RG --server $SQLSERVER --name db-inventario --service-objective Basic
az sql db create --resource-group $RG --server $SQLSERVER --name db-transporte --service-objective Basic
az sql db create --resource-group $RG --server $SQLSERVER --name db-facturacion --service-objective Basic
```

## 3. Las 3 Web Apps (mismo plan B1, runtime .NET 8)

```bash
az webapp create --resource-group $RG --plan $PLAN --name logichain-inventario --runtime "DOTNETCORE:8.0"
az webapp create --resource-group $RG --plan $PLAN --name logichain-transporte --runtime "DOTNETCORE:8.0"
az webapp create --resource-group $RG --plan $PLAN --name logichain-facturacion --runtime "DOTNETCORE:8.0"

# Always On + health check, como se decidió en el Avance 1
for app in logichain-inventario logichain-transporte logichain-facturacion; do
  az webapp config set --resource-group $RG --name $app --always-on true
  az webapp config set --resource-group $RG --name $app --health-check-path /health
done
```

## 4. Cadenas de conexión y URL de Transporte para Facturación

```bash
az webapp config connection-string set --resource-group $RG --name logichain-inventario \
  --connection-string-type SQLAzure \
  --settings Default="Server=tcp:$SQLSERVER.database.windows.net,1433;Database=db-inventario;User ID=$SQLADMIN;Password=$SQLPASSWORD;Encrypt=true;"

az webapp config connection-string set --resource-group $RG --name logichain-transporte \
  --connection-string-type SQLAzure \
  --settings Default="Server=tcp:$SQLSERVER.database.windows.net,1433;Database=db-transporte;User ID=$SQLADMIN;Password=$SQLPASSWORD;Encrypt=true;"

az webapp config connection-string set --resource-group $RG --name logichain-facturacion \
  --connection-string-type SQLAzure \
  --settings Default="Server=tcp:$SQLSERVER.database.windows.net,1433;Database=db-facturacion;User ID=$SQLADMIN;Password=$SQLPASSWORD;Encrypt=true;"

# Facturación necesita saber dónde vive Transporte una vez desplegado
az webapp config appsettings set --resource-group $RG --name logichain-facturacion \
  --settings Servicios__TransporteBaseUrl="https://logichain-transporte.azurewebsites.net"
```

## 5. Publicar cada servicio (zip deploy)

```bash
cd Inventario/Inventario.Api
dotnet publish -c Release -o ./publish
cd publish && zip -r ../publish.zip . && cd ..
az webapp deploy --resource-group $RG --name logichain-inventario --src-path publish.zip --type zip
cd ../..

cd Transporte/Transporte.Api
dotnet publish -c Release -o ./publish
cd publish && zip -r ../publish.zip . && cd ..
az webapp deploy --resource-group $RG --name logichain-transporte --src-path publish.zip --type zip
cd ../..

cd Facturacion/Facturacion.Api
dotnet publish -c Release -o ./publish
cd publish && zip -r ../publish.zip . && cd ..
az webapp deploy --resource-group $RG --name logichain-facturacion --src-path publish.zip --type zip
cd ../..
```

## 6. Verificar

```
https://logichain-inventario.azurewebsites.net/health
https://logichain-transporte.azurewebsites.net/health
https://logichain-facturacion.azurewebsites.net/health
```

Los tres deben responder `{"estado":"ok", ...}`. Si `/health` de Facturación
responde pero facturar falla, revisen que `Servicios__TransporteBaseUrl`
apunte a la URL real de Transporte en Azure (paso 4).

## 7. Actualizar el Environment de Postman

En `postman/LogiChain.postman_environment.json`, cambien `inventarioUrl`,
`transporteUrl` y `facturacionUrl` de `http://localhost:xxxx` a las URLs de
Azure (`https://logichain-inventario.azurewebsites.net`, etc.) antes de
correr la colección contra el ambiente desplegado.

## Costo

Revisen el saldo de su suscripción de estudiante antes de la entrega — el
plan B1 no es gratuito (referencia ~USD 13.14/mes) y Azure SQL Basic tampoco.
Si usan crédito de Azure for Students, confirmen que quede saldo hasta el día
de la demo. Nada de esto se despliega ni se cobra automáticamente: son
ustedes quienes corren estos comandos con su propia cuenta.
