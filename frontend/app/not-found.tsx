import { Container } from "@/components/site/blocks";
import { NotFoundContent } from "@/components/site/not-found-content";

export default function NotFound() {
  return (
    <main>
      <Container className="py-20">
        <NotFoundContent />
      </Container>
    </main>
  );
}
