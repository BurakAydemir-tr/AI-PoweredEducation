import { useForm } from 'react-hook-form'
import {
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
} from '@mui/material'
import type { LearningGame, LearningGameRequest } from '../types/api'

type GameFormProps = {
  game?: LearningGame
  isSubmitting?: boolean
  submitLabel: string
  onSubmit: (values: LearningGameRequest) => Promise<void> | void
}

export function GameForm({
  game,
  isSubmitting,
  submitLabel,
  onSubmit,
}: GameFormProps) {
  const {
    formState: { errors },
    handleSubmit,
    register,
    setValue,
    watch,
  } = useForm<LearningGameRequest>({
    defaultValues: {
      gradeLevel: game?.gradeLevel ?? '',
      subject: game?.subject ?? '',
      topic: game?.topic ?? '',
      environmentType: game?.environmentType ?? 0,
      expectedStudentCount: game?.expectedStudentCount ?? 1,
    },
  })

  return (
    <Stack component="form" spacing={3} onSubmit={handleSubmit(onSubmit)}>
      <TextField
        disabled={isSubmitting}
        error={Boolean(errors.gradeLevel)}
        helperText={errors.gradeLevel?.message}
        label="Sınıf seviyesi"
        {...register('gradeLevel', { required: 'Sınıf seviyesi zorunludur.' })}
      />
      <TextField
        disabled={isSubmitting}
        error={Boolean(errors.subject)}
        helperText={errors.subject?.message}
        label="Ders"
        {...register('subject', { required: 'Ders zorunludur.' })}
      />
      <TextField
        disabled={isSubmitting}
        error={Boolean(errors.topic)}
        helperText={errors.topic?.message}
        label="Konu"
        {...register('topic', { required: 'Konu zorunludur.' })}
      />
      <FormControl>
        <InputLabel id="environment-type-label">Ortam</InputLabel>
        <Select
          disabled={isSubmitting}
          label="Ortam"
          labelId="environment-type-label"
          value={watch('environmentType')}
          onChange={(event) =>
            setValue('environmentType', Number(event.target.value) as 0 | 1)
          }
        >
          <MenuItem value={0}>İç mekan</MenuItem>
          <MenuItem value={1}>Dış mekan</MenuItem>
        </Select>
      </FormControl>
      <TextField
        disabled={isSubmitting}
        error={Boolean(errors.expectedStudentCount)}
        helperText={errors.expectedStudentCount?.message}
        label="Beklenen öğrenci sayısı"
        type="number"
        {...register('expectedStudentCount', {
          min: { value: 1, message: 'Beklenen öğrenci sayısı 0’dan büyük olmalıdır.' },
          required: 'Beklenen öğrenci sayısı zorunludur.',
          valueAsNumber: true,
        })}
      />
      <Button disabled={isSubmitting} type="submit" variant="contained">
        {submitLabel}
      </Button>
    </Stack>
  )
}
