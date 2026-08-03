<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Plus,
  Edit,
  Delete,
  CircleClose,
  ArrowRight,
  Top,
  Bottom
} from '@element-plus/icons-vue'
import {
  getFlows,
  getFlowById,
  createFlow,
  updateFlow,
  deleteFlow,
  type FlowDto,
  type FlowStationDto
} from '/@/api/stationFlow'
import { getStations, type Station } from '/@/api/station'

defineOptions({ name: 'DasFlow' })

// 列表数据
const loading = ref(false)
const tableData = ref<FlowDto[]>([])
const stations = ref<Station[]>([])

// 弹窗状态
const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const submitting = ref(false)
const detailLoading = ref(false)
const currentId = ref<number | null>(null)

// 表单
const form = reactive({
  flowCode: '',
  flowName: '',
  description: '',
  isActive: true
})

// 站点步骤构建器
const stepStationId = ref<number | null>(null)
const stepRequired = ref(true)
interface StepItem extends FlowStationDto {
  stationName?: string
}
const steps = ref<StepItem[]>([])

// 站点步骤排序后的副本（用于表格展示）
const sortedSteps = (list?: FlowStationDto[]) =>
  [...(list || [])].sort((a, b) => a.order - b.order)

// 站点名称查找
const stationNameOf = (code: string) =>
  stations.value.find(s => s.stationCode === code)?.stationName || code

// 加载流程列表
const loadData = async () => {
  loading.value = true
  try {
    tableData.value = await getFlows()
  } catch (e: any) {
    ElMessage.error(e.message || '加载流程列表失败')
  } finally {
    loading.value = false
  }
}

// 加载站点列表
const loadStations = async () => {
  try {
    stations.value = await getStations()
  } catch (e: any) {
    ElMessage.error(e.message || '加载站点列表失败')
  }
}

// 重置表单
const resetForm = () => {
  form.flowCode = ''
  form.flowName = ''
  form.description = ''
  form.isActive = true
  stepStationId.value = null
  stepRequired.value = true
  steps.value = []
  currentId.value = null
}

// 打开新建
const openCreate = () => {
  resetForm()
  isEdit.value = false
  dialogTitle.value = '新建流程'
  dialogVisible.value = true
}

// 打开编辑
const openEdit = async (row: FlowDto) => {
  resetForm()
  isEdit.value = true
  dialogTitle.value = '编辑流程'
  currentId.value = row.id
  dialogVisible.value = true
  detailLoading.value = true
  try {
    const detail = await getFlowById(row.id)
    form.flowCode = detail.flowCode
    form.flowName = detail.flowName
    form.description = detail.description || ''
    form.isActive = detail.isActive
    steps.value = sortedSteps(detail.stations).map(s => ({ ...s }))
  } catch (e: any) {
    ElMessage.error(e.message || '加载流程详情失败')
    dialogVisible.value = false
  } finally {
    detailLoading.value = false
  }
}

// 添加步骤
const addStep = () => {
  if (stepStationId.value == null) {
    ElMessage.warning('请选择站点')
    return
  }
  const station = stations.value.find(s => s.id === stepStationId.value)
  if (!station) return
  if (steps.value.some(s => s.stationId === station.id)) {
    ElMessage.warning('该站点已添加')
    return
  }
  steps.value.push({
    stationId: station.id,
    stationCode: station.stationCode,
    order: steps.value.length + 1,
    required: stepRequired.value,
    stationName: station.stationName
  })
  stepStationId.value = null
  stepRequired.value = true
}

// 移除步骤
const removeStep = (index: number) => {
  steps.value.splice(index, 1)
  steps.value.forEach((s, i) => (s.order = i + 1))
}

// 上移
const moveUp = (index: number) => {
  if (index === 0) return
  const arr = steps.value
  ;[arr[index - 1], arr[index]] = [arr[index], arr[index - 1]]
  arr.forEach((s, i) => (s.order = i + 1))
}

// 下移
const moveDown = (index: number) => {
  const arr = steps.value
  if (index === arr.length - 1) return
  ;[arr[index + 1], arr[index]] = [arr[index], arr[index + 1]]
  arr.forEach((s, i) => (s.order = i + 1))
}

