<template>
  <div class="color-picker">
    <label for="color-input">Color:</label>

    <div id="color-picker-container">
      <input id="color-input" type="color" @input="(event) => emit('update:modelValue', event.target.value)" />
      <div id="hex">{{ color }}</div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: String,
    default: '#000000'
  }
});

const color = computed(() => props.modelValue);

const emit = defineEmits(['update:modelValue']);
</script>

<style>
.color-picker #color-picker-container {
  display: flex;
  align-items: center;
  padding: .25rem .75rem;
  height: 2.5rem;
  border-radius: 50px;
  border: 1px solid var(--primary);
  box-shadow: var(--card-shadow);
  background-color: white;
}

.color-picker #color-input {
  width: 2rem;
  height: 2rem;
  border-radius: 50%;
  border: none;
  cursor: pointer;
}

.color-picker #hex {
  margin-left: 0.5rem;
  font-size: 14px;
}
</style>
