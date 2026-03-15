import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import { PlusIcon } from "lucide-react"

export const Route = createFileRoute("/button")({
  component: () => (
    <section className="space-y-6">
      <h1 className="text-2xl font-bold mb-6">Button</h1>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Variants</h2>
        <div className="flex flex-wrap gap-3">
          <Button>Default</Button>
          <Button variant="destructive">Destructive</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="link">Link</Button>
        </div>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Sizes</h2>
        <div className="flex items-center gap-3">
          <Button size="sm">Small</Button>
          <Button>Default</Button>
          <Button size="lg">Large</Button>
          <Button size="icon">
            <PlusIcon className="h-4 w-4" />
          </Button>
        </div>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Disabled</h2>
        <Button disabled>Disabled</Button>
      </div>
    </section>
  ),
})
