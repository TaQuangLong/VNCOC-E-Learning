interface TextRendererProps {
  content: string
}

export default function TextRenderer({ content }: TextRendererProps) {
  return (
    <div className="prose prose-neutral max-w-none">
      <pre className="whitespace-pre-wrap break-words font-sans text-sm leading-relaxed text-foreground">
        {content}
      </pre>
    </div>
  )
}
