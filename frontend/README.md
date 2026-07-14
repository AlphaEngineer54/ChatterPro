# ChatterPro — Frontend (React + Vite)

Frontend de démonstration pour ChatterPro : authentification JWT, conversations de
groupe et messagerie temps réel via SignalR.

## Fonctionnalités
- 🔐 Authentification (login / signup), session persistée, JWT sur toutes les requêtes
- 💬 Chat de groupe temps réel (création, join par code, historique, envoi)
- ✏️ Édition & suppression de ses propres messages
- 🚪 Quitter une conversation sans la supprimer
- 📤 Export d'une conversation en PDF / CSV / JSON
- 🔔 Notifications (cloche + badge, polling REST)
- 👤 Profil (voir/mettre à jour), recherche de contact, suppression de compte (danger zone)
- 🌗 Mode clair / sombre (persisté, suit la préférence système)

## Stack
- Vite + React 18 (JavaScript)
- Tailwind CSS (UI moderne, fond blanc)
- `react-router-dom` (routing + routes protégées)
- `@microsoft/signalr` (temps réel)
- Vitest + React Testing Library (tests unitaires : `npm test`)

## Prérequis backend
Le backend .NET doit tourner :
- **API Gateway Ocelot** sur `http://localhost:5000` (REST : auth, conversations, messages).
- **MessageService** sur `http://localhost:5003` (Hub SignalR direct — Ocelot ne route pas les WebSockets).
- `JWT_SECRET` identique sur la gateway et AuthService, bases MySQL et RabbitMQ démarrés.

Le plus simple depuis la racine `ChatterPro/` : `docker-compose up -d --build` (avec un `.env` renseigné).

> ⚠️ Le dev server **doit** rester sur le port **3000** : c'est la seule origine autorisée
> par le CORS de la gateway et de MessageService.

## Configuration
Copiez/ajustez `.env` si vos ports diffèrent :
```
VITE_GATEWAY_URL=http://localhost:5000
VITE_CHAT_HUB_URL=http://localhost:5003/hubs/chat
```

## Démarrage
```bash
npm install
npm run dev      # http://localhost:3000
npm run build    # build de production
```

## Parcours de démonstration
1. Créez un compte (`/signup`) → redirection vers `/chat`.
2. « + Nouvelle » → un **code d'invitation** s'affiche (bouton copier).
3. Dans un 2e navigateur, créez un autre compte puis « Rejoindre » avec ce code.
4. Ouvrez la conversation des deux côtés et échangez : les messages arrivent en
   temps réel (SignalR `ReceiveMessage`).
5. Rechargez la page → la session est conservée (localStorage) et l'historique se
   recharge (`GET /conversation/{id}?limit=50`).

## Architecture des dossiers
```
src/
  api/          client fetch (Bearer JWT) + auth, conversations, messages,
                users, notifications, export (+ tests *.test.js)
  context/      AuthContext (session + localStorage)
  realtime/     chatHub (connexion SignalR, JWT via accessTokenFactory)
  components/   sidebar, fenêtre de chat, bulles (édition/suppression),
                composer, modales, ExportMenu, NotificationBell
  pages/        LoginPage, SignupPage, ChatPage, ProfilePage
  test/         setup Vitest (jsdom + polyfill localStorage)
```

## Notes / limites
- Les WebSockets ne passent pas par la gateway → le hub de chat se connecte en direct sur `:5003`.
- Les notifications utilisent un **polling REST** (`GET /notification/{userId}`) : le push
  temps réel nécessite un `IUserIdProvider` côté backend, non configuré.
- L'édition/suppression de message est REST et met à jour l'affichage local ; le hub ne
  diffuse pas ces événements aux autres membres.
