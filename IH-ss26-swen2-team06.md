# IH SS26 SWEN2 – Team 06 Review

**Repository:** [drumnadrochit/IH-ss26swen2team6](https://github.com/drumnadrochit/IH-ss26swen2team6)
**Reviewer notes basis:** 3-tier architecture, SOLID principles (refactoring.guru), Must-Haves checklist.
**Tech actually used:** React 18 + TypeScript + Vite + Zustand + Leaflet (frontend), ASP.NET Core 9 + EF Core + PostgreSQL (backend).

---

## 1. Architecture & Design

### 3-Tier Architecture — ✅ (backend) / ⚠ (frontend)
The backend cleanly separates the three classic tiers in a layer-based ASP.NET Core solution:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Presentation / API | `TourPlanner.API` (Controllers, `Program.cs`) | HTTP, auth, DI wiring, image upload |
| Business Logic | `TourPlanner.BL` (Services, DTOs, `HttpClients/`) | Tour/TourLog/Auth/Route services, ORS integration |
| Data Access | `TourPlanner.DAL` (Entities, Repositories, EF `DbContext`, Migrations) | Persistence, repository pattern |
| Tests | `TourPlanner.Tests` | NUnit unit tests (per README: 22+) |

Dependency direction `API → BL → DAL` is respected (controllers never touch EF / repositories directly). 👍

The **frontend** does not implement a comparable layered split — see MVVM section below.

### SOLID — generally good on backend, weak on frontend

| Principle | Backend assessment | Issues |
|-----------|-------------------|--------|
| **S** – Single Responsibility | Mostly good. `TourService`, `TourLogService`, `AuthService`, `RouteService` each own one concern. | `ToursController.SaveImageAsync` does file I/O directly in the controller — that is an infrastructure concern and belongs to BL or a dedicated `IImageStorage` service. |
| **O** – Open/Closed | Generic `Repository<T>` + interfaces support extension without modification. | `ComputeChildFriendliness` is a hardcoded `"N/A"` stub — needs strategy/policy class once implemented. |
| **L** – Liskov | Interface implementations look substitutable. | n/a |
| **I** – Interface Segregation | `ITourService`, `ITourLogService`, `IOpenRouteServiceClient`, per-entity repository interfaces — good granularity. | n/a |
| **D** – Dependency Inversion | All services and repositories are injected via constructor; controllers depend on `ITourService` etc., not concretes. 👍 | `TourService` directly instantiates `staticmap.openstreetmap.de` URLs as strings — a small `IStaticMapUrlBuilder` would make this testable. |

Frontend SOLID concerns:
- `TourDetailPage.tsx` (~145 LOC) violates **SRP**: it owns tour CRUD, log CRUD, route fetching, three different edit modes (`editing`, `showLogForm`, `editingLog`), and presentation. Extract a `TourHeaderCard`, a `TourLogsSection`, and a small `useTourDetail` hook.
- Inline styles (`style={{ display: 'flex', ... }}`) are scattered throughout pages — couples presentation to components; move to CSS classes (you already have `index.css` utility classes — use them consistently).
- `any`-shaped error parsing repeated in every form:  
  `(err as { response?: { data?: { message?: string } } })?.response?.data?.message` — extract a shared `getApiErrorMessage(err)` helper (DRY + SRP).

### Refactoring.guru patterns observed
- **Repository pattern** (`IRepository<T>` / `Repository<T>`, plus `TourRepository`, `TourLogRepository`, `UserRepository`) — ✅
- **Dependency Injection** via the ASP.NET Core container — ✅
- **DTO pattern** (`TourPlanner.BL/DTOs`) separating wire models from entities — ✅
- **Adapter** for the ORS HTTP client (`IOpenRouteServiceClient`) — ✅
- Missing: no **Strategy** for `TransportType`-specific logic (currently flat `enum` + `switch`-style), no **Factory** for `Tour` creation, no **Unit of Work** wrapping the repositories (each repo calls `SaveChangesAsync` independently — risk of partial writes when creating a tour with logs).

---

## 2. Must-Haves — Compliance Matrix

| # | Requirement | Status | Evidence / Comment |
|---|-------------|--------|--------------------|
| **1** | **Uses Angular as frontend framework** | ❌ **NOT MET** | `frontend/package.json` declares `react ^18.3.1`, `react-dom`, `react-router-dom`, `@vitejs/plugin-react`. There is **no Angular** anywhere in the repo. This is a hard requirement for SWEN2 — **must be addressed before submission**. |
| **2** | **Uses MVVM for UI** | ❌ **NOT MET** | The chosen stack is React + Zustand stores (`tourStore.ts`, `tourLogStore.ts`, `authStore.ts`). This is a Flux-style unidirectional pattern, not MVVM. There are no ViewModels (no `*.viewmodel.ts`, no observable VMs, no two-way binding to a VM). Even if you keep React, you would have to introduce explicit ViewModel classes per view and bind UI ↔ VM properties — currently components hold their own `useState` and read directly from stores. |
| **3** | GUI in general | ✅ | App boots, has navbar, routing (`App.tsx`), login/register, tour list, tour detail. |
| **4** | Correct data binding between UI elements and view model properties | ⚠ | React controlled inputs (`value={…} onChange={…}`) work, but there is no VM — state lives in `useState` inside components or in Zustand stores. Strictly per MVVM this is **not** “VM property binding”. |
| **5** | UI responds to window size changes | ✅ (basic) | `index.css` has two media queries (`max-width: 640px`, `max-width: 480px`) handling `.page`, `.grid-2col`, `.stack-mobile`, iOS-zoom prevention. Reasonable but minimal — the inline `style={{ gridTemplateColumns: '1fr 1fr' }}` in forms bypasses the responsive class and stays 2-column on mobile (override only applies because of `!important` in `.grid-2col`). Verify visually. |
| **6** | Defines reusable UI Component | ✅ | `components/common/` (`Navbar`, `ProtectedRoute`, `LoadingSpinner`), `tours/TourCard`, `tours/TourForm`, `tours/TourList`, `tours/TourSearch`, `tourLogs/TourLogCard/Form/List`, `map/RouteMap`. Good componentisation. |
| **7** | Create / modify / delete tour | ✅ | `ToursController` (POST/PUT/DELETE), `TourService.{Create,Update,Delete}TourAsync`, frontend `tourStore.{addTour,editTour,removeTour}`, `TourForm`, delete button in `TourDetailPage`. |
| **8** | Tours have required attributes (incl. Image) and are managed in a list view | ✅ | `Tour` entity carries `Name, Description, From, To, TransportType, Distance, EstimatedTime, RouteImagePath, CreatedAt, UpdatedAt`. Image upload is supported (`multipart/form-data` in controller, `SaveImageAsync`, `RouteImagePath`). `ToursPage` + `TourList` + `TourCard` provide the list view. |
| **9** | Tour Details show all tour attributes of a selected tour and a map-placeholder | ✅ | `TourDetailPage.tsx` renders name, badge(transportType), from→to, description, image, distance, estimatedTime, popularity, childFriendliness, plus `<RouteMap />` (Leaflet) — more than a placeholder, actual map. |
| **10** | Validates user-input (no crash on wrong input) | ⚠ | Client: `TourForm` checks required fields; server: `TourService.CreateTourAsync` throws `ArgumentException` → `400` in controller — good. But: no length limits, no URL/coordinate sanity, no image MIME/size validation (`SaveImageAsync` accepts any file). Add `[DataAnnotations]` or FluentValidation. |
| **11** | Create / modify / delete tour log | ✅ | `TourLogsController`, `TourLogService`, `tourLogStore.{addLog,editLog,removeLog}`, `TourLogForm`. |
| **12** | Tour log has required attributes | ✅ | `TourLog` entity: `DateTime, Comment, Difficulty, TotalDistance, TotalTime, Rating, CreatedAt`. Matches the typical assignment spec. |
| **13** | Tour Logs showing all logs of a selected tour with all log attributes in a list view | ✅ | `TourDetailPage` renders `<TourLogList logs={logs} … />` with edit/delete callbacks. |
| **14** | Validates user-input (no crash on wrong input) | ✅ | `TourLogForm` validates: `dateTime`, `difficulty 1–5`, `rating 1–5`, `totalDistance > 0`, `totalTime > 0`. Errors shown via `<p className="error">`. Good. Backend ranges should mirror these (currently `Difficulty`/`Rating` are plain `int` with no `Range` attribute). |
| **15** | Protocol — describes UX (incl. wireframes) | ❌ **MISSING** | Repository contains only `README.md`, no `docs/`, no `Protocol.*`, no wireframes (PNG/PDF), no UX description. Required deliverable not in repo. |

---

## 3. Strengths

- Clean 3-project backend solution following classic layering (API/BL/DAL/Tests).
- Repository pattern + DI + DTOs + service interfaces — testable architecture.
- Real map integration with OpenRouteService + Leaflet (not just a placeholder).
- Image upload end-to-end working (multipart → file storage → `RouteImagePath`).
- JWT auth + protected routes on both ends (`ProtectedRoute.tsx`, `[Authorize]` controllers).
- Per-user isolation enforced in services (`tour.UserId != userId → Forbid`).
- Docker Compose for the full stack (postgres + backend + frontend + nginx).
- log4net configured, info/warn logging on service operations.
- Decent responsive baseline + reusable components folder structure.

## 4. Findings to Fix (priority order)

### 🔴 Blockers — assignment compliance
1. **Frontend framework**: Replace / re-do the frontend in **Angular** (the brief requires it). React + Zustand will not satisfy the must-have, regardless of feature completeness.
2. **MVVM**: Introduce explicit ViewModels per view (e.g. `TourListViewModel`, `TourDetailViewModel`, `TourLogFormViewModel`) with observable properties and commands; bind the Angular template to VM properties (two-way `[(ngModel)]`, async pipe on observables). Components/templates = View; services in `@Injectable()` = Model layer.
3. **Protocol / wireframes**: Add a `docs/Protocol.md` (or PDF) under version control that contains:
   - Use case overview
   - Wireframes per screen (Tours list, Tour detail, Tour log create/edit, Login)
   - UX flow diagram
   - Architecture diagram (3-tier)

### 🟠 Strongly recommended
4. **`ToursController.SaveImageAsync`** — move file persistence into BL (`IImageStorage`) so the controller is thin and the logic is unit-testable.
5. **No Unit of Work** — wrap multi-repository operations (e.g. delete tour + cascade logs) in a single transaction. Today each repository calls `SaveChangesAsync` itself.
6. **Backend input validation** — annotate DTOs with `[Required]`, `[Range(1,5)]` for `Difficulty`/`Rating`, `[StringLength]` for `Name/Description`, validate uploaded image MIME and size (currently arbitrary files can be uploaded under arbitrary extensions).
7. **`ComputeChildFriendliness` returns `"N/A"`** — implement using `TourLog.Difficulty` aggregates, or remove from the response until implemented (currently misleading).
8. **Secrets in `docker-compose.yml`** — `JwtSettings__SecretKey` default `"change-me-in-production-min-32-chars!!"` and a hardcoded ORS API key are committed. Move to a `.env` (already have `.env.example` on the FE side; mirror it for backend) and reference with `${VAR}` only — never commit a working key.
9. **No backend tests of the BL services visible in the listing** — verify that the README claim of “22+ NUnit tests” covers Services and not just trivial entity tests. Tests should exercise: validation paths, `Unauthorized` branches, route fetch failure (`FetchRouteDataAsync` swallows exceptions silently — assert that the entity is still created with `Distance=0`).
10. **CSS strategy** — replace ad-hoc inline `style={{}}` blocks with classes; consider CSS Modules / SCSS partials per component, especially once you migrate to Angular (use component-scoped styles).

### 🟡 Nice-to-have
11. `TourDetailPage` is too long — split as suggested above.
12. Extract `getApiErrorMessage(err)` helper to remove duplicated `unknown`-cast error handling in every form.
13. Add OpenAPI/Swagger DTO examples; you already enable Swagger but `multipart/form-data` parameters are listed as loose `[FromForm]` primitives, which is hard to consume.
14. Confirmation dialogs use the native `confirm(...)` — replace with a reusable `ConfirmDialog` component (also helps the “Defines reusable UI Component” criterion strongly).
15. README mentions “React 18 … Zustand … React Router … Leaflet” — this directly contradicts the assignment must-have; update the README **after** the Angular/MVVM migration.

---

## 5. Summary Score (informal)

| Area | Score | Comment |
|------|-------|---------|
| 3-tier architecture (backend) | 9/10 | Textbook layering, good DI, repository pattern |
| SOLID adherence | 7/10 | Solid on backend; frontend has SRP/DRY issues |
| Frontend framework requirement | **0/10** | Wrong stack — Angular is mandatory |
| MVVM | **0/10** | Not implemented (Flux/store instead) |
| Tour feature coverage | 9/10 | CRUD, image, list, detail, validation all present |
| Tour log feature coverage | 9/10 | CRUD, validation, list inside detail page |
| Responsiveness | 6/10 | Basic media queries, partially bypassed by inline styles |
| Reusable components | 8/10 | Good folder split, room to add `ConfirmDialog`, `FormField` |
| Protocol/UX docs | **0/10** | Not present in repo |
| Code quality / readability | 7/10 | Clear naming, but inline styles & long pages hurt FE |

**Overall:** The backend is in good shape and demonstrates understanding of 3-tier + SOLID + common GoF/enterprise patterns. **However, the project as submitted cannot pass the SWEN2 must-have gate** because (a) the frontend is React, not Angular, (b) there is no MVVM, and (c) the Protocol/UX documentation with wireframes is missing. Address these three items first; the rest of the feedback can be done as polish on top.
