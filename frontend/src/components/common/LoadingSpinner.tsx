export default function LoadingSpinner({ text = 'Loading...' }: { text?: string }) {
  return (
    <div className="flex-row items-center gap-3" style={{ padding: 24, justifyContent: 'center' }}>
      <span className="spinner" />
      <span className="text-sm text-muted">{text}</span>
    </div>
  );
}
