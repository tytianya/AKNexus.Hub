<script setup lang="ts">
/**
 * 404 页面
 */
import { useRouter } from 'vue-router'

defineOptions({ name: 'NotFoundPage' })

const router = useRouter()

const goHome = () => {
  router.push('/')
}
</script>

<template>
  <div class="error-page">
    <div class="error-content">
      <h1 class="error-code">404</h1>
      <h2 class="error-title">页面不存在</h2>
      <p class="error-description">抱歉，您访问的页面不存在或已被移除</p>
      <el-button type="primary" @click="goHome">返回首页</el-button>
    </div>
  </div>
</template>

<style scoped lang="scss">
.error-page {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  background: var(--ds-bg-base);
}

.error-content {
  text-align: center;
}

.error-code {
  font-size: 120px;
  font-weight: 700;
  color: var(--ds-primary);
  line-height: 1;
  margin-bottom: 16px;
}

.error-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--ds-text-title);
  margin-bottom: 8px;
}

.error-description {
  font-size: 14px;
  color: var(--ds-text-secondary);
  margin-bottom: 24px;
}
</style>