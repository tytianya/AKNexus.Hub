<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getStations,
  createStation,
  updateStation,
  deleteStation,
  type Station,
  type StationCreateDto
} from '/@/api/station'

defineOptions({ name: 'DasStations' })

const loading = ref(false)
const submitting = ref(false)
const tableData = ref<Station[]>([])

const dialogVisible = ref(false)
const isEdit = ref(false)

const form = reactive({
  id: 0,
  stationCode: '',
  stationName: '',
  stationType: 'Check',
  location: '',
  sortOrder: 0
})

const stationTypeLabels: Record<string, string> = {
  Check: '检验',
  Process: '加工',
  Test: '测试',
  Pack: '包装'
}

const stationTypeOptions = [
  { value: 'Check', label: 'Check 检验' },
  { value: 'Process', label: 'Process 加工' },
  { value: 'Test', label: 'Test 测试' },
  { value: 'Pack', label: 'Pack 包装' }
]

const stationTypeLabel = (t: string) => stationTypeLabels[t] || t || '-'

const loadData = async () => {
  loading.value = true
  try {
    tableData.value = await getStations()
  } catch (e: any) {
    ElMessage.error(e.message || '加载失败')
  } finally {
    loading.value = false
  }
}

const resetForm = () => {
  form.id = 0
  form.stationCode = ''
  form.stationName = ''
  form.stationType = 'Check'
  form.location = ''
  form.sortOrder = 0
}

const openCreate = () => {
  isEdit.value = false
  resetForm()
  dialogVisible.value = true
}

const openEdit = (row: Station) => {
  isEdit.value = true
  form.id = row.id
  form.stationCode = row.stationCode
  form.stationName = row.stationName
  form.stationType = row.stationType
  form.location = row.location
  form.sortOrder = row.sortOrder
  dialogVisible.value = true
}

const handleSave = async () => {
  if (!form.stationCode.trim()) {
    ElMessage.warning('请输入站点编码')
    return
  }
  if (!form.stationName.trim()) {
    ElMessage.warning('请输入站点名称')
    return
  }

  const payload: StationCreateDto = {
    stationCode: form.stationCode.trim(),
    stationName: form.stationName.trim(),
    stationType: form.stationType,
    location: form.location.trim(),
    sortOrder: form.sortOrder
  }

  submitting.value = true
  try {
    if (isEdit.value) {
      await updateStation(form.id, payload)
      ElMessage.success('更新成功')
    } else {
      await createStation(payload)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (e: any) {
    ElMessage.error(e.message || '保存失败')
  } finally {
    submitting.value = false
  }
}

const handleDelete = (row: Station) => {
  ElMessageBox.confirm(`确认删除站点「${row.stationName}」吗？`, '提示', {
    type: 'warning',
    confirmButtonText: '确定',
    cancelButtonText: '取消'
  })
    .then(async () => {
      try {
        await deleteStation(row.id)
        ElMessage.success('删除成功')
        loadData()
      } catch (e: any) {
        ElMessage.error(e.message || '删除失败')
      }
    })
    .catch(() => {})
}

onMounted(() => loadData())
</script>

<template>
  <div class="das-page">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>站点管理</span>
          <el-button type="primary" @click="openCreate">新建站点</el-button>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData" border stripe style="width: 100%">
        <el-table-column label="站点编码" prop="stationCode" min-width="140" show-overflow-tooltip />
        <el-table-column label="站点名称" prop="stationName" min-width="160" show-overflow-tooltip />
        <el-table-column label="站点类型" width="120" align="center">
          <template #default="{ row }">
            <el-tag type="info" effect="light">{{ stationTypeLabel(row.stationType) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="位置" prop="location" min-width="140" show-overflow-tooltip>
          <template #default="{ row }">
            <span>{{ row.location || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="排序号" prop="sortOrder" width="100" align="center" />
        <el-table-column label="操作" width="150" align="center" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link size="small" @click="openEdit(row as Station)">编辑</el-button>
            <el-button type="danger" link size="small" @click="handleDelete(row as Station)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑站点' : '新建站点'"
      width="500px"
      destroy-on-close
    >
      <el-form :model="form" label-width="100px">
        <el-form-item label="站点编码" required>
          <el-input
            v-model="form.stationCode"
            placeholder="请输入站点编码"
            :disabled="isEdit"
          />
        </el-form-item>
        <el-form-item label="站点名称" required>
          <el-input v-model="form.stationName" placeholder="请输入站点名称" />
        </el-form-item>
        <el-form-item label="站点类型" required>
          <el-select v-model="form.stationType" style="width: 100%">
            <el-option
              v-for="o in stationTypeOptions"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="位置">
          <el-input v-model="form.location" placeholder="请输入位置" />
        </el-form-item>
        <el-form-item label="排序号">
          <el-input-number v-model="form.sortOrder" :min="0" :step="1" controls-position="right" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSave">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;

  .card-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-weight: 600;
  }
}
</style>
