# CloudCart

A production-grade e-commerce platform built with .NET 10 microservices, Angular 21, and a full cloud-native DevOps stack.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                        Client                           │
│                   Angular 21 SPA                        │
│              (Keycloak PKCE OAuth2)                     │
└───────────────────────┬─────────────────────────────────┘
                        │ /api/*
                        ▼
┌─────────────────────────────────────────────────────────┐
│                   API Gateway (YARP)                    │
│              JWT validation · CORS · Routing            │
└────┬──────┬──────┬──────┬──────┬──────┬────────┬───────┘
     │      │      │      │      │      │        │
     ▼      ▼      ▼      ▼      ▼      ▼        ▼
 Catalog Basket Order Inventory Payment Discount Notification
                        │
                   RabbitMQ
                (MassTransit)
```

## Services

| Service | Description | Tech |
|---|---|---|
| **Catalog** | Products, categories, image upload | EF Core · SQL Server · MinIO |
| **Basket** | Shopping cart | Redis |
| **Ordering** | Order lifecycle, CQRS | EF Core · SQL Server · MassTransit |
| **Inventory** | Stock management | EF Core · SQL Server |
| **Payment** | Stripe checkout | Stripe API · MassTransit |
| **Discount** | Coupon/promo codes | gRPC · MongoDB |
| **Notification** | Email notifications, SignalR | MassTransit · SignalR |
| **Gateway** | API Gateway, auth | YARP · Keycloak JWT |

## Tech Stack

### Backend
- **ASP.NET Core 10** — Minimal API, MediatR, CQRS
- **MassTransit + RabbitMQ** — event-driven communication between services
- **Entity Framework Core** — SQL Server, code-first migrations
- **gRPC** — inter-service communication (Discount)
- **SignalR** — real-time notifications
- **YARP** — reverse proxy API Gateway
- **Keycloak 25** — authentication (JWT, PKCE OAuth2, realm auto-import)
- **Stripe** — payment processing
- **MinIO** — S3-compatible object storage for product images
- **OpenTelemetry** — distributed tracing and metrics

### Frontend
- **Angular 21** — signal-based state, standalone components
- **keycloak-js** — PKCE authentication flow
- **Stripe Elements** — checkout UI
- **@microsoft/signalr** — real-time order updates

### Infrastructure
- **Docker** — multi-stage builds, build context at repo root (shared BuildingBlocks)
- **Kubernetes** — Docker Desktop (local), deployable to any cluster
- **Helm** — single generic chart (`cloudcart-service`) with per-service values files
- **ArgoCD** — GitOps continuous delivery with ApplicationSet
- **GitHub Actions** — CI/CD, matrix builds, ghcr.io container registry
- **Nginx Ingress** — ingress controller
- **Prometheus + Grafana + Loki** — observability stack

---

## Project Structure

```
cloudcart-microservices/
├── src/
│   ├── BuildingBlocks/          # Shared libraries (messaging, extensions)
│   ├── Gateway/
│   │   └── CloudCart.Gateway/  # YARP API Gateway
│   └── Services/
│       ├── Basket/
│       ├── Catalog/
│       ├── Discount/
│       ├── Inventory/
│       ├── Notification/
│       ├── Ordering/
│       └── Payment/
├── frontend/
│   └── cloudcart-angular/      # Angular 21 SPA
├── k8s/
│   ├── helm/
│   │   ├── cloudcart-service/  # Generic Helm chart
│   │   └── values/             # Per-service values files
│   └── infra/                  # Keycloak, ArgoCD manifests
└── .github/
    └── workflows/              # CI/CD pipelines
```

---

## Running Locally (Kubernetes)

### Prerequisites
- Docker Desktop with Kubernetes enabled
- `kubectl`, `helm`, `argocd` CLI
- Nginx Ingress Controller installed

### 1. Add hosts entries

```
127.0.0.1 cloudcart.local
127.0.0.1 keycloak.local
```

### 2. Deploy infrastructure (Keycloak, ArgoCD)

```bash
kubectl apply -f k8s/infra/
```

### 3. Install ArgoCD ApplicationSet

```bash
kubectl apply -f k8s/argocd/ --server-side
```

### 4. Port-forward services

```bash
# Frontend
kubectl port-forward svc/cloudcart-frontend -n cloudcart 4200:80

# Keycloak
kubectl port-forward svc/keycloak -n cloudcart 8080:8080

# ArgoCD
kubectl port-forward svc/argocd-server -n argocd 8443:443

# Ingress (port 80 occupied by Docker Desktop on Windows)
kubectl port-forward svc/ingress-nginx-controller -n ingress-nginx 8888:80
```

### 5. Access

| URL | Description |
|---|---|
| `http://localhost:4200` | Angular frontend |
| `http://localhost:8080/admin` | Keycloak admin (admin / admin123) |
| `https://localhost:8443` | ArgoCD UI |

> **Note:** Use `localhost` URLs for authentication. Keycloak PKCE requires HTTPS or `localhost` (Web Crypto API limitation).

---

## CI/CD

GitHub Actions builds and pushes Docker images to `ghcr.io/marokos999/cloudcart-*` on every push to `main`. ArgoCD automatically detects new images and syncs deployments.

```
push to main → GitHub Actions → build & push to ghcr.io → ArgoCD sync → K8s rollout
```

---

## Authentication Flow

1. Angular app initiates PKCE flow via `keycloak-js`
2. User authenticates against Keycloak (`cloudcart` realm)
3. Keycloak issues JWT access token
4. Angular attaches token to every API request
5. Gateway validates JWT and forwards to microservices

The `cloudcart` realm is auto-imported from a ConfigMap on Keycloak startup — no manual configuration needed.

---

## Key Design Decisions

- **Single generic Helm chart** — all services use `cloudcart-service` chart with per-service `values/` files; reduces duplication
- **Config over env vars** — complex config keys (e.g. YARP cluster routes) live in `appsettings.Development.json` since Kubernetes env var names cannot contain hyphens
- **CORS from config** — all services read allowed origins from `appsettings`, never hardcoded
- **MassTransit namespace-based queue naming** — prevents queue collisions between services
- **Build context at repo root** — required for .NET multi-stage builds to access shared `BuildingBlocks`
