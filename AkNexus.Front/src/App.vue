<script setup lang="ts">
/**
 * 应用根组件
 */
import { onMounted } from 'vue'
import { initSkinSystem } from '/@/skins'

onMounted(() => {
  // 初始化皮肤系统
  initSkinSystem()
})
</script>

<template>
  <el-config-provider>
    <router-view />
  </el-config-provider>
</template>

<style>
/* 全局样式 */
*,
*::before,
*::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

html,
body,
#app {
  width: 100%;
  height: 100%;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial,
    'Noto Sans', sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol';
  font-size: 14px;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
</style>