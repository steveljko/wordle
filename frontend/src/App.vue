<script setup>
import GameService from '@/services/game';
import {ref, reactive, onMounted} from 'vue';
import * as signalR from '@microsoft/signalr';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';

const game = reactive({
  scene: 'login',
  playersInLobby: [],
});

const username = ref('');

function getCookieValue(cookieName) {
  const cookies = document.cookie;
  const regex = new RegExp('(^|; )' + cookieName + '=([^;]*)');
  const match = regex.exec(cookies);

  if (match) {
    return decodeURIComponent(match[2]);
  } else {
    return null;
  }
}

const conn = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:8080/game', { withCredentials: true })
    .withAutomaticReconnect()
    .build();

const gameService = new GameService();

onMounted(async () => {
  const cookieUsername = getCookieValue('username');

  if (cookieUsername) {
    await gameService.join(cookieUsername);

    conn.start()
        .then(() => console.log('connected'))
        .catch((err) => console.error('SignalR connection error:', err));

    game.scene = 'lobby';
    
    toast(`Ỳou are auto logged in as ${cookieUsername}`, { autoClose: 3000 });
  }
});

conn.on('UserJoined', (data) => {
  game.playersInLobby = data.players;
});

conn.on('UserLeft', (data) => {
  game.playersInLobby = data.players;
});

conn.on('GameStarted', (data) => {
  game.scene = 'game';
});

const join = async () => {
  try {
    const { status } = await gameService.join(username.value);
    
    if (status == 200) {
      await conn.start();
      game.scene = 'lobby';
      username.value = '';
    }
  } catch (err) {
    console.error(err);
  }
}

const leave = async () => {
  await gameService.leave();
  game.scene = 'login';
}

const startGame = async () => {
  await gameService.startGame();
}
</script>

<template>
  <div>
    <section id="login" v-if="game.scene == 'login'">
      <form @submit.prevent="join">
        <div>
          <label for="username">Player Username</label>
          <input type="text" v-model="username" id="username" placeholder="Enter username...">
        </div>
        <button type="submit">Join</button>
      </form>
    </section>

    <section id="lobby" v-if="game.scene == 'lobby'">
      <h3>Players</h3>
      <ul>
        <li v-for="(player, index) in game.playersInLobby" :key="index">{{ player.username }}</li>
      </ul>
      <button @click="startGame">Start Game</button>
      <button @click="leave">Logout</button>
    </section>
    
    <section id="game" v-if="game.scene == 'game'">
      <h3>Woho game is started</h3>
    </section>
  </div>
</template>
