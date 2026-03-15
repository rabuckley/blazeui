import { createFileRoute } from "@tanstack/react-router"
import { Avatar, AvatarFallback, AvatarImage, AvatarBadge, AvatarGroup, AvatarGroupCount } from "@/components/ui/avatar"

export const Route = createFileRoute("/avatar")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold mb-6">Avatar</h1>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Sizes</h2>
        <div className="flex items-center gap-4">
          <Avatar size="sm">
            <AvatarFallback>SM</AvatarFallback>
          </Avatar>
          <Avatar>
            <AvatarFallback>MD</AvatarFallback>
          </Avatar>
          <Avatar size="lg">
            <AvatarFallback>LG</AvatarFallback>
          </Avatar>
        </div>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With fallback</h2>
        <Avatar>
          <AvatarImage src="https://invalid-url.example" alt="User" />
          <AvatarFallback>JD</AvatarFallback>
        </Avatar>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With badge</h2>
        <div className="flex items-center gap-4">
          <Avatar size="sm">
            <AvatarFallback>SM</AvatarFallback>
            <AvatarBadge />
          </Avatar>
          <Avatar>
            <AvatarFallback>MD</AvatarFallback>
            <AvatarBadge />
          </Avatar>
          <Avatar size="lg">
            <AvatarFallback>LG</AvatarFallback>
            <AvatarBadge />
          </Avatar>
        </div>
      </div>
      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Group</h2>
        <AvatarGroup>
          <Avatar>
            <AvatarFallback>AB</AvatarFallback>
          </Avatar>
          <Avatar>
            <AvatarFallback>CD</AvatarFallback>
          </Avatar>
          <Avatar>
            <AvatarFallback>EF</AvatarFallback>
          </Avatar>
          <AvatarGroupCount>+3</AvatarGroupCount>
        </AvatarGroup>
      </div>
    </div>
  ),
})
