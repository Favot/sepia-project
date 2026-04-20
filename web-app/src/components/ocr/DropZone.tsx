import { useState, useCallback } from "react";
import { Upload } from "lucide-react";

interface DropZoneProps {
  onFilesSelected: (files: File[]) => void;
}

export function DropZone({ onFilesSelected }: DropZoneProps) {
  const [isDragging, setIsDragging] = useState(false);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragging(false);
      const droppedFiles = Array.from(e.dataTransfer.files);
      onFilesSelected(droppedFiles);
    },
    [onFilesSelected],
  );

  const handleFileSelect = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const selectedFiles = Array.from(e.target.files || []);
      onFilesSelected(selectedFiles);
      e.target.value = "";
    },
    [onFilesSelected],
  );

  return (
    <div
      className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
        isDragging ? "border-primary bg-primary/10" : "border-border hover:border-muted-foreground"
      }`}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
    >
      <Upload className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
      <p className="text-muted-foreground mb-2">
        Glisser-déposer un PDF ici, ou cliquer pour sélectionner
      </p>
      <input
        type="file"
        accept="application/pdf"
        onChange={handleFileSelect}
        className="hidden"
        id="file-input"
      />
      <label
        htmlFor="file-input"
        className="cursor-pointer inline-block px-4 py-2 bg-primary text-primary-foreground rounded hover:bg-primary/90 transition-colors"
      >
        Parcourir
      </label>
    </div>
  );
}
