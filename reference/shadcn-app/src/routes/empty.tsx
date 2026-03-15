import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import {
  Empty,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
  EmptyDescription,
  EmptyContent,
} from "@/components/ui/empty"
import { PackageIcon } from "lucide-react"

export const Route = createFileRoute("/empty")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Empty</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Default</h2>
        <Empty className="border">
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <PackageIcon />
            </EmptyMedia>
            <EmptyTitle>No items found</EmptyTitle>
            <EmptyDescription>Get started by creating your first item.</EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            <Button>Create item</Button>
          </EmptyContent>
        </Empty>
      </div>
    </div>
  ),
})
