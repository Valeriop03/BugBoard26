export type UserRole = 'ADMIN' | 'USER' | 'READONLY';

export interface User {
  id: number;
  email: string;
  role: UserRole;
  isActive: boolean;
}
