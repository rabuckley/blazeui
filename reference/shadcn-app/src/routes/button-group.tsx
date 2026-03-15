import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import { ButtonGroup, ButtonGroupText } from "@/components/ui/button-group"

export const Route = createFileRoute("/button-group")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">ButtonGroup</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Horizontal</h2>
        <ButtonGroup>
          <Button variant="outline">Left</Button>
          <Button variant="outline">Center</Button>
          <Button variant="outline">Right</Button>
        </ButtonGroup>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Vertical</h2>
        <ButtonGroup orientation="vertical">
          <Button variant="outline">Top</Button>
          <Button variant="outline">Middle</Button>
          <Button variant="outline">Bottom</Button>
        </ButtonGroup>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With text</h2>
        <ButtonGroup>
          <ButtonGroupText>Filter:</ButtonGroupText>
          <Button variant="outline">All</Button>
          <Button variant="outline">Active</Button>
          <Button variant="outline">Inactive</Button>
        </ButtonGroup>
      </div>
    </div>
  ),
})
