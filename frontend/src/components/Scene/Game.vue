<script setup>
import { ref, computed, reactive, inject } from 'vue';
import GameService from '@/services/game';
import { toast } from 'vue3-toastify';

import Canvas from '@/components/Canvas.vue';
import Modal from '@/components/Modal.vue';
import Chat from '@/components/Chat.vue';

const scene = inject('scene');
const hub = inject('hub');
const gameService = new GameService();

const GameEvents = {
  GameStarted: 'GameStarted',
  GameDone: 'GameDone',
  UpdateLeaderboard: 'UpdateLeaderboard',
  YourTurn: 'YourTurn',
  WordSelected: 'WordSelected',
  StartTimer: 'StartTimer',
  ResetTimer: 'ResetTimer'
}

const leaderboard = ref([]);
const isDrawer = ref(false);

const yourTurn = ref(false);
const words = ref([]);
const wordToDraw = ref('');

hub.on(GameEvents.GameStarted, _ => scene.value = 'game');

hub.on(GameEvents.UpdateLeaderboard, (data) => leaderboard.value = data);

hub.on(GameEvents.YourTurn, ({ words: wordsToPick }) => {
  isDrawer.value = true;
  yourTurn.value = true;
  words.value = wordsToPick;
});

hub.on(GameEvents.GameDone, () => {
  scene.value = 'lobby';
  toast('All players left, and game is done!', { autoClose: 3000 });
});

hub.on(GameEvents.WordSelected, _ => yourTurn.value = false);

// turn timer
const timeLeft = ref(60);
const formattedTime = computed(() => {
  const minutes = String(Math.floor(timeLeft.value / 60)).padStart(2, '0');
  const seconds = String(timeLeft.value % 60).padStart(2, '0');
  return `${minutes}:${seconds}`;
});

let timerInterval = null;
hub.on(GameEvents.StartTimer, ({ endTime, duration }) => {
  endTime = new Date(endTime);
  timeLeft.value = duration;
  
  if (timerInterval) clearInterval(timerInterval);

  const updateTimer = () => {
    const now = new Date();
    const remaining = endTime - now;
  
    if (remaining <= 0) {
      timeLeft.value = 0;
      clearInterval(timerInterval);
      timerInterval = null;
      return;
    }
  
    timeLeft.value = Math.ceil(remaining / 1000);
  };

  updateTimer();

  timerInterval = setInterval(updateTimer, 100);
});

hub.on(GameEvents.ResetTimer, () => {
  if (timerInterval) {
    clearInterval(timerInterval);
    timerInterval = null;
  }

  timeLeft.value = 60;
  endTime = null;
});

const selectWord = async (word) => {
  await gameService.selectWord(word);
}
</script>

<template>
  <section id="game" v-if="scene == 'game'">
    <div id="leaderboard">
      <ul>
        <li v-for="(player, index) in leaderboard" :key="index">
          {{ index + 1 }}. {{ player.username }}: {{ player.points }} points
        </li>
      </ul>
    </div>
    <div>
      <Modal :isOpen="yourTurn" id="modal">
        <template #header>Select a Word</template>
        <template #content>
          <button
            v-for="(word, index) in words"
            @click="selectWord(word)"
            :key="index">
            {{ word }}
          </button>
        </template>
      </Modal>

      Timeleft: {{ formattedTime }}
      <Canvas :isDrawer="isDrawer"/>
    </div>
    <Chat />
  </section>
</template>
