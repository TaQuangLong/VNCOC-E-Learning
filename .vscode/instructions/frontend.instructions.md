---
applyTo: "frontend/src/**/*.{ts,tsx}"
---

# Frontend Coding Instructions — ChurchLearn

## Feature Module Structure
```
src/features/courses/
  api.ts             <- TanStack Query hooks + API calls
  types.ts           <- TypeScript interfaces + Zod schemas
  components/        <- Feature-specific components
  CoursesPage.tsx    <- Page component (if routable)
```

## Required UI States
Every component that fetches data MUST handle all three states:
```tsx
if (isLoading) return <LoadingSkeleton />;
if (isError) return <ErrorMessage message="Could not load courses." />;
if (!data?.length) return <EmptyState message="No courses available yet." />;
```

## TanStack Query Pattern
```ts
// api.ts
export function useGetCourses() {
  return useQuery({
    queryKey: ['courses'],
    queryFn: () => apiClient.get<CourseDto[]>('/courses').then(r => r.data),
  });
}

export function useCreateCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCourseRequest) =>
      apiClient.post<CourseDto>('/admin/courses', data).then(r => r.data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['courses'] }),
  });
}
```

## Form Pattern (React Hook Form + Zod)
```tsx
const schema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  slug: z.string().min(1).regex(/^[a-z0-9-]+$/, 'Lowercase letters, numbers and hyphens only'),
});

type FormValues = z.infer<typeof schema>;

const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
  resolver: zodResolver(schema),
});
```

## Responsive Layout Rules
- Mobile-first: start with base styles, add sm: md: lg: prefixes for larger screens
- Course grid: `grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6`
- Learning page: single column mobile → `lg:grid lg:grid-cols-[280px_1fr]` desktop
- Lesson sidebar: `hidden lg:block` with mobile drawer alternative
- Minimum tap target: `py-2 px-4` on all buttons and links

## API Client
- Base URL always from `import.meta.env.VITE_API_URL`
- Never hardcode URLs in components or hooks
- Use the shared `apiClient` from `src/lib/api-client.ts`

## Rules
- TypeScript strict — no `any` types, no `@ts-ignore`
- PascalCase for components: `CourseCard`, `LessonSidebar`
- camelCase for hooks, functions, variables: `useCourses`, `handleSubmit`
- Prefix all custom hooks with `use`
- Keep components under 150 lines — extract sub-components if larger
- Separate data fetching (api.ts) from rendering (components)
