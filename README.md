# LogiChain — Avance 3 (final)

Grupo 4 — 574 Desarrollo de Aplicaciones de Vanguardia — Ing. María José Salinas

Los 3 microservicios completos: **Inventario**, **Transporte** y
**Facturación**, cada uno con sus 4 capas (API → Application → Domain →
Infrastructure), mismo patrón que `PatronMulticapaMonolito` (caso Biblioteca)
visto en clase, más la colección de Postman y la guía de despliegue en Azure.

## Requisitos

- .NET SDK 8.0 (`dotnet --version`)
- EF Core CLI: `dotnet tool install --global dotnet-ef` (si no lo tenés)
- Postman (para correr la colección)

## Primer arranque (una sola vez por servicio)

```bash
cd Inventario
dotnet restore
dotnet ef migrations add InicialInventario -p Inventario.Infrastructure -s Inventario.Api
cd ..

cd Transporte
dotnet restore
dotnet ef migrations add InicialTransporte -p Transporte.Infrastructure -s Transporte.Api
cd ..

cd Facturacion
dotnet restore
dotnet ef migrations add InicialFacturacion -p Facturacion.Infrastructure -s Facturacion.Api
cd ..
```

`Program.cs` en cada Api ya llama `db.Database.Migrate()` al iniciar, así que
la base SQLite se crea y se siembra sola la primera vez que corrés el
servicio.

## Correr los 3 servicios (tres terminales, deben quedar corriendo a la vez)

```bash
# Terminal 1 — Inventario -> http://localhost:5101/swagger
cd Inventario/Inventario.Api
dotnet run

# Terminal 2 — Transporte -> http://localhost:5102/swagger
cd Transporte/Transporte.Api
dotnet run

# Terminal 3 — Facturacion -> http://localhost:5103/swagger
cd Facturacion/Facturacion.Api
dotnet run
```

**Importante:** Facturación llama a Transporte por HTTP (`GET /envios/{id}`)
para saber si un envío ya fue entregado, con un timeout de 3 segundos y sin
reintentos. Si Transporte no está corriendo, `POST /api/v1/facturas`
responde **503**, no un error genérico — es la resiliencia decidida en el
Avance 1, no un bug.

## Datos de prueba (seed automático)

- **Inventario:** Bodega Central (Id 1) y Bodega Norte (Id 2), producto
  SKU-001, con 100 unidades en Bodega Central.
- **Transporte:** dos rutas activas entre esas mismas bodegas (240.5 km).
- **Facturación:** una tarifa activa (cargo base 50 + 2.5 por km, en HNL).

## Probar con Postman

La colección está en `postman/LogiChain.postman_collection.json`, con su
Environment en `postman/LogiChain.postman_environment.json`.

1. Abrí Postman → **Import** → arrastrá los dos archivos de `postman/`.
2. Seleccioná el Environment **"LogiChain - Local"** (arriba a la derecha).
3. Con los 3 servicios corriendo, abrí la colección **"LogiChain - Grupo 4"**
   → click derecho → **Run collection** → corré las 3 carpetas en orden
   (1. Inventario, 2. Transporte, 3. Facturacion). Cada request encadena con
   la siguiente usando variables de entorno (ids, versiones) que se van
   guardando solas.
4. Todos los `pm.test` deberían pasar en verde — cubren los casos de éxito
   *y* los de rechazo (409, 503) de cada regla de negocio.

## Endpoints implementados

**Inventario** (`http://localhost:5101`)
| Método y ruta | Resultado |
|---|---|
| GET /bodegas | 200 |
| GET /bodegas/{id} | 200 / 404 |
| GET /productos | 200 |
| GET /bodegas/{bodegaId}/stock/{productoId} | 200 / 404 |
| POST /movimientos-stock | 201 / 200 (reintento) / 400 / 404 / 409 |
| GET /movimientos-stock/{id} | 200 / 404 |
| GET /health | 200 |

**Transporte** (`http://localhost:5102`)
| Método y ruta | Resultado |
|---|---|
| GET /rutas | 200 |
| POST /envios | 201 / 404 / 409 |
| GET /envios/{id} | 200 / 404 |
| PATCH /envios/{id}/estado | 200 / 404 / 409 |
| GET /health | 200 |

**Facturación** (`http://localhost:5103`)
| Método y ruta | Resultado |
|---|---|
| POST /api/v1/facturas | 201 / 404 / 409 / 503 |
| GET /api/v1/facturas | 200 (con `?envioId=` opcional) |
| GET /api/v1/facturas/{id} | 200 / 404 |
| GET /health | 200 |

## Reglas de negocio implementadas

- **R1** — El stock nunca queda negativo (Inventario): validado antes de
  descontar, con `CHECK (Cantidad >= 0)` en la BD como último resguardo.
- **R2** — Un envío no puede crearse sin una ruta existente y activa
  (Transporte).
- **R3** — Los estados del envío solo avanzan en orden (Pendiente →
  EnTransito → Entregado), con token de versión para rechazar transiciones
  sobre datos desactualizados (Transporte).
- **R4** — Facturación consulta a Transporte el estado real antes de emitir;
  si Transporte no responde a tiempo, no se factura y se devuelve 503; si el
  envío no está Entregado, 409.
- **R5** — Un envío, una factura: validado en el servicio y respaldado con un
  índice único en `EnvioId` a nivel de BD (cubre la carrera de dos
  solicitudes simultáneas).
- **R6** — Una transferencia registra salida y entrada (dos saldos, dos
  `DetalleMovimiento`) en un solo `SaveChangesAsync` (Inventario).

## Despliegue en Azure

Ver `DESPLIEGUE.md` — comandos exactos de Azure CLI para los 3 App Services
(plan B1 compartido) y las 3 Azure SQL Database independientes, según lo
decidido en el Avance 1. Ese paso lo tienen que correr ustedes con su propia
cuenta de Azure; el código ya soporta ambos motores (SQLite en local, Azure
SQL en producción — se elige solo según la cadena de conexión).
