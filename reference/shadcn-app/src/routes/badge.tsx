import { createFileRoute } from "@tanstack/react-router"
import { Badge } from "@/components/ui/badge"

export const Route = createFileRoute("/badge")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Badge</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Variants</h2>
        <div className="flex items-center gap-2">
          <Badge>Default</Badge>
          <Badge variant="secondary">Secondary</Badge>
          <Badge variant="destructive">Destructive</Badge>
          <Badge variant="outline">Outline</Badge>
        </div>
      </div>
    </div>
  ),
})
