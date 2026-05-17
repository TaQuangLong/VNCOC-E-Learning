interface YouTubePlayerProps {
  url: string
}

// Only alphanumeric, dash, and underscore — YouTube video ID format
const YOUTUBE_ID_REGEX = /^[a-zA-Z0-9_-]{1,20}$/

function extractYouTubeId(url: string): string | null {
  try {
    const parsed = new URL(url)
    let id: string | null = null
    if (
      parsed.hostname === 'www.youtube.com' ||
      parsed.hostname === 'youtube.com'
    ) {
      id = parsed.searchParams.get('v')
    } else if (parsed.hostname === 'youtu.be') {
      id = parsed.pathname.slice(1).split('?')[0]
    } else if (parsed.pathname.startsWith('/embed/')) {
      id = parsed.pathname.split('/embed/')[1].split('?')[0]
    }
    // Validate ID format before use to prevent injecting unexpected characters
    return id && YOUTUBE_ID_REGEX.test(id) ? id : null
  } catch {
    // invalid URL
  }
  return null
}

export default function YouTubePlayer({ url }: YouTubePlayerProps) {
  const videoId = extractYouTubeId(url)

  if (!videoId) {
    return (
      <div className="flex aspect-video items-center justify-center rounded-md bg-muted text-sm text-muted-foreground">
        Invalid YouTube URL
      </div>
    )
  }

  return (
    <div className="aspect-video w-full overflow-hidden rounded-md">
      <iframe
        src={`https://www.youtube.com/embed/${videoId}`}
        title="Lesson video"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
        allowFullScreen
        className="h-full w-full border-0"
      />
    </div>
  )
}
