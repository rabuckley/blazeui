import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
  FieldError,
} from "@/components/ui/field"

export const Route = createFileRoute("/field")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Field</h1>
      <div className="max-w-sm">
        <FieldGroup>
          <Field>
            <FieldLabel htmlFor="field-username">Username</FieldLabel>
            <Input
              id="field-username"
              type="text"
              placeholder="Enter your username"
            />
            <FieldDescription>
              Choose a unique username for your account.
            </FieldDescription>
          </Field>
          <Field>
            <FieldLabel htmlFor="field-email">Email</FieldLabel>
            <Input
              id="field-email"
              type="email"
              placeholder="name@example.com"
            />
            <FieldDescription>
              We'll send updates to this address.
            </FieldDescription>
          </Field>
          <Field data-invalid="true">
            <FieldLabel htmlFor="field-invalid">Invalid Input</FieldLabel>
            <Input id="field-invalid" placeholder="Error" aria-invalid />
            <FieldError>This field contains validation errors.</FieldError>
          </Field>
          <Field data-disabled="true">
            <FieldLabel htmlFor="field-disabled">Email</FieldLabel>
            <Input
              id="field-disabled"
              type="email"
              placeholder="Email"
              disabled
            />
            <FieldDescription>
              This field is currently disabled.
            </FieldDescription>
          </Field>
          <Field orientation="horizontal">
            <Button type="button" variant="outline">
              Cancel
            </Button>
            <Button type="submit">Submit</Button>
          </Field>
        </FieldGroup>
      </div>
    </div>
  ),
})
