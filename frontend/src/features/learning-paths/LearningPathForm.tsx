import { useEffect } from 'react'
import {
  useFieldArray,
  useForm,
  useWatch,
  type Control,
  type FieldErrors,
  type UseFormRegister,
  type UseFormSetValue,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { ChevronDown, ChevronUp, Plus, Trash2 } from 'lucide-react'
import { AxiosError } from 'axios'

import { Button } from '@/components/ui/button'
import { usePublishedCoursesForPicker } from '@/features/courses/api'
import { slugify } from '@/features/courses/types'
import {
  learningPathFormSchema,
  type LearningPathFormData,
  type LearningPathFormInput,
} from './types'

export interface LearningPathCourseLabel {
  title: string
  status: 'Draft' | 'Published' | 'Archived'
}

interface LearningPathFormProps {
  defaultValues?: Partial<LearningPathFormInput>
  courseLabels?: Record<number, LearningPathCourseLabel>
  onSubmit: (data: LearningPathFormData) => Promise<void>
  submitLabel: string
  isEditMode?: boolean
}

interface SectionBuilderProps {
  sectionIndex: number
  sectionCount: number
  control: Control<LearningPathFormInput>
  register: UseFormRegister<LearningPathFormInput>
  setValue: UseFormSetValue<LearningPathFormInput>
  errors: FieldErrors<LearningPathFormInput>
  publishedCourses: Array<{ id: number; title: string }>
  selectedCourseIds: number[]
  courseLabels: Record<number, LearningPathCourseLabel>
  onMoveUp: () => void
  onMoveDown: () => void
  onRemove: () => void
}

const inputClassName =
  'border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring'

function getValidationMessage(error: unknown): string | undefined {
  if (!error || typeof error !== 'object') return undefined

  const candidate = error as {
    message?: unknown
    root?: { message?: unknown }
  }
  if (typeof candidate.message === 'string') return candidate.message
  if (typeof candidate.root?.message === 'string') {
    return candidate.root.message
  }

  return undefined
}

function getErrorMessage(error: unknown) {
  if (error instanceof AxiosError) {
    const data = error.response?.data as
      | { error?: string; message?: string }
      | undefined
    return data?.error ?? data?.message ?? 'Unable to save learning path.'
  }

  return error instanceof Error
    ? error.message
    : 'Unable to save learning path.'
}

function SectionBuilder({
  sectionIndex,
  sectionCount,
  control,
  register,
  setValue,
  errors,
  publishedCourses,
  selectedCourseIds,
  courseLabels,
  onMoveUp,
  onMoveDown,
  onRemove,
}: SectionBuilderProps) {
  const {
    fields: courseFields,
    append: appendCourse,
    remove: removeCourse,
    move: moveCourse,
  } = useFieldArray({
    control,
    name: `sections.${sectionIndex}.courses`,
  })

  const courses =
    useWatch({
      control,
      name: `sections.${sectionIndex}.courses`,
    }) ?? []

  useEffect(() => {
    courseFields.forEach((_, courseIndex) => {
      setValue(
        `sections.${sectionIndex}.courses.${courseIndex}.orderIndex`,
        courseIndex,
      )
    })
  }, [courseFields, sectionIndex, setValue])

  const sectionErrors = errors.sections?.[sectionIndex]

  return (
    <fieldset className="space-y-4 rounded-lg border bg-muted/20 p-4">
      <legend className="sr-only">Section {sectionIndex + 1}</legend>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-start">
        <div className="min-w-0 flex-1 space-y-1">
          <label
            htmlFor={`section-${sectionIndex}-title`}
            className="text-sm font-medium"
          >
            Section {sectionIndex + 1} title
          </label>
          <input
            id={`section-${sectionIndex}-title`}
            className={inputClassName}
            placeholder="e.g. Foundations"
            {...register(`sections.${sectionIndex}.title`)}
          />
          {sectionErrors?.title && (
            <p className="text-xs text-destructive">
              {sectionErrors.title.message}
            </p>
          )}
        </div>

        <div className="flex shrink-0 items-center gap-1">
          <Button
            type="button"
            variant="outline"
            size="icon-sm"
            onClick={onMoveUp}
            disabled={sectionIndex === 0}
            aria-label={`Move section ${sectionIndex + 1} up`}
          >
            <ChevronUp />
          </Button>
          <Button
            type="button"
            variant="outline"
            size="icon-sm"
            onClick={onMoveDown}
            disabled={sectionIndex === sectionCount - 1}
            aria-label={`Move section ${sectionIndex + 1} down`}
          >
            <ChevronDown />
          </Button>
          <Button
            type="button"
            variant="destructive"
            size="icon-sm"
            onClick={onRemove}
            aria-label={`Remove section ${sectionIndex + 1}`}
          >
            <Trash2 />
          </Button>
        </div>
      </div>

      <div className="space-y-1">
        <label
          htmlFor={`section-${sectionIndex}-description`}
          className="text-sm font-medium"
        >
          Description
        </label>
        <textarea
          id={`section-${sectionIndex}-description`}
          rows={2}
          className={`${inputClassName} resize-y`}
          {...register(`sections.${sectionIndex}.description`)}
        />
      </div>

      <div className="space-y-3">
        <div className="flex items-center justify-between gap-3">
          <p className="text-sm font-medium">Courses</p>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() =>
              appendCourse({ courseId: 0, orderIndex: courseFields.length })
            }
          >
            <Plus />
            Add course
          </Button>
        </div>

        {courseFields.length === 0 && (
          <div className="rounded-md border border-dashed p-4 text-center text-sm text-muted-foreground">
            No courses in this section yet.
          </div>
        )}

        {courseFields.map((field, courseIndex) => {
          const courseId = courses[courseIndex]?.courseId ?? 0
          const retainedCourse = courseLabels[courseId]
          const isUnavailable =
            courseId > 0 && retainedCourse?.status !== 'Published'

          return (
            <div
              key={field.id}
              className="rounded-md border bg-background p-3"
            >
              <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
                <span className="w-6 shrink-0 text-center text-xs font-medium text-muted-foreground">
                  {courseIndex + 1}
                </span>
                <select
                  className={`${inputClassName} min-w-0 flex-1`}
                  aria-label={`Course ${courseIndex + 1} in section ${sectionIndex + 1}`}
                  {...register(
                    `sections.${sectionIndex}.courses.${courseIndex}.courseId`,
                    { valueAsNumber: true },
                  )}
                >
                  <option value={0}>Select a published course...</option>
                  {retainedCourse &&
                    !publishedCourses.some((course) => course.id === courseId) && (
                      <option value={courseId}>
                        {retainedCourse.title} ({retainedCourse.status})
                      </option>
                    )}
                  {publishedCourses.map((course) => {
                    const selectedElsewhere =
                      course.id !== courseId &&
                      selectedCourseIds.includes(course.id)

                    return (
                      <option
                        key={course.id}
                        value={course.id}
                        disabled={selectedElsewhere}
                      >
                        {course.title}
                        {selectedElsewhere ? ' (already added)' : ''}
                      </option>
                    )
                  })}
                </select>
                <div className="flex shrink-0 items-center gap-1">
                  <Button
                    type="button"
                    variant="outline"
                    size="icon-sm"
                    onClick={() => moveCourse(courseIndex, courseIndex - 1)}
                    disabled={courseIndex === 0}
                    aria-label={`Move course ${courseIndex + 1} up`}
                  >
                    <ChevronUp />
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="icon-sm"
                    onClick={() => moveCourse(courseIndex, courseIndex + 1)}
                    disabled={courseIndex === courseFields.length - 1}
                    aria-label={`Move course ${courseIndex + 1} down`}
                  >
                    <ChevronDown />
                  </Button>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon-sm"
                    onClick={() => removeCourse(courseIndex)}
                    aria-label={`Remove course ${courseIndex + 1}`}
                  >
                    <Trash2 />
                  </Button>
                </div>
              </div>

              {sectionErrors?.courses?.[courseIndex]?.courseId && (
                <p className="mt-1 text-xs text-destructive">
                  {sectionErrors.courses[courseIndex].courseId.message}
                </p>
              )}
              {isUnavailable && (
                <p className="mt-2 text-xs text-amber-700">
                  This course is {retainedCourse.status.toLowerCase()} and must
                  be replaced or removed before saving.
                </p>
              )}
            </div>
          )
        })}

        {getValidationMessage(sectionErrors?.courses) && (
          <p className="text-xs text-destructive">
            {getValidationMessage(sectionErrors?.courses)}
          </p>
        )}
      </div>
    </fieldset>
  )
}

