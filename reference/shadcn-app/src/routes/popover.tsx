import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Popover,
  PopoverTrigger,
  PopoverContent,
} from "@/components/ui/popover"

export const Route = createFileRoute("/popover")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Popover</h1>
      <div>
        <Popover>
          <PopoverTrigger render={<Button variant="outline" />}>
            Open Popover
          </PopoverTrigger>
          <PopoverContent>
            <div className="grid gap-4">
              <div className="space-y-2">
                <h4 className="font-medium leading-none">Dimensions</h4>
                <p className="text-sm text-muted-foreground">
                  Set the dimensions for the layer.
                </p>
              </div>
              <div className="grid gap-2">
                <div className="grid grid-cols-3 items-center gap-4">
                  <label className="text-sm">Width</label>
                  <Input className="col-span-2 h-8" placeholder="100%" />
                </div>
                <div className="grid grid-cols-3 items-center gap-4">
                  <label className="text-sm">Height</label>
                  <Input className="col-span-2 h-8" placeholder="25px" />
                </div>
              </div>
            </div>
          </PopoverContent>
        </Popover>
      </div>
    </div>
  ),
})
