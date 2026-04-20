import ThemeToggle from "./ThemeToggle";

export default function Header() {
  return (
    <header className="sticky top-0 z-50 border-b border-border bg-background/80 px-4 backdrop-blur-lg">
      <div className="max-w-4xl mx-auto flex items-center justify-between py-3 sm:py-4">
        <h1 className="m-0 text-lg font-bold tracking-tight text-foreground">Analyse PDF OCR</h1>
        <ThemeToggle />
      </div>
    </header>
  );
}
