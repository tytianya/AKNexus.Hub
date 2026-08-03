<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Delete, Refresh, Share } from '@element-plus/icons-vue'
import {
  getMacRanges,
  createMacRange,
  deleteMacRange,
  allocateMac,
  getMacAllocations,
  type MacRangeDto,
  type MacAllocationDto
} from '/@/api/mac'

defineOptions({ name: 'DasMac' })

// MAC 格式校验：AA:BB:CC:00:00:00
const MAC_RE = /^([0-9A-F]{2}:){5}[0-9A-F]{2}$/

// 地址段列表
const loading = ref(false)
const tableData = ref<MacRangeDto[]>([])

// 分配记录
const allocLoading = ref(false)
const allocData = ref<MacAllocationDto[]>([])

// 新建地址段
const createVisible = ref(false)
const createSubmitting = ref(false)
const createForm = reactive({
  rangeName: '',
  startMac: '',
  endMac: ''
})

// 分配 MAC
const allocateVisible = ref(false)
const allocateSubmitting = ref(false)
const allocateRow = ref<MacRangeDto | null>(null)
const allocateForm = reactive({ allocatedTo: '' })
const allocateResult = reactive({
  visible: false,
  success: false,
  text: ''
})

const loadRanges = async () => {
  loading.value = true
  try {
    tableData.value = await getMacRanges()
  } catch (e: any) {
    ElMessage.error(e.message || '加载MAC地址段失败')
  } finally {
    loading.value = false
  }
}

const loadAllocations = async () => {
  allocLoading.value = true
  try {
    allocData.value = await getMacAllocations()
  } catch (e: any) {
    ElMessage.error(e.message || '加载分配记录失败')
  } finally {
    allocLoading.value = false
  }
}

const resetCreate = () => {
  createForm.rangeName = ''
  createForm.startMac = ''
  createForm.endMac = ''
}

const openCreate = () => {
  resetCreate()
  createVisible.value = true
}

const handleCreate = async () => {
  if (!createForm.rangeName.trim()) {
    ElMessage.warning('请输入段名称')
    return
  }
  const start = createForm.startMac.trim().toUpperCase()
  const end = createForm.endMac.trim().toUpperCase()
  if (!MAC_RE.test(start)) {
    ElMessage.warning('起始MAC格式不正确，应为 AA:BB:CC:00:00:00')
    return
  }
  if (!MAC_RE.test(end)) {
    ElMessage.warning('结束MAC格式不正确，应为 AA:BB:CC:FF:FF:FF')
    return
  }

  createSubmitting.value = true
  try {
    await createMacRange({
      rangeName: createForm.rangeName.trim(),
      startMac: start,
      endMac: end
    })
    ElMessage.success('地址段创建成功')
    createVisible.value = false
    loadRanges()
  } catch (e: any) {
    ElMessage.error(e.message || '创建失败')
  } finally {
    createSubmitting.value = false
  }
}

const handleDelete = (row: MacRangeDto) => {
  ElMessageBox.confirm(
    `确认删除地址段「${row.rangeName}」吗？已分配的MAC记录将保留。`,
    '提示',
    {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    }
  )
    .then(async () => {
      try {
        await deleteMacRange(row.id)
        ElMessage.success('删除成功')
        loadRanges()
      } catch (e: any) {
        ElMessage.error(e.message || '删除失败')
      }
    })
    .catch(() => {})
}

const openAllocate = (row: MacRangeDto) => {
  allocateRow.value = row
  allocateForm.allocatedTo = ''
  allocateResult.visible = false
  allocateResult.success = false
  allocateResult.text = ''
  allocateVisible.value = true
}

const handleAllocate = async () => {
  if (!allocateRow.value) return
  if (allocateRow.value.remainingCount <= 0) {
    ElMessage.warning('该地址段已无可用MAC')
    return
  }
  allocateSubmitting.value = true
  allocateResult.visible = false
  try {
    const res: MacAllocationDto[] = await allocateMac({
      rangeName: allocateRow.value.rangeName,
      allocatedTo: allocateForm.allocatedTo.trim(),
      count: 1
    })
    if (res && res.length > 0) {
      allocateResult.success = true
      allocateResult.text = res.map((a) => a.macAddress).join(', ')
    } else {
      allocateResult.success = false
      allocateResult.text = '该地址段已无可用MAC地址'
    }
    allocateResult.visible = true
    loadRanges()
    loadAllocations()
  } catch (e: any) {
    allocateResult.success = false
    allocateResult.text = e.message || '分配失败'
    allocateResult.visible = true
  } finally {
    allocateSubmitting.value = false
  }
}

