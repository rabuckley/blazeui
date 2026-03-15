import { createFileRoute } from "@tanstack/react-router"
import { Label } from "@/components/ui/label"

export const Route = createFileRoute("/label")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Label</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Default</h2>
        <Label htmlFor="email">Your email address</Label>
      </div>
    </div>
  ),
})
