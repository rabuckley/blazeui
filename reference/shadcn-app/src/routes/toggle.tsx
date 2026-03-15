import { createFileRoute } from "@tanstack/react-router"
import { Toggle } from "@/components/ui/toggle"
import { SlidersHorizontalIcon } from "lucide-react"

export const Route = createFileRoute("/toggle")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold mb-6">Toggle</h1>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Default</h2>
        <Toggle aria-label="Toggle sliders">
          <SlidersHorizontalIcon className="h-4 w-4" />
        </Toggle>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Outline variant</h2>
        <Toggle variant="outline" aria-label="Toggle bold">
          Bold
        </Toggle>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Sizes</h2>
        <div className="flex items-center gap-3">
          <Toggle size="sm" aria-label="Small toggle">S</Toggle>
          <Toggle aria-label="Default toggle">M</Toggle>
          <Toggle size="lg" aria-label="Large toggle">L</Toggle>
        </div>
      </div>
    </div>
  ),
})
