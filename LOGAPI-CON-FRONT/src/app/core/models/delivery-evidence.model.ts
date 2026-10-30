export interface DeliveryEvidence {
  id: number;
  orderId: number;
  evidenceType: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  comments: string | null;
  uploadedByUserId: string;
  uploadedAtUtc: string;
}
