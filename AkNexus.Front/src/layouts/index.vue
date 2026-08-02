<script setup lang="ts">
/**
 * 主布局容器
 * 整合侧边栏、头部、主内容区
 */
import LayoutSidebar from './components/Sidebar.vue'
import LayoutHeader from './components/Header.vue'
import LayoutMain from './components/Main.vue'

defineOptions({ name: 'LayoutContainer' })
</script>

<template>
  <div class="layout-container">
    <!-- 侧边栏 -->
    <LayoutSidebar />

    <!-- 右侧区域 -->
    <div class="layout-right">
      <!-- 顶部工具栏 -->
      <LayoutHeader />

      <!-- 主内容区 -->
      <LayoutMain />
    </div>
  </div>
</template>

<style scoped lang="scss">
.layout-container {
  display: flex;
  width: 100%;
  height: 100%;
  overflow: hidden;
}

.layout-right {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
</style>