# Velcro — Gestionnaire de tâches collaboratif

Application web de type Kanban (inspirée de Trello) développée avec **ASP.NET Core 10 MVC**, **SQLite/EF Core**, **JWT** et **SignalR**.

## Stack technique

| Couche | Technologie |
|---|---|
| Backend | ASP.NET Core 10.0 MVC |
| Base de données | SQLite via Entity Framework Core 9 |
| Authentification | JWT Bearer (access 30 min + refresh 7 jours) |
| Temps réel | SignalR |
| Validation | FluentValidation 11 |
| Hachage | BCrypt.Net-Next |
| Documentation API | Swagger (Swashbuckle 10) |
| Frontend | Bootstrap 5, Vanilla JS, HTML5 Drag & Drop |

## Fonctionnalités

- **Authentification** : inscription, connexion, rotation des refresh tokens
- **Workspaces** : espaces de travail multi-membres avec rôles (Owner / Admin / Member)
- **Boards** : tableaux Kanban avec couleur de fond personnalisable
- **Listes** : colonnes ordonnées, réordonnables
- **Cartes** : titre, description, date d'échéance, archivage, déplacement entre listes
- **Commentaires** : ajout/modification/suppression par l'auteur
- **Temps réel** : toutes les mutations sont broadcastées via SignalR aux membres connectés

## Lancer le projet

### Prérequis

- .NET 10 SDK
- Git

### Installation

```bash
git clone https://github.com/KHMalik03/Velcro.git
cd Velcro
dotnet restore
dotnet run
```

L'application démarre sur `http://localhost:5265`.  
Les migrations SQLite sont appliquées automatiquement au démarrage en mode `Development`.

### Swagger

Disponible sur `http://localhost:5265/swagger` en mode développement.

## Architecture

```
velcro/
├── Controllers/          # API controllers ([ApiController]) + MVC controllers (View)
│   ├── AuthController    → /api/auth/*
│   ├── WorkspaceController → /api/workspaces/*
│   ├── BoardController   → /api/boards/*
│   ├── ListController    → /api/lists/*
│   ├── CardController    → /api/cards/*
│   ├── HomeController    → pages MVC (dashboard, board view)
│   └── AccountController → pages MVC (login, register)
├── Services/             # Logique métier + interfaces
├── Models/
│   ├── Entities/         # Entités EF Core
│   └── DTOs/             # Records request/response
├── Data/
│   └── ApplicationDbContext.cs
├── Hubs/
│   └── BoardHub.cs       # SignalR hub
├── Middleware/
│   └── ExceptionMiddleware.cs
├── Validators/           # FluentValidation validators
└── wwwroot/
    ├── js/               # api.js, dashboard.js, board.js
    └── css/              # board.css
```

## API — Endpoints principaux

### Auth
| Méthode | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Créer un compte |
| POST | `/api/auth/login` | Se connecter (retourne access + refresh token) |
| POST | `/api/auth/refresh` | Renouveler le token |
| POST | `/api/auth/logout` | Révoquer le refresh token |
| GET  | `/api/auth/me` | Profil de l'utilisateur connecté |

### Workspaces
| Méthode | Route | Description |
|---|---|---|
| GET | `/api/workspaces` | Lister mes workspaces |
| POST | `/api/workspaces` | Créer un workspace |
| PUT | `/api/workspaces/{id}` | Modifier |
| DELETE | `/api/workspaces/{id}` | Supprimer |

### Boards
| Méthode | Route | Description |
|---|---|---|
| GET | `/api/boards/workspace/{wsId}` | Boards d'un workspace |
| GET | `/api/boards/{id}` | Détail d'un board |
| GET | `/api/boards/{id}/lists` | Listes + cartes d'un board |
| POST | `/api/boards` | Créer un board |
| PUT | `/api/boards/{id}` | Modifier |
| DELETE | `/api/boards/{id}` | Supprimer |

### Lists
| Méthode | Route | Description |
|---|---|---|
| POST | `/api/lists` | Créer une liste |
| PUT | `/api/lists/{id}` | Modifier |
| DELETE | `/api/lists/{id}` | Supprimer |
| PUT | `/api/lists/reorder/{boardId}` | Réordonner les listes |

### Cards
| Méthode | Route | Description |
|---|---|---|
| GET | `/api/cards/{id}` | Détail d'une carte |
| POST | `/api/cards` | Créer une carte |
| PUT | `/api/cards/{id}` | Modifier |
| DELETE | `/api/cards/{id}` | Supprimer |
| PATCH | `/api/cards/{id}/move` | Déplacer vers une autre liste |
| POST | `/api/cards/{cardId}/comments` | Ajouter un commentaire |
| PUT | `/api/cards/{cardId}/comments/{id}` | Modifier un commentaire |
| DELETE | `/api/cards/{cardId}/comments/{id}` | Supprimer un commentaire |

## SignalR

Connexion : `ws://localhost:5265/hubs/board?access_token=<jwt>`

| Événement (serveur → client) | Payload |
|---|---|
| `CardCreated` | `CardDetailDto` |
| `CardUpdated` | `CardDetailDto` |
| `CardDeleted` | `Guid` (cardId) |
| `CardMoved` | `CardDetailDto` |
| `ListCreated` | `ListDto` |
| `ListUpdated` | `ListDto` |
| `ListDeleted` | `Guid` (listId) |
| `ListsReordered` | `ListPositionDto[]` |
| `BoardUpdated` | `BoardDto` |
| `BoardDeleted` | `Guid` (boardId) |
| `CommentAdded` | `CommentDto` |
| `CommentUpdated` | `CommentDto` |
| `CommentDeleted` | `{ cardId, commentId }` |

## Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=taskboard.db"
  },
  "Jwt": {
    "Secret": "<clé-secrète-min-32-chars>",
    "Issuer": "TaskBoard",
    "Audience": "TaskBoardUsers",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  }
}
```

## Modèle de données

```
User ──< WorkspaceMember >── Workspace ──< Board ──< List ──< Card
                                                 └──< BoardMember      └──< Comment
                                                 └──< Label             └──< Checklist ──< ChecklistItem
                                                                        └──< CardLabel
                                                                        └──< CardMember
```
