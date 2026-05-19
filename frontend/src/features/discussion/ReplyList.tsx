import { useState } from 'react'
import { Button } from '@/components/ui/button'
import {
  useDiscussionReplies,
  useCreateReply,
  useUpdateDiscussion,
  useDeleteDiscussion,
  useAdminDeleteDiscussion,
} from './api'
import DiscussionForm from './DiscussionForm'
import type { ReplyDto } from './types'

interface Props {
  parentDiscussionId: number
  replyCount: number
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

export default function ReplyList({
  parentDiscussionId,
  replyCount,
  lessonId,
  currentUserId,
  isAdmin,
}: Props) {
  const [isExpanded, setIsExpanded] = useState(false)
  const [showReplyForm, setShowReplyForm] = useState(false)

  const { data, isLoading, isError } = useDiscussionReplies(
    parentDiscussionId,
    isExpanded && replyCount > 0,
  )
  const createReply = useCreateReply(lessonId, parentDiscussionId)

  const handleSubmitReply = async (content: string) => {
    await createReply.mutateAsync(content)
    setShowReplyForm(false)
  }

  const hasContent = isExpanded && (replyCount > 0 || showReplyForm)

  return (
    <div className="space-y-2">
      {/* Toggle / reply controls */}
      <div className="flex items-center gap-3">
        {replyCount > 0 && (
          <button
            type="button"
            onClick={() => setIsExpanded((p) => !p)}
            className="text-xs text-muted-foreground hover:text-foreground"
          >
            {isExpanded ? 'Hide Replies' : `See Replies (${replyCount})`}
          </button>
        )}
        {!showReplyForm && (
          <button
            type="button"
            onClick={() => {
              setShowReplyForm(true)
              setIsExpanded(true)
            }}
            className="text-xs text-primary hover:underline"
          >
            Reply
          </button>
        )}
      </div>

      {/* Replies */}
      {hasContent && (
        <div className="space-y-2 border-l-2 border-border pl-3 sm:pl-4">
          {isLoading && (
            <div className="space-y-2">
              {[1, 2].map((i) => (
                <div
                  key={i}
                  className="h-10 animate-pulse rounded-md bg-muted"
                />
              ))}
            </div>
          )}

          {isError && (
            <p className="text-xs text-destructive">Could not load replies.</p>
          )}

          {data?.items.map((reply) => (
            <ReplyItem
              key={reply.id}
              reply={reply}
              lessonId={lessonId}
              parentDiscussionId={parentDiscussionId}
              currentUserId={currentUserId}
              isAdmin={isAdmin}
            />
          ))}

          {showReplyForm && (
            <DiscussionForm
              onSubmit={handleSubmitReply}
              onCancel={() => setShowReplyForm(false)}
              isPending={createReply.isPending}
              placeholder="Write a reply…"
              submitLabel="Reply"
              autoFocus
            />
          )}
          {createReply.isError && (
            <p className="text-xs text-destructive">
              Failed to post reply. Please try again.
            </p>
          )}
        </div>
      )}
    </div>
  )
}

// ─── Reply item ───────────────────────────────────────────────────────────────

interface ReplyItemProps {
  reply: ReplyDto
  lessonId: number
  parentDiscussionId: number
  currentUserId: string
  isAdmin: boolean
}

type Mode = 'view' | 'editing' | 'confirmDelete'

function ReplyItem({
  reply,
  lessonId,
  parentDiscussionId,
  currentUserId,
  isAdmin,
}: ReplyItemProps) {
  const [mode, setMode] = useState<Mode>('view')

  const isOwn = reply.userId === currentUserId && !reply.isDeleted
  const canAdminRemove =
    isAdmin && !reply.isDeleted && reply.userId !== currentUserId

  const updateDiscussion = useUpdateDiscussion(lessonId, parentDiscussionId)
  const deleteDiscussion = useDeleteDiscussion(lessonId, parentDiscussionId)
  const adminDelete = useAdminDeleteDiscussion(lessonId, parentDiscussionId)

  const handleSaveEdit = async (content: string) => {
    await updateDiscussion.mutateAsync({ discussionId: reply.id, content })
    setMode('view')
  }

  const handleDelete = async () => {
    await deleteDiscussion.mutateAsync(reply.id)
    setMode('view')
  }

  const handleAdminDelete = async () => {
    await adminDelete.mutateAsync(reply.id)
    setMode('view')
  }

  const isDeleting = deleteDiscussion.isPending || adminDelete.isPending

  return (
    <div className="rounded-md bg-muted/30 px-3 py-2">
      {/* Header */}
      <div className="flex items-start justify-between gap-2">
        <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
          <span className="text-xs font-semibold">
            {reply.isDeleted ? 'Deleted' : reply.authorName}
          </span>
          <span className="text-xs text-muted-foreground">
            {formatDate(reply.createdAt)}
          </span>
          {!reply.isDeleted && reply.updatedAt !== reply.createdAt && (
            <span className="text-xs text-muted-foreground">(edited)</span>
          )}
        </div>

        {mode === 'view' && (
          <div className="flex shrink-0 gap-1">
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
        <div className="mt-2">
          <DiscussionForm
            initialValue={reply.content}
            onSubmit={handleSaveEdit}
            onCancel={() => setMode('view')}
            isPending={updateDiscussion.isPending}
            submitLabel="Save"
            autoFocus
          />
        </div>
      ) : (
        <p
          className={`mt-1 text-xs leading-relaxed ${
            reply.isDeleted ? 'italic text-muted-foreground' : ''
          }`}
        >
          {reply.content}
        </p>
      )}

      {updateDiscussion.isError && (
        <p className="mt-1 text-xs text-destructive">
          Failed to save. Please try again.
        </p>
      )}

      {/* Delete confirmation */}
      {mode === 'confirmDelete' && (
        <div className="mt-2 rounded-md border border-destructive/30 bg-destructive/5 p-2">
          <p className="mb-2 text-xs text-destructive">
            {canAdminRemove ? 'Remove this reply?' : 'Delete this reply?'}
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
              Failed. Please try again.
            </p>
          )}
        </div>
      )}
    </div>
  )
}
