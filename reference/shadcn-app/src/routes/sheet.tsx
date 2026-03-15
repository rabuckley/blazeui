import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import {
  Sheet,
  SheetTrigger,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from "@/components/ui/sheet"

export const Route = createFileRoute("/sheet")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Sheet</h1>

      <div>
        <div className="flex gap-3">
          <Sheet>
            <SheetTrigger render={<Button variant="outline" />}>
              Right
            </SheetTrigger>
            <SheetContent side="right">
              <SheetHeader>
                <SheetTitle>Right Sheet</SheetTitle>
                <SheetDescription>This sheet slides in from the right.</SheetDescription>
              </SheetHeader>
              <div className="p-4">
                <p className="text-sm text-muted-foreground">Sheet body content goes here.</p>
              </div>
            </SheetContent>
          </Sheet>

          <Sheet>
            <SheetTrigger render={<Button variant="outline" />}>
              Left
            </SheetTrigger>
            <SheetContent side="left">
              <SheetHeader>
                <SheetTitle>Left Sheet</SheetTitle>
                <SheetDescription>This sheet slides in from the left.</SheetDescription>
              </SheetHeader>
              <div className="p-4">
                <p className="text-sm text-muted-foreground">Sheet body content goes here.</p>
              </div>
            </SheetContent>
          </Sheet>

          <Sheet>
            <SheetTrigger render={<Button variant="outline" />}>
              Top
            </SheetTrigger>
            <SheetContent side="top">
              <SheetHeader>
                <SheetTitle>Top Sheet</SheetTitle>
                <SheetDescription>This sheet slides in from the top.</SheetDescription>
              </SheetHeader>
              <div className="p-4">
                <p className="text-sm text-muted-foreground">Sheet body content goes here.</p>
              </div>
            </SheetContent>
          </Sheet>

          <Sheet>
            <SheetTrigger render={<Button variant="outline" />}>
              Bottom
            </SheetTrigger>
            <SheetContent side="bottom">
              <SheetHeader>
                <SheetTitle>Bottom Sheet</SheetTitle>
                <SheetDescription>This sheet slides in from the bottom.</SheetDescription>
              </SheetHeader>
              <div className="p-4">
                <p className="text-sm text-muted-foreground">Sheet body content goes here.</p>
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </div>
  ),
})
