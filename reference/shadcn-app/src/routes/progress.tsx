import { createFileRoute } from "@tanstack/react-router"
import { Progress } from "@/components/ui/progress"

export const Route = createFileRoute("/progress")({
  component: () => (
    <div>
      <h1 className="text-2xl font-bold mb-6">Progress</h1>
      <div className="max-w-md space-y-6">
        <div>
          <p className="text-sm text-muted-foreground mb-2">25%</p>
          <Progress value={25} />
        </div>
        <div>
          <p className="text-sm text-muted-foreground mb-2">50%</p>
          <Progress value={50} />
        </div>
        <div>
          <p className="text-sm text-muted-foreground mb-2">75%</p>
          <Progress value={75} />
        </div>
        <div>
          <p className="text-sm text-muted-foreground mb-2">Indeterminate (null)</p>
          <Progress value={null} />
        </div>
      </div>
    </div>
  ),
})
