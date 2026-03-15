import { createFileRoute } from "@tanstack/react-router"
import {
  Accordion,
  AccordionItem,
  AccordionTrigger,
  AccordionContent,
} from "@/components/ui/accordion"

export const Route = createFileRoute("/accordion")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Accordion</h1>
      <Accordion className="w-full max-w-lg">
        <AccordionItem value="item-1">
          <AccordionTrigger>Is it accessible?</AccordionTrigger>
          <AccordionContent>
            Yes. It adheres to the WAI-ARIA design pattern.
          </AccordionContent>
        </AccordionItem>
        <AccordionItem value="item-2">
          <AccordionTrigger>Is it styled?</AccordionTrigger>
          <AccordionContent>
            Yes. It comes with default styles that match the other components'
            aesthetic.
          </AccordionContent>
        </AccordionItem>
        <AccordionItem value="item-3">
          <AccordionTrigger>Is it animated?</AccordionTrigger>
          <AccordionContent>
            Yes. It's animated by default with Tailwind CSS animations.
          </AccordionContent>
        </AccordionItem>
      </Accordion>
    </div>
  ),
})
