<script setup>
import GameService from '@/services/game';
import { ref, reactive } from 'vue';
import * as signalR from "@microsoft/signalr";

const game = reactive({
  scene: 'login',
  playersInLobby: [],
});

const username = ref('');
const message = ref('');

const conn = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:8080/game', { withCredentials: false })
    .withAutomaticReconnect()
    .build();

const gameService = new GameService(conn);

conn.start()
    .then(() => console.log('connected'))
    .catch((err) => console.error('SignalR connection error:', err));

conn.on('UserJoined', (data) => {
  game.playersInLobby = data.players;
});

conn.on('UserLeft', (data) => {
  game.playersInLobby = data.players;
});

const join = () => {
  gameService.join(username.value)
      .then(_ => game.scene = 'lobby')
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
          <span v-if="message" id="validation-message">{{ message }}</span>
        </div>
        <button type="submit">Join</button>
      </form>
    </section>

    <section id="lobby" v-if="game.scene == 'lobby'">
      <h3>Players</h3>
      <ul>
        <li v-for="(player, index) in game.playersInLobby" :key="index">{{ player.username }}</li>
      </ul>
    </section>
  </div>
</template>
