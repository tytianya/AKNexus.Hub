<script setup lang="ts">
/**
 * 顶部工具栏组件
 */
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { toggleDarkMode, isDarkMode } from '/@/skins'
import { useAuthStore } from '/@/modules/auth/store'

defineOptions({ name: 'LayoutHeader' })

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// isDarkMode() 返回 ComputedRef<boolean>，在 setup 中解包后模板才能正确使用
const darkMode = isDarkMode()

// 面包屑数据
const breadcrumbs = computed(() => {
  const matched = route.matched.filter(item => item.meta?.title)
  return matched.map(item => ({
    title: item.meta?.title as string,
    path: item.path
  }))
})

// 切换主题
const handleToggleTheme = () => {
  toggleDarkMode()
}

// 退出登录
const handleLogout = async () => {
  await authStore.logout()
  router.push('/')
}
</script>

<template>
  <header class="layout-header">
    <!-- 左侧：面包屑 -->
    <div class="header-left">
      <el-breadcrumb separator="/">
        <el-breadcrumb-item v-for="(item, index) in breadcrumbs" :key="item.path">
          <span v-if="index === breadcrumbs.length - 1" class="breadcrumb-current">
            {{ item.title }}
          </span>
          <router-link v-else :to="item.path" class="breadcrumb-link">
            {{ item.title }}
          </router-link>
        </el-breadcrumb-item>
      </el-breadcrumb>
    </div>

    <!-- 右侧：工具栏 -->
    <div class="header-right">
      <!-- 主题切换 -->
      <el-tooltip :content="darkMode ? '切换浅色模式' : '切换深色模式'" placement="bottom">
        <el-button text @click="handleToggleTheme">
          <el-icon :size="18">
            <component :is="darkMode ? 'Sunny' : 'Moon'" />
          </el-icon>
        </el-button>
      </el-tooltip>

      <!-- 用户头像 -->
      <el-dropdown trigger="click">
        <div class="user-avatar">
          <el-avatar :size="32" class="avatar">
            <el-icon :size="18"><User /></el-icon>
          </el-avatar>
          <span v-if="authStore.isLoggedIn" class="user-name">
            {{ authStore.user?.nickname || authStore.user?.username }}
          </span>
          <el-tag v-if="authStore.isSuperAdmin" type="danger" size="small" effect="plain" class="super-tag">
            超管
          </el-tag>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item>个人设置</el-dropdown-item>
            <el-dropdown-item @click="router.push('/system/menu')">菜单配置</el-dropdown-item>
            <el-dropdown-item divided @click="handleLogout">退出登录</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </header>
</template>

<style scoped lang="scss">
.layout-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 50px;
  padding: 0 16px;
  background: var(--ds-toolbar-bg);
  border-bottom: 1px solid var(--ds-toolbar-border-color);
}

.header-left {
  display: flex;
  align-items: center;
}

.breadcrumb-current {
  color: var(--ds-text-primary);
  font-weight: 500;
}

.breadcrumb-link {
  color: var(--ds-text-secondary);
  text-decoration: none;

  &:hover {
    color: var(--ds-primary);
  }
}

.header-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-avatar {
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-name {
  font-size: 13px;
  color: var(--ds-text-primary);
}

.super-tag {
  margin-left: 2px;
}

.avatar {
  background: var(--ds-primary-bg);
  color: var(--ds-primary);
}
</style>