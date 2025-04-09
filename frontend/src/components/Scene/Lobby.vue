<script setup>
import { inject, reactive, onMounted } from 'vue';
import getCookieValue from '@/helpers/cookie.js';
import GameService from '@/services/game.js';
const gameService = new GameService;

const scene = inject('scene');
const hub = inject('hub');

const data = reactive({
  players: [],
});

onMounted(async () => {
  const username = getCookieValue('username');

  if (username) {
    const { status } = await gameService.join(username);

    if (status === 200) {
      hub.start();
      scene.value = 'lobby';
    }
  }
});

const LobbyEvents = {
  UserJoined: 'UserJoined',
  UserLeft: 'UserLeft',
}

hub.on(LobbyEvents.UserJoined, ({ players }) => data.players = players);
hub.on(LobbyEvents.UserLeft, ({ players }) => data.players = players);

const start = async () => {
  const { status } = await gameService.startGame();

  if (status === 200) {
    scene.value = 'game';
  }
}

const leave = async () => {
  await gameService.leave();
  scene.value = 'login';
  hub.close();
}
</script>

<template>
  <section id="lobby" v-if="scene == 'lobby'">
    <h3>Players</h3>
    <ul>
      <li v-for="(player, index) in data.players" :key="index">{{ player.username }}</li>
    </ul>
    <button @click="start">Start Game</button>
    <button @click="leave">Logout</button>
  </section>
</template>
