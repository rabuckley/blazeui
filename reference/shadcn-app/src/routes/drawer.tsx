import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import {
  Drawer,
  DrawerTrigger,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerDescription,
} from "@/components/ui/drawer"

export const Route = createFileRoute("/drawer")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Drawer</h1>
      <div className="flex gap-3">
        <Drawer direction="right">
          <DrawerTrigger asChild>
            <Button variant="outline">Right</Button>
          </DrawerTrigger>
          <DrawerContent>
            <DrawerHeader>
              <DrawerTitle>Right Drawer</DrawerTitle>
              <DrawerDescription>
                This drawer slides in from the right.
              </DrawerDescription>
            </DrawerHeader>
            <div className="p-4">
              <p className="text-sm text-muted-foreground">
                Drawer body content goes here.
              </p>
            </div>
          </DrawerContent>
        </Drawer>

        <Drawer direction="left">
          <DrawerTrigger asChild>
            <Button variant="outline">Left</Button>
          </DrawerTrigger>
          <DrawerContent>
            <DrawerHeader>
              <DrawerTitle>Left Drawer</DrawerTitle>
              <DrawerDescription>
                This drawer slides in from the left.
              </DrawerDescription>
            </DrawerHeader>
            <div className="p-4">
              <p className="text-sm text-muted-foreground">
                Drawer body content goes here.
              </p>
            </div>
          </DrawerContent>
        </Drawer>

        <Drawer direction="bottom">
          <DrawerTrigger asChild>
            <Button variant="outline">Bottom</Button>
          </DrawerTrigger>
          <DrawerContent>
            <DrawerHeader>
              <DrawerTitle>Bottom Drawer</DrawerTitle>
              <DrawerDescription>
                This drawer slides in from the bottom.
              </DrawerDescription>
            </DrawerHeader>
            <div className="p-4">
              <p className="text-sm text-muted-foreground">
                Drawer body content goes here.
              </p>
            </div>
          </DrawerContent>
        </Drawer>
      </div>
    </div>
  ),
})
