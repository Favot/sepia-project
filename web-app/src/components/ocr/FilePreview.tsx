import { File, Loader2, X } from "lucide-react";

interface FilePreviewProps {
  file: File;
  isUploading: boolean;
  onRemove: () => void;
  onUpload: () => void;
}

export function FilePreview({
  file,
  isUploading,
  onRemove,
  onUpload,
}: FilePreviewProps) {
  return (
    <div className="mt-6">
      <div className="flex items-center justify-between p-3 bg-card rounded border border-border">
        <div className="flex items-center gap-3 overflow-hidden">
          <File className="h-5 w-5 text-muted-foreground shrink-0" />
          <span className="truncate text-foreground">{file.name}</span>
          <span className="text-sm text-muted-foreground shrink-0">
            ({(file.size / 1024).toFixed(1)} Ko)
          </span>
        </div>
        <button
          onClick={onRemove}
          className="p-1 hover:bg-muted rounded shrink-0 text-foreground"
        >
          <X className="h-4 w-4" />
        </button>
      </div>

      <button
        onClick={onUpload}
        disabled={isUploading}
        className="mt-4 w-full py-3 bg-primary text-primary-foreground rounded font-medium hover:bg-primary/90 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
      >
        {isUploading && <Loader2 className="h-4 w-4 animate-spin" />}
        {isUploading ? "Traitement OCR en cours..." : "Traiter l'OCR"}
      </button>
    </div>
  );
}