const formatTime = (t?: string | null) => {
  if (!t) return '-'
  const d = new Date(t)
  if (isNaN(d.getTime())) return t
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

onMounted(() => {
  loadRanges()
  loadAllocations()
})
</script>

<template>
  <div class="das-page">
    <!-- MAC地址段管理 -->
    <el-card shadow="never" class="panel-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">MAC地址段管理</span>
          <div class="header-actions">
            <el-button :icon="Refresh" @click="loadRanges">刷新</el-button>
            <el-button type="primary" :icon="Plus" @click="openCreate">新建地址段</el-button>
          </div>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData" border stripe style="width: 100%">
        <el-table-column label="名称" prop="rangeName" min-width="140" show-overflow-tooltip />
        <el-table-column label="起始MAC" min-width="180">
          <template #default="{ row }">
            <span class="mac-mono">{{ row.startMac }}</span>
          </template>
        </el-table-column>
        <el-table-column label="结束MAC" min-width="180">
          <template #default="{ row }">
            <span class="mac-mono">{{ row.endMac }}</span>
          </template>
        </el-table-column>
        <el-table-column label="当前指针" min-width="180">
          <template #default="{ row }">
            <span class="mac-mono">{{ row.nextMac || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="总数" prop="totalCount" width="90" align="center" />
        <el-table-column label="已分配" prop="allocatedCount" width="90" align="center" />
        <el-table-column label="剩余" width="100" align="center">
          <template #default="{ row }">
            <el-tag
              :type="row.remainingCount <= 0 ? 'danger' : row.remainingCount < 10 ? 'warning' : 'success'"
              effect="light"
            >
              {{ row.remainingCount }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" align="center" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link size="small" :icon="Share" @click="openAllocate(row as MacRangeDto)">分配</el-button>
            <el-button type="danger" link size="small" :icon="Delete" @click="handleDelete(row as MacRangeDto)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 分配记录 -->
    <el-card shadow="never" class="panel-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">分配记录</span>
          <el-button :icon="Refresh" @click="loadAllocations">刷新</el-button>
        </div>
      </template>

      <el-table v-loading="allocLoading" :data="allocData" border stripe style="width: 100%">
        <el-table-column label="MAC地址" min-width="180">
          <template #default="{ row }">
            <span class="mac-mono">{{ row.macAddress }}</span>
          </template>
        </el-table-column>
        <el-table-column label="地址段" prop="rangeName" min-width="140" show-overflow-tooltip />
        <el-table-column label="分配对象" min-width="140">
          <template #default="{ row }">
            <span>{{ row.allocatedTo || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="时间" min-width="180">
          <template #default="{ row }">
            <span>{{ formatTime(row.allocatedAt) }}</span>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新建地址段弹窗 -->
    <el-dialog
      v-model="createVisible"
      title="新建地址段"
      width="480px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form :model="createForm" label-width="90px">
        <el-form-item label="段名称" required>
          <el-input v-model="createForm.rangeName" placeholder="请输入段名称" />
        </el-form-item>
        <el-form-item label="起始MAC" required>
          <el-input v-model="createForm.startMac" placeholder="AA:BB:CC:00:00:00" class="mac-input" />
        </el-form-item>
        <el-form-item label="结束MAC" required>
          <el-input v-model="createForm.endMac" placeholder="AA:BB:CC:FF:FF:FF" class="mac-input" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="createSubmitting" @click="handleCreate">确定</el-button>
      </template>
    </el-dialog>

    <!-- 分配 MAC 弹窗 -->
    <el-dialog
      v-model="allocateVisible"
      title="分配MAC地址"
      width="480px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-descriptions :column="1" border size="small" class="alloc-desc">
        <el-descriptions-item label="地址段">{{ allocateRow?.rangeName }}</el-descriptions-item>
        <el-descriptions-item label="起始MAC">
          <span class="mac-mono">{{ allocateRow?.startMac }}</span>
        </el-descriptions-item>
        <el-descriptions-item label="结束MAC">
          <span class="mac-mono">{{ allocateRow?.endMac }}</span>
        </el-descriptions-item>
        <el-descriptions-item label="可用剩余">{{ allocateRow?.remainingCount }}</el-descriptions-item>
      </el-descriptions>

      <el-form :model="allocateForm" label-width="90px" style="margin-top: 16px">
        <el-form-item label="分配对象">
          <el-input v-model="allocateForm.allocatedTo" placeholder="选填，如 设备SN / 工位" />
        </el-form-item>
      </el-form>

      <div
        v-if="allocateResult.visible"
        class="result-panel"
        :class="allocateResult.success ? 'success' : 'error'"
      >
        <div class="result-label">{{ allocateResult.success ? '分配成功' : '分配失败' }}</div>
        <div class="result-text" :class="{ 'mac-mono': allocateResult.success }">
          {{ allocateResult.text }}
        </div>
      </div>

      <template #footer>
        <el-button @click="allocateVisible = false">关闭</el-button>
        <el-button
          type="primary"
          :loading="allocateSubmitting"
          :disabled="!allocateRow || allocateRow.remainingCount <= 0"
          @click="handleAllocate"
        >
          分配
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.panel-card {
  background: var(--ds-bg-panel, #ffffff);
  border: 1px solid var(--ds-border-light, #ebeef5);
  border-radius: var(--ds-card-radius, 8px);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;

  .header-actions {
    display: flex;
    align-items: center;
    gap: 8px;
  }
}

.card-title {
  font-weight: 600;
  color: var(--ds-text-title, #303133);
}

.mac-mono {
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  letter-spacing: 0.5px;
}

.mac-input :deep(.el-input__inner) {
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  text-transform: uppercase;
}

.alloc-desc {
  margin-bottom: 4px;
}

.result-panel {
  margin-top: 8px;
  padding: 12px 16px;
  border-radius: 6px;
  border: 1px solid;

  .result-label {
    font-weight: 600;
    margin-bottom: 4px;
  }

  .result-text {
    word-break: break-all;
    font-size: 15px;
  }

  &.success {
    background: var(--el-color-success-light-9, #f0f9eb);
    border-color: var(--el-color-success-light-5, #b3e19d);
    color: var(--el-color-success, #67c23a);

    .result-text {
      color: var(--el-color-success-dark-2, #529b2e);
      font-weight: 600;
    }
  }

  &.error {
    background: var(--el-color-danger-light-9, #fef0f0);
    border-color: var(--el-color-danger-light-5, #fbc4c4);
    color: var(--el-color-danger, #f56c6c);

    .result-text {
      color: var(--el-color-danger-dark-2, #c45656);
    }
  }
}
</style>
