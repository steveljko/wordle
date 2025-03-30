FROM node:latest

WORKDIR /app

COPY frontend/package*.json ./
RUN npm install

CMD ["npm", "run", "dev"]
