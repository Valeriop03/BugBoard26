export interface Notification {
  id: number;
  userId: number;
  issueId: number;
  issueTitle: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}
