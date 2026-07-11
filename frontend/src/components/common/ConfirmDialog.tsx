interface Props {
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDialog({ message, onConfirm, onCancel }: Props) {
  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="card modal-box flex flex-col gap-4" onClick={(e) => e.stopPropagation()}>
        <p>{message}</p>
        <div className="flex-row gap-2">
          <button className="btn-danger" onClick={onConfirm}>Confirm</button>
          <button className="btn-secondary" onClick={onCancel}>Cancel</button>
        </div>
      </div>
    </div>
  );
}
