import { useState, useCallback } from "react";
import { Copy, Check } from "lucide-react";

interface RawOcrPanelProps {
  rawOcr: string;
}

export function RawOcrPanel({ rawOcr }: RawOcrPanelProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = useCallback(async () => {
    await navigator.clipboard.writeText(rawOcr);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }, [rawOcr]);

  if (!rawOcr) return null;

  return (
    <div className="mt-6">
      <div className="flex items-center justify-between mb-3">
        <h2 className="text-lg font-semibold text-foreground">OCR brut</h2>
        <button
          onClick={handleCopy}
          className="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-muted hover:bg-muted/80 text-foreground rounded border border-border transition-colors"
        >
          {copied ? (
            <Check className="h-3.5 w-3.5 text-primary" />
          ) : (
            <Copy className="h-3.5 w-3.5" />
          )}
          {copied ? "Copié !" : "Copier"}
        </button>
      </div>
      <div className="p-5 bg-card rounded-lg border border-border shadow-sm">
        <pre className="whitespace-pre-wrap text-sm leading-relaxed text-foreground font-mono max-h-[500px] overflow-y-auto">
          {rawOcr}
        </pre>
      </div>
      <p className="mt-2 text-xs text-muted-foreground text-right">
        {rawOcr.length.toLocaleString()} caractères
      </p>
    </div>
  );
}
