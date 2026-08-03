<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getRules,
  createRule,
  updateRule,
  deleteRule,
  type GateRuleDto,
  type GateRuleCreateDto
} from '/@/api/gateRule'
import { getStations, type Station } from '/@/api/station'

defineOptions({ name: 'DasRules' })

const loading = ref(false)
const submitting = ref(false)
const tableData = ref<GateRuleDto[]>([])
const stations = ref<Station[]>([])

const dialogVisible = ref(false)
const isEdit = ref(false)

interface ConditionModel {
  Field: string
  Operator: string
  Value: string
}

const defaultCondition = (): ConditionModel => ({
  Field: 'BatchNo',
  Operator: 'Equals',
  Value: ''
})

const form = reactive({
  id: 0,
  stationId: undefined as number | undefined,
  ruleName: '',
  ruleType: 'Block',
  condition: defaultCondition(),
  isActive: true,
  priority: 0
})

const fieldLabels: Record<string, string> = {
  BatchNo: '批次号',
  Status: '产品状态',
  ProductCode: '产品编码'
}
const operatorLabels: Record<string, string> = {
  Equals: '等于',
  NotEquals: '不等于',
  Contains: '包含'
}

const ruleTypeOptions = [
  { value: 'Block', label: 'Block 卡关' },
  { value: 'Warn', label: 'Warn 警告' },
  { value: 'Skip', label: 'Skip 跳过' }
]
const fieldOptions = [
  { value: 'BatchNo', label: 'BatchNo 批次号' },
  { value: 'Status', label: 'Status 产品状态' },
  { value: 'ProductCode', label: 'ProductCode 产品编码' }
]
const operatorOptions = [
  { value: 'Equals', label: 'Equals 等于' },
  { value: 'NotEquals', label: 'NotEquals 不等于' },
  { value: 'Contains', label: 'Contains 包含' }
]

const ruleTypeTagType = (t: string) => {
  switch (t) {
    case 'Block':
      return 'danger'
    case 'Warn':
      return 'warning'
    case 'Skip':
      return 'primary'
    default:
      return 'info'
  }
}
const ruleTypeLabel = (t: string) => ruleTypeOptions.find((o) => o.value === t)?.label || t

const formatCondition = (cond: string) => {
  if (!cond) return '-'
  try {
    const c = JSON.parse(cond)
    const f = fieldLabels[c.Field] || c.Field || '-'
    const o = operatorLabels[c.Operator] || c.Operator || ''
    const v = c.Value ?? ''
    return `${f} ${o} ${v}`
  } catch {
    return cond
  }
}

const loadData = async () => {
  loading.value = true
  try {
    tableData.value = await getRules()
  } catch (e: any) {
    ElMessage.error(e.message || '加载失败')
  } finally {
    loading.value = false
  }
}

const loadStations = async () => {
  try {
    stations.value = await getStations()
  } catch (e: any) {
    ElMessage.error(e.message || '站点加载失败')
  }
}

const resetForm = () => {
  form.id = 0
  form.stationId = undefined
  form.ruleName = ''
  form.ruleType = 'Block'
  form.condition = defaultCondition()
  form.isActive = true
  form.priority = 0
}

const openCreate = () => {
  isEdit.value = false
  resetForm()
  dialogVisible.value = true
}

const openEdit = (row: GateRuleDto) => {
  isEdit.value = true
  form.id = row.id
  form.stationId = row.stationId
  form.ruleName = row.ruleName
  form.ruleType = row.ruleType
  form.isActive = row.isActive
  form.priority = row.priority
  try {
    const parsed = JSON.parse(row.condition || '{}')
    form.condition = {
      Field: parsed.Field || 'BatchNo',
      Operator: parsed.Operator || 'Equals',
      Value: parsed.Value ?? ''
    }
  } catch {
    form.condition = defaultCondition()
  }
  dialogVisible.value = true
}

const handleSave = async () => {
  if (!form.stationId) {
    ElMessage.warning('请选择站点')
    return
  }
  if (!form.ruleName.trim()) {
    ElMessage.warning('请输入规则名')
    return
  }
  if (!form.condition.Value.trim()) {
    ElMessage.warning('请输入目标值')
    return
  }

  const conditionStr = JSON.stringify({
    Field: form.condition.Field,
    Operator: form.condition.Operator,
    Value: form.condition.Value.trim()
  })

  const payload: GateRuleCreateDto = {
    stationId: form.stationId,
    ruleName: form.ruleName.trim(),
    ruleType: form.ruleType,
    condition: conditionStr,
    isActive: form.isActive,
    priority: form.priority
  }

  submitting.value = true
  try {
    if (isEdit.value) {
      await updateRule(form.id, payload)
      ElMessage.success('更新成功')
    } else {
      await createRule(payload)
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

const handleDelete = (row: GateRuleDto) => {
  ElMessageBox.confirm(`确认删除规则「${row.ruleName}」吗？`, '提示', {
    type: 'warning',
    confirmButtonText: '确定',
    cancelButtonText: '取消'
  })
    .then(async () => {
      try {
        await deleteRule(row.id)
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
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>卡关规则</span>
          <el-button type="primary" @click="openCreate">新建规则</el-button>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData" border stripe style="width: 100%">
        <el-table-column label="规则名称" prop="ruleName" min-width="160" show-overflow-tooltip />
        <el-table-column label="站点" min-width="140">
          <template #default="{ row }">
            <span>{{ row.stationName || row.stationCode || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="类型" width="120" align="center">
          <template #default="{ row }">
            <el-tag :type="ruleTypeTagType(row.ruleType)" effect="light">
              {{ ruleTypeLabel(row.ruleType) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="条件" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">
            {{ formatCondition(row.condition) }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" effect="light">
              {{ row.isActive ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="优先级" prop="priority" width="90" align="center" />
        <el-table-column label="操作" width="150" align="center" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link size="small" @click="openEdit(row as GateRuleDto)">编辑</el-button>
            <el-button type="danger" link size="small" @click="handleDelete(row as GateRuleDto)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑规则' : '新建规则'"
      width="560px"
      destroy-on-close
    >
      <el-form :model="form" label-width="100px">
        <el-form-item label="站点" required>
          <el-select v-model="form.stationId" placeholder="请选择站点" style="width: 100%">
            <el-option
              v-for="s in stations"
              :key="s.id"
              :label="`${s.stationCode} - ${s.stationName}`"
              :value="s.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="规则名" required>
          <el-input v-model="form.ruleName" placeholder="请输入规则名" />
        </el-form-item>
        <el-form-item label="类型" required>
          <el-select v-model="form.ruleType" style="width: 100%">
            <el-option
              v-for="o in ruleTypeOptions"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>

        <el-divider content-position="left">匹配条件</el-divider>

        <el-form-item label="检查字段" required>
          <el-select v-model="form.condition.Field" style="width: 100%">
            <el-option
              v-for="o in fieldOptions"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="比较方式" required>
          <el-select v-model="form.condition.Operator" style="width: 100%">
            <el-option
              v-for="o in operatorOptions"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="目标值" required>
          <el-input v-model="form.condition.Value" placeholder="请输入目标值" />
        </el-form-item>

        <el-divider />

        <el-form-item label="优先级">
          <el-input-number v-model="form.priority" :min="0" :step="1" controls-position="right" />
        </el-form-item>
        <el-form-item label="启用">
          <el-checkbox v-model="form.isActive">启用该规则</el-checkbox>
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
