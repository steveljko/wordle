<script setup>
import GameService from '@/services/game';
import { ref, reactive } from 'vue';
import * as signalR from '@microsoft/signalr';

import 'vue3-toastify/dist/index.css';

const game = reactive({
  scene: 'login',
  playersInLobby: [],
});

const username = ref('');

const conn = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:8080/game', { withCredentials: true })
    .withAutomaticReconnect()
    .build();

const gameService = new GameService(conn);

conn.on('UserJoined', (data) => {
  game.playersInLobby = data.players;
});

conn.on('UserLeft', (data) => {
  console.log('run')
  console.log(data)
  game.playersInLobby = data.players;
});

const join = async () => {
  await gameService.join(username.value);

  conn.start()
      .then(() => console.log('connected'))
      .catch((err) => console.error('SignalR connection error:', err));
  
  game.scene = 'lobby';
}
const startGame = () => {
  gameService.startGame()
      .then(res => console.log('starting'))
      .catch(err => console.log(err));
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
    </section>
  </div>
</template>
