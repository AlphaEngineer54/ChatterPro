# 📨 Distributed Messaging App – Microservices Architecture

Une application de messagerie distribuée avec une architecture orientée microservices et une communication asynchrone entre services. Le frontend est développé en WPF (.NET).

![Docker](https://img.shields.io/badge/containerized-Docker-blue?logo=docker)
![Architecture](https://img.shields.io/badge/architecture-Microservices-ff69b4)
![Status](https://img.shields.io/badge/status-En%20développement-yellow)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

---

## 🚀 Présentation

Application de messagerie quasi temps réel avec conteneurisation complète et scalabilité assurée.

---

## 📐 Architecture Technique

| Service               | Rôle                                                               |
|------------------------|--------------------------------------------------------------------|
| AuthService           | Authentification, JWT, autorisations                              |
| UserService           | Données utilisateur (profil, contacts)                            |
| MessageService        | Envoi, réception, persistance des messages ainsi que des conversations                      |
| NotificationService   | Push/email/système de notification asynchrone                     |
| DataExportService     | Exportation de données au format PDF, CSV, JSON                   |    
| Gateway (Ocelot)      | Point d’entrée unique pour tous les services (reverse proxy)      |

---

## 🧰 Technologies utilisées

| Côté Client         | Backend / Services                   | Infrastructure          |
|---------------------|------------------------------------|------------------------|
| WPF (.NET)          | ASP.NET Core (.NET 8), JWT, Argon2 | Docker, Docker Compose |
| REST HTTP Client    | MySQL, RabbitMQ                    | NGINX (reverse proxy)  |
|                     | iText7, CsvHelper                  | Kubernetes (à venir)   |
|                     | .NET Logging, CORS                 | Event-driven architecture |

---

## 🗃️ Modèle de Données (Backend .NET)

La capture suivante présente la structure conceptuelle du modèle de données, utilisée principalement par les services `UserService`, `MessageService` et `NotificationService`.

![Modèle de données](https://github.com/AlphaEngineer54/messaging-app/blob/main/entites_model.png)

> *Le modèle est représenté sous forme d'un diagramme de classes ou d'entités-relation (selon l’outil utilisé), illustrant les relations clés entre les entités : Utilisateur, Message, Conversation, Notification, etc.*

---

# 🌐 API Gateway – Documentation des Routes

Toutes les requêtes frontend doivent transiter par l’API Gateway (`http://localhost:5000`).  
Le gateway redirige vers les microservices locaux selon les routes définies ci-dessous.

---

## AuthService

| Méthode HTTP | Route Frontend           | Route Backend                   | Authentification requise |
|--------------|-------------------------|--------------------------------|--------------------------|
| GET, POST    | `/auth/{everything}`    | `http://localhost:5001/api/auth/{everything}` | Non                      |

---

## UserService

| Méthode HTTP             | Route Frontend           | Route Backend                   | Authentification requise |
|-------------------------|-------------------------|--------------------------------|--------------------------|
| GET, POST, DELETE, PUT  | `/user/{everything}`    | `http://localhost:5002/api/user/{everything}` | Oui (Bearer JWT)          |

---

## MessageService

| Méthode HTTP             | Route Frontend            | Route Backend                    | Authentification requise |
|-------------------------|--------------------------|---------------------------------|--------------------------|
| GET, POST, DELETE, PUT  | `/message/{everything}`  | `http://localhost:5003/api/message/{everything}`  | Oui (Bearer JWT)          |
| GET, POST, DELETE       | `/conversation/{everything}` | `http://localhost:5003/api/conversation/{everything}` | Oui (Bearer JWT)          |

> **Communication en temps réel via WebSocket avec SignalR disponible sur ce service.**

---

## DataExportService

| Méthode HTTP   | Route Frontend            | Route Backend                    | Authentification requise |
|---------------|--------------------------|---------------------------------|--------------------------|
| GET, POST     | `/dataexport/{everything}` | `http://localhost:5004/api/dataexport/{everything}` | Oui (Bearer JWT)          |

---

## NotificationService

| Méthode HTTP             | Route Frontend            | Route Backend                    | Authentification requise |
|-------------------------|--------------------------|---------------------------------|--------------------------|
| GET, POST, DELETE, PUT  | `/notification/{everything}` | `http://localhost:5005/api/notification/{everything}` | Oui (Bearer JWT)          |

> **Communication en temps réel via WebSocket avec SignalR disponible sur ce service.**

---

## Notes

- Le frontend doit toujours communiquer via l’API Gateway (`localhost:5000`).
- Les routes avec authentification exigent un token JWT valide dans l’en-tête `Authorization`.
- `{everything}` représente toute sous-route ou paramètre.
- Les services `MessageService` et `NotificationService` offrent une interface WebSocket basée sur SignalR pour la gestion temps réel des messages et notifications.

---

## 🧭 Fonctionnalités Clés

### 🔐 Authentification
- JWT, rafraîchissement de token
- Stockage sécurisé des mots de passe (Argon2)
- Validation et nettoyage des entrées

### 💬 Messagerie
- Messages texte
- Asynchrone avec file RabbitMQ
- Persistance dans MySQL
- Communication temps réel via SignalR WebSocket

### 📇 Gestion utilisateur
- Profil et contacts gérés via UserService

### 📤 Export de données
- JSON, CSV, PDF
- Téléchargement sécurisé

### 🔔 Notifications
- Événements déclencheurs
- Notification asynchrone
- Communication temps réel via SignalR WebSocket

### 🧱 Déploiement
- Docker multi-conteneur
- Orchestration via Docker Compose

---

## 📦 Démarrage local

### Prérequis
- Docker
- Docker Compose

### Commandes

```bash
git clone https://github.com/AlphaEngineer54/messaging-app.git
cd distributed-messaging-app
docker-compose up -d --build


