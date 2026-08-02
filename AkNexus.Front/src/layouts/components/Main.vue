<script setup lang="ts">
/**
 * 主内容区组件
 */
defineOptions({ name: 'LayoutMain' })
</script>

<template>
  <main class="layout-main">
    <div class="layout-main-scroll">
      <div class="layout-parent">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <keep-alive>
              <component :is="Component" />
            </keep-alive>
          </transition>
        </router-view>
      </div>
    </div>
  </main>
</template>

<style scoped lang="scss">
.layout-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: var(--ds-bg-base);
}

.layout-main-scroll {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 100%;
  overflow: auto;
}

.layout-parent {
  flex: 1;
  display: flex;
  flex-direction: column;
  position: relative;
  padding: 12px;
}

// 页面切换动画
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>