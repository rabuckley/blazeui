import { createFileRoute } from "@tanstack/react-router"
import { AspectRatio } from "@/components/ui/aspect-ratio"

export const Route = createFileRoute("/aspect-ratio")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Aspect Ratio</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">16:9</h2>
        <div className="w-[450px]">
          <AspectRatio ratio={16 / 9}>
            <div className="flex h-full items-center justify-center rounded-md bg-muted">
              <span className="text-muted-foreground">16:9</span>
            </div>
          </AspectRatio>
        </div>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">1:1</h2>
        <div className="w-[200px]">
          <AspectRatio ratio={1}>
            <div className="flex h-full items-center justify-center rounded-md bg-muted">
              <span className="text-muted-foreground">1:1</span>
            </div>
          </AspectRatio>
        </div>
      </div>
    </div>
  ),
})
