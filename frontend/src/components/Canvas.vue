<template>
  <div class="drawing-container">
    <canvas 
      ref="canvas" 
      width="800px" 
      height="600px"
      @mousedown="startDrawing"
      @mousemove="draw"
      @mouseup="stopDrawing"
      @mouseleave="stopDrawing"
      @touchstart="handleTouchStart"
      @touchmove="handleTouchMove"
      @touchend="stopDrawing"
    ></canvas>
    
    <div class="controls" v-if="isDrawer">
      <div class="color-picker">
        <label for="color-input">Color:</label>
        <input id="color-input" type="color" v-model="drawingColor" />
      </div>
      
      <div class="line-width">
        <label for="width-input">Line width:</label>
        <input 
          id="width-input" 
          type="range" 
          min="1" 
          max="20" 
          v-model="lineWidth" 
        />
        <span>{{ lineWidth }}px</span>
      </div>
      
      <button @click="clearCanvas">Clear Canvas</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';

const props = defineProps({
  isDrawer: {
    type: Boolean,
    default: false
  }
});

const canvas = ref(null);
const isDrawing = ref(false);
const drawingColor = ref("#000");
const lineWidth = ref(5);
const ctx = ref(null);

onMounted(() => {
  if (canvas.value) {
    ctx.value = canvas.value.getContext('2d');
    setupCanvas();
  }
});

watch(drawingColor, () => {
  if (ctx.value) ctx.value.strokeStyle = drawingColor.value;
});

watch(lineWidth, () => {
  if (ctx.value) ctx.value.lineWidth = lineWidth.value;
});

const setupCanvas = () => {
  const context = ctx.value;
  context.lineCap = 'round';
  context.lineJoin = 'round';
  context.strokeStyle = drawingColor.value;
  context.lineWidth = lineWidth.value;
};

const startDrawing = (event) => {
  if (!props.isDrawer) return;

  isDrawing.value = true;
  draw(event);
};

const draw = (event) => {
  if (!isDrawing.value) return;
  
  const canvas = canvas.value;
  const rect = canvas.getBoundingClientRect();
  const x = event.clientX - rect.left;
  const y = event.clientY - rect.top;
  
  ctx.value.lineTo(x, y);
  ctx.value.stroke();
  ctx.value.beginPath();
  ctx.value.moveTo(x, y);
};

const stopDrawing = () => {
  if (isDrawing.value) {
    isDrawing.value = false;
    ctx.value.beginPath();
  }
};

const handleTouchStart = (event) => {
  event.preventDefault();

  const touch = event.touches[0];
  const mouseEvent = new MouseEvent('mousedown', {
    clientX: touch.clientX,
    clientY: touch.clientY
  });

  startDrawing(mouseEvent);
};

const handleTouchMove = (event) => {
  event.preventDefault();

  const touch = event.touches[0];
  const mouseEvent = new MouseEvent('mousemove', {
    clientX: touch.clientX,
    clientY: touch.clientY
  });

  draw(mouseEvent);
};

const clearCanvas = () => ctx.value.clearRect(0, 0, canvas.value.width, canvas.value.height);
</script>

<style scoped>
.drawing-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

canvas {
  border: 1px solid #ccc;
  cursor: crosshair;
  touch-action: none;
  background-color: white;
}

.controls {
  display: flex;
  gap: 20px;
  align-items: center;
  flex-wrap: wrap;
}

.color-picker, .line-width {
  display: flex;
  align-items: center;
  gap: 8px;
}

input[type="color"] {
  width: 40px;
  height: 40px;
  border: none;
  cursor: pointer;
}

input[type="range"] {
  width: 100px;
}
</style>