export default function LearningPathForm({
  defaultValues,
  courseLabels = {},
  onSubmit,
  submitLabel,
  isEditMode = false,
}: LearningPathFormProps) {
  const {
    data: publishedCoursesData,
    isLoading: coursesLoading,
    isError: coursesError,
  } = usePublishedCoursesForPicker()

  const {
    control,
    register,
    handleSubmit,
    setValue,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LearningPathFormInput, unknown, LearningPathFormData>({
    resolver: zodResolver(learningPathFormSchema),
    defaultValues: {
      title: '',
      slug: '',
      shortDescription: '',
      description: '',
      thumbnailUrl: '',
      estimatedDurationLabel: '',
      sections: [
        {
          title: '',
          description: '',
          orderIndex: 0,
          courses: [{ courseId: 0, orderIndex: 0 }],
        },
      ],
      ...defaultValues,
    },
  })

  const {
    fields: sectionFields,
    append: appendSection,
    remove: removeSection,
    move: moveSection,
  } = useFieldArray({ control, name: 'sections' })

  const title = useWatch({ control, name: 'title' })
  const sections = useWatch({ control, name: 'sections' }) ?? []
  const selectedCourseIds = sections
    .flatMap((section) => section.courses ?? [])
    .map((course) => course.courseId)
    .filter((courseId) => courseId > 0)

  useEffect(() => {
    if (!isEditMode && title) {
      setValue('slug', slugify(title), { shouldValidate: false })
    }
  }, [isEditMode, setValue, title])

  useEffect(() => {
    sectionFields.forEach((_, sectionIndex) => {
      setValue(`sections.${sectionIndex}.orderIndex`, sectionIndex)
    })
  }, [sectionFields, setValue])

  const publishedCourses =
    publishedCoursesData?.items.map((course) => ({
      id: course.id,
      title: course.title,
    })) ?? []

  const handleFormSubmit = async (data: LearningPathFormData) => {
    const normalizedData: LearningPathFormData = {
      ...data,
      sections: data.sections.map((section, sectionIndex) => ({
        ...section,
        orderIndex: sectionIndex,
        courses: section.courses.map((course, courseIndex) => ({
          ...course,
          orderIndex: courseIndex,
        })),
      })),
    }

    try {
      await onSubmit(normalizedData)
    } catch (error: unknown) {
      setError('root', { message: getErrorMessage(error) })
    }
  }

  return (
    <form
      onSubmit={handleSubmit(handleFormSubmit)}
      className="space-y-6"
      noValidate
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-1">
          <label htmlFor="title" className="text-sm font-medium">
            Title <span className="text-destructive">*</span>
          </label>
          <input id="title" className={inputClassName} {...register('title')} />
          {errors.title && (
            <p className="text-xs text-destructive">{errors.title.message}</p>
          )}
        </div>

        <div className="space-y-1">
          <label htmlFor="slug" className="text-sm font-medium">
            Slug <span className="text-destructive">*</span>
          </label>
          <input
            id="slug"
            className={`${inputClassName} font-mono`}
            {...register('slug')}
          />
          {errors.slug && (
            <p className="text-xs text-destructive">{errors.slug.message}</p>
          )}
        </div>
      </div>

      <div className="space-y-1">
        <label htmlFor="shortDescription" className="text-sm font-medium">
          Short description
        </label>
        <textarea
          id="shortDescription"
          rows={2}
          className={`${inputClassName} resize-y`}
          {...register('shortDescription')}
        />
        {errors.shortDescription && (
          <p className="text-xs text-destructive">
            {errors.shortDescription.message}
          </p>
        )}
      </div>

      <div className="space-y-1">
        <label htmlFor="description" className="text-sm font-medium">
          Description
        </label>
        <textarea
          id="description"
          rows={5}
          className={`${inputClassName} resize-y`}
          {...register('description')}
        />
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-1">
          <label htmlFor="thumbnailUrl" className="text-sm font-medium">
            Thumbnail URL
          </label>
          <input
            id="thumbnailUrl"
            type="url"
            placeholder="https://example.com/path.jpg"
            className={inputClassName}
            {...register('thumbnailUrl')}
          />
          {errors.thumbnailUrl && (
            <p className="text-xs text-destructive">
              {errors.thumbnailUrl.message}
            </p>
          )}
        </div>

        <div className="space-y-1">
          <label
            htmlFor="estimatedDurationLabel"
            className="text-sm font-medium"
          >
            Estimated duration
          </label>
          <input
            id="estimatedDurationLabel"
            placeholder="e.g. 8 weeks"
            className={inputClassName}
            {...register('estimatedDurationLabel')}
          />
        </div>
      </div>

      <div className="space-y-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-lg font-semibold">Sections</h2>
            <p className="text-sm text-muted-foreground">
              Arrange published courses into an ordered learning journey.
            </p>
          </div>
          <Button
            type="button"
            variant="outline"
            onClick={() =>
              appendSection({
                title: '',
                description: '',
                orderIndex: sectionFields.length,
                courses: [],
              })
            }
          >
            <Plus />
            Add section
          </Button>
        </div>

        {coursesLoading && (
          <div className="h-10 animate-pulse rounded-md bg-muted" />
        )}
        {coursesError && (
          <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
            Published courses could not be loaded. Please try again.
          </p>
        )}
        {!coursesLoading && !coursesError && publishedCourses.length === 0 && (
          <p className="rounded-md border border-dashed px-4 py-3 text-sm text-muted-foreground">
            No published courses are available. Publish a course before
            building a learning path.
          </p>
        )}

        {sectionFields.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-sm text-muted-foreground">
            No sections yet. Add a section to continue.
          </div>
        )}

        {sectionFields.map((field, sectionIndex) => (
          <SectionBuilder
            key={field.id}
            sectionIndex={sectionIndex}
            sectionCount={sectionFields.length}
            control={control}
            register={register}
            setValue={setValue}
            errors={errors}
            publishedCourses={publishedCourses}
            selectedCourseIds={selectedCourseIds}
            courseLabels={courseLabels}
            onMoveUp={() => moveSection(sectionIndex, sectionIndex - 1)}
            onMoveDown={() => moveSection(sectionIndex, sectionIndex + 1)}
            onRemove={() => removeSection(sectionIndex)}
          />
        ))}

        {getValidationMessage(errors.sections) && (
          <p className="text-xs text-destructive">
            {getValidationMessage(errors.sections)}
          </p>
        )}
      </div>

      {errors.root && (
        <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
          {errors.root.message}
        </p>
      )}

      <Button type="submit" disabled={isSubmitting || coursesLoading}>
        {isSubmitting ? 'Saving...' : submitLabel}
      </Button>
    </form>
  )
}