// 提交
const submit = async () => {
  if (!form.flowCode.trim()) {
    ElMessage.warning('请输入流程编码')
    return
  }
  if (!form.flowName.trim()) {
    ElMessage.warning('请输入流程名称')
    return
  }
  if (steps.value.length === 0) {
    ElMessage.warning('请至少添加一个站点步骤')
    return
  }
  submitting.value = true
  const payload = {
    flowCode: form.flowCode.trim(),
    flowName: form.flowName.trim(),
    description: form.description.trim(),
    isActive: form.isActive,
    stations: steps.value.map((s, i) => ({
      stationId: s.stationId,
      stationCode: s.stationCode,
      order: i + 1,
      required: s.required
    }))
  }
  try {
    if (isEdit.value && currentId.value != null) {
      await updateFlow(currentId.value, payload)
      ElMessage.success('更新成功')
    } else {
      await createFlow(payload)
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

// 删除
const handleDelete = (row: FlowDto) => {
  ElMessageBox.confirm(`确认删除流程「${row.flowName}」吗？`, '提示', {
    type: 'warning',
    confirmButtonText: '确定',
    cancelButtonText: '取消'
  })
    .then(async () => {
      try {
        await deleteFlow(row.id)
        ElMessage.success('删除成功')
        loadData()
      } catch (e: any) {
        ElMessage.error(e.message || '删除失败')
      }
    })
    .catch(() => {})
}

onMounted(() => {
  loadData()
  loadStations()
})
</script>

<template>
  <div class="das-page">
    <el-card class="flow-card" v-loading="loading">
      <template #header>
        <div class="card-header">
          <span class="card-title">流程配置</span>
          <el-button type="primary" :icon="Plus" @click="openCreate">新建流程</el-button>
        </div>
      </template>

      <el-table :data="tableData" stripe style="width: 100%">
        <el-table-column label="流程编码" prop="flowCode" min-width="120" />
        <el-table-column label="流程名称" prop="flowName" min-width="140" />
        <el-table-column label="站点步骤" min-width="320">
          <template #default="{ row }">
            <div class="step-chain" v-if="row.stations && row.stations.length">
              <template v-for="(st, idx) in sortedSteps(row.stations)" :key="idx">
                <el-tag
                  size="small"
                  :type="st.required ? 'primary' : 'info'"
                  effect="light"
                >
                  {{ idx + 1 }}. {{ st.stationCode }}
                </el-tag>
                <el-icon v-if="idx < sortedSteps(row.stations).length - 1" class="chain-arrow">
                  <ArrowRight />
                </el-icon>
              </template>
            </div>
            <span v-else class="text-muted">-</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'info'" effect="light">
              {{ row.isActive ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="180">
          <template #default="{ row }">
            <span>{{ (row as any).createdAt || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" :icon="Edit" @click="openEdit(row as FlowDto)">编辑</el-button>
            <el-button link type="danger" :icon="Delete" @click="handleDelete(row as FlowDto)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新建/编辑弹窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="720px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form :model="form" label-width="90px" label-position="right" v-loading="detailLoading">
        <el-form-item label="流程编码">
          <el-input v-model="form.flowCode" placeholder="请输入流程编码" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="流程名称">
          <el-input v-model="form.flowName" placeholder="请输入流程名称" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" placeholder="请输入描述" />
        </el-form-item>
        <el-form-item label="启用">
          <el-checkbox v-model="form.isActive">启用流程</el-checkbox>
        </el-form-item>

        <el-divider content-position="left">站点步骤配置</el-divider>

        <div class="step-builder">
          <div class="builder-row">
            <el-select
              v-model="stepStationId"
              placeholder="选择站点"
              filterable
              style="width: 280px"
            >
              <el-option
                v-for="s in stations"
                :key="s.id"
                :label="`${s.stationCode} - ${s.stationName}`"
                :value="s.id"
              />
            </el-select>
            <el-checkbox v-model="stepRequired">必过</el-checkbox>
            <el-button type="primary" :icon="Plus" @click="addStep">添加步骤</el-button>
          </div>

          <div class="step-list" v-if="steps.length">
            <div v-for="(st, idx) in steps" :key="idx" class="step-item">
              <span class="step-order">{{ idx + 1 }}</span>
              <el-tag type="primary" effect="light">
                {{ st.stationCode }} · {{ stationNameOf(st.stationCode) }}
              </el-tag>
              <el-tag v-if="st.required" size="small" type="danger" effect="plain">必过</el-tag>
              <el-tag v-else size="small" type="info" effect="plain">选过</el-tag>
              <span class="step-actions">
                <el-button
                  link
                  type="primary"
                  :icon="Top"
                  :disabled="idx === 0"
                  @click="moveUp(idx)"
                />
                <el-button
                  link
                  type="primary"
                  :icon="Bottom"
                  :disabled="idx === steps.length - 1"
                  @click="moveDown(idx)"
                />
                <el-button link type="danger" :icon="CircleClose" @click="removeStep(idx)" />
              </span>
            </div>
          </div>
          <el-empty v-else description="暂无步骤，请添加站点" :image-size="60" />
        </div>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="submit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;
}

.flow-card {
  background: var(--ds-bg-panel);
  border: 1px solid var(--ds-border-light);
  border-radius: var(--ds-card-radius);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-weight: 600;
  color: var(--ds-text-title);
}

.step-chain {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
}

.chain-arrow {
  color: var(--ds-text-tertiary);
  font-size: 12px;
}

.text-muted {
  color: var(--ds-text-tertiary);
}

.step-builder {
  margin-top: 8px;
}

.builder-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 16px;
}

.step-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px;
  background: var(--ds-bg-header);
  border: 1px dashed var(--ds-border-light);
  border-radius: var(--ds-card-radius);
}

.step-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 10px;
  background: var(--ds-bg-panel);
  border: 1px solid var(--ds-border-light);
  border-radius: var(--ds-card-radius);
}

.step-order {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: var(--ds-primary-bg);
  color: var(--ds-primary);
  font-size: 12px;
  font-weight: 600;
  flex-shrink: 0;
}

.step-actions {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
}
</style>
