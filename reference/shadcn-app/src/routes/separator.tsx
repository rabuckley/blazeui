import { createFileRoute } from "@tanstack/react-router"
import { Separator } from "@/components/ui/separator"

export const Route = createFileRoute("/separator")({
  component: () => (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold mb-6">Separator</h1>
      <div>
        <h3 className="text-sm font-medium">Horizontal</h3>
        <p className="text-sm text-muted-foreground">Content above</p>
        <Separator className="my-4" />
        <p className="text-sm text-muted-foreground">Content below</p>
      </div>
      <div className="flex h-5 items-center gap-4 text-sm">
        <span>Blog</span>
        <Separator orientation="vertical" />
        <span>Docs</span>
        <Separator orientation="vertical" />
        <span>Source</span>
      </div>
    </div>
  ),
})
