<script setup lang="ts">
/**
 * 侧边栏组件
 * 使用皮肤系统的 CSS 变量
 */
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getGlobalMenus } from '/@/modules'
import type { ModuleMenuItem } from '/@/modules'

defineOptions({ name: 'LayoutSidebar' })

const route = useRoute()
const router = useRouter()

// 获取菜单数据
const menus = computed(() => getGlobalMenus())

// 当前激活的菜单ID
const activeMenu = computed(() => {
  return route.path
})

// 点击菜单
const handleMenuClick = (menu: ModuleMenuItem) => {
  if (menu.path) {
    router.push(menu.path)
  }
}

// 判断菜单是否激活
const isMenuActive = (menu: ModuleMenuItem): boolean => {
  return route.path === menu.path || route.path.startsWith(menu.path + '/')
}
</script>

<template>
  <aside class="layout-sidebar">
    <!-- Logo 区域 -->
    <div class="sidebar-logo">
      <div class="logo-icon">
        <span class="logo-text">AK</span>
      </div>
      <span class="sidebar-title">AKNexus Hub</span>
    </div>

    <!-- 菜单区域 -->
    <div class="sidebar-menu">
      <template v-for="menu in menus" :key="menu.id">
        <div
          class="menu-item"
          :class="{ 'is-active': isMenuActive(menu) }"
          @click="handleMenuClick(menu)"
        >
          <el-icon v-if="menu.icon" class="menu-icon">
            <component :is="menu.icon" />
          </el-icon>
          <span class="menu-title">{{ menu.title }}</span>
        </div>

        <!-- 子菜单 -->
        <template v-if="menu.children && menu.children.length > 0">
          <div
            v-for="child in menu.children"
            :key="child.id"
            class="menu-item sub-menu-item"
            :class="{ 'is-active': isMenuActive(child) }"
            @click="handleMenuClick(child)"
          >
            <el-icon v-if="child.icon" class="menu-icon">
              <component :is="child.icon" />
            </el-icon>
            <span class="menu-title">{{ child.title }}</span>
          </div>
        </template>
      </template>
    </div>

    <!-- 底部区域 -->
    <div class="sidebar-footer">
      <!-- 版本信息或其它内容 -->
    </div>
  </aside>
</template>

<style scoped lang="scss">
.layout-sidebar {
  display: flex;
  flex-direction: column;
  width: 240px;
  height: 100%;
  background: var(--ds-menu-bar-bg);
  border-right: 1px solid var(--ds-border-light);
  transition: width 0.3s ease;
}

.sidebar-logo {
  display: flex;
  align-items: center;
  height: 50px;
  padding: 0 16px;
  border-bottom: 1px solid var(--ds-border-light);
}

.logo-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: var(--ds-primary);
  border-radius: 6px;
}

.logo-text {
  color: var(--ds-text-inverse);
  font-weight: 700;
  font-size: 14px;
}

.sidebar-title {
  margin-left: 12px;
  color: var(--ds-menu-bar-text);
  font-weight: 600;
  font-size: 14px;
}

.sidebar-menu {
  flex: 1;
  overflow-y: auto;
  padding: 8px 0;
}

.menu-item {
  display: flex;
  align-items: center;
  height: 40px;
  padding: 0 16px;
  margin: 2px 8px;
  color: var(--ds-menu-bar-text);
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s ease;

  &:hover {
    background: var(--ds-menu-bar-hover-bg);
    color: var(--ds-text-title);
  }

  &.is-active {
    background: var(--ds-menu-bar-active-bg);
    color: var(--ds-menu-bar-active-text);
    font-weight: 600;

    // 左侧激活指示条
    &::before {
      content: '';
      position: absolute;
      left: 0;
      top: 8px;
      bottom: 8px;
      width: 3px;
      background: var(--ds-primary);
      border-radius: 0 2px 2px 0;
    }
  }
}

.sub-menu-item {
  padding-left: 48px;
}

.menu-icon {
  margin-right: 8px;
  font-size: 16px;
}

.menu-title {
  font-size: 14px;
}

.sidebar-footer {
  height: 40px;
  border-top: 1px solid var(--ds-border-light);
}
</style>