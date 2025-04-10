import './assets/main.css';
import 'vue3-toastify/dist/index.css';

import { createApp } from 'vue';
import * as signalR from '@microsoft/signalr';
import App from './App.vue';

const GAME_HUB_URL = 'http://localhost:8080/game';
const hub = new signalR.HubConnectionBuilder()
  .withUrl(GAME_HUB_URL, { withCredentials: true })
  .withAutomaticReconnect()
  .build();

createApp(App)
  .provide('hub', hub)
  .mount('#app');
