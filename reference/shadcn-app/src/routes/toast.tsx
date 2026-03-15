import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import { Toaster } from "@/components/ui/sonner"
import { toast } from "sonner"

export const Route = createFileRoute("/toast")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Toast</h1>
      <div className="flex flex-wrap gap-3">
        <Button
          variant="outline"
          onClick={() =>
            toast("Event has been created", {
              description: "Monday, January 3rd at 6:00pm",
            })
          }
        >
          Default
        </Button>
        <Button
          variant="outline"
          onClick={() =>
            toast.success("Event has been created", {
              description: "Monday, January 3rd at 6:00pm",
            })
          }
        >
          Success
        </Button>
        <Button
          variant="outline"
          onClick={() =>
            toast.info("Be at the area 10 minutes before the event time", {
              description: "Monday, January 3rd at 6:00pm",
            })
          }
        >
          Info
        </Button>
        <Button
          variant="outline"
          onClick={() =>
            toast.warning("Event start time is in the past", {
              description: "Monday, January 3rd at 6:00pm",
            })
          }
        >
          Warning
        </Button>
        <Button
          variant="outline"
          onClick={() =>
            toast.error("Event has not been created", {
              description: "Monday, January 3rd at 6:00pm",
            })
          }
        >
          Error
        </Button>
        <Button
          variant="outline"
          onClick={() =>
            toast("Event has been created", {
              description: "Monday, January 3rd at 6:00pm",
              action: {
                label: "Undo",
                onClick: () => {},
              },
            })
          }
        >
          With Action
        </Button>
        <Button
          variant="outline"
          onClick={() => {
            toast.promise<{ name: string }>(
              () =>
                new Promise((resolve) =>
                  setTimeout(() => resolve({ name: "Event" }), 2000)
                ),
              {
                loading: "Loading...",
                success: (data) => `${data.name} has been created`,
                error: "Error",
              }
            )
          }}
        >
          Promise
        </Button>
      </div>
      <Toaster />
    </div>
  ),
})
