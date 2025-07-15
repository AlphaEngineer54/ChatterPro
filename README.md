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
| MessageService        | Envoi, réception, persistance des messages                        |
| ConversationService   | Gestion des conversations                                          |
| NotificationService   | Push/email/système de notification asynchrone                     |
| DataExportService     | Exportation de données au format PDF, CSV, JSON                   |    
| Gateway (Ocelot)      | Point d’entrée unique pour tous les services (reverse proxy)      |

---

## 🧰 Technologies utilisées

| Côté Client         | Backend / Services                   | Infrastructure          |
|---------------------|--------------------------------------|--------------------------|
| WPF (.NET)          | ASP.NET Core (.NET 8), JWT, Argon2   | Docker, Docker Compose   |
| REST HTTP Client    | MySQL, RabbitMQ                      | NGINX (reverse proxy)    |
|                     | iText7, CsvHelper                    | Kubernetes (à venir)     |
|                     | .NET Logging, CORS                   | Event-driven architecture|

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

---

## Notes

- Le frontend doit toujours communiquer via l’API Gateway (`localhost:5000`).
- Les routes avec authentification exigent un token JWT valide dans l’en-tête `Authorization`.
- `{everything}` représente toute sous-route ou paramètre.
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

### 📇 Contacts
- Ajout/suppression
- Service dédié avec vérification d’identité

### 📤 Export de données
- JSON, CSV, PDF
- Téléchargement sécurisé

### 🔔 Notifications
- Événements déclencheurs
- Notification asynchrone

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

