import { useEffect } from 'react'
import { useForm, useFieldArray, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { questionFormSchema, type QuestionFormInput, type QuestionType } from './types'

interface Props {
  initialValues?: Partial<QuestionFormInput>
  nextOrderIndex?: number
  onSave: (data: QuestionFormInput) => Promise<void>
  onCancel: () => void
  saveLabel?: string
  isSaving?: boolean
  saveError?: boolean
}

export default function QuestionEditor({
  initialValues,
  nextOrderIndex = 0,
  onSave,
  onCancel,
  saveLabel = 'Save Question',
  isSaving = false,
  saveError = false,
}: Props) {
  const {
    register,
    control,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<QuestionFormInput>({
    resolver: zodResolver(questionFormSchema),
    defaultValues: initialValues ?? {
      text: '',
      type: 'SingleChoice',
      orderIndex: nextOrderIndex,
      options: [
        { text: '', isCorrect: false, orderIndex: 0 },
        { text: '', isCorrect: false, orderIndex: 1 },
      ],
    },
  })

  const questionType = watch('type') as QuestionType

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'options',
  })

  // When type changes to TrueFalse, reset options to True/False
  useEffect(() => {
    if (questionType === 'TrueFalse') {
      setValue('options', [
        { text: 'True', isCorrect: false, orderIndex: 0 },
        { text: 'False', isCorrect: false, orderIndex: 1 },
      ])
    }
  }, [questionType, setValue])

  const handleAddOption = () => {
    append({ text: '', isCorrect: false, orderIndex: fields.length })
  }

  // For SingleChoice and TrueFalse — selecting one option deselects others
  const handleSingleCorrectChange = (selectedIndex: number) => {
    fields.forEach((_, i) => {
      setValue(`options.${i}.isCorrect`, i === selectedIndex, {
        shouldValidate: false,
      })
    })
  }

  const isTrueFalse = questionType === 'TrueFalse'
  const isSingle = questionType === 'SingleChoice'

  return (
    <form
      onSubmit={handleSubmit(onSave)}
      noValidate
      className="space-y-4 rounded-md border border-border bg-muted/30 p-4"
    >
      {/* Question text */}
      <div className="space-y-1">
        <label className="text-sm font-medium">
          Question <span className="text-destructive">*</span>
        </label>
        <textarea
          {...register('text')}
          rows={2}
          className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          placeholder="Enter question text…"
        />
        {errors.text && (
          <p className="text-xs text-destructive">{errors.text.message}</p>
        )}
      </div>

      {/* Type selector */}
      <div className="space-y-1">
        <label className="text-sm font-medium">Type</label>
        <select
          {...register('type')}
          className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
        >
          <option value="SingleChoice">Single Choice</option>
          <option value="MultipleChoice">Multiple Choice</option>
          <option value="TrueFalse">True / False</option>
        </select>
      </div>

      {/* Answer options */}
      <div className="space-y-2">
        <p className="text-sm font-medium">
          {isTrueFalse
            ? 'Select the correct answer'
            : isSingle
              ? 'Options (select one correct)'
              : 'Options (select all correct)'}
        </p>

        {fields.map((field, index) => (
          <div key={field.id} className="flex items-center gap-2">
            {/* Correct toggle */}
            <Controller
              control={control}
              name={`options.${index}.isCorrect`}
              render={({ field: f }) => {
                if (isSingle || isTrueFalse) {
                  return (
                    <input
                      type="radio"
                      name="correct-radio"
                      checked={f.value}
                      onChange={() => handleSingleCorrectChange(index)}
                      className="h-4 w-4 shrink-0 accent-primary"
                      aria-label={`Mark option ${index + 1} as correct`}
                    />
                  )
                }
                return (
                  <input
                    type="checkbox"
                    checked={f.value}
                    onChange={(e) => f.onChange(e.target.checked)}
                    className="h-4 w-4 shrink-0 accent-primary"
                    aria-label={`Mark option ${index + 1} as correct`}
                  />
                )
              }}
            />

            {/* Option text */}
            {isTrueFalse ? (
              <span className="flex-1 text-sm">{field.text}</span>
            ) : (
              <input
                {...register(`options.${index}.text`)}
                className="border-input bg-background flex-1 rounded-md border px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                placeholder={`Option ${index + 1}`}
              />
            )}

            {/* Remove (only for non-TrueFalse and when more than 2 options) */}
            {!isTrueFalse && fields.length > 2 && (
              <button
                type="button"
                onClick={() => remove(index)}
                className="text-xs text-destructive hover:underline"
                aria-label="Remove option"
              >
                ✕
              </button>
            )}
          </div>
        ))}

        {errors.options && !Array.isArray(errors.options) && (
          <p className="text-xs text-destructive">{errors.options.message}</p>
        )}

        {/* Add option button (not for TrueFalse) */}
        {!isTrueFalse && (
          <button
            type="button"
            onClick={handleAddOption}
            className="text-xs text-muted-foreground hover:text-foreground hover:underline"
          >
            + Add option
          </button>
        )}
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2">
        <Button type="submit" size="sm" disabled={isSaving}>
          {isSaving ? 'Saving…' : saveLabel}
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={onCancel}>
          Cancel
        </Button>
      </div>

      {saveError && (
        <p className="text-xs text-destructive">Failed to save question. Please try again.</p>
      )}
    </form>
  )
}
