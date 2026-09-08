export enum Department {
  Technical = 'Technical',
  Billing = 'Billing',
  General = 'General'
}

export interface Agent {
  id: number;
  fullName: string;
  email: string;
  department: Department;
  active: boolean;
}