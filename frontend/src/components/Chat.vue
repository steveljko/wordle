<script setup>
import { ref, inject, onMounted } from 'vue';
import moment from 'moment';
import { toast } from 'vue3-toastify';
import * as signalR from '@microsoft/signalr';

const messages = ref([]);
const input = ref('');
const hub = inject('hub');

onMounted(() => {
  hub.on('Broadcast', (data) => {
    messages.value.push(data);
    input.value = '';
  });
});

const guess = async () => {
  if (input.value === "") {
    toast("You can't submit", { autoClose: 2000 });
    return;
  }

  await hub.invoke('GuessWord', input.value);
};
</script>

<template>
  <div id="chat">
    <ul id="messages">
      <li
          v-for="(message, index) in messages"
          :key="index"
          :class="{ 'success': message.success, 'fail': !message.success }"
      >{{ message.message }} {{ moment(message.timestamp).format('HH:mm') }}</li>
    </ul>
    <form @submit.prevent="guess">
      <input v-model="input" placeholder="Enter message or word guess">
      <button type="submit">Send</button>
    </form>
  </div>
</template>
