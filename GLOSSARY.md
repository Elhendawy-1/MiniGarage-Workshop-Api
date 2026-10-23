# Mini Garage Workshop — Domain Glossary (GLOSSARY.md)

This glossary defines the ubiquitous language for the **Mini Garage** application domain, establishing common terminology for models, modules, seams, and operational workflows.

---

## 1. Domain Entities & Value Concepts

### Vehicle
A motor vehicle brought to the workshop for maintenance or diagnostic repair. Represented in persistence by `Car` and externally by `CarResponseDto`.
- **Properties**: `Id`, `OwnerName`, `CarModel`, `LicensePlate`, `ProblemDescription`, `RepairStatus`, `CreatedAt`.

### License Plate
The canonical governmental registration tag of a vehicle.
- **Invariants**: Alphanumeric, spaces, and hyphens only; canonicalized into trimmed uppercase characters (e.g., `DXB-7788`, `KSA-1020`).

### Repair Status
The discrete lifecycle state of a registered vehicle:
1. **Pending** (`0`): Vehicle registered and parked; awaiting mechanic inspection.
2. **InProgress** (`1`): Vehicle actively undergoing repair or service on the hydraulic lift.
3. **Completed** (`2`): Maintenance completed, quality-checked, and ready for owner pickup.

### Problem Description
Mechanical or electrical symptoms reported by the customer during workshop intake (5 to 500 characters).

### Retirement (Deletion)
The permanent removal of a vehicle record from workshop history. Restricted strictly to authorized workshop administrators possessing a valid secret key.

---

## 2. Architectural Modules & Seams

### Workshop Registry (`IWorkshopRegistry`)
The **deep module** encapsulating all vehicle intake, status progression, license plate canonicalization, administrative retirement verification, and SQL persistence.
- **Seam**: Defined by `IWorkshopRegistry`.
- **Primary Adapter**: `WorkshopRegistry` backed by Entity Framework Core 9 and SQL Server.
- **Secondary Adapter**: In-memory fake or mock for unit test verification.

### Cars Transport Adapter (`CarsController`)
The shallow HTTP adapter sitting at the public network seam (`/api/cars`). Responsible exclusively for route binding, JSON deserialization, HTTP status translation (200, 201, 204, 400, 401, 404), and OpenAPI metadata.

### Transport Defense Shield
The HTTP pipeline module responsible for client rate limiting, Strict Content Security Policy (CSP), HTTP/3 QUIC advertisement (`Alt-Svc`), and CORS enforcement.
