import { useState, useCallback } from "react";

export interface OcrResult {
  numeroSiret: string | null;
  numeroEngagement: string | null;
  codeService: string | null;
  extractionMethod: string | null;
  rawOcr: string | null;
}

export function useOcrUpload() {
  const [files, setFiles] = useState<File[]>([]);
  const [isUploading, setIsUploading] = useState(false);
  const [result, setResult] = useState<OcrResult>({
    numeroSiret: null,
    numeroEngagement: null,
    codeService: null,
    extractionMethod: null,
    rawOcr: null,
  });
  const [error, setError] = useState<string | null>(null);

  const reset = useCallback(() => {
    setFiles([]);
    setResult({
      numeroSiret: null,
      numeroEngagement: null,
      codeService: null,
      extractionMethod: null,
      rawOcr: null,
    });
    setError(null);
  }, []);

  const setDroppedFiles = useCallback((newFiles: File[]) => {
    if (newFiles.length > 0) {
      setFiles(newFiles);
      setResult({
        numeroSiret: null,
        numeroEngagement: null,
        codeService: null,
        extractionMethod: null,
        rawOcr: null,
      });
      setError(null);
    }
  }, []);

  const removeFile = useCallback(() => {
    setFiles([]);
    setResult({
      numeroSiret: null,
      numeroEngagement: null,
      codeService: null,
      extractionMethod: null,
      rawOcr: null,
    });
    setError(null);
  }, []);

  const handleUpload = async () => {
    if (files.length === 0) return;

    setIsUploading(true);
    setResult({
      numeroSiret: null,
      numeroEngagement: null,
      codeService: null,
      extractionMethod: null,
      rawOcr: null,
    });
    setError(null);

    try {
      const formData = new FormData();
      formData.append("file", files[0]);

      const apiUrl = import.meta.env.VITE_API_URL || "http://localhost:3000";
      const response = await fetch(`${apiUrl}/uploads`, {
        method: "POST",
        body: formData,
      });

      if (response.ok) {
        const data = await response.json();
        setResult({
          numeroSiret: data.numeroSiret || "Non trouvé",
          numeroEngagement: data.numeroEngagement || "Non trouvé",
          codeService: data.codeService || null,
          extractionMethod: data.extractionMethod || null,
          rawOcr: data.rawOcr || "",
        });
        setFiles([]);
      } else {
        setError(`Échec de l'envoi : ${response.statusText}`);
      }
    } catch (err) {
      setError(`Échec de l'envoi : ${err instanceof Error ? err.message : "Erreur inconnue"}`);
    } finally {
      setIsUploading(false);
    }
  };

  return {
    files,
    isUploading,
    result,
    error,
    setDroppedFiles,
    removeFile,
    handleUpload,
    reset,
  };
}
