<script setup>
import { ref, inject } from 'vue';
import GameService from '@/services/game';
const gameService = new GameService();

const scene = inject('scene');
const hub = inject('hub');

const username = ref('');

const join = async () => {
  try {
    const { status } = await gameService.join(username.value);

    if (status === 200) {
      hub.start();
      scene.value = 'lobby';
    }
  } catch (err) {
    console.error(err);
  }
}
</script>

<template>
  <section id="login" v-if="scene == 'login'">
    <form @submit.prevent="join">
      <div>
        <label for="username">Player Username</label>
        <input type="text" v-model="username" id="username" placeholder="Enter username...">
      </div>
      <button type="submit">Join</button>
    </form>
  </section>
</template>
