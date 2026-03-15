import { createFileRoute } from "@tanstack/react-router"
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group"

export const Route = createFileRoute("/toggle-group")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold mb-6">ToggleGroup</h1>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Single selection</h2>
        <ToggleGroup defaultValue={["center"]}>
          <ToggleGroupItem value="left" aria-label="Left">Left</ToggleGroupItem>
          <ToggleGroupItem value="center" aria-label="Center">Center</ToggleGroupItem>
          <ToggleGroupItem value="right" aria-label="Right">Right</ToggleGroupItem>
        </ToggleGroup>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Multiple selection</h2>
        {/* @ts-expect-error toggleMultiple changes the generic type parameter */}
        <ToggleGroup toggleMultiple>
          <ToggleGroupItem value="bold" aria-label="Bold">Bold</ToggleGroupItem>
          <ToggleGroupItem value="italic" aria-label="Italic">Italic</ToggleGroupItem>
          <ToggleGroupItem value="underline" aria-label="Underline">Underline</ToggleGroupItem>
        </ToggleGroup>
      </div>
    </div>
  ),
})
