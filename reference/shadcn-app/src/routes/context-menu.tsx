import { createFileRoute } from "@tanstack/react-router"
import {
  ContextMenu,
  ContextMenuTrigger,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuSeparator,
} from "@/components/ui/context-menu"

export const Route = createFileRoute("/context-menu")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Context Menu</h1>
      <div>
        <ContextMenu>
          <ContextMenuTrigger>
            <div className="flex h-36 w-72 items-center justify-center rounded-md border border-dashed text-sm text-muted-foreground">
              Right-click here
            </div>
          </ContextMenuTrigger>
          <ContextMenuContent>
            <ContextMenuItem>Cut</ContextMenuItem>
            <ContextMenuItem>Copy</ContextMenuItem>
            <ContextMenuSeparator />
            <ContextMenuItem>Paste</ContextMenuItem>
          </ContextMenuContent>
        </ContextMenu>
      </div>
    </div>
  ),
})
