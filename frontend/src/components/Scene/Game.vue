<script setup>
import { ref, reactive, inject } from 'vue';
import GameService from '@/services/game';
import { toast } from 'vue3-toastify';

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
  WordSelected: 'WordSelected'
}

const leaderboard = ref([]);
const yourTurn = ref(false);
const words = ref([]);
const wordToDraw = ref('');

hub.on(GameEvents.GameStarted, _ => scene.value = 'game');

hub.on(GameEvents.UpdateLeaderboard, (data) => leaderboard.value = data);

hub.on(GameEvents.YourTurn, ({ words: wordsToPick }) => {
  yourTurn.value = true;
  words.value = wordsToPick;
});

hub.on(GameEvents.GameDone, () => {
  scene.value = 'lobby';
  toast('All players left, and game is done!', { autoClose: 3000 });
});

hub.on(GameEvents.WordSelected, _ => yourTurn.value = false);

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

      {{ wordToDraw }}
    </div>
    <Chat />
  </section>
</template>
