<template>
  <div class="drawing-container">
    <canvas 
      ref="canvasRef" 
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
      
      <button @click="hub.invoke('ClearCanvas')">Clear Canvas</button>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, inject, onMounted, watch } from 'vue';

const props = defineProps({
  isDrawer: {
    type: Boolean,
    default: false
  }
});

const hub = inject('hub');

const sendDraw = (type, x, y, color, lineWidth) => {
  hub.invoke("Draw", {
    actionType: type,
    x,
    y,
    color,
    lineWidth: Number(lineWidth)
  });
}

const canvasRef = ref(null);
const isDrawing = ref(false);
const drawingColor = ref("#000");
const lineWidth = ref(5);
const ctx = ref(null);

const options = reactive({
  color: '#000',
  lineWIdth: 5,
});

onMounted(() => {
  if (canvasRef.value) {
    ctx.value = canvasRef.value.getContext('2d');
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

  const canvas = canvasRef.value;
  const rect = canvas.getBoundingClientRect();
  const x = event.clientX - rect.left;
  const y = event.clientY - rect.top;

  ctx.value.beginPath();
  ctx.value.moveTo(x, y);

  sendDraw("start", x, y, drawingColor.value, lineWidth.value);
  draw(event);
};


const draw = (event) => {
  if (!isDrawing.value) return;
  
  const canvas = canvasRef.value;
  const rect = canvas.getBoundingClientRect();
  const x = event.clientX - rect.left;
  const y = event.clientY - rect.top;
  
  ctx.value.lineTo(x, y);
  ctx.value.stroke();
  ctx.value.beginPath();
  ctx.value.moveTo(x, y);

  sendDraw("move", x, y, drawingColor.value, lineWidth.value);
};

hub.on("ReceiveDrawAction", (drawAction) => {
    if (!props.isDrawer) {
      handleRemoteDrawAction(drawAction);
    }
  });

const handleRemoteDrawAction = (drawAction) => {
  const { x, y, actionType, color, lineWidth } = drawAction;
  const currentColor = ctx.value.strokeStyle;
  const currentLineWidth = ctx.value.lineWidth;
  
  ctx.value.strokeStyle = color;
  ctx.value.lineWidth = lineWidth;
  
  switch(actionType) {
    case "start":
      ctx.value.beginPath();
      ctx.value.moveTo(x, y);
      break;
    case "move":
      ctx.value.lineTo(x, y);
      ctx.value.stroke();
      ctx.value.beginPath();
      ctx.value.moveTo(x, y);
      break;
    case "end":
      ctx.value.beginPath();
      break;
  }
  
  ctx.value.strokeStyle = currentColor;
  ctx.value.lineWidth = currentLineWidth;
};

const stopDrawing = () => {
  if (isDrawing.value) {
    isDrawing.value = false;
    ctx.value.beginPath();

    sendDraw("end", 0, 0, drawingColor.value, lineWidth.value);
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

const clearCanvas = () => ctx.value.clearRect(0, 0, canvasRef.value.width, canvasRef.value.height);

hub.on('ClearCanvas', () => clearCanvas());
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
