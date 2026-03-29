import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { Input } from "@/components/ui/input"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"
import {
  Field,
  FieldContent,
  FieldDescription,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldError,
  FieldSeparator,
  FieldSet,
  FieldTitle,
} from "@/components/ui/field"

export const Route = createFileRoute("/field")({
  component: () => (
    <div className="space-y-12">
      <h1 className="text-2xl font-bold">Field</h1>

      {/* Basic Input */}
      <section className="space-y-4">
        <h2 className="text-lg font-semibold">Input</h2>
        <div className="max-w-sm">
          <FieldSet>
            <FieldLegend>Account Details</FieldLegend>
            <FieldDescription>
              Fill in your account information below.
            </FieldDescription>
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
          </FieldSet>
        </div>
      </section>

      {/* Checkbox */}
      <section className="space-y-4">
        <h2 className="text-lg font-semibold">Checkbox</h2>
        <div className="max-w-xs">
          <FieldGroup>
            <FieldSet>
              <FieldLegend variant="label">
                Show these items on the desktop
              </FieldLegend>
              <FieldDescription>
                Select the items you want to show on the desktop.
              </FieldDescription>
              <FieldGroup className="gap-3">
                <Field orientation="horizontal">
                  <Checkbox id="field-cb-hard-disks" defaultChecked />
                  <FieldLabel
                    htmlFor="field-cb-hard-disks"
                    className="font-normal"
                  >
                    Hard disks
                  </FieldLabel>
                </Field>
                <Field orientation="horizontal">
                  <Checkbox id="field-cb-external-disks" />
                  <FieldLabel
                    htmlFor="field-cb-external-disks"
                    className="font-normal"
                  >
                    External disks
                  </FieldLabel>
                </Field>
                <Field orientation="horizontal">
                  <Checkbox id="field-cb-cds-dvds" />
                  <FieldLabel
                    htmlFor="field-cb-cds-dvds"
                    className="font-normal"
                  >
                    CDs, DVDs, and iPods
                  </FieldLabel>
                </Field>
                <Field orientation="horizontal">
                  <Checkbox id="field-cb-connected-servers" />
                  <FieldLabel
                    htmlFor="field-cb-connected-servers"
                    className="font-normal"
                  >
                    Connected servers
                  </FieldLabel>
                </Field>
              </FieldGroup>
            </FieldSet>
            <FieldSeparator />
            <Field orientation="horizontal">
              <Checkbox id="field-cb-sync-folders" defaultChecked />
              <FieldContent>
                <FieldLabel htmlFor="field-cb-sync-folders">
                  Sync Desktop & Documents folders
                </FieldLabel>
                <FieldDescription>
                  Your Desktop & Documents folders are being synced with iCloud
                  Drive. You can access them from other devices.
                </FieldDescription>
              </FieldContent>
            </Field>
          </FieldGroup>
        </div>
      </section>

      {/* Radio */}
      <section className="space-y-4">
        <h2 className="text-lg font-semibold">Radio</h2>
        <div className="max-w-xs">
          <FieldSet>
            <FieldLegend variant="label">Subscription Plan</FieldLegend>
            <FieldDescription>
              Yearly and lifetime plans offer significant savings.
            </FieldDescription>
            <RadioGroup defaultValue="monthly">
              <Field orientation="horizontal">
                <RadioGroupItem value="monthly" id="field-radio-monthly" />
                <FieldLabel
                  htmlFor="field-radio-monthly"
                  className="font-normal"
                >
                  Monthly ($9.99/month)
                </FieldLabel>
              </Field>
              <Field orientation="horizontal">
                <RadioGroupItem value="yearly" id="field-radio-yearly" />
                <FieldLabel
                  htmlFor="field-radio-yearly"
                  className="font-normal"
                >
                  Yearly ($99.99/year)
                </FieldLabel>
              </Field>
              <Field orientation="horizontal">
                <RadioGroupItem value="lifetime" id="field-radio-lifetime" />
                <FieldLabel
                  htmlFor="field-radio-lifetime"
                  className="font-normal"
                >
                  Lifetime ($299.99)
                </FieldLabel>
              </Field>
            </RadioGroup>
          </FieldSet>
        </div>
      </section>

      {/* Choice Card */}
      <section className="space-y-4">
        <h2 className="text-lg font-semibold">Choice Card</h2>
        <div className="max-w-xs">
          <FieldGroup>
            <FieldSet>
              <FieldLegend variant="label">Compute Environment</FieldLegend>
              <FieldDescription>
                Select the compute environment for your cluster.
              </FieldDescription>
              <RadioGroup defaultValue="kubernetes">
                <FieldLabel htmlFor="field-cc-kubernetes">
                  <Field orientation="horizontal">
                    <FieldContent>
                      <FieldTitle>Kubernetes</FieldTitle>
                      <FieldDescription>
                        Run GPU workloads on a K8s cluster.
                      </FieldDescription>
                    </FieldContent>
                    <RadioGroupItem
                      value="kubernetes"
                      id="field-cc-kubernetes"
                    />
                  </Field>
                </FieldLabel>
                <FieldLabel htmlFor="field-cc-vm">
                  <Field orientation="horizontal">
                    <FieldContent>
                      <FieldTitle>Virtual Machine</FieldTitle>
                      <FieldDescription>
                        Access a cluster to run GPU workloads.
                      </FieldDescription>
                    </FieldContent>
                    <RadioGroupItem value="vm" id="field-cc-vm" />
                  </Field>
                </FieldLabel>
              </RadioGroup>
            </FieldSet>
          </FieldGroup>
        </div>
      </section>
    </div>
  ),
})
