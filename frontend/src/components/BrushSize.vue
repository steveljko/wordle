<template>
  <div class="brush-size-control">
    <label for="width-input">Line width:</label>

    <div class="brush-size-adjuster">
      <button 
        class="size-button decrease" 
        @click="decreaseSize" 
        :disabled="lineWidth <= minWidth"
        aria-label="Decrease brush size"
      >
        <span>−</span>
      </button>

      <div class="size">
        <input 
          id="width-input" 
          type="range" 
          :min="minWidth" 
          :max="maxWidth" 
          v-model.number="lineWidth" 
        />
        <span class="size-value">{{ lineWidth }}px</span>
      </div>

      <button 
        class="size-button increase" 
        @click="increaseSize" 
        :disabled="lineWidth >= maxWidth"
        aria-label="Increase brush size"
      >
        <span>+</span>
      </button>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: Number,
    default: 5
  },
  minWidth: {
    type: Number,
    default: 1
  },
  maxWidth: {
    type: Number,
    default: 30
  },
});

const emit = defineEmits(['update:modelValue']);

const lineWidth = computed({
  get() {
    return props.modelValue;
  },
  set(value) {
    emit('update:modelValue', value);
  }
});

const increaseSize = () => {
  if (lineWidth.value < props.maxWidth) {
    lineWidth.value = Math.min(lineWidth.value + 1, props.maxWidth);
  }
};

const decreaseSize = () => {
  if (lineWidth.value > props.minWidth) {
    lineWidth.value = Math.max(lineWidth.value - 1, props.minWidth);
  }
};
</script>

<style scoped>
.brush-size-adjuster {
  display: flex;
  align-items: center;
  padding: 0 0.25rem;
  height: 2.5rem;
  border-radius: 50px;
  border: 1px solid var(--primary);
  box-shadow: var(--card-shadow);
  background-color: white;
}

.brush-size-adjuster .size-button {
  display: block;
  width: 2rem;
  height: 2rem;
  font-size: 1rem;
  font-weight: 800;
  border-radius: 50%;
  background-color: var(--primary);
  color: white;
  cursor: pointer;
  user-select: none;
}

.brush-size-adjuster .size {
  margin: auto .75rem;
  font-size: 0.875rem;
}

#width-input {
  display: none;
}
</style>
