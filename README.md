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

![Modèle de données](https://github.com/AlphaEngineer54/messaging-app/blob/main/entities_model.png)

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

> **Communication en temps réel via WebSocket avec SignalR disponible sur ce service.**

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
## 🖥️ Exemple de client WPF (.NET 8) – Intégration SignalR

### MessageService - Communication temp réel via SignalR

#### Fontionnalités SignalR

| Méthode SignalR             | Description                                                                 |
|-----------------------------|-----------------------------------------------------------------------------|
| `SendMessage(dto)`          | Envoie un message à **tous les clients connectés**.                         |
| `SendMessageToUser(dto)`    | Envoie un message à **un utilisateur spécifique** (`ReceiverId`).           |
| `SendMessageToGroup(dto)`   | Diffuse un message à tous les **membres d’une conversation** (`ConversationId`). |
| `JoinGroup(dto)`            | Ajoute un utilisateur à un **groupe SignalR** représentant une conversation. |

> ⚠️ Les objets `NewMessageDTO` et `JoinConversationDTO` sont validés côté serveur.  
> En cas d’erreur, un événement `"ValidationError"` est émis vers le client appelant.

---

### NotificationService – Communication temps réel via SignalR

Le service `NotificationService` utilise un hub SignalR nommé `NotificationHubs` pour gérer les notifications en temps réel destinées aux utilisateurs.

### Fonctionnalités SignalR exposées

| Fonctionnalité                         | Description                                                  |
|--------------------------------------|--------------------------------------------------------------|
| Envoi de notification privée         | Le serveur pousse une notification à un utilisateur spécifique via `Clients.User(userId)` avec l’événement `"ReceiveNotification"` |

### UserService - Coommunication temps réel via SignalR

Le composant `MultiEventHandler` intègre **SignalR** afin d’émettre des messages temps réel à des clients spécifiques lorsque des événements sont traités dans le système.

### 📌 Scénarios gérés via `HandleEventAsync`

| Type d'événement              | Action réalisée                                                            | Notification envoyée                     |
|------------------------------|----------------------------------------------------------------------------|-------------------------------------------|
| `GetUserIEvent`              | Récupération des profils utilisateurs à partir d’une liste d’IDs          | `Clients.User(userId).SendAsync("ReceiveUsers", users)` |

### ⚙️ Exemple pratique d’utilisation côté client en C# (WPF .NET 8)

#### 1. MessageService Hub

```csharp
using Microsoft.AspNetCore.SignalR.Client;

public class RealtimeMessagingClient
{
    private HubConnection _connection;

    public async Task InitAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://IP_ADDRESS:PORT/chathub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<object>("ReceiveMessage", message =>
        {
            // Mise à jour de l'UI WPF
            Console.WriteLine("Message reçu : " + message);
        });

        _connection.On<List<string>>("ValidationError", errors =>
        {
            foreach (var err in errors)
                Console.WriteLine("Erreur : " + err);
        });

        await _connection.StartAsync();
    }

    public async Task SendMessageToGroup()
    {
        var messageDto = new
        {
            SenderId = 1,
            ReceiverId = 2,
            ConversationId = 10,
            Content = "Bonjour en temps réel",
            Status = "Sent"
        };
        await _connection.InvokeAsync("SendMessageToGroup", messageDto);
    }

    public async Task JoinGroup()
    {
        var joinDto = new
        {
            UserId = 1,
            ConversationId = 10
        };
        await _connection.InvokeAsync("JoinGroup", joinDto);
    }
}
```

#### 2. NotificationService Hub

```csharp
using Microsoft.AspNetCore.SignalR.Client;

public class NotificationClient
{
    private HubConnection _connection;

    public async Task InitAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://IP_ADDRESS:PORT/notificationhub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<Notification>("ReceiveNotification", notification =>
        {
            // Gérer l'affichage de la notification dans l'UI
            Console.WriteLine($"Notification reçue : {notification.Message}");
        });

        await _connection.StartAsync();
    }
}
```
#### 3. UserSercice Hub

```csharp
// MainWindow.xaml.cs
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;

namespace WpfSignalRClient
{
    public partial class MainWindow : Window
    {
        private readonly HubConnection _hubConnection;
        public ObservableCollection<User> Users { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/userHub") // Adaptez selon le port de votre backend
                .WithAutomaticReconnect()
                .Build();

            RegisterSignalRHandlers();
            ConnectToSignalR();
        }

        private void RegisterSignalRHandlers()
        {
            _hubConnection.On<List<User>>("ReceiveUsers", users =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Users.Clear();
                    foreach (var user in users)
                        Users.Add(user);
                });
            });
        }

        private async void ConnectToSignalR()
        {
            try
            {
                await _hubConnection.StartAsync();
                StatusLabel.Content = "Connecté à SignalR";
            }
            catch (Exception ex)
            {
                StatusLabel.Content = $"Erreur de connexion : {ex.Message}";
            }
        }

        private async void GetUsersButton_Click(object sender, RoutedEventArgs e)
        {
            var httpClient = new HttpClient();
            var request = new
            {
                UserId = 42, // ID du client courant (doit correspondre à l'identité SignalR côté serveur)
                ids = new[] { 1, 2, 3 } // Liste des IDs à récupérer
            };

            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:5000/api/events/dispatch", content);
            if (response.IsSuccessStatusCode)
                StatusLabel.Content = "Requête GetUserIEvent envoyée";
            else
                StatusLabel.Content = "Erreur lors de l'envoi";
        }
    }

    public class UserConversationInfo
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
    }
}
```

## 📦 Démarrage local

### Prérequis
- Docker
- Docker Compose

### Commandes

```bash
git clone https://github.com/AlphaEngineer54/messaging-app.git
cd distributed-messaging-app
docker-compose up -d --build


