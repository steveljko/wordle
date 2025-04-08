<script setup>
import { ref, reactive, onMounted } from 'vue';

import Modal from '@/components/Modal.vue';
import Chat from '@/components/Chat.vue';

import * as signalR from '@microsoft/signalr';
import GameService from '@/services/game';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';

const game = reactive({
  scene: 'login',
  playersInLobby: [],
  leaderboard: [],
  words: [],
  drawingWord: '',
});

const username = ref('');
const yourTurn = ref(false);

const conn = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:8080/game', { withCredentials: true })
    .withAutomaticReconnect()
    .build();

const gameService = new GameService();

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

onMounted(async () => {
  const cookieUsername = getCookieValue('username');

  if (cookieUsername) {
    await gameService.join(cookieUsername);

    conn.start()
        .then(() => console.log('connected'))
        .catch((err) => console.error('SignalR connection error:', err));

    game.scene = 'lobby';

    toast(`You are auto logged in as ${cookieUsername}`, { autoClose: 3000 });
  }
});

conn.on('UserJoined', (data) => game.playersInLobby = data.players);
conn.on('UserLeft', (data) => game.playersInLobby = data.players);
conn.on('GameStarted', _ => game.scene = 'game');
conn.on('GameStopped', _ => game.scene = 'lobby');
conn.on('GameDone', _ => game.scene = 'lobby');
conn.on('UpdateLeaderboard', (data) => game.leaderboard = data);

conn.on('YourTurn', (data) => {
  yourTurn.value = true;
  game.words = data.words;
});

conn.on('WordSelected', () => {
  yourTurn.value = false;
});

const join = async () => {
  try {
    const { status } = await gameService.join(username.value);

    if (status === 200) {
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

const stopGame = async () => {
  await gameService.stopGame();
}

const selectWord = async (word) => {
  const { status } = await gameService.selectWord(word);
  if (status === 200) {
    yourTurn.value = false;
    game.drawingWord = word;
  }
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
      <div id="leaderboard">
        <ul>
          <li v-for="(player, index) in game.leaderboard" :key="index">
            {{ index + 1 }}. {{ player.username }}: {{ player.points }} points
          </li>
        </ul>
      </div>
      <div>
        <Modal :isOpen="yourTurn" id="modal">
          <template #header>Select a Word</template>
          <template #content>
            <button
                v-for="word in game.words"
                @click="selectWord(word)"
                :key="word">
              {{ word }}
            </button>
          </template>
        </Modal>

        {{ game.drawingWord }}

        <button @click="stopGame">Stop Game</button>
      </div>
      
      <Chat :connection="conn" />
    </section>
  </div>
</template>
