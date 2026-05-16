---
mode: agent
description: Scaffold a complete React feature module for ChurchLearn
---

Create a complete React feature module for ChurchLearn.

Follow ChurchLearn frontend structure:
- Folder: `src/features/{featureName}/`
- `api.ts` — TanStack Query hooks + axios API calls
- `types.ts` — TypeScript interfaces + Zod validation schemas
- `components/` — feature-specific components
- `{FeatureName}Page.tsx` — page component if this is a routable page

Rules (from .github/copilot-instructions.md):
- TypeScript strict — no `any` types
- TanStack Query for all data fetching (useQuery / useMutation)
- React Hook Form + Zod for all forms
- Tailwind CSS + shadcn/ui for styling
- Handle loading, error, and empty states on every query component
- Mobile-responsive layout using Tailwind responsive prefixes (mobile-first)
- API URL from `import.meta.env.VITE_API_URL` via shared apiClient

Also reference:
- `knowledge-graph/entities.md` for entity shapes
- `knowledge-graph/api-map.md` for endpoint paths and auth requirements

Feature to scaffold:
[DESCRIBE THE FEATURE OR PASTE THE SPEC.md CONTENT HERE]
