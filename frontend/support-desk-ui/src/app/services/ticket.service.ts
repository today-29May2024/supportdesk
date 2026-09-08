import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  TicketListDto,
  TicketDetailDto,
  PagedResult,
  TicketFilterParams,
  CreateTicketDto,
  UpdateTicketDto,
  CreateCommentDto,
  TicketStatus,
  Comment
} from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5062/api/tickets';

  getTickets(filter: TicketFilterParams): Observable<PagedResult<TicketListDto>> {
    let params = new HttpParams();
    if (filter.search) params = params.set('search', filter.search);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.priority) params = params.set('priority', filter.priority);
    if (filter.assignedAgentId) params = params.set('assignedAgentId', filter.assignedAgentId.toString());
    if (filter.overdueOnly !== undefined) params = params.set('overdueOnly', filter.overdueOnly.toString());
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<TicketListDto>>(this.apiUrl, { params });
  }

  getTicketById(id: number): Observable<TicketDetailDto> {
    return this.http.get<TicketDetailDto>(`${this.apiUrl}/${id}`);
  }

  createTicket(dto: CreateTicketDto): Observable<TicketDetailDto> {
    return this.http.post<TicketDetailDto>(this.apiUrl, dto);
  }

  updateTicket(id: number, dto: UpdateTicketDto): Observable<TicketDetailDto> {
    return this.http.put<TicketDetailDto>(`${this.apiUrl}/${id}`, dto);
  }

  deleteTicket(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  assignAgent(ticketId: number, agentId: number): Observable<TicketDetailDto> {
    return this.http.put<TicketDetailDto>(`${this.apiUrl}/${ticketId}/assign`, { agentId });
  }

  unassignAgent(ticketId: number): Observable<TicketDetailDto> {
    return this.http.put<TicketDetailDto>(`${this.apiUrl}/${ticketId}/unassign`, {});
  }

  changeStatus(ticketId: number, newStatus: TicketStatus): Observable<TicketDetailDto> {
    return this.http.put<TicketDetailDto>(`${this.apiUrl}/${ticketId}/status`, { newStatus });
  }

  addComment(ticketId: number, dto: CreateCommentDto): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/${ticketId}/comments`, dto);
  }
}