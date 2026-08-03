<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import {
  CircleCheckFilled,
  CircleCloseFilled,
  WarningFilled,
  Search,
  RefreshLeft
} from '@element-plus/icons-vue'
import { check, type GateCheckResult } from '/@/api/gateCheck'
import { getStations, type Station } from '/@/api/station'

defineOptions({ name: 'DasCheck' })

const stations = ref<Station[]>([])
const loading = ref(false)
const hasResult = ref(false)

const form = reactive({
  productCode: '',
  stationCode: '',
  operator: '',
  remark: ''
})

const result = ref<GateCheckResult | null>(null)

// 结果样式映射
const resultMeta = computed(() => {
  if (!result.value) return null
  const r = (result.value.result || '').toLowerCase()
  if (r === 'pass') {
    return { cls: 'pass', icon: CircleCheckFilled, label: '通过 (Pass)' }
  }
  if (r === 'block') {
    return { cls: 'block', icon: CircleCloseFilled, label: '拦截 (Block)' }
  }
  if (r === 'skip') {
    return { cls: 'skip', icon: WarningFilled, label: '跳过 (Skip)' }
  }
  return { cls: 'unknown', icon: WarningFilled, label: result.value.result || '未知' }
})

// 执行过站检查
const doCheck = async () => {
  if (!form.productCode.trim()) {
    ElMessage.warning('请输入产品编码/条码')
    return
  }
  if (!form.stationCode) {
    ElMessage.warning('请选择站点')
    return
  }
  loading.value = true
  hasResult.value = false
  result.value = null
  try {
    result.value = await check({
      productCode: form.productCode.trim(),
      stationCode: form.stationCode,
      operator: form.operator.trim() || undefined,
      remark: form.remark.trim() || undefined
    })
    hasResult.value = true
  } catch (e: any) {
    ElMessage.error(e.message || '过站检查失败')
  } finally {
    loading.value = false
  }
}

// 重置
const reset = () => {
  form.productCode = ''
  form.stationCode = ''
  form.operator = ''
  form.remark = ''
  result.value = null
  hasResult.value = false
}

onMounted(() => {
  getStations()
    .then(data => (stations.value = data))
    .catch(e => ElMessage.error(e.message || '加载站点列表失败'))
})
</script>

<template>
  <div class="das-page">
    <!-- 检查表单 -->
    <el-card class="check-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">过站检查</span>
        </div>
      </template>

      <el-form :model="form" label-width="110px" label-position="right" @submit.prevent>
        <el-row :gutter="16">
          <el-col :xs="24" :sm="12" :md="6">
            <el-form-item label="产品编码/条码">
              <el-input
                v-model="form.productCode"
                placeholder="请输入产品编码或条码"
                clearable
                @keyup.enter="doCheck"
              />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <el-form-item label="站点">
              <el-select
                v-model="form.stationCode"
                placeholder="请选择站点"
                filterable
                style="width: 100%"
              >
                <el-option
                  v-for="s in stations"
                  :key="s.id"
                  :label="`${s.stationCode} - ${s.stationName}`"
                  :value="s.stationCode"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <el-form-item label="操作员">
              <el-input v-model="form.operator" placeholder="请输入操作员" clearable />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <el-form-item label="备注">
              <el-input v-model="form.remark" placeholder="请输入备注" clearable />
            </el-form-item>
          </el-col>
        </el-row>

        <div class="form-actions">
          <el-button type="primary" :icon="Search" :loading="loading" @click="doCheck">
            过站检查
          </el-button>
          <el-button :icon="RefreshLeft" @click="reset">重置</el-button>
        </div>
      </el-form>
    </el-card>

    <!-- 检查结果 -->
    <el-card class="result-card" v-if="hasResult && result && resultMeta">
      <template #header>
        <div class="card-header">
          <span class="card-title">检查结果</span>
          <el-tag
            :type="resultMeta.cls === 'pass' ? 'success' : resultMeta.cls === 'block' ? 'danger' : 'info'"
            effect="dark"
          >
            {{ result.passed ? '已通过' : '未通过' }}
          </el-tag>
        </div>
      </template>

      <!-- 结果横幅 -->
      <div class="result-banner" :class="resultMeta.cls">
        <el-icon :size="44" class="banner-icon">
          <component :is="resultMeta.icon" />
        </el-icon>
        <div class="banner-text">
          <div class="banner-title">{{ resultMeta.label }}</div>
          <div class="banner-message">{{ result.message || '-' }}</div>
        </div>
      </div>

      <!-- 产品信息 -->
      <div class="info-grid">
        <div class="info-item">
          <span class="info-label">产品编码</span>
          <span class="info-value">{{ result.productCode || '-' }}</span>
        </div>
        <div class="info-item">
          <span class="info-label">条码</span>
          <span class="info-value">{{ result.barcode || '-' }}</span>
        </div>
        <div class="info-item">
          <span class="info-label">当前站点</span>
          <span class="info-value">
            {{ result.stationCode || '-' }}
            <span v-if="result.stationName" class="info-sub">（{{ result.stationName }}）</span>
          </span>
        </div>
        <div class="info-item">
          <span class="info-label">下一站</span>
          <span class="info-value">
            <template v-if="result.nextStationCode">
              {{ result.nextStationCode }}
              <span v-if="result.nextStationName" class="info-sub">（{{ result.nextStationName }}）</span>
            </template>
            <span v-else class="text-muted">无</span>
          </span>
        </div>
      </div>

      <!-- 命中规则 -->
      <div class="rules-section">
        <div class="section-title">命中规则</div>
        <div v-if="result.matchedRules && result.matchedRules.length" class="rules-list">
          <el-tag
            v-for="(rule, idx) in result.matchedRules"
            :key="idx"
            class="rule-tag"
            type="warning"
            effect="light"
          >
            <span class="rule-type">{{ rule.ruleType }}</span>
            <span class="rule-name">{{ rule.ruleName }}</span>
          </el-tag>
        </div>
        <el-empty v-else description="未命中任何规则" :image-size="60" />
      </div>
    </el-card>

    <!-- 空状态 -->
    <el-card class="result-card empty-card" v-else>
      <el-empty description="请输入产品信息并点击「过站检查」" :image-size="100" />
    </el-card>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.check-card,
