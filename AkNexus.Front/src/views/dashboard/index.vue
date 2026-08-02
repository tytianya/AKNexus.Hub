<script setup lang="ts">
/**
 * 仪表盘页面
 */
import { ref, onMounted } from 'vue'
import { isDarkMode } from '/@/skins'

defineOptions({ name: 'DashboardPage' })

const stats = ref([
  { label: '用户总数', value: '1,234', icon: 'User' },
  { label: '活跃设备', value: '856', icon: 'Monitor' },
  { label: '告警数量', value: '23', icon: 'Bell' },
  { label: '运行时长', value: '99.9%', icon: 'Timer' }
])
</script>

<template>
  <div class="dashboard-page">
    <div class="page-header">
      <h1 class="page-title">仪表盘</h1>
      <p class="page-description">欢迎使用 AKNexus Hub 管理平台</p>
    </div>

    <!-- 统计卡片 -->
    <div class="stats-grid">
      <el-card v-for="stat in stats" :key="stat.label" class="stat-card">
        <div class="stat-content">
          <div class="stat-icon">
            <el-icon :size="24">
              <component :is="stat.icon" />
            </el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 主要内容区 -->
    <el-card class="content-card">
      <template #header>
        <span class="card-title">系统概览</span>
      </template>
      <div class="content-placeholder">
        <el-empty description="模块化设计，后续扩展" />
      </div>
    </el-card>
  </div>
</template>

<style scoped lang="scss">
.dashboard-page {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-header {
  margin-bottom: 8px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--ds-text-title);
  margin-bottom: 4px;
}

.page-description {
  font-size: 14px;
  color: var(--ds-text-secondary);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;

  @media (max-width: 1200px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 640px) {
    grid-template-columns: 1fr;
  }
}

.stat-card {
  background: var(--ds-bg-panel);
  border: 1px solid var(--ds-border-light);
  border-radius: var(--ds-card-radius);
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  background: var(--ds-primary-bg);
  color: var(--ds-primary);
  border-radius: 8px;
}

.stat-info {
  flex: 1;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
  color: var(--ds-text-title);
}

.stat-label {
  font-size: 14px;
  color: var(--ds-text-secondary);
}

.content-card {
  flex: 1;
  background: var(--ds-bg-panel);
  border: 1px solid var(--ds-border-light);
  border-radius: var(--ds-card-radius);
}

.card-title {
  font-weight: 600;
  color: var(--ds-text-title);
}

.content-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
}
</style>