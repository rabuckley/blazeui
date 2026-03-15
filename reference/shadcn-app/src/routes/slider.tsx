import { createFileRoute } from "@tanstack/react-router"
import { Slider } from "@/components/ui/slider"

export const Route = createFileRoute("/slider")({
  component: () => (
    <div>
      <h1 className="text-2xl font-bold mb-6">Slider</h1>
      <div className="max-w-md space-y-6">
        <div>
          <p className="text-sm text-muted-foreground mb-2">Default (50)</p>
          <Slider defaultValue={[50]} />
        </div>
        <div>
          <p className="text-sm text-muted-foreground mb-2">Step = 10</p>
          <Slider defaultValue={[30]} step={10} />
        </div>
        <div>
          <p className="text-sm text-muted-foreground mb-2">Disabled</p>
          <Slider defaultValue={[40]} disabled />
        </div>
      </div>
    </div>
  ),
})
