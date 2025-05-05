function getMimeTypeFromBase64(base64: string) {
  if (base64.startsWith("iVBORw0KGgo")) {
    return "png"
  }
  if (base64.startsWith("/9j/")) {
    return "jpeg"
  }
  if (base64.startsWith("R0lGODdh")) {
    return "gif"
  }
  if (base64.startsWith("UklGR")) {
    return "webp"
  }
  return null
}

export function addBase64Prefix(base64Image: string) {
  const mimeType = getMimeTypeFromBase64(base64Image)
  if (!mimeType) {
    throw new Error("Unsupported image format")
  }
  if (!base64Image.startsWith("data:image")) {
    return `data:image/${mimeType};base64,${base64Image}`
  }
  return base64Image
}
