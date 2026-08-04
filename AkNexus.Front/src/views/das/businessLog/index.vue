<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Refresh } from '@element-plus/icons-vue'
import {
  getBusinessLogs,
  pushBusinessLog,
  businessTypeOptions,
  businessStatusOptions,
  businessTypeName,
  businessStatusName,
  BusinessType,
  BusinessStatus,
  type BusinessLogDto,
  type BusinessLogQuery
} from '/@/api/businessLog'

defineOptions({ name: 'DasBusinessLog' })

const loading = ref(false)
const tableData = ref<BusinessLogDto[]>([])
const total = ref(0)

const query = reactive<BusinessLogQuery>({
  type: undefined,
  status: undefined,
  keyword: '',
  pageNumber: 1,
  pageSize: 20
})

// 推送弹窗
const pushVisible = ref(false)
const pushLoading = ref(false)
const pushForm = reactive({
  type: BusinessType.BarcodeTest,
  content: '',
  remark: '',
  status: BusinessStatus.Success
})

const loadData = async () => {
  loading.value = true
  try {
    const res = await getBusinessLogs(query)
    tableData.value = res.items
    total.value = res.totalCount
  } catch (e: any) {
    ElMessage.error(e.message || '加载失败')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  query.pageNumber = 1
  loadData()
}

const handlePageChange = (page: number) => {
  query.pageNumber = page
  loadData()
}

const handleSizeChange = (size: number) => {
  query.pageSize = size
  query.pageNumber = 1
  loadData()
}

const openPush = () => {
  pushForm.type = BusinessType.BarcodeTest
  pushForm.content = ''
  pushForm.remark = ''
  pushForm.status = BusinessStatus.Success
  pushVisible.value = true
}

const handlePush = async () => {
  if (!pushForm.content) {
    ElMessage.warning('请输入业务内容')
    return
  }
  pushLoading.value = true
  try {
    await pushBusinessLog({
      type: pushForm.type,
      content: pushForm.content,
      remark: pushForm.remark || undefined,
      status: pushForm.status
    })
    ElMessage.success('推送成功')
    pushVisible.value = false
    loadData()
  } catch (e: any) {
    ElMessage.error(e.message || '推送失败')
  } finally {
    pushLoading.value = false
  }
}

const statusTagType = (s: BusinessStatus) =>
  s === BusinessStatus.Success ? 'success' : 'danger'

const formatTime = (t: string) => {
  const d = new Date(t)
  if (isNaN(d.getTime())) return t
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

onMounted(() => loadData())
</script>

<template>
  <div class="das-page">
    <el-card shadow="never" class="panel-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">业务日志</span>
          <div class="header-actions">
            <el-button :icon="Refresh" @click="loadData">刷新</el-button>
            <el-button type="primary" @click="openPush">推送日志</el-button>
          </div>
        </div>
      </template>

      <!-- 筛选 -->
      <div class="filter-bar">
        <el-select v-model="query.type" placeholder="业务类型" clearable style="width: 140px">
          <el-option
            v-for="o in businessTypeOptions"
            :key="o.value"
            :label="o.label"
            :value="o.value"
          />
        </el-select>
        <el-select v-model="query.status" placeholder="状态" clearable style="width: 120px">
          <el-option
            v-for="o in businessStatusOptions"
            :key="o.value"
            :label="o.label"
            :value="o.value"
          />
        </el-select>
        <el-input
          v-model="query.keyword"
          placeholder="搜索业务编码/内容"
          clearable
          style="width: 220px"
          @keyup.enter="handleSearch"
        />
        <el-button type="primary" @click="handleSearch">查询</el-button>
      </div>

      <!-- 列表 -->
      <el-table v-loading="loading" :data="tableData" border stripe style="width: 100%">
        <el-table-column label="ID" prop="id" width="70" align="center" />
        <el-table-column label="业务类型" min-width="110" align="center">
          <template #default="{ row }">
            <el-tag effect="plain">{{ row.typeName }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="业务编码" prop="code" width="120" />
        <el-table-column label="业务内容" prop="content" min-width="240" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="mono-text">{{ row.content }}</span>
          </template>
        </el-table-column>
        <el-table-column label="描述" prop="remark" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            <span>{{ row.remark || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="statusTagType(row.status)" effect="light">
              {{ businessStatusName(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时间" width="180">
          <template #default="{ row }">
            <span>{{ formatTime(row.createdAt) }}</span>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pager">
        <el-pagination
          v-model:current-page="query.pageNumber"
          v-model:page-size="query.pageSize"
          :total="total"
          :page-sizes="[20, 50, 100]"
          layout="total, sizes, prev, pager, next"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </el-card>

    <!-- 推送弹窗 -->
    <el-dialog v-model="pushVisible" title="推送业务日志" width="520px" :close-on-click-modal="false" destroy-on-close>
      <el-form :model="pushForm" label-width="90px">
        <el-form-item label="业务类型">
          <el-select v-model="pushForm.type" style="width: 100%">
            <el-option
              v-for="o in businessTypeOptions"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="业务内容">
          <el-input
            v-model="pushForm.content"
            type="textarea"
            :rows="3"
            placeholder="条码、JSON 字符串等"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-radio-group v-model="pushForm.status">
            <el-radio :value="1">成功</el-radio>
            <el-radio :value="2">失败</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="pushForm.remark" placeholder="选填" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="pushVisible = false">取消</el-button>
        <el-button type="primary" :loading="pushLoading" @click="handlePush">推送</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;
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

.filter-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.mono-text {
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  letter-spacing: 0.3px;
}

.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
}
</style>
