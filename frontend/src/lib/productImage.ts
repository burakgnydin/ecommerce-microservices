// Backend product data has no image field yet. Rather than showing random,
// unrelated stock photos, derive a consistent gradient per product id so the
// same product always gets the same placeholder color.
const GRADIENTS = [
  'from-neutral-700 to-neutral-950',
  'from-neutral-500 to-neutral-800',
  'from-zinc-600 to-neutral-900',
  'from-stone-600 to-stone-900',
  'from-neutral-800 to-black',
  'from-zinc-500 to-zinc-800',
]

export function getProductGradient(productId: string) {
  let hash = 0
  for (let i = 0; i < productId.length; i++) {
    hash = (hash * 31 + productId.charCodeAt(i)) >>> 0
  }
  return GRADIENTS[hash % GRADIENTS.length]
}