.result-card {
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

.form-actions {
  display: flex;
  gap: 12px;
  padding-left: 110px;
}

// 结果横幅
.result-banner {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  border-radius: var(--ds-card-radius);
  border: 1px solid transparent;

  .banner-icon {
    flex-shrink: 0;
  }

  .banner-title {
    font-size: 20px;
    font-weight: 700;
    margin-bottom: 4px;
  }

  .banner-message {
    font-size: 14px;
    color: var(--ds-text-secondary);
  }

  &.pass {
    background: var(--ds-success-bg);
    border-color: var(--ds-success-border);
    color: var(--ds-success-text);

    .banner-icon,
    .banner-title {
      color: var(--ds-success-text);
    }
  }

  &.block {
    background: var(--ds-danger-bg);
    border-color: var(--ds-danger-border);
    color: var(--ds-danger-text);

    .banner-icon,
    .banner-title {
      color: var(--ds-danger-text);
    }
  }

  &.skip {
    background: var(--ds-primary-bg);
    border-color: var(--ds-primary-border);
    color: var(--ds-primary);

    .banner-icon,
    .banner-title {
      color: var(--ds-primary);
    }
  }

  &.unknown {
    background: var(--ds-info-bg);
    border-color: var(--ds-info-border);
    color: var(--ds-info-text);

    .banner-icon,
    .banner-title {
      color: var(--ds-info-text);
    }
  }
}

// 信息网格
.info-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-top: 20px;
  padding: 16px;
  background: var(--ds-bg-header);
  border: 1px solid var(--ds-border-light);
  border-radius: var(--ds-card-radius);

  @media (max-width: 992px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 576px) {
    grid-template-columns: 1fr;
  }
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.info-label {
  font-size: 12px;
  color: var(--ds-text-tertiary);
}

.info-value {
  font-size: 14px;
  font-weight: 500;
  color: var(--ds-text-primary);
  word-break: break-all;
}

.info-sub {
  font-weight: 400;
  color: var(--ds-text-secondary);
}

.text-muted {
  color: var(--ds-text-tertiary);
}

// 命中规则
.rules-section {
  margin-top: 20px;
}

.section-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--ds-text-title);
  margin-bottom: 12px;
  padding-left: 8px;
  border-left: 3px solid var(--ds-primary);
}

.rules-list {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.rule-tag {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  height: auto;

  .rule-type {
    font-weight: 600;
  }

  .rule-name {
    opacity: 0.85;
  }
}

.empty-card {
  min-height: 260px;

  :deep(.el-card__body) {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 220px;
  }
}
</style>
