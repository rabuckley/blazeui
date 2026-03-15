import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardAction,
  CardContent,
  CardFooter,
} from "@/components/ui/card"

export const Route = createFileRoute("/card")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Card</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Default</h2>
        <Card className="w-[380px]">
          <CardHeader>
            <CardTitle>Notifications</CardTitle>
            <CardDescription>You have 3 unread messages.</CardDescription>
          </CardHeader>
          <CardContent>
            <p>Card content goes here.</p>
          </CardContent>
          <CardFooter>
            <div className="w-full"><Button>Mark all as read</Button></div>
          </CardFooter>
        </Card>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Small size</h2>
        <Card size="sm" className="w-[350px]">
          <CardHeader>
            <CardTitle>Small Card</CardTitle>
            <CardDescription>A more compact card variant.</CardDescription>
          </CardHeader>
          <CardContent>
            <p>Compact content.</p>
          </CardContent>
        </Card>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With action</h2>
        <Card className="w-[380px]">
          <CardHeader>
            <CardTitle>Project Settings</CardTitle>
            <CardDescription>Manage your project configuration.</CardDescription>
            <CardAction>
              <Button variant="outline" size="sm">Edit</Button>
            </CardAction>
          </CardHeader>
          <CardContent>
            <p>Configure project settings here.</p>
          </CardContent>
        </Card>
      </div>
    </div>
  ),
})
