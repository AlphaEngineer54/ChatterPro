# ---- Étape 1 : build de l'application Vite ----
FROM node:20-alpine AS build
WORKDIR /app

# Installation des dépendances (cache optimisé)
COPY package*.json ./
RUN npm ci

# Copie du code et build de production
COPY . .
# Les URLs backend sont injectées au build (variables VITE_*). Elles peuvent être
# surchargées via --build-arg lors du `docker build`.
ARG VITE_GATEWAY_URL=http://localhost:5000
ARG VITE_CHAT_HUB_URL=http://localhost:5003/hubs/chat
ARG VITE_FRONTEND_PORT=3000
ENV VITE_GATEWAY_URL=$VITE_GATEWAY_URL
ENV VITE_CHAT_HUB_URL=$VITE_CHAT_HUB_URL
ENV VITE_FRONTEND_PORT=$VITE_FRONTEND_PORT
RUN npm run build

# ---- Étape 2 : service statique via Nginx ----
FROM nginx:1.27-alpine AS runtime

# Config Nginx avec fallback SPA (React Router) et écoute sur le port 3000
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

# Port 3000 : seule origine autorisée par le CORS de la gateway et de MessageService
EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
