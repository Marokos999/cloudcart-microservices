<div align="center">

# 🛒 CloudCart

**Production-grade e-commerce platform built with .NET 10 microservices**

[![CI](https://github.com/Marokos999/cloudcart-microservices/actions/workflows/ci.yml/badge.svg)](https://github.com/Marokos999/cloudcart-microservices/actions)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)
![Kubernetes](https://img.shields.io/badge/Kubernetes-K8s-326CE5?logo=kubernetes)
![ArgoCD](https://img.shields.io/badge/ArgoCD-GitOps-EF7B4D?logo=argo)
![License](https://img.shields.io/badge/license-MIT-green)

*8 microservices · Angular SPA · Kubernetes · ArgoCD GitOps · Keycloak · Stripe*

</div>

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

---

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

---

## Tech Stack

### Backend
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?logo=dotnet)
![MassTransit](https://img.shields.io/badge/MassTransit-RabbitMQ-FF6600)
![EF Core](https://img.shields.io/badge/EF_Core-SQL_Server-CC2927?logo=microsoftsqlserver)
![gRPC](https://img.shields.io/badge/gRPC-inter--service-244c5a)
![Keycloak](https://img.shields.io/badge/Keycloak-25-4D4D4D?logo=keycloak)
![Stripe](https://img.shields.io/badge/Stripe-Payments-635BFF?logo=stripe)
![MinIO](https://img.shields.io/badge/MinIO-Object_Storage-C72E49)
![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Tracing-425CC7)

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
![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript)
![Stripe](https://img.shields.io/badge/Stripe_Elements-checkout-635BFF?logo=stripe)

- **Angular 21** — signal-based state, standalone components
- **keycloak-js** — PKCE authentication flow
- **Stripe Elements** — checkout UI
- **@microsoft/signalr** — real-time order updates

### Infrastructure
![Docker](https://img.shields.io/badge/Docker-multi--stage-2496ED?logo=docker)
![Kubernetes](https://img.shields.io/badge/Kubernetes-local_+_cloud-326CE5?logo=kubernetes)
![Helm](https://img.shields.io/badge/Helm-generic_chart-0F1689?logo=helm)
![ArgoCD](https://img.shields.io/badge/ArgoCD-GitOps-EF7B4D?logo=argo)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI%2FCD-2088FF?logo=githubactions)
![Prometheus](https://img.shields.io/badge/Prometheus-Grafana-E6522C?logo=prometheus)

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

### 2. Deploy infrastructure

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

# Ingress (port 80 occupied by Docker Desktop on Windows — use 8888)
kubectl port-forward svc/ingress-nginx-controller -n ingress-nginx 8888:80
```

### 5. Access

| URL | Description |
|---|---|
| `http://localhost:4200` | Angular frontend |
| `http://localhost:8080/admin` | Keycloak admin (`admin` / `admin123`) |
| `https://localhost:8443` | ArgoCD UI |

> **Note:** Use `localhost` URLs for authentication. Keycloak PKCE requires HTTPS or `localhost` (Web Crypto API limitation — custom domains without HTTPS will block auth).

---

## CI/CD Pipeline

```
push to main
    │
    ▼
GitHub Actions (matrix build — one job per service)
    │
    ▼
ghcr.io/marokos999/cloudcart-{service}:latest
    │
    ▼
ArgoCD detects new image → syncs → kubectl rollout
```

---

## Authentication Flow

```
Angular → Keycloak (PKCE) → JWT → Gateway (validate) → Microservices
```

1. Angular initiates PKCE flow via `keycloak-js`
2. User authenticates against Keycloak (`cloudcart` realm)
3. Keycloak issues JWT access token
4. Angular attaches Bearer token to every API request
5. Gateway validates JWT, then forwards to the appropriate microservice

> The `cloudcart` realm is auto-imported from a ConfigMap on Keycloak startup — no manual setup required.

---

## Key Design Decisions

| Decision | Why |
|---|---|
| Single generic Helm chart | All services share one chart; per-service `values/` reduces duplication |
| Config over K8s env vars | K8s env var names can't contain hyphens — YARP cluster config lives in `appsettings` |
| CORS from config | Never hardcoded — different origins per environment without rebuilds |
| MassTransit namespace queues | `includeNamespace: true` prevents silent queue collisions between services |
| Build context at repo root | Required for .NET multi-stage builds to access shared `BuildingBlocks` |
| `imagePullPolicy: Always` | Ensures latest image is pulled after every ArgoCD sync |
