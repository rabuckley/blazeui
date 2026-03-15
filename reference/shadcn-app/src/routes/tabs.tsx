import { createFileRoute } from "@tanstack/react-router"
import {
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
} from "@/components/ui/tabs"

export const Route = createFileRoute("/tabs")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Tabs</h1>
      <Tabs defaultValue="account" className="w-full max-w-lg">
        <TabsList>
          <TabsTrigger value="account">Account</TabsTrigger>
          <TabsTrigger value="password">Password</TabsTrigger>
        </TabsList>
        <TabsContent value="account">
          <div className="rounded-lg border p-4">
            <h3 className="text-lg font-medium">Account</h3>
            <p className="text-sm text-muted-foreground">
              Make changes to your account here.
            </p>
          </div>
        </TabsContent>
        <TabsContent value="password">
          <div className="rounded-lg border p-4">
            <h3 className="text-lg font-medium">Password</h3>
            <p className="text-sm text-muted-foreground">
              Change your password here.
            </p>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  ),
})
