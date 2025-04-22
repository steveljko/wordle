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
  <section id="login" class="card" v-if="scene == 'login'">
    <div class="mb-4">
      <h1 class="login-title">Welcome to Wordle</h1>
      <p class="login-subtitle">Join the online drawing game and have fun!</p>
    </div>

    <form @submit.prevent="join">
      <div>
        <label for="username" class="form-label mb-2">Username</label>
        <input 
          type="text" 
          v-model="username" 
          class="form-input mb-2" 
          id="username" 
          placeholder="Enter your name..."
          autocomplete="off"
        >
      </div>
      <button class="btn btn-primary full" type="submit">Join</button>
    </form>
  </section>
</template>
