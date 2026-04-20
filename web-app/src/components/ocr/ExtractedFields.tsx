import { Check, Copy } from "lucide-react";
import { useCallback, useState } from "react";
import type { OcrResult } from "./use-ocr-upload";

interface ExtractedFieldsProps {
  result: Pick<
    OcrResult,
    "numeroSiret" | "numeroEngagement" | "codeService" | "extractionMethod"
  >;
}

export function ExtractedFields({ result }: ExtractedFieldsProps) {
  const { numeroSiret, numeroEngagement, codeService, extractionMethod } =
    result;

  if (!numeroSiret && !numeroEngagement && !codeService) return null;

  return (
    <div className="mt-6">
      <h2 className="text-lg font-semibold text-foreground mb-4">
        Champs extraits
        {extractionMethod && (
          <span
            className={`ml-3 text-xs font-mono px-2 py-0.5 rounded ${extractionMethod === "llm" || extractionMethod === "bbox_annotation" ? "bg-primary/10 text-primary" : "bg-secondary/10 text-secondary"}`}
          >
            {extractionMethod === "llm"
              ? "llm ✓"
              : extractionMethod === "bbox_annotation"
                ? "bbox_annotation ✓"
                : "regex_fallback"}
          </span>
        )}
      </h2>
      <div className="space-y-3">
        <FieldRow label="Numéro de SIRET" value={numeroSiret} />
        <FieldRow label="Numéro d'engagement" value={numeroEngagement} />
        {codeService && (
          <FieldRow label="Code de service" value={codeService} />
        )}
      </div>
    </div>
  );
}

function FieldRow({ label, value }: { label: string; value: string | null }) {
  const [copied, setCopied] = useState(false);

  const handleCopy = useCallback(async () => {
    if (!value) return;
    await navigator.clipboard.writeText(value);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }, [value]);

  if (!value) return null;

  return (
    <div className="flex items-center gap-3 p-4 bg-card rounded border border-border">
      <span className="text-sm font-medium text-muted-foreground min-w-45">
        {label}
      </span>
      <span className="text-base font-mono font-semibold text-foreground flex-1">
        {value}
      </span>
      <button
        onClick={handleCopy}
        className="p-1.5 hover:bg-muted rounded text-muted-foreground hover:text-foreground transition-colors"
        title="Copier"
      >
        {copied ? (
          <Check className="h-3.5 w-3.5 text-primary" />
        ) : (
          <Copy className="h-3.5 w-3.5" />
        )}
      </button>
    </div>
  );
}
