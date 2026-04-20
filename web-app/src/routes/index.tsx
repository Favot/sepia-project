import { createFileRoute } from "@tanstack/react-router";
import { useOcrUpload } from "../components/ocr/use-ocr-upload";
import { DropZone } from "../components/ocr/DropZone";
import { FilePreview } from "../components/ocr/FilePreview";
import { ExtractedFields } from "../components/ocr/ExtractedFields";
import { RawOcrPanel } from "../components/ocr/RawOcrPanel";

export const Route = createFileRoute("/")({
  component: Index,
});

function Index() {
  const { files, isUploading, result, error, setDroppedFiles, removeFile, handleUpload } =
    useOcrUpload();

  return (
    <main className="max-w-4xl mx-auto px-4 pb-8 pt-14">
      <div className="max-w-2xl mx-auto">
        <DropZone onFilesSelected={setDroppedFiles} />

        {files.length > 0 && (
          <FilePreview
            file={files[0]}
            isUploading={isUploading}
            onRemove={removeFile}
            onUpload={handleUpload}
          />
        )}

        {error && (
          <div className="mt-4 p-4 rounded bg-destructive/10 text-destructive">{error}</div>
        )}

        <ExtractedFields result={result} />

        {result.rawOcr && <RawOcrPanel rawOcr={result.rawOcr} />}
      </div>
    </main>
  );
}
