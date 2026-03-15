import { createFileRoute } from "@tanstack/react-router"
import { Alert, AlertTitle, AlertDescription } from "@/components/ui/alert"
import { InfoIcon, XCircleIcon } from "lucide-react"

export const Route = createFileRoute("/alert")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Alert</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Default</h2>
        <Alert>
          <InfoIcon />
          <AlertTitle>Heads up!</AlertTitle>
          <AlertDescription>You can add components to your app using the CLI.</AlertDescription>
        </Alert>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Destructive</h2>
        <Alert variant="destructive">
          <XCircleIcon />
          <AlertTitle>Error</AlertTitle>
          <AlertDescription>Your session has expired. Please log in again.</AlertDescription>
        </Alert>
      </div>
    </div>
  ),
})
