# 💬 ChatterPro – Application de messagerie

![Docker](https://img.shields.io/badge/containerized-Docker-blue?logo=docker)
![Architecture](https://img.shields.io/badge/architecture-Microservices-ff69b4)
![Backend](https://img.shields.io/badge/backend-.NET%208-512BD4?logo=dotnet)
![Frontend](https://img.shields.io/badge/frontend-React%2018%20%2B%20Vite-61DAFB?logo=react)
![Realtime](https://img.shields.io/badge/realtime-SignalR-0078D4)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

---

## 🚀 Présentation

**ChatterPro** est une application de messagerie distribuée bâtie sur une architecture
**microservices .NET 8** entièrement conteneurisée. Elle démontre une chaîne complète :
authentification par **JWT**, messagerie de groupe en **temps réel via SignalR**,
communication inter-services **asynchrone via RabbitMQ**, et un **frontend React** moderne.

Points clés :

- 🔄 Messagerie de groupe quasi temps réel (WebSocket SignalR)
- 🧩 Découplage des services derrière une **API Gateway Ocelot**
- 📦 Déploiement conteneurisé avec **Docker Compose**
- 🧵 Communication événementielle asynchrone entre services (**RabbitMQ**)
- 🗄️ Persistance **MySQL** (une base par service)

> ℹ️ **Projet portfolio** : l'objectif est d'illustrer des compétences backend/.NET et
> l'intégration React/SignalR. Certaines fonctionnalités « entreprise » (refresh token,
> tests exhaustifs, orchestration Kubernetes) sont volontairement hors périmètre.

---

## ✨ Fonctionnalités

### 🔐 Authentification
- 🔑 Émission de **JWT** (HMAC-SHA256, durée de vie **1 h**) à la connexion et à l'inscription
- 🛡️ Hachage des mots de passe avec **Argon2** (`Isopoh.Cryptography.Argon2`, Argon2i, 64 Mo, sel aléatoire)
- 🧼 Validation des entrées via *DataAnnotations* (email valide, mot de passe 5–50 caractères, pseudo 3–50)
- 🚪 Validation du JWT centralisée à l'**API Gateway** (issuer / audience / signature / expiration)

### 💬 Messagerie & conversations de groupe
- ✉️ Envoi de **messages texte** en temps réel via **SignalR** (Hub `ChatHub`)
- 👥 **Conversations de groupe** avec **code d'invitation** (join code) pour rejoindre un groupe
- 🗄️ Persistance des messages et conversations dans **MySQL**
- 🧵 Événements inter-services **asynchrones** via **RabbitMQ**

### 👤 Gestion des utilisateurs
- 🧑‍💼 Données utilisateur (profil, contacts) via le **UserService** dédié
- 📡 Hub SignalR `UserHub` pour la remontée d'utilisateurs (événement `ReceiveUsers`)

### 📤 Exportation de données
- 📦 Formats **JSON**, **CSV**, **PDF** (iText7, CsvHelper) via le **DataExportService**

### 🔔 Notifications
- ⚙️ Déclenchées sur événements système, transmises en **temps réel** via SignalR (`NotificationHubs`)

### 🖥️ Frontend React
- ⚛️ SPA **React 18 + Vite** (JavaScript), **Tailwind CSS** (UI moderne, fond blanc)
- 🔐 Authentification (login / signup) avec session persistée et **JWT porté sur toutes les requêtes**
- 💬 Chat de groupe temps réel (création de conversation, join par code, historique, envoi)
- ✏️ **Édition & suppression** de ses propres messages
- 📤 **Export** d'une conversation en **PDF / CSV / JSON** (DataExportService)
- 🔔 **Notifications** (cloche + badge, near real-time par polling REST)
- 👤 **Profil utilisateur** (voir/mettre à jour) et recherche d'un contact par pseudo
- 📁 Code dans [`./frontend`](./frontend) — voir son [README](./frontend/README.md)

---

## 📐 Architecture technique

| Composant             | Rôle                                                                       |
|-----------------------|----------------------------------------------------------------------------|
| **Gateway (Ocelot)**  | Point d'entrée unique REST, reverse proxy, validation du JWT               |
| **AuthService**       | Authentification, émission des JWT, hachage Argon2                         |
| **UserService**       | Données utilisateur (profil, contacts) — Hub `UserHub`                     |
| **MessageService**    | Messages & conversations, persistance — Hub `ChatHub` (temps réel)        |
| **NotificationService** | Notifications asynchrones — Hub `NotificationHubs`                        |
| **DataExportService** | Exportation de données (PDF, CSV, JSON)                                    |
| **Frontend (React)**  | Interface web (auth + chat de groupe) servie par NGINX                     |

Chaque microservice possède sa **propre base MySQL** ; les services communiquent de manière
asynchrone via **RabbitMQ**. Le tout est orchestré par **Docker Compose**.

---

## 🧰 Technologies utilisées

| Frontend                         | Backend / Services                        | Infrastructure                     |
|----------------------------------|-------------------------------------------|------------------------------------|
| React 18, Vite                   | ASP.NET Core (.NET 8)                      | Docker, Docker Compose             |
| Tailwind CSS                     | ASP.NET Core SignalR                       | API Gateway **Ocelot**             |
| `@microsoft/signalr`             | JWT (System.IdentityModel.Tokens.Jwt)     | **NGINX** (sert le build frontend) |
| React Router                     | **Argon2** (Isopoh.Cryptography.Argon2)   | RabbitMQ (message broker)          |
| Fetch API                        | EF Core + MySQL                           | MySQL 8 (une base par service)     |
|                                  | iText7, CsvHelper (export)                | Architecture event-driven          |

---

## 🗃️ Modèle de données (backend .NET)

Le modèle conceptuel (Utilisateur, Message, Conversation, Notification, etc.) est illustré ici :

![Modèle de données](https://github.com/AlphaEngineer54/messaging-app/blob/main/entities_model.png)

---

# 🌐 API Gateway – Documentation des routes

Les appels **REST** du frontend transitent par l'**API Gateway** (`http://localhost:5000`),
qui réécrit le chemin (préfixe retiré, `/api` ajouté) et redirige vers le microservice cible.

| Méthode(s)            | Route Gateway (frontend)      | Cible backend                                    | Auth (Bearer JWT) |
|-----------------------|-------------------------------|--------------------------------------------------|-------------------|
| POST, GET             | `/auth/{everything}`          | `authservice:5001/api/auth/{everything}`         | ❌ Non             |
| GET, POST, DELETE     | `/user/{everything}`          | `userservice:5002/api/user/{everything}`         | ✅ Oui             |
| GET, POST, DELETE     | `/message/{everything}`       | `messageservice:5003/api/message/{everything}`   | ✅ Oui             |
| GET, POST             | `/message`                    | `messageservice:5003/api/message`                | ✅ Oui             |
| GET, POST, DELETE     | `/conversation/{everything}`  | `messageservice:5003/api/conversation/{...}`     | ✅ Oui             |
| GET, POST             | `/conversation`               | `messageservice:5003/api/conversation`           | ✅ Oui             |
| GET, POST             | `/dataexport/{everything}`    | `dataexportservice:5004/api/dataexport/{...}`    | ✅ Oui             |
| GET, POST, DELETE     | `/notification/{everything}`  | `notificationservice:5005/api/notification/{...}`| ✅ Oui             |

**Notes importantes :**

- 🔑 Toutes les routes **sauf `/auth/*`** exigent l'en-tête `Authorization: Bearer <jwtToken>`.
  Le JWT est **validé au niveau de la gateway** (issuer `http://localhost:5000`, audience
  `messaging_api`, signature via `JWT_SECRET`).
- ➕ Les routes racines `/message` et `/conversation` (sans sous-segment) permettent la
  **création** et la **liste globale** — le motif catch-all `/{everything}` seul ne les couvre pas.
- 🔁 Le verbe **`PUT`** est routé pour `/message/{id}` et `/user/{id}` (édition de message et mise
  à jour de profil). Un verbe non-standard `UPDATE` subsiste dans la config d'origine mais n'est
  pas utilisé par le frontend.

---

# 📡 Temps réel (SignalR)

> ⚠️ **Les WebSockets SignalR ne passent pas par la gateway Ocelot** : le client se connecte
> **directement** au microservice concerné.

| Service              | URL du Hub (directe)                 | Événements serveur → client                                   |
|----------------------|--------------------------------------|---------------------------------------------------------------|
| MessageService       | `http://localhost:5003/hubs/chat`    | `ReceiveMessage`, `JoinedGroup`, `ConnectedToGroup`, `Error`, `ValidationError` |
| NotificationService  | `http://localhost:5005/notifications`| `ReceiveNotification`                                          |
| UserService          | `http://localhost:5002/userHub`      | `ReceiveUsers`                                                 |

### Méthodes du Hub de chat (`ChatHub`)

| Méthode (client → serveur)      | Description                                                                       |
|---------------------------------|-----------------------------------------------------------------------------------|
| `SendMessage(dto)`              | Diffuse un message à **tous les clients connectés** (`Clients.All`).              |
| `SendMessageToUser(dto)`        | Envoie à **un utilisateur** (`ReceiverId`). *(nécessite un `IUserIdProvider`, non configuré)* |
| `SendMessageToGroup(dto)`       | Diffuse à tous les membres d'une **conversation** (`ConversationId`).             |
| `JoinGroup(dto)`                | Rejoint une conversation via son **`JoinCode`** ; répond `JoinedGroup` ou `Error`. |
| `ConnectToGroup(conversationId)`| Rejoint le groupe temps réel d'une conversation par son **id** ; répond `ConnectedToGroup`. |

`dto` (`NewMessageDTO`) : `{ content (≤250), status: "sent"|"delivered"|"read", senderId, receiverId, conversationId }`.
En envoi de groupe, `receiverId` est requis par le DTO (placeholder `0`).

> Les DTO sont validés côté serveur ; en cas d'erreur, un événement `ValidationError`
> (tableau de messages) est renvoyé à l'appelant.

---

## 📦 Démarrage local

### Prérequis
- Docker & Docker Compose
- Node.js 20+ (pour lancer le frontend en mode développement)

### 1. Configuration (`.env`)
Le `docker-compose.yaml` s'appuie sur des variables d'environnement (ports, identifiants,
`JWT_SECRET`, chaînes de connexion). Créez un fichier `.env` à la racine `ChatterPro/` avec
notamment :

```env
GATEWAY_PORT=5000
AUTHSERVICE_PORT=5001
USERSERVICE_PORT=5002
MESSAGESERVICE_PORT=5003
DATAEXPORT_PORT=5004
NOTIFSERVICE_PORT=5005

JWT_SECRET=<une_clé_secrète_longue_et_identique_partout>
# + identifiants MySQL / RabbitMQ et chaînes de connexion (voir docker-compose.yaml)
```

> 🔐 `JWT_SECRET` **doit être identique** pour l'AuthService (émission) et la Gateway
> (validation), sinon tous les appels authentifiés échoueront en 401.

### 2. Backend
```bash
git clone https://github.com/AlphaEngineer54/ChatterPro.git
cd ChatterPro
docker-compose up -d --build
```

### 3. Frontend
```bash
cd frontend
npm install
npm run dev      # http://localhost:3000
```

> ⚠️ Le dev server **doit** rester sur le **port 3000** : c'est la seule origine autorisée
> par le CORS de la Gateway et du MessageService (`WithOrigins("http://localhost:3000")`).

Le frontend peut aussi être conteneurisé via son `Dockerfile` (build Vite → NGINX) :
```bash
cd frontend
docker build -t chatterpro-frontend .
docker run -p 3000:3000 chatterpro-frontend
```

---

## 🐳 Exécuter depuis Docker Hub (images pré-construites)

Le pipeline CI/CD publie automatiquement une image par service sur le dépôt Docker Hub
**public [`dev329`](https://hub.docker.com/u/dev329)** à chaque push sur `dev`/`main`. Cette
méthode **ne compile rien localement** : on récupère les images et on lance toute la stack
(services, bases MySQL, RabbitMQ, gateway, frontend) avec Docker Compose.

### Images publiées (dépôt public `dev329`)
| Image Docker Hub                          | Rôle                     | Port |
|-------------------------------------------|--------------------------|------|
| `dev329/chatterpro-gateway:latest`        | API Gateway Ocelot       | 5000 |
| `dev329/chatterpro-authservice:latest`    | Authentification / JWT   | 5001 |
| `dev329/chatterpro-userservice:latest`    | Utilisateurs             | 5002 |
| `dev329/chatterpro-messageservice:latest` | Messages + Hub SignalR   | 5003 |
| `dev329/chatterpro-dataexportservice:latest` | Export PDF/CSV/JSON   | 5004 |
| `dev329/chatterpro-notificationservice:latest` | Notifications       | 5005 |
| `dev329/chatterpro-frontend:latest`       | Frontend React (NGINX)   | 3000 |

> Le dépôt est **public** : aucun `docker login` n'est nécessaire pour tirer ces images.
> MySQL (`mysql:8`) et RabbitMQ (`rabbitmq:3-management`) proviennent des images officielles.

### Prérequis
- Docker + Docker Compose v2 (`docker compose`)
- Les fichiers `docker-compose.yaml` et `.env.example` (à la racine du dépôt)

### 1. Préparer le `.env`
Le compose utilise déjà `dev329` par défaut ; il suffit de fournir les autres variables
(ports, `JWT_SECRET`, identifiants MySQL/RabbitMQ) :
```bash
cp .env.example .env
# éditez .env : JWT_SECRET, identifiants MySQL/RabbitMQ, ports…
# (DOCKER_USERNAME est optionnel — laissé vide, les images dev329/* sont utilisées)
```

### 2. Tirer les images depuis Docker Hub
```bash
docker compose pull
```
Cela télécharge les 7 images applicatives `dev329/*` + MySQL + RabbitMQ (aucune compilation locale).

### 3. Créer et démarrer les conteneurs
```bash
docker compose up -d --no-build      # démarre à partir des images tirées, en arrière-plan
```
> `--no-build` garantit l'utilisation des images Docker Hub (et non un build local).

### 4. Vérifier l'état (healthchecks)
```bash
docker compose ps          # statut + colonne "health" de chaque service
docker compose logs -f gateway   # suivre les logs d'un service
```
Attendez que les services passent `healthy` (les bases MySQL mettent quelques secondes à s'initialiser).

### 5. Accéder à l'application
- Frontend : <http://localhost:3000>
- API Gateway : <http://localhost:5000>

### 6. Arrêter / nettoyer
```bash
docker compose down          # arrête et supprime les conteneurs
docker compose down -v       # + supprime les volumes (efface les données MySQL)
```

### (Optionnel) Tirer / lancer une seule image
```bash
docker pull dev329/chatterpro-frontend:latest
docker run -p 3000:3000 dev329/chatterpro-frontend:latest   # frontend statique autonome
```
> Les microservices ont besoin de leurs bases et de RabbitMQ : préférez `docker compose`
> pour un lancement complet plutôt que des `docker run` isolés.

---

## 🧭 Parcours de démonstration

1. Créez un compte (`/signup`) → redirection vers `/chat`.
2. « **+ Nouvelle** » → un **code d'invitation** s'affiche (copiable).
3. Dans un **second navigateur**, créez un autre compte, puis « **Rejoindre** » avec ce code.
4. Ouvrez la conversation des deux côtés : les messages arrivent **en temps réel**.
5. Survolez un de vos messages → **Éditer** / **Supprimer**.
6. En-tête de conversation → **Exporter** en PDF / CSV / JSON.
7. Barre latérale → **🔔** notifications, **👤** profil (mise à jour + recherche d'un contact).
8. Rechargez la page → la session est conservée et l'historique est rechargé.

---

## 🛣️ Feuille de route

Fonctionnalités **non encore implémentées** (transparence pour les relecteurs) :

- 🔁 **Refresh token** (actuellement le JWT expire après 1 h sans renouvellement automatique)
- 🧑‍🤝‍🧑 Messagerie 1-à-1 et **push de notifications temps réel** (nécessitent un `IUserIdProvider`
  côté SignalR ; les notifications fonctionnent aujourd'hui par **polling REST**)
- 🔀 Propagation temps réel des éditions/suppressions de messages (le hub ne diffuse pas ces
  événements ; l'affichage est mis à jour localement côté auteur)
- 🔀 Routage des WebSockets au travers de la Gateway
- 👥 Endpoint de **liste de contacts** (le UserService n'expose que la recherche par id/pseudo)
- ☸️ Orchestration Kubernetes

---

## 📄 Licence

Distribué sous licence **MIT**.
