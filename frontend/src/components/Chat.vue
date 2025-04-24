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
      <input v-model="input" class="form-input" placeholder="Enter message or word guess">
      <button class="btn btn-primary" type="submit">Send</button>
    </form>
  </div>
</template>

<style>
#chat {
  display: flex;
  flex-direction: column;
  border-radius: 12px;
  background-color: white;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  width: 100%;
  height: 100%;
  margin-top: 20px;
  overflow: hidden;
}

#messages {
  flex: 1;
  list-style-type: none;
  padding: 15px;
  margin: 0;
  overflow-y: auto;
  max-height: calc(100% - 60px);
}

#messages li {
  padding: 8px 12px;
  margin-bottom: 8px;
  border-radius: 8px;
  font-size: 14px;
  display: flex;
  justify-content: space-between;
  background-color: #f5f7fa;
  word-break: break-word;
}

#messages li.success {
  background-color: #e3fcef;
  color: #0d6832;
  font-weight: 500;
}

#messages li.fail {
  background-color: #fff5f5;
  color: #cf000f;
  font-weight: 500;
}

#chat form {
  display: flex;
  margin: 0 0.75rem;
}

#chat form input,
#chat form button {
  height: 2.5rem;
  margin: auto 0;
}

#chat form button {
  padding: 0 1rem;
}
</style>
