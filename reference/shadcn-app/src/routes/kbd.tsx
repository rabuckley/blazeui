import { createFileRoute } from "@tanstack/react-router"
import { Kbd } from "@/components/ui/kbd"

export const Route = createFileRoute("/kbd")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Kbd</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Single keys</h2>
        <div className="flex items-center gap-2">
          <Kbd>⌘</Kbd>
          <Kbd>K</Kbd>
          <Kbd>Shift</Kbd>
          <Kbd>Enter</Kbd>
        </div>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Key combinations</h2>
        <div className="flex items-center gap-1">
          <Kbd>⌘</Kbd>
          <span className="text-xs text-muted-foreground">+</span>
          <Kbd>C</Kbd>
        </div>
      </div>
    </div>
  ),
})
