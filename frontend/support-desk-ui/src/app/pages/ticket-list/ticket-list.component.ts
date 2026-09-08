import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { TicketService } from '../../services/ticket.service';
import { AgentService } from '../../services/agent.service';
import { TicketListDto, TicketFilterParams, TicketPriority, TicketStatus, CreateTicketDto, TICKET_PRIORITY_MAP, TICKET_STATUS_MAP } from '../../models/ticket.model';
import { Agent } from '../../models/agent.model';
import { TicketFormDialogComponent } from '../../components/ticket-form-dialog/ticket-form-dialog.component';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, TicketFormDialogComponent],
  templateUrl: './ticket-list.component.html',
  styleUrls: ['./ticket-list.component.scss']
})
export class TicketListComponent implements OnInit, OnDestroy {
  private readonly ticketService = inject(TicketService);
  private readonly agentService = inject(AgentService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  tickets: TicketListDto[] = [];
  agents: Agent[] = [];
  
  loading = false;
  totalCount = 0;
  
  // Requirement #6: RxJS debounced search
  searchControl = new FormControl('');
  
  filters: TicketFilterParams = {
    search: '',
    status: '',
    priority: '',
    assignedAgentId: '',
    overdueOnly: false,
    page: 1,
    pageSize: 10
  };

  isCreateModalOpen = false;
  createError: string | null = null;

  statuses = Object.values(TicketStatus);
  priorities = Object.values(TicketPriority);

  ticketPriorityMap = TICKET_PRIORITY_MAP;
  ticketStatusMap = TICKET_STATUS_MAP;

  ngOnInit(): void {
    this.loadAgents();
    this.loadTickets();

    // Debounce search input by 300ms
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(searchTerm => {
        this.filters.search = searchTerm || '';
        this.filters.page = 1;
        this.loadTickets();
      });
  }

  loadTickets(): void {
    this.loading = true;
    this.ticketService.getTickets(this.filters).subscribe({
      next: (res) => {
        this.tickets = res.items;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadAgents(): void {
    this.agentService.getAgents().subscribe(data => this.agents = data);
  }

  onFilterChange(): void {
    this.filters.page = 1;
    this.loadTickets();
  }

  onPageChange(newPage: number): void {
    this.filters.page = newPage;
    this.loadTickets();
  }

  openCreateModal(): void {
    this.createError = null;
    this.isCreateModalOpen = true;
  }

  handleCreateTicket(dto: CreateTicketDto): void {
    this.ticketService.createTicket(dto).subscribe({
      next: () => {
        this.isCreateModalOpen = false;
        this.loadTickets();
      },
      error: (err) => {
        this.createError = err.error?.message || 'Failed to create ticket.';
      }
    });
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / (this.filters.pageSize || 10));
  }

  toInt(s: any): number {
    return parseInt(s, 10);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}