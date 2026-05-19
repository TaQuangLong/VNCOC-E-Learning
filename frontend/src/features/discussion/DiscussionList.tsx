import { Button } from '@/components/ui/button'
import { useAuth } from '@/hooks/useAuth'
import { useLessonDiscussions, useCreateDiscussion } from './api'
import DiscussionForm from './DiscussionForm'
import DiscussionPost from './DiscussionPost'

interface Props {
  lessonId: number
}

export default function DiscussionList({ lessonId }: Props) {
  const { user } = useAuth()
  const currentUserId = user?.userId ?? ''
  const isAdmin =
    user?.roles.some((r) => r === 'Admin' || r === 'SuperAdmin') ?? false

  const {
    data,
    isLoading,
    isError,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
  } = useLessonDiscussions(lessonId)

  const createDiscussion = useCreateDiscussion(lessonId)

  const allItems = data?.pages.flatMap((p) => p.items) ?? []

  return (
    <div className="space-y-6">
      {/* New post form */}
      <div className="rounded-md border border-border p-4">
        <p className="mb-3 text-sm font-medium">
          Ask a question or share a thought
        </p>
        <DiscussionForm
          onSubmit={async (content) => {
            await createDiscussion.mutateAsync(content)
          }}
          isPending={createDiscussion.isPending}
        />
        {createDiscussion.isError && (
          <p className="mt-1 text-xs text-destructive">
            Failed to post. Please try again.
          </p>
        )}
      </div>

      {/* Loading skeleton */}
      {isLoading && (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-24 animate-pulse rounded-md bg-muted" />
          ))}
        </div>
      )}

      {/* Error state */}
      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Could not load discussions. Please try again.
        </div>
      )}

      {/* Empty state */}
      {!isLoading && !isError && allItems.length === 0 && (
        <div className="rounded-md border border-dashed border-border py-12 text-center text-sm text-muted-foreground">
          Be the first to ask a question in this lesson.
        </div>
      )}

      {/* Posts */}
      {allItems.map((post) => (
        <DiscussionPost
          key={post.id}
          post={post}
          lessonId={lessonId}
          currentUserId={currentUserId}
          isAdmin={isAdmin}
        />
      ))}

      {/* Load More */}
      {hasNextPage && (
        <div className="flex justify-center">
          <Button
            variant="outline"
            onClick={() => fetchNextPage()}
            disabled={isFetchingNextPage}
          >
            {isFetchingNextPage ? 'Loading…' : 'Load More'}
          </Button>
        </div>
      )}
    </div>
  )
}
