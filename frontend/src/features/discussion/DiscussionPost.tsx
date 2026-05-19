import { useState } from 'react'
import { Button } from '@/components/ui/button'
import {
  useUpdateDiscussion,
  useDeleteDiscussion,
  useAdminDeleteDiscussion,
} from './api'
import DiscussionForm from './DiscussionForm'
import ReplyList from './ReplyList'
import type { DiscussionSummary } from './types'

interface Props {
  post: DiscussionSummary
  lessonId: number
  currentUserId: string
  isAdmin: boolean
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

type Mode = 'view' | 'editing' | 'confirmDelete'

export default function DiscussionPost({
  post,
  lessonId,
  currentUserId,
  isAdmin,
}: Props) {
  const [mode, setMode] = useState<Mode>('view')

  const isOwn = post.userId === currentUserId && !post.isDeleted
  const canAdminRemove =
    isAdmin && !post.isDeleted && post.userId !== currentUserId

  const updateDiscussion = useUpdateDiscussion(lessonId)
  const deleteDiscussion = useDeleteDiscussion(lessonId)
  const adminDelete = useAdminDeleteDiscussion(lessonId)

  const handleSaveEdit = async (content: string) => {
    await updateDiscussion.mutateAsync({ discussionId: post.id, content })
    setMode('view')
  }

  const handleDelete = async () => {
    await deleteDiscussion.mutateAsync(post.id)
    setMode('view')
  }

  const handleAdminDelete = async () => {
    await adminDelete.mutateAsync(post.id)
    setMode('view')
  }

  const isDeleting = deleteDiscussion.isPending || adminDelete.isPending

  return (
    <div className="space-y-3 rounded-md border border-border p-4">
      {/* Header */}
      <div className="flex items-start justify-between gap-2">
        <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
          <span className="text-sm font-semibold">
            {post.isDeleted ? 'Deleted' : post.authorName}
          </span>
          <span className="text-xs text-muted-foreground">
            {formatDate(post.createdAt)}
          </span>
          {!post.isDeleted && post.updatedAt !== post.createdAt && (
            <span className="text-xs text-muted-foreground">(edited)</span>
          )}
        </div>

        {mode === 'view' && (
          <div className="flex shrink-0 items-center gap-1">
            {isOwn && (
              <>
                <button
                  type="button"
                  onClick={() => setMode('editing')}
                  className="text-xs text-muted-foreground hover:text-foreground"
                >
                  Edit
                </button>
                <button
                  type="button"
                  onClick={() => setMode('confirmDelete')}
                  className="text-xs text-destructive hover:text-destructive/80"
                >
                  Delete
                </button>
              </>
            )}
            {canAdminRemove && (
              <button
                type="button"
                onClick={() => setMode('confirmDelete')}
                className="text-xs text-destructive hover:text-destructive/80"
              >
                Remove
              </button>
            )}
          </div>
        )}
      </div>

      {/* Content or inline editor */}
      {mode === 'editing' ? (
        <DiscussionForm
          initialValue={post.content}
          onSubmit={handleSaveEdit}
          onCancel={() => setMode('view')}
          isPending={updateDiscussion.isPending}
          submitLabel="Save"
          autoFocus
        />
      ) : (
        <p
          className={`text-sm leading-relaxed ${
            post.isDeleted ? 'italic text-muted-foreground' : ''
          }`}
        >
          {post.content}
        </p>
      )}

      {updateDiscussion.isError && (
        <p className="text-xs text-destructive">
          Failed to save. Please try again.
        </p>
      )}

      {/* Delete confirmation */}
      {mode === 'confirmDelete' && (
        <div className="rounded-md border border-destructive/30 bg-destructive/5 p-3">
          <p className="mb-2 text-sm text-destructive">
            {canAdminRemove
              ? 'Remove this post for all users?'
              : 'Delete this post? This cannot be undone.'}
          </p>
          <div className="flex gap-2">
            <Button
              size="sm"
              variant="destructive"
              onClick={canAdminRemove ? handleAdminDelete : handleDelete}
              disabled={isDeleting}
            >
              {isDeleting ? 'Deleting…' : 'Confirm'}
            </Button>
            <Button
              size="sm"
              variant="outline"
              onClick={() => setMode('view')}
              disabled={isDeleting}
            >
              Cancel
            </Button>
          </div>
          {(deleteDiscussion.isError || adminDelete.isError) && (
            <p className="mt-1 text-xs text-destructive">
              Failed to delete. Please try again.
            </p>
          )}
        </div>
      )}

      {/* Replies section (hidden for deleted posts) */}
      {!post.isDeleted && (
        <ReplyList
          parentDiscussionId={post.id}
          replyCount={post.replyCount}
          lessonId={lessonId}
          currentUserId={currentUserId}
          isAdmin={isAdmin}
        />
      )}
    </div>
  )
}
