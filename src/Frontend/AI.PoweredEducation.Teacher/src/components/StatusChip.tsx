import { Chip } from '@mui/material'
import type { LearningGameStatus } from '../types/api'
import { learningGameStatusLabels } from '../utils/enumLabels'

type StatusChipProps = {
  status: LearningGameStatus
}

export function StatusChip({ status }: StatusChipProps) {
  const color =
    status === 2
      ? 'success'
      : status === 3
        ? 'default'
        : status === 1
          ? 'info'
          : 'warning'

  return (
    <Chip
      color={color}
      label={learningGameStatusLabels[status]}
      size="small"
      variant={status === 3 ? 'outlined' : 'filled'}
    />
  )
}
