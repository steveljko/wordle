<script setup>
import { ref, provide } from 'vue';

import SceneSwitcher from '@/components/SceneSwitcher.vue';

const scene = ref('login');
provide('scene', scene);
</script>

<template>
  <div>
    <SceneSwitcher />
  </div>
</template>
