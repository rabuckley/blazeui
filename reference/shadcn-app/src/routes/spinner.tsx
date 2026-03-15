import { createFileRoute } from "@tanstack/react-router"
import { Spinner } from "@/components/ui/spinner"
import { Button } from "@/components/ui/button"

export const Route = createFileRoute("/spinner")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Spinner</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Default</h2>
        <Spinner />
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Custom size</h2>
        <div className="flex items-center gap-4">
          <Spinner className="size-4" />
          <Spinner className="size-6" />
          <Spinner className="size-8" />
        </div>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With button</h2>
        <Button disabled>
          <Spinner className="size-4" />
          Loading...
        </Button>
      </div>
    </div>
  ),
})
