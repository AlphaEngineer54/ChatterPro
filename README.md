# 📨 Distributed Messaging App – Microservices Architecture

Une application de messagerie distribuée conçue pour démontrer une architecture orientée microservices avec un front-end moderne et réactif.

![Docker](https://img.shields.io/badge/containerized-Docker-blue?logo=docker)
![Architecture](https://img.shields.io/badge/architecture-Microservices-ff69b4?style=flat-square)
![Status](https://img.shields.io/badge/status-En%20développement-yellow)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

---

## 🚀 Présentation

Ce projet est une **plateforme de messagerie en temps quasi réel**, construite avec une architecture distribuée, entièrement conteneurisée avec Docker, et reposant sur une communication asynchrone entre services.

---

## 📐 Architecture Technique

L'application est composée des microservices suivants :

| Service              | Rôle                                                                 |
|----------------------|----------------------------------------------------------------------|
| `AuthService`        | Gestion de l'inscription, connexion, JWT, et autorisation            |
| `MessageService`     | Création, envoi et réception des messages                            |
| `UserService`        | Gestion des informations non-sensibles des utilisateurs                |
| `NotificationService`| Envoi de notifications (email, push, etc.)                           |
| `DataExportService`  | Exportation des messages et données utilisateur (PDF, JSON, etc.)     |
| `Frontend`           | Interface utilisateur (React) avec authentification et messagerie     |

Les services communiquent via **HTTP REST** et **events asynchrones** (selon les cas d’usage, une file de messages peut être intégrée ultérieurement).

---

## 🧰 Technologies utilisées

| Frontend       | Backend / Services                     | Infrastructure        |
|----------------|----------------------------------------|------------------------|
| React.js       | ASP.NET Core (.NET 8)                  | Docker                 |
| React Router   | JWT, Argon2                            | Docker Compose         |
| Context API    | MySQL                                  | NGINX (proxy possible) |
| CSS Modules    | REST API, RabbitMQ                              | Kubernetes (ochestration)|
| D'autres technologies à venir...          | Event-driven architecture              |                        |
|                | iText7 (exportation PDF), CSVHelper (export CSV) |                        |
|                | .NET Logging (journalisation)          |                        |
|                | CORS                                   |                        |


---
## 🧭 Navigation et Fonctionnalités

### 🔐 Authentification & Sécurité
- Authentification sécurisée via JWT (JSON Web Tokens)
- Gestion des sessions utilisateurs avec expiration et rafraîchissement
- Stockage sécurisé des mots de passe (hash + salage)
- Validation côté serveur des entrées sensibles (anti-injection, etc.)

### 💬 Messagerie
- Envoi et réception de messages texte
- Architecture orientée microservices (MessageService)
- File d’attente pour traitement asynchrone des messages (RabbitMQ ou équivalent)
- Interface de messagerie en temps réel **simulé** (ex. : long polling)
- Persistance des messages dans une base de données

### 📇 Gestion des contacts
- Ajout, suppression et gestion des contacts utilisateur
- Microservice dédié : ContactService
- Vérification de l’identité utilisateur à l’ajout d’un contact

### 📤 Exportation des données
- Service d’export des données personnelles : DataExportService
- Génération de fichiers exportables (JSON ou CSV)
- Envoi sécurisé des données exportées à l’utilisateur

### 🔔 Notifications
- Envoi de notifications à l’utilisateur (NotificationService)
- Notifications lors d’un nouveau message ou événement important
- Architecture asynchrone pour la gestion des événements déclencheurs

### 🧱 Architecture & Déploiement
- Architecture modulaire : chaque service est indépendant
- Communication inter-services par messages asynchrones
- Conteneurisation complète via Docker
- Déploiement orchestré (Docker Compose ou équivalent)

### 🖼️ Interface utilisateur – Intégration backend en cours de développement
- UI stylisée avec design UX ergonomique
- Authentification connectée au backend
- Intégration fluide avec les services de messagerie, contacts, export et notifications

---

## 📦 Lancer le projet localement

### Prérequis

- Docker & Docker Compose installés

### Étapes

```bash
# 1. Cloner le dépôt
git clone https://github.com/AlphaEngineer54/messaging-app.git
cd distributed-messaging-app

# 2. Démarrer tous les services
docker-compose up -d --build
