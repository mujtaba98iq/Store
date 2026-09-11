/** Mirrors `RestApi.Extensions.FormFileExtensions`. */
export const MAX_IMAGE_SIZE_IN_MEGABYTES = 5;
const MAX_IMAGE_SIZE_IN_BYTES = MAX_IMAGE_SIZE_IN_MEGABYTES * 1024 * 1024;
const ALLOWED_IMAGE_TYPES: readonly string[] = [
  'image/jpeg',
  'image/jpg',
  'image/png',
  'image/webp',
];

/** What the file picker offers - the API enforces the same list. */
export const IMAGE_ACCEPT = ALLOWED_IMAGE_TYPES.join(',');

/**
 * Why `file` cannot be uploaded, or null when it can. The API checks the same
 * rules (plus the file signature); this only saves the round trip of an upload
 * that was never going to be accepted.
 */
export function imageFileError(file: File): string | null {
  const contentType = file.type.split(';')[0].trim().toLowerCase();
  if (!ALLOWED_IMAGE_TYPES.includes(contentType)) {
    return 'Choose a JPEG, PNG or WebP image.';
  }
  if (file.size === 0) {
    return 'That file is empty.';
  }
  if (file.size > MAX_IMAGE_SIZE_IN_BYTES) {
    return `Image cannot exceed ${MAX_IMAGE_SIZE_IN_MEGABYTES} MB.`;
  }
  return null;
}
