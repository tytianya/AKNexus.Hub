<script setup lang="ts">
/**
 * 仪表盘页面 - 驾驶舱看板
 * 对接真实 API：产品数、站点数、来料数、MAC 地址池、过站记录
 */
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { getProducts } from '/@/api/product'
import { getStations } from '/@/api/station'
import { getIncomingMaterials } from '/@/api/incomingMaterial'
import { getMacRanges, type MacRangeDto } from '/@/api/mac'
import { getBusinessLogs, type BusinessLogDto, BusinessStatus } from '/@/api/businessLog'

defineOptions({ name: 'DashboardPage' })

const router = useRouter()

// 加载状态
const loading = ref(false)

// 统计数据
const productCount = ref(0)
const stationCount = ref(0)
const incomingCount = ref(0)
const macRemaining = ref(0)
const macTotal = ref(0)

// 最近业务日志
const recentLogs = ref<BusinessLogDto[]>([])

// 统计卡片
const stats = computed(() => [
  { label: '产品总数', value: productCount.value, icon: 'Box', color: '#409eff', path: '/das/products' },
  { label: '站点数量', value: stationCount.value, icon: 'OfficeBuilding', color: '#67c23a', path: '/das/stations' },
  { label: '来料批次', value: incomingCount.value, icon: 'Goods', color: '#e6a23c', path: '/das/incoming' },
  { label: 'MAC剩余', value: `${macRemaining.value}/${macTotal.value}`, icon: 'Connection', color: '#f56c6c', path: '/das/mac' }
])

// 日志状态标签
const logStatusType = (s: BusinessStatus) => s === BusinessStatus.Success ? 'success' : 'danger'
const logStatusText = (s: BusinessStatus) => s === BusinessStatus.Success ? '成功' : '失败'

const formatTime = (t: string) => {
  const d = new Date(t)
  if (isNaN(d.getTime())) return t
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const handleStatClick = (path: string) => {
  router.push(path)
}

const loadData = async () => {
  loading.value = true
  try {
    const [products, stations, incomings, macRanges, logs] = await Promise.all([
      getProducts().catch(() => []),
      getStations().catch(() => []),
      getIncomingMaterials().catch(() => []),
      getMacRanges().catch(() => []),
      getBusinessLogs({ pageNumber: 1, pageSize: 8 }).catch(() => ({ items: [], totalCount: 0 }))
    ])

    productCount.value = (products as any[]).length
    stationCount.value = (stations as any[]).length
    incomingCount.value = (incomings as any[]).length

    const macData = macRanges as MacRangeDto[]
    macTotal.value = macData.reduce((s, r) => s + r.totalCount, 0)
    macRemaining.value = macData.reduce((s, r) => s + r.remainingCount, 0)

    recentLogs.value = (logs as any).items ?? []
  } catch {
    // 忽略全局错误
  } finally {
    loading.value = false
  }
}

onMounted(() => loadData())
</script>

<template>
  <div class="dashboard-page" v-loading="loading">
    <!-- 统计卡片 -->
    <div class="stats-grid">
      <el-card
        v-for="stat in stats"
        :key="stat.label"
        class="stat-card"
        shadow="hover"
        @click="handleStatClick(stat.path)"
      >
        <div class="stat-content">
          <div class="stat-icon" :style="{ background: stat.color + '20', color: stat.color }">
            <el-icon :size="28">
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

    <!-- 最近业务日志 -->
    <el-card class="content-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span class="card-title">最近业务日志</span>
          <el-button text type="primary" @click="router.push('/das/business-log')">查看全部</el-button>
        </div>
      </template>

      <el-table v-if="recentLogs.length" :data="recentLogs" border stripe size="default">
        <el-table-column label="业务类型" prop="typeName" width="120" align="center">
          <template #default="{ row }">
            <el-tag size="small" effect="plain">{{ row.typeName }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="业务编码" prop="code" width="120" />
        <el-table-column label="业务内容" prop="content" min-width="200" show-overflow-tooltip />
        <el-table-column label="状态" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="logStatusType(row.status)" size="small" effect="light">
              {{ logStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时间" width="140">
          <template #default="{ row }">
            <span>{{ formatTime(row.createdAt) }}</span>
          </template>
        </el-table-column>
      </el-table>

      <el-empty v-else description="暂无业务日志" :image-size="80" />
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

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;

  @media (max-width: 1200px) {
    grid-template-columns: repeat(2, 1fr);
  }
}

.stat-card {
  cursor: pointer;
  transition: transform 0.15s ease;

  &:hover {
    transform: translateY(-2px);
  }
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
  width: 56px;
  height: 56px;
  border-radius: 10px;
  flex-shrink: 0;
}

.stat-value {
  font-size: 26px;
  font-weight: 700;
  color: var(--ds-text-title, #303133);
  line-height: 1.2;
}

.stat-label {
  font-size: 13px;
  color: var(--ds-text-secondary, #909399);
  margin-top: 4px;
}

.content-card {
  flex: 1;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-weight: 600;
  color: var(--ds-text-title, #303133);
}
</style>
