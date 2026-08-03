<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { queryLogs, type QueryRequest, type ProductStationLogDto } from '/@/api/query'
import { getStations, type Station } from '/@/api/station'

defineOptions({ name: 'DasQuery' })

const loading = ref(false)
const tableData = ref<ProductStationLogDto[]>([])
const stations = ref<Station[]>([])

const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const searchForm = reactive({
  productCode: '',
  stationCode: '',
  passResult: '',
  startTime: '',
  endTime: ''
})

const resultOptions = [
  { value: '', label: '全部' },
  { value: 'Pass', label: 'Pass 通过' },
  { value: 'Block', label: 'Block 卡关' },
  { value: 'Skip', label: 'Skip 跳过' }
]

const passResultTag = (r: string): { type: 'success' | 'danger' | 'primary' | 'info'; text: string } => {
  switch (r) {
    case 'Pass':
      return { type: 'success', text: '通过' }
    case 'Block':
      return { type: 'danger', text: '卡关' }
    case 'Skip':
      return { type: 'primary', text: '跳过' }
    default:
      return { type: 'info', text: r || '-' }
  }
}

const loadStations = async () => {
  try {
    stations.value = await getStations()
  } catch (e: any) {
    ElMessage.error(e.message || '站点加载失败')
  }
}

const loadData = async () => {
  loading.value = true
  try {
    const req: QueryRequest = {
      page: page.value,
      pageSize: pageSize.value
    }
    if (searchForm.productCode.trim()) req.productCode = searchForm.productCode.trim()
    if (searchForm.stationCode) req.stationCode = searchForm.stationCode
    if (searchForm.passResult) req.passResult = searchForm.passResult
    if (searchForm.startTime) req.startTime = searchForm.startTime
    if (searchForm.endTime) req.endTime = searchForm.endTime

    const res = await queryLogs(req)
    tableData.value = res.items || []
    total.value = res.total || 0
  } catch (e: any) {
    ElMessage.error(e.message || '查询失败')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  page.value = 1
  loadData()
}

const handleReset = () => {
  searchForm.productCode = ''
  searchForm.stationCode = ''
  searchForm.passResult = ''
  searchForm.startTime = ''
  searchForm.endTime = ''
  page.value = 1
  loadData()
}

const handlePageChange = (p: number) => {
  page.value = p
  loadData()
}

const handleSizeChange = (s: number) => {
  pageSize.value = s
  page.value = 1
  loadData()
}

onMounted(() => {
  loadStations()
  loadData()
})
</script>

<template>
  <div class="das-page">
    <el-card shadow="never" class="search-card">
      <el-form :model="searchForm" inline label-width="80px">
        <el-form-item label="产品编码">
          <el-input
            v-model="searchForm.productCode"
            placeholder="请输入产品编码"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="站点">
          <el-select
            v-model="searchForm.stationCode"
            placeholder="全部站点"
            clearable
            style="width: 200px"
          >
            <el-option
              v-for="s in stations"
              :key="s.id"
              :label="`${s.stationCode} - ${s.stationName}`"
              :value="s.stationCode"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="结果">
          <el-select
            v-model="searchForm.passResult"
            placeholder="全部"
            clearable
            style="width: 160px"
          >
            <el-option
              v-for="o in resultOptions"
              :key="o.value || 'all'"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="开始时间">
          <el-date-picker
            v-model="searchForm.startTime"
            type="datetime"
            placeholder="开始时间"
            value-format="YYYY-MM-DD HH:mm:ss"
            style="width: 200px"
          />
        </el-form-item>
        <el-form-item label="结束时间">
          <el-date-picker
            v-model="searchForm.endTime"
            type="datetime"
            placeholder="结束时间"
            value-format="YYYY-MM-DD HH:mm:ss"
            style="width: 200px"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never" class="result-card">
      <template #header>
        <div class="card-header">
          <span>记录查询</span>
          <span class="total-text">共 {{ total }} 条</span>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData" border stripe style="width: 100%">
        <el-table-column label="产品编码" prop="productCode" min-width="140" show-overflow-tooltip />
        <el-table-column label="产品名称" prop="productName" min-width="140" show-overflow-tooltip />
        <el-table-column label="站点" min-width="130" show-overflow-tooltip>
          <template #default="{ row }">
            <span>{{ row.stationName || row.stationCode || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="流程" prop="flowCode" min-width="120" show-overflow-tooltip />
        <el-table-column label="结果" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="passResultTag(row.passResult).type" effect="light">
              {{ passResultTag(row.passResult).text }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作员" prop="operator" min-width="100" show-overflow-tooltip />
        <el-table-column label="备注" prop="remark" min-width="140" show-overflow-tooltip />
        <el-table-column label="时间" prop="passTime" min-width="160" show-overflow-tooltip />
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          :current-page="page"
          :page-size="pageSize"
          :total="total"
          :page-sizes="[20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          background
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;

  .search-card {
    margin-bottom: 16px;
  }

  .card-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-weight: 600;

    .total-text {
      font-size: 13px;
      font-weight: 400;
      color: var(--el-text-color-secondary);
    }
  }

  .pagination-wrap {
    display: flex;
    justify-content: flex-end;
    margin-top: 16px;
  }
}
</style>
