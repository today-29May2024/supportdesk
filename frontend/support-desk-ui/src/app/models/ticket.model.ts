import { Agent } from './agent.model';

export enum TicketPriority {
  Low = 'Low',
  Normal = 'Normal',
  High = 'High',
  Critical = 'Critical'
}

export enum TicketStatus {
  New = 'New',
  InProgress = 'InProgress',
  Resolved = 'Resolved',
  Closed = 'Closed'
}

export const TICKET_PRIORITY_MAP : { [key: number]: string } = {
    0: TicketPriority.Low,
    1: TicketPriority.Normal,
    2: TicketPriority.High,
    3: TicketPriority.Critical
}

export const TICKET_PRIORITY_MAP_BY_STRING : { [key: string]: number } = {
    'Low': 0,
    'Normal': 1,
    'High': 2,
    'Critical': 3
}


export const TICKET_STATUS_MAP : { [key: number]: string } = {
    0: TicketStatus.New,
    1: TicketStatus.InProgress,
    2: TicketStatus.Resolved,
    3: TicketStatus.Closed
}

export interface Comment {
  id: number;
  ticketId: number;
  authorName: string;
  body: string;
  createdDate: string;
}

export interface TicketListDto {
  id: number;
  reference: string;
  title: string;
  customerName: string;
  priority: TicketPriority;
  status: TicketStatus;
  assignedAgentName?: string;
  assignedAgentId?: number;
  dueDate: string;
  isOverdue: boolean;
}

export interface TicketDetailDto {
  id: number;
  reference: string;
  title: string;
  description: string;
  customerName: string;
  customerEmail: string;
  priority: TicketPriority;
  status: TicketStatus;
  assignedAgentId?: number;
  assignedAgent?: Agent;
  createdDate: string;
  lastModifiedDate: string;
  resolvedDate?: string;
  closedDate?: string;
  dueDate: string;
  isOverdue: boolean;
  comments: Comment[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface TicketFilterParams {
  search?: string;
  status?: TicketStatus | '';
  priority?: TicketPriority | '';
  assignedAgentId?: number | '';
  overdueOnly?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CreateTicketDto {
  title: string;
  description: string;
  customerName: string;
  customerEmail: string;
  priority: TicketPriority;
}

export interface UpdateTicketDto extends CreateTicketDto {}

export interface CreateCommentDto {
  authorName: string;
  body: string;
}